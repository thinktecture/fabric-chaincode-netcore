using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Protos;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;
using Thinktecture.HyperledgerFabric.Chaincode.Handler;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
    public class ChaincodeFromContracts : IChaincode
    {
        private delegate Task<ByteString> DynamicMethodInvocationDelegate(
            IContractContext context,
            params string[] parameters
        );

        private class DynamicMethodInvocation
        {
            public DynamicMethodInvocationDelegate Delegate { get; set; }
            public int ParameterCount { get; set; }
        }

        private class ChaincodeContract
        {
            public string Namespace { get; set; }
            public Type ContractType { get; set; }
            public IDictionary<string, DynamicMethodInvocation> Functions { get; set; }
            public IContract Contract { get; set; }
        }

        private readonly IContractContextFactory _contractContextFactory;
        private readonly ILogger<ChaincodeFromContracts> _logger;

        private readonly IDictionary<string, ChaincodeContract> _chaincodeContracts =
            new ConcurrentDictionary<string, ChaincodeContract>();

        public ChaincodeFromContracts(
            IEnumerable<IContract> contracts,
            IContractContextFactory contractContextFactory,
            ILogger<ChaincodeFromContracts> logger
        )
        {
            _contractContextFactory = contractContextFactory;
            _logger = logger;

            Initialize(contracts.ToArray());
        }

        private void Initialize(ICollection<IContract> contracts)
        {
            _logger.LogInformation($"Initializing Chaincode with {contracts.Count} contracts.");

            if (contracts.Count == 0)
            {
                throw new Exception("Can not start Chaincode without any contracts.");
            }

            foreach (var contract in contracts)
            {
                var chaincodeContract = new ChaincodeContract()
                {
                    Namespace = contract.Namespace,
                    Contract = contract,
                    ContractType = contract.GetType(),
                    Functions = contract.GetType()
                        .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                        .Where(m => m.ReturnType ==
                                    typeof(Task<ByteString>
                                    )) // for easier calling we only take a Task<ByteString> for now
                        .ToDictionary(key => key.Name, element =>
                        {
                            var contractContextExpressionParameter = Expression.Parameter(typeof(IContractContext));
                            var stringArrayExpressionParameter = Expression.Parameter(typeof(string[]));

                            var lambdaParameters = new[]
                            {
                                contractContextExpressionParameter,
                                stringArrayExpressionParameter,
                            };

                            var methodPositionalStringExpressionParameters = element.GetParameters()
                                .Skip(1) // skips the IContractContext
                                .Select((p, index) =>
                                    Expression.ArrayIndex(stringArrayExpressionParameter, Expression.Constant(index)))
                                .Cast<Expression>();

                            var parameters = new[]
                                {
                                    contractContextExpressionParameter
                                }
                                .Concat(methodPositionalStringExpressionParameters);

                            var callExpression = Expression.Call(
                                Expression.Constant(contract),
                                element,
                                parameters
                            );

                            var compiledCall = Expression.Lambda<DynamicMethodInvocationDelegate>(
                                // (context, string[]) => functionName(context, string...) 
                                callExpression, lambdaParameters
                            ).Compile();

                            return new DynamicMethodInvocation()
                            {
                                Delegate = compiledCall,
                                ParameterCount = element.GetParameters().Length
                            };
                        })
                };

                if (chaincodeContract.Functions.Count == 0)
                {
                    throw new Exception($"Contract {contract.GetType().Name} does not implement any suitable method.");
                }

                _chaincodeContracts.Add(contract.Namespace, chaincodeContract);
                _logger.LogInformation($"Added contract {chaincodeContract.ContractType.Name}", chaincodeContract);
            }
        }

        public Task<Response> Init(IChaincodeStub stub)
        {
            return Invoke(stub);
        }

        public async Task<Response> Invoke(IChaincodeStub stub)
        {
            try
            {
                var functionAndParameters = stub.GetFunctionAndParameters();

                var splitFunctionName = functionAndParameters.Function.Split('_');

                var @namespace = splitFunctionName[0];
                var functionName = splitFunctionName[1];

                if (!_chaincodeContracts.TryGetValue(@namespace, out var chaincodeContract))
                {
                    throw new Exception($"Namespace {@namespace} is not known!");
                }

                var context = _contractContextFactory.Create(stub);

                if (!chaincodeContract.Functions.TryGetValue(functionName, out var dynamicMethodInvocation))
                {
                    chaincodeContract.Contract.UnknownFunctionCalled(context, functionName);
                    return Shim.Error($"Unknown function {functionName} called in namespace {@namespace}");
                }

                // TODO: clarify, if we just fill or remove parameters if there are too less or too much
                // -1 for contractContext
                if (functionAndParameters.Parameters.Count != dynamicMethodInvocation.ParameterCount - 1)
                {
                    throw new ArgumentException(
                        $"Expected {dynamicMethodInvocation.ParameterCount - 1} parameters, but got {functionAndParameters.Parameters.Count}");
                }

                _logger.LogDebug($"Start \"BeforeInvocation\" for {functionAndParameters.Function}");
                context = chaincodeContract.Contract.BeforeInvocation(context);
                _logger.LogDebug($"End \"BeforeInvocation\" for {functionAndParameters.Function}");

                _logger.LogDebug($"Start chaincode invocation for {functionAndParameters.Function}");
                var result =
                    await dynamicMethodInvocation.Delegate(context, functionAndParameters.Parameters.ToArray());
                _logger.LogDebug($"End chaincode invocation for {functionAndParameters.Function}");

                _logger.LogDebug($"Start \"AfterInvocation\" for {functionAndParameters.Function}");
                chaincodeContract.Contract.AfterInvocation(context, result);
                _logger.LogDebug($"End \"AfterInvocation\" for {functionAndParameters.Function}");

                return Shim.Success(result);
            }
            catch (Exception e)
            {
                return Shim.Error(e);
            }
        }
    }
}
