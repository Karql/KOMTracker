namespace KomTracker.WEB.Infrastructure.Services.Currency;

/// <summary>The signed-in user's preferred display currency, cached for the session so amounts can be formatted anywhere.</summary>
public interface ICurrencyPreference
{
    /// <summary>The user's currency code (cached; fetched once). Falls back to the default on any failure.</summary>
    Task<string> GetAsync();

    /// <summary>Update the cached value after the user changes it, so amounts reflect it immediately.</summary>
    void Set(string currency);
}
