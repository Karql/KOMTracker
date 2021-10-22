using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Utils.Tests.HttpClient
{
    public class MockHttpClientFactory : IHttpClientFactory
    {
        private readonly MockHttpMessageHandler _mockHttpMessageHandler;

        public MockHttpClientFactory(MockHttpMessageHandler mockHttpMessageHandler)
        {
            _mockHttpMessageHandler = mockHttpMessageHandler ?? throw new ArgumentNullException(nameof(mockHttpMessageHandler));
        }

        public System.Net.Http.HttpClient CreateClient(string name)
        {
            return _mockHttpMessageHandler.ToHttpClient();
        }
    }
}
