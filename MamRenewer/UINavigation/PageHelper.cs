using OpenQA.Selenium;
using Polly;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MamRenewer.Polly
{
    class PageHelper
    {
        internal static readonly PolicyWrap WaitForWebElementPolicy = Policy
            .Handle<RetryException>()
            .WaitAndRetryUntilTimeout(
                retryDelay: TimeSpan.FromMilliseconds(10),
                timeout: TimeSpan.FromSeconds(15));

        internal static readonly AsyncPolicyWrap WaitForWebElementPolicyAsync = Policy
            .Handle<RetryException>()
            .WaitAndRetryUntilTimeoutAsync(
                retryDelay: TimeSpan.FromMilliseconds(10),
                timeout: TimeSpan.FromSeconds(15));

        public sealed class RetryException : Exception
        {
            public RetryException(string message) : base(message)
            {
            }

            public RetryException(string message, Exception innerException) : base(message, innerException)
            {
            }

            public RetryException() : base()
            {
            }
        }
    }
}
