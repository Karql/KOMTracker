using Strava.API.Client.Model.Gear;
using Utils.Extensions;

namespace Strava.API.Client.Tests.Extensions.Model.Gear;

public static class GearDetailedModelExtensions
{
    /// <summary>
    /// JSON equivalent to API response.
    /// </summary>
    /// <remarks>Manually created string for testing deserialization.</remarks>
    public static string ToJson(this GearDetailedModel model)
    {
        return @$"{{
                ""id"": ""{model.Id}"",
                ""resource_state"": {(int)model.ResourceState},
                ""primary"": {model.Primary.ToLowerString()},
                ""name"": ""{model.Name}"",
                ""nickname"": ""{model.Nickname}"",
                ""retired"": {model.Retired.ToLowerString()},
                ""distance"": {model.Distance},
                ""converted_distance"": {model.ConvertedDistance},
                ""brand_name"": ""{model.BrandName}"",
                ""model_name"": ""{model.ModelName}"",
                ""frame_type"": {model.FrameType},
                ""description"": ""{model.Description}"",
                ""weight"": {model.Weight}
            }}";
    }
}
