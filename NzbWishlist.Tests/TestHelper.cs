using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace NzbWishlist.Tests
{
    public static class TestHelper
    {
        public static HttpRequest CreateHttpRequest() => Create(HttpMethod.Get, null).Object;

        public static HttpRequest CreateHttpRequest(string url) => Create(HttpMethod.Get, url).Object;

        public static HttpRequest CreateHttpRequest(object body = null)
        {
            var request = Create(HttpMethod.Post);

            if (body != null)
            {
                var ms = new MemoryStream();
                var sw = new StreamWriter(ms, Encoding.UTF8, 1024, true);

                var json = JsonConvert.SerializeObject(body);
                sw.Write(json);
                sw.Flush();
                ms.Position = 0;

                request.SetupProperty(r => r.Body, ms);

            }

            return request.Object;
        }

        private static Mock<HttpRequest> Create(HttpMethod method, string url = "https://dev.tests.local")
        {
            var req = new Mock<HttpRequest>(MockBehavior.Strict);
            var ctx = new Mock<HttpContext>(MockBehavior.Strict);
            var resp = new Mock<HttpResponse>(MockBehavior.Strict);

            resp.Setup(r => r.Headers).Returns(new HeaderDictionary());
            ctx.Setup(c => c.Response).Returns(resp.Object);

            req.Setup(r => r.Method).Returns(method.ToString());
            req.Setup(r => r.HttpContext).Returns(ctx.Object);

            if (url != null)
            {
                var uri = new Uri(url);
                var qs = uri.ParseQueryString();
                var store = new Dictionary<string, StringValues>();

                foreach (var k in qs.AllKeys)
                {
                    store[k] = qs[k];
                }

                req.Setup(r => r.Scheme).Returns(uri.Scheme);
                req.Setup(r => r.Host).Returns(new HostString(uri.Host, uri.Port));
                req.Setup(r => r.QueryString).Returns(new QueryString(uri.Query));
                req.Setup(r => r.Query).Returns(new QueryCollection(store));
            }

            return req;
        }
    }
}