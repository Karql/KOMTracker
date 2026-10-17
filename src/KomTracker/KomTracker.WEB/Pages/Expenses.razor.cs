using System.Net.Http.Json;
using KomTracker.API.Shared.ViewModels.Expense;
using KomTracker.Domain.Entities.Expense;
using KomTracker.WEB.Infrastructure;
using KomTracker.WEB.Infrastructure.Services.Currency;
using KomTracker.WEB.Infrastructure.Services.Preference;
using KomTracker.WEB.Models.Preference;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KomTracker.WEB.Pages;

public partial class Expenses
{
    // localStorage key for this page's card/table preference.
    private const string ViewPreferenceKey = "expenses";

    private bool _loaded;
    private bool _includeInactive;
    private ListViewMode _view = ListViewMode.Card;
    private string _searchString = "";
    private ExpenseCategoryGroup? _groupFilter;
    private ExpenseCategory? _categoryFilter;
    private int? _yearFilter;
    private string _currency = Currencies.Default;
    private IEnumerable<ExpenseViewModel> _expenses = Enumerable.Empty<ExpenseViewModel>();

    [CascadingParameter]
    public required MainLayout Layout { get; set; }

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IPreferenceService Preferences { get; set; } = default!;
    [Inject] private ICurrencyPreference CurrencyPreference { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Layout.SetBreadCrumbs(new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Expenses", href: "expenses"),
        });

        _view = await Preferences.GetListViewAsync(ViewPreferenceKey);
        _currency = await CurrencyPreference.GetAsync();

        await LoadExpensesAsync();

        _loaded = true;
    }

    private async Task SetViewAsync(ListViewMode mode)
    {
        if (_view == mode)
        {
            return;
        }

        _view = mode;
        await Preferences.SetListViewAsync(ViewPreferenceKey, mode);
    }

    // The whole list is small (hundreds of rows), so it's fetched once and the lifecycle toggle filters client-side.
    private async Task LoadExpensesAsync()
    {
        _expenses = await Http.GetFromJsonAsync<ExpenseViewModel[]>("expenses")
            ?? Enumerable.Empty<ExpenseViewModel>();
    }

    private IEnumerable<ExpenseViewModel> VisibleExpenses => _expenses
        .Where(e => _includeInactive || e.Lifecycle == ExpenseLifecycle.Active);

    // Category options narrow to the picked group.
    private IEnumerable<ExpenseCategory> CategoryOptions => ExpenseCategoryMetadata.CategoriesByGroup()
        .Where(g => _groupFilter is null || g.Group == _groupFilter)
        .SelectMany(g => g.Categories);

    private IEnumerable<int> Years => _expenses
        .Select(e => e.PurchaseDate.Year)
        .Distinct()
        .OrderByDescending(y => y);

    private void OnGroupChanged()
    {
        if (_categoryFilter is { } category && _groupFilter is { } group && ExpenseCategoryMetadata.Group(category) != group)
        {
            _categoryFilter = null;
        }
    }

    private bool Search(ExpenseViewModel expense)
    {
        if (string.IsNullOrWhiteSpace(_searchString))
        {
            return true;
        }

        return expense.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase)
            || (expense.PurchasePlace?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
            || ExpenseCategoryMetadata.DisplayName(expense.Category).Contains(_searchString, StringComparison.OrdinalIgnoreCase);
    }

    private IEnumerable<ExpenseViewModel> FilteredExpenses => VisibleExpenses
        .Where(Search)
        .Where(e => _groupFilter is null || e.CategoryGroup == _groupFilter)
        .Where(e => _categoryFilter is null || e.Category == _categoryFilter)
        .Where(e => _yearFilter is null || e.PurchaseDate.Year == _yearFilter);

    private async Task AddAsync()
    {
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<AddEditExpenseDialog>("Add expense", options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadExpensesAsync();
        }
    }

    private async Task EditAsync(ExpenseViewModel expense)
    {
        var parameters = new DialogParameters<AddEditExpenseDialog> { { x => x.Expense, expense } };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<AddEditExpenseDialog>("Edit expense", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadExpensesAsync();
        }
    }

    private async Task SellAsync(ExpenseViewModel expense)
    {
        var parameters = new DialogParameters<SellExpenseDialog> { { x => x.Expense, expense } };
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<SellExpenseDialog>("Sell expense", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadExpensesAsync();
        }
    }

    private async Task ArchiveAsync(ExpenseViewModel expense)
    {
        var parameters = new DialogParameters<ArchiveExpenseDialog> { { x => x.Expense, expense } };
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<ArchiveExpenseDialog>("Archive expense", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadExpensesAsync();
        }
    }

    private async Task ActivateAsync(ExpenseViewModel expense)
    {
        var body = new ChangeExpenseLifecycleViewModel { Lifecycle = ExpenseLifecycle.Active };
        var response = await Http.PutAsJsonAsync($"expenses/{expense.Id}/lifecycle", body);

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Expense active", Severity.Success);
            await LoadExpensesAsync();
        }
        else
        {
            await response.ShowProblemAsync(Snackbar);
        }
    }

    private async Task DeleteAsync(ExpenseViewModel expense)
    {
        var confirmed = await DialogService.ShowMessageBoxAsync(
            "Delete expense",
            $"Delete \"{expense.Name}\"? This cannot be undone.",
            yesText: "Delete",
            cancelText: "Cancel");

        if (confirmed != true)
        {
            return;
        }

        var response = await Http.DeleteAsync($"expenses/{expense.Id}");

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Expense deleted", Severity.Success);
            await LoadExpensesAsync();
        }
        else
        {
            await response.ShowProblemAsync(Snackbar);
        }
    }

    internal static Color LifecycleColor(ExpenseLifecycle lifecycle) => lifecycle switch
    {
        ExpenseLifecycle.Active => Color.Success,
        ExpenseLifecycle.Archived => Color.Default,
        ExpenseLifecycle.Sold => Color.Info,
        _ => Color.Default
    };
}
