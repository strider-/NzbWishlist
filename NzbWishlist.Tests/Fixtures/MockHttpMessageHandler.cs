using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NzbWishlist.Tests.Fixtures
{
    public class MockHttpMessageHandler : Mock<FakeHttpMessageHandler>
    {
        private Queue<(HttpStatusCode, object)> _requestQueue;

        public MockHttpMessageHandler()
        {
            CallBase = true;
        }

        public MockHttpMessageHandler SetupAnyRequestToReturn(HttpStatusCode statusCode, object obj = null, Action<HttpRequestMessage> requestInspector = null)
        {
            Setup(h => h.Send(It.IsAny<HttpRequestMessage>()))
                .Callback(requestInspector ?? (m => { }))
                .Returns(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = obj == null ? null : new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
                });

            return this;
        }

        public MockHttpMessageHandler SetupRequestSequence(IEnumerable<(HttpStatusCode, object)> responses)
        {
            _requestQueue = new Queue<(HttpStatusCode, object)>(responses);
            Setup(h => h.Send(It.IsAny<HttpRequestMessage>()))
                .Returns(() =>
                {
                    var (statusCode, obj) = _requestQueue.Dequeue();
                    return new HttpResponseMessage
                    {
                        StatusCode = statusCode,
                        Content = obj == null ? null : new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
                    };
                });

            return this;
        }

        public MockHttpMessageHandler CauseRemoteError()
        {
            Setup(h => h.Send(It.IsAny<HttpRequestMessage>()))
                .Throws(new Exception("holy crackers!"));

            return this;
        }
    }

    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        public virtual HttpResponseMessage Send(HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Send(request));
        }
    }
}