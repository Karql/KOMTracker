using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Interfaces.Services.Strava;
using KomTracker.Infrastructure.Strava.Services;
using NSubstitute;
using Strava.API.Client.Api;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using ApiModel = Strava.API.Client.Model;

namespace KomTracker.Infrastructure.Tests.Strava.Services;

public class ActivityServiceTests
{
    private const int TestAthleteId = 1;
    private const long TestActivityId = 19598505831;
    private const string TestToken = "t";

    private readonly IActivityApi _activityApi;
    private readonly ActivityService _activityService;

    public ActivityServiceTests()
    {
        _activityApi = Substitute.For<IActivityApi>();
        _activityService = new ActivityService(_activityApi);
    }

    private static ApiModel.Activity.ActivityDetailedModel Detailed(long id, int athleteId) => new()
    {
        Id = id,
        Athlete = new ApiModel.Athlete.AthleteMetaModel { Id = athleteId },
        Name = "Afternoon Ride",
        GearId = "b10707658",
        Distance = 3830.6f
    };

    [Fact]
    public async Task Get_athlete_activity_maps_to_entity_when_athlete_matches()
    {
        // Arrange
        _activityApi.GetActivityAsync(TestActivityId, TestToken)
            .Returns(Result.Ok(Detailed(TestActivityId, TestAthleteId)));

        // Act
        var res = await _activityService.GetAthleteActivityAsync(TestAthleteId, TestToken, TestActivityId);

        // Assert
        res.Should().BeSuccess();
        res.Value.Id.Should().Be(TestActivityId);
        res.Value.AthleteId.Should().Be(TestAthleteId);
        res.Value.GearId.Should().Be("b10707658");
        res.Value.Name.Should().Be("Afternoon Ride");
    }

    [Fact]
    public async Task Get_athlete_activity_returns_not_found_when_athlete_mismatch()
    {
        // Arrange — activity belongs to a different athlete (public ride pulled by id)
        _activityApi.GetActivityAsync(TestActivityId, TestToken)
            .Returns(Result.Ok(Detailed(TestActivityId, athleteId: 999)));

        // Act
        var res = await _activityService.GetAthleteActivityAsync(TestAthleteId, TestToken, TestActivityId);

        // Assert
        res.Should().BeFailure();
        res.HasError<GetAthleteActivitiesError>(x => x.Message == GetAthleteActivitiesError.NotFound).Should().BeTrue();
    }

    [Theory]
    [InlineData(ApiModel.Activity.Error.GetActivityError.Unauthorized, GetAthleteActivitiesError.Unauthorized)]
    [InlineData(ApiModel.Activity.Error.GetActivityError.TooManyRequests, GetAthleteActivitiesError.TooManyRequests)]
    [InlineData(ApiModel.Activity.Error.GetActivityError.NotFound, GetAthleteActivitiesError.NotFound)]
    [InlineData(ApiModel.Activity.Error.GetActivityError.UnknownError, GetAthleteActivitiesError.UnknownError)]
    public async Task Get_athlete_activity_passes_error(string apiError, string serviceError)
    {
        // Arrange
        _activityApi.GetActivityAsync(TestActivityId, TestToken)
            .Returns(Result.Fail<ApiModel.Activity.ActivityDetailedModel>(new ApiModel.Activity.Error.GetActivityError(apiError)));

        // Act
        var res = await _activityService.GetAthleteActivityAsync(TestAthleteId, TestToken, TestActivityId);

        // Assert
        res.Should().BeFailure();
        res.HasError<GetAthleteActivitiesError>(x => x.Message == serviceError).Should().BeTrue();
    }
}
