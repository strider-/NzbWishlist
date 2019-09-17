using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbWishlist.Core.Models
{
    class SearchResults
    {
        [JsonExtensionData]
        public IDictionary<string, JToken> Props { get; set; }

        public IEnumerable<Result> GetItems()
        {
            if (Props.ContainsKey("channel"))
            {
                return Props["channel"]["item"].ToObject<IEnumerable<Result>>();
            }
            else if (Props.ContainsKey("item"))
            {
                return Props["item"].ToObject<IEnumerable<Result>>();
            }

            return Enumerable.Empty<Result>();
        }
    }

    class Result
    {
        public WishResult ToWishResult() => new WishResult
        {
            Title = Title,
            PubDate = PubDate.UtcDateTime,
            NzbUrl = Link,
            Category = Category,
            Size = Size,
            DetailsUrl = DetailsUrl,
        };

        public string Title { get; set; }

        public DateTimeOffset PubDate { get; set; }

        public string Link { get; set; }

        public string Category { get; set; }

        [JsonIgnore]
        public string DetailsUrl
        {
            get
            {
                var guid = AdditionalData["guid"];
                if (guid.Type == JTokenType.Object)
                {
                    return guid["text"].Value<string>();
                }
                else
                {
                    return guid.Value<string>();
                }
            }
        }

        [JsonIgnore]
        public long Size
        {
            get
            {
                var enclosure = AdditionalData["enclosure"];
                if (enclosure["_length"] != null)
                {
                    return enclosure["_length"].Value<long>();
                }
                else
                {
                    return enclosure.SelectToken("@attributes.length").Value<long>();
                }
            }
        }

        [JsonIgnore]
        public string Guid => DetailsUrl.Substring(DetailsUrl.LastIndexOf('/') + 1);

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; } = new Dictionary<string, JToken>();
    }
}
