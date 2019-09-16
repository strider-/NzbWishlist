using Microsoft.AspNetCore.Mvc;
using System;

namespace NzbWishlist.Azure.Extensions
{
    public static class ExceptionExtensions
    {
        public static UnprocessableEntityObjectResult ToUnprocessableResult(this Exception ex)
        {
            return new UnprocessableEntityObjectResult(new { ex.Message });
        }

        public static ObjectResult ToServerError(this Exception ex)
        {
            var result = new ObjectResult(new { ex.Message });
            result.StatusCode = 500;

            return result;
        }
    }
}