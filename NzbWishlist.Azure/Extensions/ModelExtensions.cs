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

        public static Provider ToDomainModel(this ProviderViewModel provider) => new Provider
        {
            ApiKey = provider.ApiKey,
            ApiUrl = provider.ApiUrl,
            ImageDomain = provider.ImageDomain
        };

        public static ProviderViewModel ToViewModel(this Provider provider) => new ProviderViewModel
        {
            Id = provider.RowKey,
            ApiKey = provider.ApiKey,
            ApiUrl = provider.ApiUrl,
            ImageDomain = provider.ImageDomain            
        };

        public static Wish ToDomainModel(this WishViewModel wish) => new Wish
        {
            Active = wish.Active.HasValue ? wish.Active.Value : true,
            Name = wish.Name,
            Query = wish.Query,
            LastSearchDate = DateTime.UtcNow.AddDays(-6)
        };

        public static WishViewModel ToViewModel(this Wish wish) => new WishViewModel
        {
            Id = wish.RowKey,
            Active = wish.Active,
            Name = wish.Name,
            Query = wish.Query,
        };

        public static WishResultViewModel ToViewModel(this WishResult wishResult) => new WishResultViewModel
        {
            DetailsUrl = wishResult.DetailsUrl,
            Id = wishResult.Id,
            NzbUrl = wishResult.NzbUrl,
            PreviewUrl = wishResult.PreviewUrl,
            PubDate = wishResult.PubDate,
            Size = wishResult.Size,
            Title = wishResult.Title
        };
    }
}