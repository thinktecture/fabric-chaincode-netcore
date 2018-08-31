using FluentAssertions;
using Thinktecture.HyperledgerFabric.Chaincode.Extensions;
using Xunit;

namespace Thinktecture.HyperledgerFabric.Chaincode.Test.Extensions
{
    public class StringExtensionsTest
    {
        [Fact]
        public void ToByteString_converts_an_empty_string_to_an_empty_bytestring()
        {
            "".ToByteString()
                .Should()
                .Equal();
        }
        
        [Fact]
        public void ToByteString_converts_string_helloworld_to_its_bytestring_representation()
        {
            "helloworld".ToByteString()
                .Should()
                .Equal(0x68, 0x65, 0x6C, 0x6C, 0x6F, 0x77, 0x6F, 0x72, 0x6C, 0x64);
        }
    }
}
