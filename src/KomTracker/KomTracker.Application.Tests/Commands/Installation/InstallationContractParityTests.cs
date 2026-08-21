using System.Linq;
using FluentAssertions;
using KomTracker.API.Shared.ViewModels.Installation;
using KomTracker.Application.Commands.Installation;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Installation;

/// <summary>
/// Guards the deliberate duplication between the request bodies (Installation view-models) and their commands:
/// every field on the VM must exist on the command with a matching type.
/// </summary>
public class InstallationContractParityTests
{
    [Fact]
    public void InstallComponentCommand_covers_every_InstallComponentViewModel_field_with_matching_type()
        => AssertParity(typeof(InstallComponentViewModel), typeof(InstallComponentCommand));

    [Fact]
    public void MoveInstallationCommand_covers_every_MoveInstallationViewModel_field_with_matching_type()
        => AssertParity(typeof(MoveInstallationViewModel), typeof(MoveInstallationCommand));

    [Fact]
    public void RemoveInstallationCommand_covers_every_RemoveInstallationViewModel_field_with_matching_type()
        => AssertParity(typeof(RemoveInstallationViewModel), typeof(RemoveInstallationCommand));

    [Fact]
    public void UpdateInstallationCommand_covers_every_UpdateInstallationViewModel_field_with_matching_type()
        => AssertParity(typeof(UpdateInstallationViewModel), typeof(UpdateInstallationCommand));

    private static void AssertParity(System.Type viewModel, System.Type command)
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
