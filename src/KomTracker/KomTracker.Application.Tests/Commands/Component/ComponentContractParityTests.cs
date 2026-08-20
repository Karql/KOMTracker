using System.Linq;
using FluentAssertions;
using KomTracker.API.Shared.ViewModels.Component;
using KomTracker.Application.Commands.Component;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Component;

/// <summary>
/// Guards the deliberate duplication between the request body (SaveComponentViewModel) and the command
/// (SaveComponentCommand): every editable field must exist on the command with a matching type.
/// </summary>
public class ComponentContractParityTests
{
    [Fact]
    public void SaveComponentCommand_covers_every_SaveComponentViewModel_field_with_matching_type()
    {
        var commandProps = typeof(SaveComponentCommand).GetProperties()
            .ToDictionary(p => p.Name, p => p.PropertyType);

        foreach (var inputProp in typeof(SaveComponentViewModel).GetProperties())
        {
            commandProps.Should().ContainKey(inputProp.Name,
                "SaveComponentCommand must expose '{0}' from SaveComponentViewModel", inputProp.Name);
            commandProps[inputProp.Name].Should().Be(inputProp.PropertyType,
                "'{0}' must have the same type on SaveComponentViewModel and SaveComponentCommand", inputProp.Name);
        }
    }
}
