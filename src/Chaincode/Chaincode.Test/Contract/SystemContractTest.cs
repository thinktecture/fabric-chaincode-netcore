using FluentAssertions;
using Thinktecture.HyperledgerFabric.Chaincode.Contract;
using Xunit;

namespace Thinktecture.HyperledgerFabric.Chaincode.Test.Contract
{
    public class SystemContractTest
    {
        [Fact]
        public void Has_hyperledger_namespace_as_default()
        {
            var sut = new SystemContract(null);

            sut.Namespace.Should().Be("org.hyperledger.fabric");
        }
    }
}