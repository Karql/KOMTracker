using MudBlazor;
using MudBlazor.Utilities;

namespace KomTracker.WEB.Settings;

public static class Theme
{
    public static readonly MudTheme KomTrackerTheme = new MudTheme()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#FC4C02",
            AppbarBackground = "#FC4C02",
        },
        PaletteDark = new PaletteDark()
        {
            Primary = "#FC4C02",
            AppbarBackground = "#373740",
            AppbarText = "rgba(255,255,255, 0.70)",
            Background = "#32333d",
            BackgroundGray = "#27272f",
            Surface = "#373740",
            DrawerIcon = "rgba(255,255,255, 0.50)",
            DrawerBackground = "#27272f",
            DrawerText = "rgba(255,255,255, 0.50)",
            TextPrimary = "rgba(255,255,255, 0.70)",
            TextSecondary = "rgba(255,255,255, 0.50)",
            ActionDefault = "#adadb1",
            ActionDisabled = "rgba(255,255,255, 0.26)",
            ActionDisabledBackground = "rgba(255,255,255, 0.12)"            
        }
    };
}
