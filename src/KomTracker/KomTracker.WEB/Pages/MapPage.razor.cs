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
    private Map _mapRef = default!;
    private MapOptions _mapOptions = default!;
    private UserModel _user = default!;
    private IEnumerable<EffortViewModel> _koms = Enumerable.Empty<EffortViewModel>();

    [CascadingParameter]
    public required MainLayout Layout { get; set; }

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private IUserService UserService { get; set; } = default!;

    [Inject]
    private IPolylineFactory PolylineFactory { get; set; } = default!;

    [Inject]
    private ICircleMarkerFactory CircleMarkerFactory { get; set; } = default!;

    private CircleMarker? _startMarker;
    private CircleMarker? _endMarker;
    private Polyline? _highlightedPolyline;
    private LatLng? _highlightedStart;
    private bool _highlighting;

    private const int BaseWeight = 3;
    private const int HighlightWeight = 6;
    private static string SegmentColor => Theme.KomTrackerTheme.PaletteLight.Primary.ToString();

    protected override async Task OnInitializedAsync()
    {
        Layout.SetBreadCrumbs(new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Map (beta)", href: "map"),
        });

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

        _ = Task.Run(async () =>
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
        // Clicking empty map clears the highlight (like the popup closing). A polyline click does
        // not bubble to the map click here, so this only fires for genuine empty-map clicks.
        await _mapRef.OnClick(async _ => await ClearHighlightAsync());

        foreach (var effort in _koms)
        {
            var points = MapHelper.Decode(effort.Segment.MapPolyline).Select(x => new LatLng { Lat = x.Latitude, Lng = x.Longitude }).ToArray();

            if (points.Length == 0)
            {
                continue;
            }

            var polyline = await PolylineFactory.CreateAndAddToMap(points, _mapRef, new PolylineOptions
            {
                Color = Theme.KomTrackerTheme.PaletteLight.Primary.ToString(),
                Weight = BaseWeight,
            });

            var popupHtml = GetPopupHtml(effort.Segment, effort.SegmentEffort);
            await polyline.BindPopup(popupHtml);
            await polyline.BindTooltip(effort.Segment.Name);

            var start = points[0];
            var end = points[^1];
            await polyline.OnClick(async _ => await HighlightSegmentAsync(polyline, start, end));

            // Always-on small dot at the start so segment starts are visible without clicking.
            var startDot = await CircleMarkerFactory.CreateAndAddToMap(start, _mapRef, new CircleMarkerOptions
            {
                Radius = 4,
                Color = "#ffffff",
                Weight = 1,
                FillColor = Theme.KomTrackerTheme.PaletteLight.Primary.ToString(),
                FillOpacity = 1,
            });
            await startDot.BindTooltip(effort.Segment.Name);
            await startDot.BindPopup(popupHtml);
            await startDot.OnClick(async _ => await HighlightSegmentAsync(polyline, start, end));
        }
        _polylinesLoaded = true;
    }

    // Highlight the clicked segment: green dot at the start, dark dot at the finish.
    // Clicking the same segment again keeps the highlight; only an empty-map click clears it. The guard
    // stops the two click handlers (polyline + start dot, which overlap at the start) from
    // racing and leaking a marker pair. (The maps library can't reliably render a custom marker
    // icon at this version — L.Icon.createIcon errors — so both endpoints use CircleMarker.)
    private async Task HighlightSegmentAsync(Polyline polyline, LatLng start, LatLng end)
    {
        if (_highlighting)
        {
            return;
        }

        _highlighting = true;
        try
        {
            var sameAsCurrent = _highlightedStart != null
                && _highlightedStart.Lat == start.Lat
                && _highlightedStart.Lng == start.Lng;

            if (sameAsCurrent)
            {
                return; // already highlighted — keep it as is
            }

            await ClearHighlightAsync();

            await polyline.SetStyle(new PathOptions { Weight = HighlightWeight, Color = SegmentColor });
            _highlightedPolyline = polyline;

            _startMarker = await CircleMarkerFactory.CreateAndAddToMap(start, _mapRef, new CircleMarkerOptions
            {
                Radius = 7,
                Color = "#ffffff",
                Weight = 2,
                FillColor = "#2e7d32",
                FillOpacity = 1,
            });

            _endMarker = await CircleMarkerFactory.CreateAndAddToMap(end, _mapRef, new CircleMarkerOptions
            {
                Radius = 7,
                Color = "#ffffff",
                Weight = 2,
                FillColor = "#212121",
                FillOpacity = 1,
            });

            _highlightedStart = start;
        }
        finally
        {
            _highlighting = false;
        }
    }

    private async Task ClearHighlightAsync()
    {
        if (_highlightedPolyline != null)
        {
            await _highlightedPolyline.SetStyle(new PathOptions { Weight = BaseWeight, Color = SegmentColor });
            _highlightedPolyline = null;
        }

        if (_startMarker != null)
        {
            await _startMarker.Remove();
            _startMarker = null;
        }

        if (_endMarker != null)
        {
            await _endMarker.Remove();
            _endMarker = null;
        }

        _highlightedStart = null;
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