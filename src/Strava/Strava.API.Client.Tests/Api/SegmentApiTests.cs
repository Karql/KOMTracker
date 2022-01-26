using RichardSzalay.MockHttp;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Tests.HttpClient;
using Utils.Tests.Logging;

namespace Strava.API.Client.Tests.Api;

public class SegmentApiTests
{
    private readonly ITestLogger<SegmentApi> _logger;
    private readonly MockHttpMessageHandler _mockHttp;

    private readonly ISegmentApi _segmentApi;

    #region TestData
    private const int TEST_SEGMENT_ID = 666;
    #endregion

    public SegmentApiTests(ITestLogger<SegmentApi> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mockHttp = new MockHttpMessageHandler();

        _segmentApi = new SegmentApi(_logger, _mockHttp.ToHttpClientFactory());
    }
}
