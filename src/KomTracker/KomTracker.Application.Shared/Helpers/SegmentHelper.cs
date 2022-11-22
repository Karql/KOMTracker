using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Shared.Helpers;

public static class SegmentHelper
{
    public static int GetExtendedCategory(int climbCategory, float averageGrade, float distance)
    {
        if (climbCategory > 0)
        {
            return climbCategory;
        }

        return averageGrade switch
        {
            < -8 => -8 ,                      // D1
            >= -8 and < -4 => -7,             // D2
            >= -4 and < 3 =>
                distance switch
                {
                    < 1000 => -6,             // SP
                    >= 1000 and < 3500 => -5, // FM
                    >= 3500 and < 7500 => -4, // TT1
                    _ => -3                   // TT2 (>= 7500)
                },
            >= 3 and < 8 => -2,               // MC
            _ => -1                           // WL
        };
    }

    public static string GetExtendedCategoryText(int extendedCategory)
    {
        return extendedCategory switch
        {
            -8 => "D1",
            -7 => "D2",
            -6 => "SP",
            -5 => "FL",
            -4 => "TTS",
            -3 => "TTL",
            -2 => "MC",
            -1 => "WL",
            1 => "4",
            2 => "3",
            3 => "2",
            4 => "1",
            5 => "HC",
            _ => throw new ArgumentException($"{nameof(extendedCategory)} should be between -8 and 5 (without 0)")
        };
    }
}
