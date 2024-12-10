using FisSst.BlazorMaps;
using KomTracker.API.Shared.Helpers;
using KomTracker.API.Shared.Models.User;
using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Settings;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace KomTracker.WEB.Pages;

public partial class MapPage
{
    [Parameter]
    [SupplyParameterFromQuery]
    public int? AthleteId { get; set; }

    private bool _loaded = false;
    private bool _polylinesLoaded = false;
    private Map _mapRef;
    private MapOptions _mapOptions;
    private UserModel _user = default!;
    private IEnumerable<EffortViewModel> _koms = Enumerable.Empty<EffortViewModel>();

    [CascadingParameter]
    public MainLayout Layout { get; set; }

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private IUserService UserService { get; set; } = default!;

    [Inject]
    private IPolylineFactory PolylineFactory { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Layout.BreadCrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Map (beta)", href: "/map"),
        };

        _mapOptions = new MapOptions()
        {
            DivId = "mapId",
            // Center = new LatLng(50.072038, 20.037298), // Plac Centralny
            Center = new LatLng(50.061289, 19.937693), // Rynek           
            Zoom = 13,
            UrlTileLayer = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
            SubOptions = new MapSubOptions()
            {
                Attribution = "&copy; <a lhref='http://www.openstreetmap.org/copyright'>OpenStreetMap</a>",
                MaxZoom = 18,
                TileSize = 256,
                ZoomOffset = 0,
            }
        };

        _user = await UserService.GetCurrentUser();
        await GetAllKoms();

        _loaded = true;
        StateHasChanged();

        Task.Run(async () =>
        {
            await Task.Delay(500);
            await AddPolylinesAsync();
            StateHasChanged();
        });        
    }

    private async Task GetAllKoms()
    {
        var athleteId = AthleteId ?? _user.AthleteId;

        _koms = await Http.GetFromJsonAsync<EffortViewModel[]>($"athletes/{athleteId}/koms")
            ?? Enumerable.Empty<EffortViewModel>();
    }

    private async Task AddPolylinesAsync()
    {
        foreach (var effort in _koms)
        {
            var points = MapHelper.Decode(effort.Segment.MapPolyline).Select(x => new LatLng { Lat = x.Latitude, Lng = x.Longitude }).ToArray();
            var polyline = await PolylineFactory.CreateAndAddToMap(points, _mapRef, new PolylineOptions
            {
                Color = Theme.KomTrackerTheme.PaletteLight.Primary.ToString(),                
            });            

            var popupHtml = GetPopupHtml(effort.Segment, effort.SegmentEffort);
            await polyline.BindPopup(popupHtml);
            await polyline.BindTooltip(effort.Segment.Name);
        }
        _polylinesLoaded = true;
    }

    private string GetPopupHtml(SegmentViewModel segment, SegmentEffortViewModel effort)
    {
        return $@"
            <h3><a href=""https://strava.com/segments/{segment.Id}"" target=""_blank"" class=""mud-primary-text"">{segment.Name}</a></h3>
            <div class=""general-info mt-4"">
                <div class=""d-flex flex-row"">
                    <div class=""stat mr-4"">
                        <strong>{(segment.Distance / 1000).ToString("F2")} km</strong>
                        <br />
                        <span class=""label"">Distance</span>
                    </div>
                    <div class=""stat"">
                        <strong>{segment.AverageGrade.ToString("F1")}%</strong>
                        <br />
                        <span class=""label"">Grade</span>
                    </div>
                    <div class=""stat ml-4"">
                        <strong>{(segment.ElevationHigh - segment.ElevationLow).ToString("F0")} m</strong>
                        <br />
                        <span class=""label"">Elev Gain</span>
                    </div>
                </div>
                <div class=""my-4"">
                    <strong>Your Best:</strong> <a href=""https://www.strava.com/segment_efforts/{effort.Id}"" target=""_blank"" class=""mud-primary-text"">{TimeSpan.FromSeconds(effort.ElapsedTime).ToString()}</a>
                </div>                
            </div>                      
        ";
    }
}