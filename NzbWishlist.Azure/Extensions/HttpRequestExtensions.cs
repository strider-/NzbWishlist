using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using NzbWishlist.Azure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NzbWishlist.Azure.Extensions
{
    public static class HttpRequestExtensions
    {
        public static async Task<Either<TModel, IList<ValidationFailure>>> GetRequestModel<TModel, TValidator>(this HttpRequest request)
            where TModel : class
            where TValidator : AbstractValidator<TModel>, new()
        {
            if (request.Body == null)
            {
                return new[] { new ValidationFailure("Body", "Missing or invalid request body.") };
            }

            var body = await request.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<TModel>(body);
            var validator = new TValidator();
            var result = validator.Validate(model);

            if (result.IsValid)
            {
                return model;
            }

            return result.Errors.ToList();
        }

        public static string CreateLocation(this HttpRequest request, string path)
        {
            return new UriBuilder(
                request.Scheme,
                request.Host.Host,
                request.Host.Port ?? (request.Scheme == "https" ? 443 : 80),
                $"api/{path.TrimStart('/')}")
            .ToString();
        }
    }
}