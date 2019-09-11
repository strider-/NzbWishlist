using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using NzbWishlist.Azure.Models;
using NzbWishlist.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbWishlist.Azure.Extensions
{
    public static class ModelExtensions
    {
        public static BadRequestObjectResult ToBadRequest(this IList<ValidationFailure> errors)
        {
            if (errors == null || !errors.Any())
            {
                throw new ArgumentException("Model does not have any errors.");
            }

            return new BadRequestObjectResult(errors.Select(e => new
            {
                Field = e.PropertyName,
                Error = e.ErrorMessage
            }));
        }

        public static Provider ToDomainModel(this ProviderViewModel provider)
        {
            return new Provider
            {
                ApiKey = provider.ApiKey,
                ApiUrl = provider.ApiUrl,
                ImageDomain = provider.ImageDomain
            };
        }

        public static ProviderViewModel ToViewModel(this Provider provider)
        {
            return new ProviderViewModel
            {
                ApiKey = provider.ApiKey,
                ApiUrl = provider.ApiUrl,
                ImageDomain = provider.ImageDomain,
                Id = provider.RowKey
            };
        }
    }
}