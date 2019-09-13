using Microsoft.Azure.WebJobs.Description;
using System;

namespace NzbWishlist.Azure.Framework
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class PushoverAttribute :Attribute 
    {
        public PushoverAttribute() : this("%PushoverAppToken%", "%PushoverUserKey%") { }

        public PushoverAttribute(string appToken, string userKey)
        {
            AppToken = appToken;
            UserKey = userKey;
        }

        [AutoResolve]
        public string AppToken { get; set; }

        [AutoResolve]
        public string UserKey { get; set; }
    }
}
