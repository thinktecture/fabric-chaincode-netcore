using System;
using System.Collections.Generic;
using FluentAssertions;
using Thinktecture.HyperledgerFabric.Chaincode.Contract;
using Xunit;

namespace Thinktecture.HyperledgerFabric.Chaincode.Test
{
    public class ChaincodeProviderConfigurationTest
    {
        private class SampleContract : ContractBase
        {
            public SampleContract(string @namespace, IDictionary<string, string> metadata = null)
                : base(@namespace, metadata)
            {
            }
        }
        
        [Fact]
        public void ConfigureWithContracts_throws_if_a_type_is_used_not_implementing_IContract()
        {
            Action act = () =>
                ChaincodeProviderConfiguration.ConfigureWithContracts(new string[] {},
                    new[] {typeof(ChaincodeProviderConfigurationTest)});

            act.Should()
                .Throw<Exception>()
                .WithMessage("ChaincodeProviderConfigurationTest does not implement Interface IContract");
        }

        [Fact]
        public void ConfigureWithContracts_does_not_throw_if_a_type_is_implementing_IContract()
        {
            Action act = () =>
                ChaincodeProviderConfiguration.ConfigureWithContracts(new string[] { }, new[] {typeof(SampleContract)});
            
            act.Should()
                .NotThrow();
        }
    }
}
