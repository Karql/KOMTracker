using System.Net;
using System.Net.Http.Json;
using KomTracker.API.Shared.ViewModels.Expense;
using KomTracker.Domain.Entities.Expense;
using KomTracker.WEB.Infrastructure;
using KomTracker.WEB.Infrastructure.Services.Currency;
using KomTracker.WEB.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KomTracker.WEB.Pages;

public partial class ExpenseDetails
{
    [Parameter] public int Id { get; set; }

    private bool _loaded;
    private ExpenseViewModel? _expense;
    private string _currency = Currencies.Default;

    [CascadingParameter]
    public required MainLayout Layout { get; set; }

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ICurrencyPreference CurrencyPreference { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _currency = await CurrencyPreference.GetAsync();

        Layout.SetBreadCrumbs(new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Expenses", href: "expenses"),
            new BreadcrumbItem("Details", href: $"expenses/{Id}"),
        });

        await LoadAsync();

        _loaded = true;
    }

    private async Task LoadAsync()
    {
        var response = await Http.GetAsync($"expenses/{Id}");

        _expense = response.StatusCode == HttpStatusCode.NotFound
            ? null
            : await response.Content.ReadFromJsonAsync<ExpenseViewModel>();
    }

    private async Task EditAsync()
    {
        if (_expense is null)
        {
            return;
        }

        var parameters = new DialogParameters<AddEditExpenseDialog> { { x => x.Expense, _expense } };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<AddEditExpenseDialog>("Edit expense", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }

    private async Task SellAsync()
    {
        if (_expense is null)
        {
            return;
        }

        var parameters = new DialogParameters<SellExpenseDialog> { { x => x.Expense, _expense } };
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<SellExpenseDialog>("Sell expense", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }

    private async Task ArchiveAsync()
    {
        if (_expense is null)
        {
            return;
        }

        var parameters = new DialogParameters<ArchiveExpenseDialog> { { x => x.Expense, _expense } };
        var options = new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true, CloseButton = true };
        var dialog = await DialogService.ShowAsync<ArchiveExpenseDialog>("Archive expense", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadAsync();
        }
    }

    private async Task ActivateAsync()
    {
        if (_expense is null)
        {
            return;
        }

        var body = new ChangeExpenseLifecycleViewModel { Lifecycle = ExpenseLifecycle.Active };
        var response = await Http.PutAsJsonAsync($"expenses/{_expense.Id}/lifecycle", body);

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Expense active", Severity.Success);
            await LoadAsync();
        }
        else
        {
            await response.ShowProblemAsync(Snackbar);
        }
    }

    private async Task DeleteAsync()
    {
        if (_expense is null)
        {
            return;
        }

        var confirmed = await DialogService.ShowMessageBoxAsync(
            "Delete expense",
            $"Delete \"{_expense.Name}\"? This cannot be undone.",
            yesText: "Delete",
            cancelText: "Cancel");

        if (confirmed != true)
        {
            return;
        }

        var response = await Http.DeleteAsync($"expenses/{_expense.Id}");

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Expense deleted", Severity.Success);
            Navigation.NavigateTo("expenses");
        }
        else
        {
            await response.ShowProblemAsync(Snackbar);
        }
    }
}
