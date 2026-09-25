using System.Net.Http.Json;
using KomTracker.API.Shared.ViewModels.Account;
using KomTracker.WEB.Infrastructure.Services.User;

namespace KomTracker.WEB.Infrastructure.Services.Currency;

public class CurrencyPreference : ICurrencyPreference
{
    private readonly HttpClient _http;
    private readonly IUserService _userService;
    private string? _cached;

    public CurrencyPreference(HttpClient http, IUserService userService)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<string> GetAsync()
    {
        if (_cached is not null)
        {
            return _cached;
        }

        try
        {
            var user = await _userService.GetCurrentUser();
            var vm = await _http.GetFromJsonAsync<CurrencyViewModel>($"athletes/{user.AthleteId}/currency");
            _cached = string.IsNullOrWhiteSpace(vm?.Currency) ? Currencies.Default : vm!.Currency;
        }
        catch
        {
            _cached = Currencies.Default;
        }

        return _cached;
    }

    public void Set(string currency) => _cached = currency;
}
