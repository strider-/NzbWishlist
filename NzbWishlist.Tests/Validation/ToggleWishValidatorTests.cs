using NzbWishlist.Azure.Models;
using NzbWishlist.Azure.Validation;
using Xunit;

namespace NzbWishlist.Tests.Validation
{
    [Trait(nameof(Validation), nameof(ToggleWishValidator))]
    public class ToggleWishValidatorTests
    {
        private readonly ToggleWishValidator _validator = new ToggleWishValidator();

        [Fact]
        public void Validate_Fails_When_The_WishId_Is_Missing()
        {
            var model = new ToggleWishViewModel
            {
                Active = false
            };

            var results = _validator.Validate(model);

            Assert.False(results.IsValid);
            Assert.Contains(results.Errors, e => e.ErrorMessage == "Missing or invalid wish id.");
        }

        [Fact]
        public void Validate_Fails_When_The_Active_Is_Missing()
        {
            var model = new ToggleWishViewModel
            {
                WishId = "123"
            };

            var results = _validator.Validate(model);

            Assert.False(results.IsValid);
            Assert.Contains(results.Errors, e => e.ErrorMessage == "You must specifiy whether the wish is active or not.");
        }

        [Fact]
        public void Validate_Succeeds()
        {
            var model = new ToggleWishViewModel
            {
                WishId = "123",
                Active = true
            };

            var results = _validator.Validate(model);

            Assert.True(results.IsValid);
            Assert.Empty(results.Errors);
        }
    }
}
