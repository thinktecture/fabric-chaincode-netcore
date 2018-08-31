using System;
using FluentAssertions;
using Thinktecture.HyperledgerFabric.Chaincode.Extensions;
using Xunit;

namespace Thinktecture.HyperledgerFabric.Chaincode.Test.Extensions
{
    public class ByteStringExtensionsTest
    {
        [Fact]
        public void Convert_converts_a_string_to_a_string()
        {
            var byteString = "helloworld".ToByteString();

            byteString.Convert<string>().Should().Be("helloworld");
        }
        
        [Fact]
        public void Convert_converts_a_int_to_string()
        {
            var byteString = 1337.ToByteString();

            byteString.Convert<string>().Should().Be("1337");
        }
        
        [Fact]
        public void Convert_converts_a_int_to_int()
        {
            var byteString = 1337.ToByteString();

            byteString.Convert<int>().Should().Be(1337);
        }
        
        [Fact]
        public void Convert_converts_a_string_to_int()
        {
            var byteString = "1337".ToByteString();

            byteString.Convert<int>().Should().Be(1337);
        }
        
        [Fact]
        public void Convert_throws_when_trying_to_convert_a_string_which_does_contain_illegal_characters_to_int()
        {
            var byteString = "shouldnotwork".ToByteString();

            Action sut = () => byteString.Convert<int>();

            sut.Should().Throw<Exception>();
        }
    }
}
