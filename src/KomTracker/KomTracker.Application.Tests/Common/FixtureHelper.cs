using AutoFixture;
using KomTracker.Domain.Entities.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Tests.Common;

public static class FixtureHelper
{
    public static Fixture GetTestFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<SegmentEffortEntity>(x => x.Without(p => p.KomSummaries));

        return fixture;
    }
}
