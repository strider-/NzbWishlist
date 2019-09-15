using FluentValidation;
using NzbWishlist.Azure.Models;

namespace NzbWishlist.Azure.Validation
{
    public class ToggleWishValidator : AbstractValidator<ToggleWishViewModel>
    {
        public ToggleWishValidator()
        {
            RuleFor(x => x.WishId)
                .NotEmpty()
                .WithMessage("Missing or invalid wish id.");

            RuleFor(x => x.Active)
                .NotNull()
                .WithMessage("You must specifiy whether the wish is active or not.");
        }
    }
}