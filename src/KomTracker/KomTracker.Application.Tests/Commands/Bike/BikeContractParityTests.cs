using System.Linq;
using FluentAssertions;
using KomTracker.API.Shared.ViewModels.Bike;
using KomTracker.Application.Commands.Bike;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Bike;

/// <summary>
/// Guards the deliberate duplication between the request body (SaveBikeViewModel) and the command
/// (SaveBikeCommand): every editable field must exist on the command with a matching type, so a
/// rename/type change on one side can't silently break the field→error-key mapping.
/// </summary>
public class BikeContractParityTests
{
    [Fact]
    public void SaveBikeCommand_covers_every_SaveBikeViewModel_field_with_matching_type()
    {
        var commandProps = typeof(SaveBikeCommand).GetProperties()
            .ToDictionary(p => p.Name, p => p.PropertyType);

        foreach (var inputProp in typeof(SaveBikeViewModel).GetProperties())
        {
            commandProps.Should().ContainKey(inputProp.Name,
                "SaveBikeCommand must expose '{0}' from SaveBikeViewModel", inputProp.Name);
            commandProps[inputProp.Name].Should().Be(inputProp.PropertyType,
                "'{0}' must have the same type on SaveBikeViewModel and SaveBikeCommand", inputProp.Name);
        }
    }
}
