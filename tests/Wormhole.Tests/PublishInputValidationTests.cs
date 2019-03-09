using System.Collections.Generic;
using Wormhole.Api.Model.PublishModel;
using Xunit;

namespace Wormhole.Tests
{
    public class PublishInputValidationTests
    {

        [Fact]
        public void Given_Sane_Tags_ValidateTags_Should_Return_True()
        {
            var input = new PublishInput
            {
                Tenant = "sample-tenant",
                Category = "sample-category",
                Payload = "sample-payload",
                Tags = new[] { "t1", " ", "t2" }
            };
            Assert.True(input.ValidateTags());
        }

        [Fact]
        public void Given_Null_ValidateTags_Should_Return_False()
        {
            var input = new PublishInput
            {
                Tenant = "sample-tenant",
                Category = "sample-category",
                Payload = "sample-payload",
                Tags = null
            };

            Assert.False(input.ValidateTags());
        }

        [Fact]
        public void Given_Empty_List_ValidateTags_Should_Return_False()
        {
            var input = new PublishInput
            {
                Tenant = "sample-tenant",
                Category = "sample-category",
                Payload = "sample-payload",
                Tags = new List<string>()
            };
            Assert.False(input.ValidateTags());
        }


        [Fact]
        public void Given_List_With_Empty_Strings_As_Tags_ValidateTags_Should_Return_False()
        {
            var input = new PublishInput
            {
                Tenant = "sample-tenant",
                Category = "sample-category",
                Payload = "sample-payload",
                Tags = new[] { "", " ", "\t" }
            };
            Assert.False(input.ValidateTags());
        }

    }
}
