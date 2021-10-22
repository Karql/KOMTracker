using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Utils.Tests.HttpClient
{
    public static class MockHttpMessageHandlerExtensions
    {
        public static IHttpClientFactory ToHttpClientFactory(this MockHttpMessageHandler mockHttpMessageHandler)
        {
            return new MockHttpClientFactory(mockHttpMessageHandler);
        }
    }
}
