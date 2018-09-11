using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Protobuf;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;
using Thinktecture.HyperledgerFabric.Chaincode.Contract;
using Thinktecture.HyperledgerFabric.Chaincode.Extensions;
using Xunit;

namespace Thinktecture.HyperledgerFabric.Chaincode.Test.Contract
{
    public class ChaincodeFromContractTest
    {
        private class SampleContract1 : ContractBase
        {
            public SampleContract1(IDictionary<string, string> metadata = null)
                : base("SampleContract1", metadata)
            {
            }

            public Task<ByteString> Foo(IContractContext context, string param1)
            {
                return Task.FromResult(param1.ToByteString());
            }
        }

        private class SampleContract2 : ContractBase
        {
            public SampleContract2(IDictionary<string, string> metadata = null)
                : base("SampleContract2", metadata)
            {
            }

            public Task<ByteString> Bar(IContractContext context, string param1, string param2, string param3)
            {
                return Task.FromResult($"{param1}{param2}{param3}".ToByteString());
            }
        }

        private class SampleContract3 : ContractBase
        {
            public SampleContract3(IDictionary<string, string> metadata = null)
                : base("SampleContract3", metadata)
            {
            }

            public Task<ByteString> Foo(IContractContext context, string param1)
            {
                return Task.FromResult(param1.ToByteString());
            }

            public override void UnknownFunctionCalled(IContractContext context, string functionName)
            {
                // do nothing on purpose
            }
        }

        private class EmptyContract : ContractBase
        {
            public EmptyContract(IDictionary<string, string> metadata = null)
                : base("EmptyContract", metadata)
            {
            }
        }

        [Fact]
        public void Constructor_does_not_throw()
        {
            Action action = () => new ChaincodeFromContracts(
                new List<IContract>() {new SampleContract1(), new SampleContract2()},
                new Mock<IContractContextFactory>().Object, new NullLogger<ChaincodeFromContracts>()
            );

            action.Should().NotThrow();
        }

        [Fact]
        public async Task Invoke_invokes_the_contract_function_with_one_parameter()
        {
            var contractContextFactoryMock = new Mock<IContractContextFactory>();
            var chaincodeStubMock = new Mock<IChaincodeStub>();
            chaincodeStubMock.Setup(m => m.GetFunctionAndParameters())
                .Returns(new ChaincodeFunctionParameterInformation()
                {
                    Function = "SampleContract1_Foo",
                    Parameters = {"unittest"}
                });

            var sut = new ChaincodeFromContracts(new List<IContract>() {new SampleContract1()},
                contractContextFactoryMock.Object, new NullLogger<ChaincodeFromContracts>()
            );

            var result = await sut.Invoke(chaincodeStubMock.Object);
            result.Payload.Should().Equal("unittest".ToByteString());
        }

        [Fact]
        public async Task Invoke_invokes_the_contract_function_with_multiple_parameters()
        {
            var contractContextFactoryMock = new Mock<IContractContextFactory>();
            var chaincodeStubMock = new Mock<IChaincodeStub>();
            chaincodeStubMock.Setup(m => m.GetFunctionAndParameters())
                .Returns(new ChaincodeFunctionParameterInformation()
                {
                    Function = "SampleContract2_Bar",
                    Parameters = {"unittest", "unittest2", "unittest3"}
                });

            var sut = new ChaincodeFromContracts(new List<IContract>() {new SampleContract2()},
                contractContextFactoryMock.Object, new NullLogger<ChaincodeFromContracts>()
            );

            var result = await sut.Invoke(chaincodeStubMock.Object);
            result.Payload.Should().Equal("unittestunittest2unittest3".ToByteString());
        }

        [Fact]
        public async Task Invoke_invokes_the_contract_function_but_returns_a_shim_error_when_a_parameter_is_missing()
        {
            var contractContextFactoryMock = new Mock<IContractContextFactory>();
            var chaincodeStubMock = new Mock<IChaincodeStub>();
            chaincodeStubMock.Setup(m => m.GetFunctionAndParameters())
                .Returns(new ChaincodeFunctionParameterInformation()
                {
                    Function = "SampleContract2_Bar",
                    Parameters = {"unittest", "unittest2"}
                });

            var sut = new ChaincodeFromContracts(new List<IContract>() {new SampleContract2()},
                contractContextFactoryMock.Object, new NullLogger<ChaincodeFromContracts>()
            );

            var result = await sut.Invoke(chaincodeStubMock.Object);
            result.Payload.Should().Equal(ByteString.Empty);
            result.Message.Should().Contain("Expected 3 parameters, but got 2");
        }

        [Fact]
        public async Task
            Invoke_invokes_the_contract_function_but_returns_a_shim_error_when_there_are_too_many_parameters()
        {
            var contractContextFactoryMock = new Mock<IContractContextFactory>();
            var chaincodeStubMock = new Mock<IChaincodeStub>();
            chaincodeStubMock.Setup(m => m.GetFunctionAndParameters())
                .Returns(new ChaincodeFunctionParameterInformation()
                {
                    Function = "SampleContract2_Bar",
                    Parameters = {"unittest", "unittest2", "unittest3", "toomuchparams"}
                });

            var sut = new ChaincodeFromContracts(new List<IContract>() {new SampleContract2()},
                contractContextFactoryMock.Object, new NullLogger<ChaincodeFromContracts>()
            );

            var result = await sut.Invoke(chaincodeStubMock.Object);
            result.Payload.Should().Equal(ByteString.Empty);
            result.Message.Should().Contain("Expected 3 parameters, but got 4");
        }

        [Fact]
        public async Task Invoke_invokes_the_contract_function_but_returns_a_shim_error_when_namespace_is_unknown()
        {
            var contractContextFactoryMock = new Mock<IContractContextFactory>();
            var chaincodeStubMock = new Mock<IChaincodeStub>();
            chaincodeStubMock.Setup(m => m.GetFunctionAndParameters())
                .Returns(new ChaincodeFunctionParameterInformation()
                {
                    Function = "UnknownChaincode_Bar",
                    Parameters = {"unittest"}
                });

            var sut = new ChaincodeFromContracts(new List<IContract>() {new SampleContract1()},
                contractContextFactoryMock.Object, new NullLogger<ChaincodeFromContracts>()
            );

            var result = await sut.Invoke(chaincodeStubMock.Object);
            result.Payload.Should().Equal(ByteString.Empty);
            result.Message.Should().Contain("Namespace UnknownChaincode is not known!");
        }

        [Fact]
        public async Task Invoke_invokes_the_contract_function_but_returns_a_shim_error_when_function_is_unknown()
        {
            var contractContextFactoryMock = new Mock<IContractContextFactory>();
            var chaincodeStubMock = new Mock<IChaincodeStub>();
            chaincodeStubMock.Setup(m => m.GetFunctionAndParameters())
                .Returns(new ChaincodeFunctionParameterInformation()
                {
                    Function = "SampleContract1_Bar",
                    Parameters = {"unittest"}
                });

            var sut = new ChaincodeFromContracts(new List<IContract>() {new SampleContract1()},
                contractContextFactoryMock.Object, new NullLogger<ChaincodeFromContracts>()
            );

            var result = await sut.Invoke(chaincodeStubMock.Object);
            result.Payload.Should().Equal(ByteString.Empty);
            result.Message.Should().Contain("Function does not exist");
        }

        [Fact]
        public async Task
            Invoke_invokes_the_contract_function_but_returns_a_shim_error_when_function_is_unknown_even_when_the_contract_UnknownFunctionCalled_does_not_throw()
        {
            var contractContextFactoryMock = new Mock<IContractContextFactory>();
            var chaincodeStubMock = new Mock<IChaincodeStub>();
            chaincodeStubMock.Setup(m => m.GetFunctionAndParameters())
                .Returns(new ChaincodeFunctionParameterInformation()
                {
                    Function = "SampleContract3_Bar",
                    Parameters = {"unittest"}
                });

            var sut = new ChaincodeFromContracts(new List<IContract>() {new SampleContract3()},
                contractContextFactoryMock.Object, new NullLogger<ChaincodeFromContracts>()
            );

            var result = await sut.Invoke(chaincodeStubMock.Object);
            result.Payload.Should().Equal(ByteString.Empty);
            result.Message.Should().Be("Unknown function Bar called in namespace SampleContract3");
        }

        [Fact]
        public void Constructor_throws_on_empty_contract()
        {
            Action sut = () => new ChaincodeFromContracts(new List<IContract>() {new EmptyContract()},
                null, new NullLogger<ChaincodeFromContracts>()
            );

            sut .Should()
                .Throw<Exception>()
                .WithMessage("Contract EmptyContract does not implement any suitable method.");
        }
        
        [Fact]
        public void Constructor_throws_on_no_contract_supplied()
        {
            Action sut = () => new ChaincodeFromContracts(new List<IContract>() {},
                null, new NullLogger<ChaincodeFromContracts>()
            );

            sut .Should()
                .Throw<Exception>()
                .WithMessage("Can not start Chaincode without any contracts.");
        }

        [Fact]
        public void GetContracts_returns_assigned_contracts_and_the_system_contract()
        {
            var sut = new ChaincodeFromContracts(new List<IContract>() { new SampleContract1() },
                new Mock<IContractContextFactory>().Object, new NullLogger<ChaincodeFromContracts>());

            var result = sut.GetContracts();

            result.Count.Should().Be(2);
            result.Keys.Should().Contain(new List<string>() { "SystemContract", "SampleContract1"});

            var systemContract = result["SystemContract"];
            systemContract.Namespace.Should().Be("org.hyperledger.fabric");
            systemContract.FunctionNames.Should().Contain("GetMetadata");

            var sampleContract1 = result["SampleContract1"];
            sampleContract1.Namespace.Should().Be("SampleContract1");
            sampleContract1.FunctionNames.Should().Contain("Foo");
        }
    }
}
