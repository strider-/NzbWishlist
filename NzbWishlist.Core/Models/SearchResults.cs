using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbWishlist.Core.Models
{
    class SearchResults
    {
        [JsonProperty("item")]
        public IEnumerable<Result> Items { get; set; } = Enumerable.Empty<Result>();
    }

    class Result
    {
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
