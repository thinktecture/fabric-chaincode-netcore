using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Thinktecture.HyperledgerFabric.Chaincode.Contract;
using Thinktecture.HyperledgerFabric.Chaincode.Extensions;
using Xunit;

namespace Thinktecture.HyperledgerFabric.Chaincode.Test.Contract
{
    public class MetaContractTest
    {
        [Fact]
        public void Has_hyperledger_namespace_as_default()
        {
            var sut = new MetaContract();

            sut.Namespace.Should().Be("org.hyperledger.fabric");
        }

        [Fact]
        public async Task GetMetadata_outputs_null_metadata_empty_dictionary_converted_to_bytestring()
        {
            var sut = new MetaContract();

            var result = await sut.GetMetadata(null);

            result.Should().Equal(JsonConvert.SerializeObject(new Dictionary<string, string>()).ToByteString());
        }

        [Fact]
        public async Task GetMetadata_outputs_given_metadata_as_a_string_dictionary_convert_to_bytestring()
        {
            var sut = new MetaContract();
            sut.Metadata.Add("hello", "unittest");

            var result = await sut.GetMetadata(null);

            result.Should().Equal(JsonConvert.SerializeObject(new Dictionary<string, string>()
            {
                {"hello", "unittest"}
            }).ToByteString());
        }
    }
}
