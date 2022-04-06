using AutoFixture;
using Strava.API.Client.Model.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Tests.Common;

public static class FixtureHelper
{
    public static Fixture GetTestFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<SegmentDetailedModel>(x => x            
            .Without(p => p.StartLatLng) // will be assigned by StartLatitude & StartLongitude
            .Without(p => p.EndLatLng)   // will be assigned by EndLatitude & EndLongitude
        );

        fixture.Customize<SegmentSummaryModel>(x => x
            .Without(p => p.StartLatLng) // will be assigned by StartLatitude & StartLongitude
            .Without(p => p.EndLatLng)   // will be assigned by EndLatitude & EndLongitude
        );

        fixture.Register<DateTime>(() => DateTime.Today.ToUniversalTime()); // Today has no miliseconds etc.


        return fixture;
    }
}
