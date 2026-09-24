# BikeTracker — autocomplete suggestions for Brand / Model / Purchase place

## Context
Adding bikes/components repeats the same values — the same shop, the same manufacturer. Free-typing them every time is slow and drifts ("Decathlon" vs "decathlon"). Give the three text fields **Brand**, **Model**, **Purchase place** an autocomplete fed by the user's own past values (from both bikes and components), while still allowing a brand-new value.

## Decisions
- **D-SUG-1 One aggregated endpoint, loaded on dialog open.** `GET bike-tracker/suggestions` returns distinct `Brands`, `Models`, `PurchasePlaces` pooled from the user's bikes **and** components (incl. archived/sold — more history = better suggestions). *Why: a place/brand used on a bike is just as valid for a component; one call keeps both dialogs in sync.*
- **D-SUG-2 Free-text autocomplete (not a closed list).** `MudAutocomplete<string>` with `CoerceValue="true"` — suggestions help, but any new value is accepted. *Why: the whole point is quick reuse without locking out new shops/brands.*
- **D-SUG-3 Push DISTINCT to the DB (don't materialize rows).** Each repo gets `GetDistinctPurchaseFieldsAsync(userId)` running a `SELECT DISTINCT <col>` per field (only the non-empty values come back, not whole entities); the query merges the two already-small per-source lists in memory (trim + cross-source case-insensitive dedupe + sort). *Why: components carry many columns and can number in the hundreds — pulling full rows just to distinct 3 fields is wasteful; the DB does the heavy lifting.*

## Implementation
- **New** `Application/Models/Component/PurchaseSuggestionsModel.cs` — record `(IReadOnlyList<string> Brands, Models, PurchasePlaces)`.
- `IComponentRepository`/`IBikeRepository` (+ EF impls) — new `GetDistinctPurchaseFieldsAsync(userId)` returning `(Brands, Models, PurchasePlaces)`, each a DB-side `Where(non-empty).Select(col).Distinct().ToListAsync()`.
- **New** `Application/Queries/Component/GetPurchaseSuggestionsQuery.cs` `{ string UserId }` → handler calls both repos' `GetDistinctPurchaseFieldsAsync`, then `Merge(a, b)` per field = Concat → Trim → non-blank → `Distinct(OrdinalIgnoreCase)` → `OrderBy(OrdinalIgnoreCase)`.
- **New** `API.Shared/ViewModels/Suggestions/PurchaseSuggestionsViewModel.cs` — `Brands`/`Models`/`PurchasePlaces` (`IReadOnlyCollection<string>`, default empty).
- **New** `API/Controllers/SuggestionsController.cs` — `[Route("bike-tracker/suggestions")] [BearerAuthorize]`; `GET` → `GetCurrentUser()?.UserId` guard → `GetPurchaseSuggestionsQuery` → map model to VM.
- WEB `Shared/AddEditComponentDialog.razor` + `Shared/AddEditBikeDialog.razor` — load `PurchaseSuggestionsViewModel` in `OnInitialized(Async)`; replace the Brand / Model / Purchase place `MudTextField`s with `MudAutocomplete<string>` (`@bind-Value` to the same model field, `CoerceValue="true"`, `Clearable`, `MaxItems=null`, a `SearchFunc` doing case-insensitive substring over the relevant list, returning all when empty). `AddEditBikeDialog.OnInitialized` becomes async to fetch (or fetch in `OnInitializedAsync`).

## Tests
- `Application.Tests/Queries/Component/GetPurchaseSuggestionsQueryTests.cs` — components + bikes with overlapping/blank/differently-cased brands/places → asserts union, distinct (case-insensitive), blanks dropped, sorted.

## Out of scope
- Model suggestions are a flat list (not scoped to the chosen brand). Frequency ranking / most-recent-first. Persisting a separate tag dictionary.

## Verification
- `dotnet build src/KomTracker.sln`; `dotnet test` green.
- Manual (WEB): with a couple of existing bikes/components, open **Add component** and **Add bike** → Brand/Model/Purchase place show past values as you type; picking one fills it; typing a brand-new value is accepted and saves; the new value appears as a suggestion next time.
