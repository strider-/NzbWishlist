using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;

namespace NzbWishlist.Azure.Framework
{
    [Extension(nameof(PushoverExtensions))]
    public class PushoverExtensions : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            var rule = context.AddBindingRule<PushoverAttribute>();

            rule.BindToCollector(BuildCollector);
        }

        public IAsyncCollector<PushoverNotification> BuildCollector(PushoverAttribute attribute)
        {
            return new PushoverNotificationAsyncCollector(attribute);
        }
    }
}