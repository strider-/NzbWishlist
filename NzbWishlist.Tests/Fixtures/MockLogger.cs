using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using System;

namespace NzbWishlist.Tests.Fixtures
{
    public class MockLogger : Mock<ILogger>
    {
        public MockLogger() : base(MockBehavior.Strict)
        {
            Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<FormattedLogValues>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()
            )).Verifiable();
        }

        public void VerifyLoggedException(string message)
        {
            VerifyLog(LogLevel.Critical, message);
        }

        public void VerifyLog(LogLevel logLevel, string contains)
        {
            Verify(
                l => l.Log(
                    logLevel,
                    It.IsAny<EventId>(),
                    It.Is<FormattedLogValues>(v => v.ToString().Contains(contains)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()
                )
            );
        }
    }
}