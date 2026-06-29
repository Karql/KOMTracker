using FluentAssertions;
using KomTracker.Application.Models.Segment;
using Xunit;

namespace KomTracker.Application.Tests.Models.Segment;

public class ComparedEffortsModelTests
{
    // koms, lostKoms, previousKoms, expectedSuspicious
    // Cases are based on historical data captured in NOTES.md.

    #region Empty list response (koms == 0 AND lost > 20)
    [Theory]
    [InlineData(0, 30, 30, true)]      // synthetic, just above threshold
    [InlineData(0, 2608, 2608, true)]  // real glitch
    [InlineData(0, 55, 55, true)]      // real glitch
    [InlineData(0, 28, 28, true)]      // real glitch (smallest empty-list glitch in data)
    [InlineData(0, 21, 21, true)]      // boundary: 21 > 20
    [InlineData(0, 20, 20, false)]     // boundary: needs strictly > 20
    [InlineData(0, 5, 5, false)]       // small loss, likely legit total loss for a low-kom user
    #endregion

    #region Partial list response (koms > 0 AND lost > 50 AND ratio > 0.35)
    [InlineData(600, 2014, 2614, true)]  // real glitch, ratio 0.77
    [InlineData(400, 400, 800, true)]    // real glitch, ratio 0.50
    [InlineData(129, 71, 200, true)]     // boundary: ratio 0.355 > 0.35
    [InlineData(130, 70, 200, false)]    // boundary: ratio exactly 0.35 (needs strictly >)
    [InlineData(100, 50, 150, false)]    // boundary: lost == 50 (needs strictly > 50)
    #endregion

    #region Legitimate changes must NOT be blocked (user-labelled)
    [InlineData(2357, 152, 2509, false)] // legit cleanup, big account, ratio 0.06
    [InlineData(578, 68, 457, false)]    // legit return after suspension, ratio 0.15
    [InlineData(1494, 24, 1518, false)]  // small legit loss
    [InlineData(1497, 42, 1539, false)]  // small legit loss
    #endregion
    public void Is_suspicious_api_response_classifies_correctly(int koms, int lostKoms, int previousKoms, bool expectedSuspicious)
    {
        // Arrange
        var comparedEfforts = new ComparedEffortsModel
        {
            KomsCount = koms,
            LostKomsCount = lostKoms,
            PreviousKomsCount = previousKoms
        };

        // Act
        var isSuspicious = comparedEfforts.IsSuspiciousApiResponse;

        // Assert
        isSuspicious.Should().Be(expectedSuspicious);
    }

    [Fact]
    public void First_compare_with_no_previous_koms_is_not_suspicious()
    {
        // Arrange - first track: nothing fetched before, no losses
        var comparedEfforts = new ComparedEffortsModel
        {
            FirstCompare = true,
            KomsCount = 0,
            LostKomsCount = 0,
            PreviousKomsCount = 0
        };

        // Act & Assert
        comparedEfforts.IsSuspiciousApiResponse.Should().BeFalse();
    }
}
