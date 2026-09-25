# BikeTracker — currency preference + input/format polish

## Context
Money and dates are rough to enter and read: price fields drop a typed comma (only dot parses) so "12,50" becomes an integer; amounts show as a bare number with no currency; the date picker shows `dd/MM/yyyy` while detail pages render `yyyy-MM-dd`; and dates can only be clicked, not typed/pasted. This adds a per-user **currency preference** (shown as a symbol next to amounts) and polishes numeric + date input.

## Decisions
- **D-CUR-1 Currency on the user (AspNetUsers), not per bike/component.** New `UserEntity.Currency` (3-letter code, **NOT NULL DEFAULT 'PLN'** so the migration is safe on existing rows). Chosen in Account → Profile details. *Why: it's a display preference for the whole app, one value per user.*
- **D-CUR-2 Presentation-only symbol via a WEB `Currencies` helper.** A static registry holds the supported set — `CHF/EUR/GBP/USD/PLN` — each with `Code`, `Symbol`, `Name`, and a `SymbolAfter` flag (PLN → after: "1500 zł"; others → before: "$1500"). It exposes `All` (for the combo) and `Format(amount, code)`. *Why: one place to render and to grow the list; symbol placement baked in per currency.*
- **D-CUR-3 Fresh value via a scoped WEB provider, not a claim.** `ICurrencyPreference` fetches the code once (`GET athletes/{id}/currency`) and caches it for the circuit; the Account page calls `Set(code)` right after saving so amounts reflect the change immediately (a claim would be stale until re-login). *Why: change-and-see-it-now without token refresh.*
- **D-CUR-4 Direct write (no confirm flow).** `PUT athletes/{id}/currency/{code}` → `UpdateCurrencyCommand` → `IUserService.UpdateCurrencyAsync` (`UserManager.UpdateAsync`). Ownership via `GetCurrentUser()` like `change-email`; validator restricts to the allowed codes. *Why: unlike email, currency needs no verification.*
- **D-IN-1 Accept both `,` and `.` in decimals.** A shared MudBlazor `Converter<decimal?>` normalizes comma→dot and parses/formats with InvariantCulture; applied to every decimal `MudNumericField` (price, weight, initial\*, manual\*), not just price. *Why: same annoyance affects all decimals; one uniform fix.*
- **D-IN-2 Consistent dates + typeable pickers.** Every `MudDatePicker` gets `DateFormat="yyyy-MM-dd"` (matches the detail-page rendering) and `Editable="true"` (type/paste an old date instead of clicking). *Why: points 3 + 4.*

## Implementation
### Backend
- `Infrastructure/Identity/Entities/UserEntity.cs` — `public string Currency { get; set; } = "PLN";`
- `Infrastructure/Persistence/Configurations/Identity/UserEntityTypeConfiguration.cs` — `Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired().HasDefaultValue("PLN")`.
- Migration `AddUserCurrency` + `database update` (safe: NOT NULL + default).
- `Application/Models/Identity/UserModel.cs` — add `Currency` (AutoMapper convention via `IdentityProfile` maps it).
- `IUserService` (+ `Infrastructure/Identity/Services/UserService.cs`) — `Task<Result> UpdateCurrencyAsync(int athleteId, string currency)` (load by athleteId, set, `UserManager.UpdateAsync`; `UserNotFound` result).
- **New** `Application/Commands/Account/UpdateCurrencyCommand.cs` `{ AthleteId, Currency }` + validator (Currency in `CHF/EUR/GBP/USD/PLN`) → handler calls the service.
- **New** `API.Shared/ViewModels/Account/CurrencyViewModel.cs` `{ string Currency }`.
- `API/Controllers/AthletesController.cs` — `GET {athleteId}/currency` (→ `GetUserAsync(athleteId).Currency`) and `PUT {athleteId}/currency/{currency}` (→ `UpdateCurrencyCommand`), both with the existing `GetCurrentUser()` ownership check.

### WEB
- **New** `Infrastructure/Currencies.cs` — `CurrencyInfo` record + registry + `Format`.
- **New** `Infrastructure/InputConverters.cs` — `Converter<decimal?> Decimal` (comma/dot → invariant).
- **New** `Infrastructure/Services/Currency/ICurrencyPreference.cs` + `CurrencyPreference.cs` (HttpClient + WEB `IUserService`; cache + `Set`); register `AddScoped` in `DependencyInjection.cs`.
- `Pages/Account.razor` (+ `.cs`) — Profile-details **Currency** `MudSelect` (options from `Currencies.All`, display `"{Symbol} {Code} - {Name}"`); initial value from `ICurrencyPreference.GetAsync()`; on change → `PUT .../currency/{code}` then `ICurrencyPreference.Set(code)` + snackbar (auto-save, like the Strava toggles).
- Decimal inputs — add `Converter="InputConverters.Decimal"` to every decimal `MudNumericField` (price/weight/initial\*/manual\*) in `AddEditBikeDialog`, `AddEditComponentDialog`, `SellBikeDialog`, `SellComponentDialog`, `InstallComponentDialog`, `EditInstallationDialog`.
- Date pickers — add `DateFormat="yyyy-MM-dd" Editable="true"` to all 10 `MudDatePicker`s (Add/Edit bike+component, Install, EditInstallation ×2, Move, Remove, Sell bike+component).
- Price display — `BikeDetails.razor` (Price, Sale price) + `ComponentDetails.razor` (Price, Sale price): inject `ICurrencyPreference`, load `_currency` in `OnInitialized`, render via `Currencies.Format(value, _currency)`.

### Tests
- `Application.Tests/Commands/Account/UpdateCurrencyCommandTests.cs` — valid code → calls `UpdateCurrencyAsync`; invalid code → validation failure (via the validator) / rejected.

## Out of scope
- Currency conversion / FX. Per-item currency. User-editable date format. Grouping/locale of the number itself (kept plain `0.##`).

## Verification
- `dotnet build src/KomTracker.sln`; `dotnet ef migrations add AddUserCurrency …` + `database update`; `dotnet test` green.
- Manual (WEB): Account → set currency → amounts on bike/component details show the right symbol immediately (PLN after, others before). Price field accepts `12,50` and `12.50`. Date pickers show `yyyy-MM-dd` and accept a typed/pasted date. Existing users load with PLN by default.
