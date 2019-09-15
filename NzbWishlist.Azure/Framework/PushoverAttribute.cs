using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using System;

namespace NzbWishlist.Azure.Framework
{
    /// <summary>
    /// Allows for the sending of Pushover notifications by binding to an <see cref="IAsyncCollector{T}"/> where T is <see cref="PushoverNotification"/>
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class PushoverAttribute :Attribute 
    {
        /// <summary>
        /// Binds using PushoverAppToken and PushoverUserKey from app settings.
        /// </summary>
        public PushoverAttribute() : this("%PushoverAppToken%", "%PushoverUserKey%") { }

        /// <summary>
        /// Binds using the given appToken and userKey for Pushover.
        /// </summary>
        /// <param name="appToken">The application specific token</param>
        /// <param name="userKey">Your Pushover user key</param>
        public PushoverAttribute(string appToken, string userKey)
        {
            AppToken = appToken;
            UserKey = userKey;
        }

        /// <summary>
        /// Gets the application specific token
        /// </summary>
        [AutoResolve]
        public string AppToken { get; set; }

        /// <summary>
        /// Gets your Pushover user key
        /// </summary>
        [AutoResolve]
        public string UserKey { get; set; }
    }
}
