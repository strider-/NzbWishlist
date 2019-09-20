using Moq;
using Newtonsoft.Json.Linq;
using NzbWishlist.Core.Models;
using NzbWishlist.Core.Services;
using NzbWishlist.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Services
{
    [Trait(nameof(Services), nameof(NewznabClient))]
    public class NewznabClientTests
    {
        private readonly NewznabClient _client;
        private readonly Mock<IHttpClientFactory> _factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        private readonly MockHttpMessageHandler _handler = new MockHttpMessageHandler();

        public NewznabClientTests()
        {
            _factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_handler.Object));

            _client = new NewznabClient(_factory.Object);
        }

        [Fact]
        public async Task SearchAsync_Generates_The_Proper_Search_QueryString()
        {
            _handler.SetupAnyRequestToReturn(HttpStatusCode.OK, new { item = new object[] { } }, req =>
            {
                var query = req.RequestUri.ParseQueryString();

                Assert.Equal("https", req.RequestUri.Scheme);
                Assert.Equal("no.where", req.RequestUri.Host);
                Assert.Equal("/api", req.RequestUri.LocalPath);

                Assert.Equal("json", query["o"]);
                Assert.Equal("search", query["t"]);
                Assert.Equal("apikey", query["apikey"]);
                Assert.Equal("wish.query", query["q"]);
                Assert.Equal("3", query["maxage"]);
            });

            await _client.SearchAsync(Provider(), Wish());
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.Unauthorized)]
        public async Task SearchAsync_Returns_An_Empty_Result_When_The_Search_Is_Not_Successful(HttpStatusCode code)
        {
            _handler.SetupAnyRequestToReturn(code);

            var results = await _client.SearchAsync(Provider(), Wish());

            Assert.Empty(results);
        }

        [Fact]
        public async Task SearchAsync_Handles_Responses_In_Channel_Envelope()
        {
            var guid = Guid.NewGuid();
            var pubDate = DateTimeOffset.Parse("Thu, 12 Sep 2019 08:16:20 +0200").UtcDateTime;
            _handler.SetupRequestSequence(new List<(HttpStatusCode, object)>
            {
                // search request
                (HttpStatusCode.OK, ChannelResult(guid)),
                // preview image check
                (HttpStatusCode.NotFound, null)
            });

            var results = await _client.SearchAsync(Provider(), Wish());

            var wr = Assert.Single(results);
            Assert.Equal("Test", wr.Title);
            Assert.Equal(123456, wr.Size);
            Assert.Equal($"https://no.where/{guid:N}", wr.DetailsUrl);
            Assert.Equal("Test < Cat", wr.Category);
            Assert.Equal($"https://no.where/getnzb/{guid:N}", wr.NzbUrl);
            Assert.Equal(pubDate, wr.PubDate);
            Assert.Null(wr.PreviewUrl);
        }

        [Fact]
        public async Task SearchAsync_Handles_Responses_In_Flat_Envelope()
        {
            var guid = Guid.NewGuid();
            var pubDate = DateTimeOffset.Parse("Thu, 12 Sep 2019 08:16:20 +0200").UtcDateTime;
            _handler.SetupRequestSequence(new List<(HttpStatusCode, object)>
            {
                // search request
                (HttpStatusCode.OK, FlatResult(guid)),
                // preview image check
                (HttpStatusCode.NotFound, null)
            });

            var results = await _client.SearchAsync(Provider(), Wish());

            var wr = Assert.Single(results);
            Assert.Equal("Test", wr.Title);
            Assert.Equal(123456, wr.Size);
            Assert.Equal($"https://no.where/{guid:N}", wr.DetailsUrl);
            Assert.Equal("Test < Cat", wr.Category);
            Assert.Equal($"https://no.where/getnzb/{guid:N}", wr.NzbUrl);
            Assert.Equal(pubDate, wr.PubDate);
            Assert.Null(wr.PreviewUrl);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("images.no.where")]
        public async Task SearchAsync_Sets_The_Preview_Url_Of_A_Result_When_One_Exists(string imgDomain)
        {
            var provider = Provider();
            provider.ImageDomain = imgDomain;
            _handler.SetupRequestSequence(new List<(HttpStatusCode, object)>
            {
                // search request
                (HttpStatusCode.OK, FlatResult(Guid.NewGuid())),
                // preview image check
                (HttpStatusCode.OK, null)
            });

            var results = await _client.SearchAsync(provider, Wish());

            var wr = Assert.Single(results);
            Assert.NotNull(wr.PreviewUrl);
            Assert.Contains(imgDomain == null ? "no.where" : imgDomain, wr.PreviewUrl);
        }

        [Fact]
        public async Task GetNzbStreamAsync_Returns_Null_When_Nzb_Is_Not_Found()
        {
            _handler.SetupAnyRequestToReturn(HttpStatusCode.NotFound);

            var (stream, headers) = await _client.GetNzbStreamAsync(new CartEntry { NzbUrl = "https://no.where" });

            Assert.Null(stream);
            Assert.Null(headers);
        }

        [Fact]
        public async Task GetNzbStreamAsync_Returns_Nzb_With_Experimental_Headers()
        {
            _handler.SetupAnyRequestToReturn(HttpStatusCode.OK, new { }, headers: new Dictionary<string, string>
            {
                { "contentType", "application/x+nzb" },
                { "x-dnzb-site", "mysite" },
                { "x-dnzb-link", "https://mysite/details/123" },
                { "x-dnzb-category", "TV > HD" },
            });

            var (stream, headers) = await _client.GetNzbStreamAsync(new CartEntry { NzbUrl = "https://no.where" });

            Assert.NotNull(stream);
            Assert.NotNull(headers);
            Assert.DoesNotContain(headers, kvp => kvp.Key == "contentType");
            Assert.Contains(headers, kvp => kvp.Key == "x-dnzb-site" && kvp.Value == "mysite");
            Assert.Contains(headers, kvp => kvp.Key == "x-dnzb-link" && kvp.Value == "https://mysite/details/123");
            Assert.Contains(headers, kvp => kvp.Key == "x-dnzb-category" && kvp.Value == "TV > HD");
        }

        private Provider Provider() => new Provider
        {
            ApiKey = "apikey",
            ApiUrl = "https://no.where",
            Name = "Test Provider"
        };

        private Wish Wish() => new Wish
        {
            Active = true,
            LastSearchDate = DateTime.UtcNow.AddDays(-2),
            Name = "Wish",
            Query = "wish.query"
        };

        private object ChannelResult(Guid guid) => new
        {
            channel = new
            {
                item = new[]
                {
                    new {
                        title = "Test",
                        category = "Test < Cat",
                        guid = $"https://no.where/{guid:N}",
                        pubDate ="Thu, 12 Sep 2019 08:16:20 +0200",
                        link = $"https://no.where/getnzb/{guid:N}",
                        enclosure = JToken.Parse("{ \"@attributes\": { \"length\": \"123456\"} }")
                    }
                }
            }
        };

        private object FlatResult(Guid guid) => new
        {
            item = new[]
            {
                new {
                    title = "Test",
                    category = "Test < Cat",
                    guid = new { _isPermaLink = "true", text = $"https://no.where/{guid:N}" },
                    pubDate ="Thu, 12 Sep 2019 08:16:20 +0200",
                    link = $"https://no.where/getnzb/{guid:N}",
                    enclosure = new { _length = "123456" }
                }
            }
        };
    }
}