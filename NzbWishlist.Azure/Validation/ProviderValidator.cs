using FluentValidation;
using NzbWishlist.Azure.Models;
using System;

namespace NzbWishlist.Azure.Validation
{
    public class ProviderValidator : AbstractValidator<ProviderViewModel>
    {
        public ProviderValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty()
                .WithMessage("Provider name cannot be blank.");

            RuleFor(p => p.ApiUrl)
                .NotEmpty()
                .WithMessage("API url cannot be blank.")
                .Must(BeAValidUrl)
                .WithMessage("API url is invalid.");

            RuleFor(p => p.ApiKey)
                .NotEmpty()
                .WithMessage("API key cannot be blank.");

            RuleFor(p => p.ImageDomain)
                .Must(domain => domain.Contains('.'))
                .When(p => !string.IsNullOrWhiteSpace(p.ImageDomain))
                .WithMessage("Image domain is not valid.");
        }

        private bool BeAValidUrl(string s) => Uri.TryCreate(s, UriKind.Absolute, out _);
    }
}