using FluentValidation;
using NzbWishlist.Azure.Models;

namespace NzbWishlist.Azure.Validation
{
    public class WishValidator : AbstractValidator<WishViewModel>
    {
        public WishValidator()
        {
            RuleFor(w => w.Name)
                .NotEmpty()
                .WithMessage("You have to give your wish a name!");

            RuleFor(w => w.Query)
                .NotEmpty()
                .WithMessage("You have to specify a query to search for!");
        }
    }
}