﻿using Microsoft.Azure.WebJobs;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NzbWishlist.Azure.Framework
{
    internal class PushoverNotificationAsyncCollector : IAsyncCollector<PushoverNotification>
    {
        const string ApiUrl = "https://api.pushover.net/1/messages.json";

        private static readonly HttpClient _client = new HttpClient();

        private PushoverAttribute _attribute;

        public PushoverNotificationAsyncCollector(PushoverAttribute attribute) => _attribute = attribute;

        public async Task AddAsync(PushoverNotification item, CancellationToken cancellationToken = default)
        {
            await SendNotification(item);
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        private async Task SendNotification(PushoverNotification item)
        {
            var dict = new Dictionary<string, string>
            {
                { "token", _attribute.AppToken },
                { "user", _attribute.UserKey },
                { "title", item.Title },
                { "message", item.Message }
            };

            var resp = await _client.PostAsync(ApiUrl, new FormUrlEncodedContent(dict));
            resp.EnsureSuccessStatusCode();
        }
    }
}
