using KomTracker.API.Shared.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.API.Shared.Helpers;
public static class MapHelper
{
    public static IEnumerable<LatLngModel> Decode(string polyline)
    {
        if (string.IsNullOrEmpty(polyline))
            throw new ArgumentNullException("encodedPoints");

        char[] polylineChars = polyline.ToCharArray();
        int index = 0;

        int currentLat = 0;
        int currentLng = 0;
        int next5bits;
        int sum;
        int shifter;

        while (index < polylineChars.Length)
        {
            // calculate next latitude
            sum = 0;
            shifter = 0;
            do
            {
                next5bits = (int)polylineChars[index++] - 63;
                sum |= (next5bits & 31) << shifter;
                shifter += 5;
            } while (next5bits >= 32 && index < polylineChars.Length);

            if (index >= polylineChars.Length)
                break;

            currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

            //calculate next longitude
            sum = 0;
            shifter = 0;
            do
            {
                next5bits = (int)polylineChars[index++] - 63;
                sum |= (next5bits & 31) << shifter;
                shifter += 5;
            } while (next5bits >= 32 && index < polylineChars.Length);

            if (index >= polylineChars.Length && next5bits >= 32)
                break;

            currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

            yield return new LatLngModel
            {
                Latitude = Convert.ToDouble(currentLat) / 1E5,
                Longitude = Convert.ToDouble(currentLng) / 1E5
            };
        }
    }
}
