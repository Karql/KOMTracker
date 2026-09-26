#nullable enable
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Strava;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Strava;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Strava;

public class StoreStravaWebhookEventCommandTests
{
    private readonly IKOMUnitOfWork _komUoW = Substitute.For<IKOMUnitOfWork>();
    private readonly IWebhookEventRepository _repo = Substitute.For<IWebhookEventRepository>();
    private readonly StoreStravaWebhookEventCommandHandler _handler;

    public StoreStravaWebhookEventCommandTests()
    {
        _komUoW.GetRepository<IWebhookEventRepository>().Returns(_repo);
        _handler = new StoreStravaWebhookEventCommandHandler(_komUoW);
    }

    [Fact]
    public async Task Stores_the_raw_event_and_saves()
    {
        var res = await _handler.Handle(new StoreStravaWebhookEventCommand
        {
            ObjectType = "activity",
            ObjectId = 123,
            AspectType = "update",
            Updates = "{\"title\":\"x\"}",
            OwnerId = 7,
            SubscriptionId = 99,
            EventTime = 1700000000
        }, CancellationToken.None);

        res.Should().BeSuccess();
        _repo.Received().Add(Arg.Is<WebhookEventEntity>(e =>
            e.ObjectType == "activity" && e.ObjectId == 123 && e.AspectType == "update"
            && e.Updates == "{\"title\":\"x\"}" && e.OwnerId == 7 && e.SubscriptionId == 99
            && e.EventTime == 1700000000 && e.Processed == false && e.AuditCD != default));
        await _komUoW.Received().SaveChangesAsync();
    }
}
