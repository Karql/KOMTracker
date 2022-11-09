using FisSst.BlazorMaps;
using KomTracker.API.Shared.Helpers;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KomTracker.WEB.Pages;

public partial class Faq
{
    [CascadingParameter]
    public MainLayout Layout { get; set; }

    protected override Task OnInitializedAsync()
    {
        Layout.BreadCrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Faq", href: "/faq"),
        };

        return Task.CompletedTask;
    }
}