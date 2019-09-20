using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NzbWishlist.Azure.Models;
using NzbWishlist.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml.Linq;

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
            ImageDomain = provider.ImageDomain,
            Name = provider.Name
        };

        public static ProviderViewModel ToViewModel(this Provider provider) => new ProviderViewModel
        {
            Id = provider.RowKey,
            ApiKey = provider.ApiKey,
            ApiUrl = provider.ApiUrl,
            ImageDomain = provider.ImageDomain,
            Name = provider.Name
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

        public static CartEntry ToCartEntry(this WishResult wishResult, Func<string, string> grabUrlGenerator)
        {
            var entry = new CartEntry
            {
                Category = wishResult.Category,
                Description = "",
                DetailsUrl = wishResult.DetailsUrl,
                PublishDate = wishResult.PubDate,
                Title = wishResult.Title,
                NzbUrl = wishResult.NzbUrl
            };

            entry.GrabUrl = grabUrlGenerator(entry.RowKey);

            return entry;
        }

        public static CartEntryViewModel ToViewModel(this CartEntry entry) => new CartEntryViewModel
        {
            DetailsUrl = entry.DetailsUrl,
            Category = entry.Category,
            Description = entry.Description,
            GrabUrl = entry.GrabUrl,
            Id = entry.RowKey,
            PublishDate = entry.PublishDate,
            Title = entry.Title
        };

        public static SyndicationItem ToSyndicationItem(this CartEntry entry, QueryString qs)
        {
            var builder = new UriBuilder(entry.GrabUrl);
            builder.Query = qs.Value;
            var grabUri = builder.Uri;

            var feedItem = new SyndicationItem(entry.Title, entry.Description, grabUri)
            {
                PublishDate = new DateTimeOffset(entry.PublishDate),
                Id = entry.RowKey,
            };

            feedItem.AddPermalink(new Uri(entry.DetailsUrl));
            feedItem.ElementExtensions.Add(new XElement("category", entry.Category));
            feedItem.ElementExtensions.Add(new XElement("enclosure",
                                               new XAttribute("url", grabUri),
                                               new XAttribute("length", "0"),
                                               new XAttribute("type", "application/x+nzb")));

            return feedItem;
        }
    }
}