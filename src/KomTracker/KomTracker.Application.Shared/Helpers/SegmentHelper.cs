using KomTracker.Application.Shared.Models.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Shared.Helpers;

public static class SegmentHelper
{
    public static ExtendedCategoryEnum GetExtendedCategory(int climbCategory, float averageGrade, float distance)
    {
        if (climbCategory > 0)
        {
            return climbCategory switch
            {
                1 => ExtendedCategoryEnum.C4,
                2 => ExtendedCategoryEnum.C3,
                3 => ExtendedCategoryEnum.C2,
                4 => ExtendedCategoryEnum.C1,
                5 => ExtendedCategoryEnum.HC,
                _ => throw new ArgumentException($"{nameof(climbCategory)} should be between 0 and 5"),
            };
        }

        return averageGrade switch
        {
            < -8 => ExtendedCategoryEnum.D1,
            >= -8 and < -4 => ExtendedCategoryEnum.D2,
            >= -4 and < 3 =>
                distance switch
                {
                    < 1000 => ExtendedCategoryEnum.SP,
                    >= 1000 and < 3500 => ExtendedCategoryEnum.FL,
                    >= 3500 and < 7500 => ExtendedCategoryEnum.TTS,
                    _ => ExtendedCategoryEnum.TTL
                },
            >= 3 and < 8 => ExtendedCategoryEnum.MC,
            _ => ExtendedCategoryEnum.WL
        };
    }

    public static string GetExtendedCategoryText(ExtendedCategoryEnum extendedCategory)
    {
        return extendedCategory.ToString();
    }
}
