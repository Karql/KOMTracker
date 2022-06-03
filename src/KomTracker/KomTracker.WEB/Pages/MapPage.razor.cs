using FisSst.BlazorMaps;
using KomTracker.API.Shared.Helpers;
using KomTracker.API.Shared.Models.User;
using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.WEB.Infrastructure.Services.User;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace KomTracker.WEB.Pages;

public partial class MapPage
{
    private bool _loaded = false;
    private Map _mapRef;
    private MapOptions _mapOptions;

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
        _mapOptions = new MapOptions()
        {
            DivId = "mapId",
            Center = new LatLng(50.072038, 20.037298),
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

        var polyline = @"uzdpHun}wBu@VWQi@i@sAiB[Wi@[a@KSJu@d@i@Vo@`@a@`BYj@W`AYt@a@p@UVa@\u@\iCPyAA"; // bogucianka
        var points = MapHelper.Decode(polyline).Select(x => new LatLng { Lat = x.Latitude, Lng = x.Longitude}).ToArray();

        

        _loaded = true;

        Task.Run(async () =>
        {
            await Task.Delay(2000);
            var polyline = await PolylineFactory.CreateAndAddToMap(points, _mapRef);
            polyline.BindTooltip("Bogucianka");
            StateHasChanged();
        });        
    }
}