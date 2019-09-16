using NzbWishlist.Azure.Models;
using NzbWishlist.Azure.Validation;
using Xunit;

namespace NzbWishlist.Tests.Validation
{
    [Trait(nameof(Validation), nameof(WishValidator))]
    public class WishValidatorTests
    {
        private readonly WishValidator _validator = new WishValidator();

        [Fact]
        public void Validate_Fails_When_The_Name_Is_Missing()
        {
            var model = new WishViewModel
            {
                Query = "guess.whats.missing"
            };

            var results = _validator.Validate(model);

            Assert.False(results.IsValid);
            Assert.Contains(results.Errors, e => e.ErrorMessage == "You have to give your wish a name!");
        }

        [Fact]
        public void Validate_Fails_When_The_Query_Is_Missing()
        {
            var model = new WishViewModel
            {
                Name = "What Am I Searching For"
            };

            var results = _validator.Validate(model);

            Assert.False(results.IsValid);
            Assert.Contains(results.Errors, e => e.ErrorMessage == "You have to specify a query to search for!");
        }

        [Fact]
        public void Validate_Succeeds()
        {
            var model = new WishViewModel
            {
                Name = "New Wish",
                Query = "new.wish"
            };

            var results = _validator.Validate(model);

            Assert.True(results.IsValid);
            Assert.Empty(results.Errors);
        }
    }
}
