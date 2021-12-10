using MudBlazor;

namespace KomTracker.WEB.Settings;

public class Theme
{
    public static readonly MudTheme DefaultTheme = new MudTheme()
    {
        Palette = new Palette()
        {
            Primary = "#FC4C02",
            AppbarBackground = "#FC4C02",
        }
    };

    public static readonly MudTheme DarkTheme = new MudTheme()
    {
        Palette = new Palette()
        {
            Primary = "#FC4C02",
            AppbarBackground = "#373740",
            AppbarText = "rgba(255,255,255, 0.70)",
            Background = "#32333d",
            BackgroundGrey = "#27272f",
            Surface = "#373740",
            DrawerIcon = "rgba(255,255,255, 0.50)",
            DrawerBackground = "#27272f",
            DrawerText = "rgba(255,255,255, 0.50)",
            TextPrimary = "rgba(255,255,255, 0.70)",
            TextSecondary = "rgba(255,255,255, 0.50)",
            ActionDefault = "#adadb1",
            ActionDisabled = "rgba(255,255,255, 0.26)",
            ActionDisabledBackground = "rgba(255,255,255, 0.12)",
        }
    };
}
