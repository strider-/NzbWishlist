using NzbWishlist.Azure.Models;
using NzbWishlist.Azure.Validation;
using Xunit;

namespace NzbWishlist.Tests.Validation
{
    [Trait(nameof(Validation), nameof(ProviderValidator))]
    public class ProviderValidatorTests
    {
        private readonly ProviderValidator _validator = new ProviderValidator();

        [Fact]
        public void Validate_Fails_When_The_Name_Is_Missing()
        {
            var model = new ProviderViewModel
            {
                ApiKey = "key",
                ApiUrl = "https://no.where"
            };

            var results = _validator.Validate(model);

            Assert.False(results.IsValid);
            Assert.Contains(results.Errors, e => e.ErrorMessage == "Provider name cannot be blank.");
        }

        [Fact]
        public void Validate_Fails_When_The_ApiUrl_Is_Missing()
        {
            var model = new ProviderViewModel
            {
                ApiKey = "key",
                Name = "New Provider"
            };

            var results = _validator.Validate(model);

            Assert.False(results.IsValid);
            Assert.Contains(results.Errors, e => e.ErrorMessage == "API url cannot be blank.");
        }

        [Fact]
        public void Validate_Fails_When_The_ApiUrl_Is_Not_A_Valid_URI()
        {
            var model = new ProviderViewModel
            {
                ApiKey = "key",
                ApiUrl = "lol what",
                Name = "New Provider"
            };

            var results = _validator.Validate(model);

            Assert.False(results.IsValid);
            Assert.Contains(results.Errors, e => e.ErrorMessage == "API url is invalid.");
        }

        [Fact]
        public void Validate_Fails_When_The_ImageDomain_Is_Not_A_Domain()
        {
            var model = new ProviderViewModel
            {
                ApiKey = "key",
                ApiUrl = "https://no.where",
                Name = "New Provider",
                ImageDomain = "nope"
            };

            var results = _validator.Validate(model);

            Assert.False(results.IsValid);
            Assert.Contains(results.Errors, e => e.ErrorMessage == "Image domain is not valid.");
        }

        [Theory]
        [InlineData("images.no.where")]
        [InlineData(null)]
        public void Validate_Succeeds_With_Or_Without_ImageDomain(string imgDomain)
        {
            var model = new ProviderViewModel
            {
                ApiKey = "key",
                ApiUrl = "https://no.where",
                Name = "New Provider",
                ImageDomain = imgDomain
            };

            var results = _validator.Validate(model);

            Assert.True(results.IsValid);
            Assert.Empty(results.Errors);
        }
    }
}