using System;
using System.Linq;
using FluentAssertions;
using KomTracker.API.Shared.ViewModels.Expense;
using KomTracker.Application.Commands.Expense;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Expense;

/// <summary>
/// Guards the deliberate duplication between the request bodies and the commands: every field the WEB posts must
/// exist on the command with a matching type (the controller maps them one by one).
/// </summary>
public class ExpenseContractParityTests
{
    [Theory]
    [InlineData(typeof(SaveExpenseViewModel), typeof(SaveExpenseCommand))]
    [InlineData(typeof(ChangeExpenseLifecycleViewModel), typeof(ChangeExpenseLifecycleCommand))]
    public void Command_covers_every_view_model_field_with_matching_type(Type viewModel, Type command)
    {
        var commandProps = command.GetProperties().ToDictionary(p => p.Name, p => p.PropertyType);

        foreach (var inputProp in viewModel.GetProperties())
        {
            commandProps.Should().ContainKey(inputProp.Name,
                "{0} must expose '{1}' from {2}", command.Name, inputProp.Name, viewModel.Name);
            commandProps[inputProp.Name].Should().Be(inputProp.PropertyType,
                "'{0}' must have the same type on {1} and {2}", inputProp.Name, viewModel.Name, command.Name);
        }
    }
}
