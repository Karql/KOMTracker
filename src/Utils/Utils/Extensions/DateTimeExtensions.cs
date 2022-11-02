using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Extensions;

public static class DateTimeExtensions
{
    public static long ToTimeStamp(this DateTime value)
    {
        return Convert.ToInt64((value - DateTime.UnixEpoch).TotalSeconds);
    }

    public static string ToUtcIso(this DateTime value)
    {
        return value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
    }

    public static DateTime SetDay(this DateTime value, int day) =>
        new(value.Year, value.Month, day, value.Hour, value.Minute, value.Second, value.Millisecond, value.Kind);

    public static DateTime FirstDayOfMonth(this DateTime value) =>
        value.SetDay(1);

    public static DateTime LastDayOfMonth(this DateTime value) =>
        value.SetDay(DateTime.DaysInMonth(value.Year, value.Month));

    public static DateTime FirstDayOfWeek(this DateTime value, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
    {
        var offset = value.DayOfWeek - firstDayOfWeek < 0 ? 7 : 0;
        var numberOfDaysSinceBeginningOfTheWeek = value.DayOfWeek + offset - firstDayOfWeek;

        return value.AddDays(-numberOfDaysSinceBeginningOfTheWeek);
    }

    public static DateTime LastDayOfWeek(this DateTime value, DayOfWeek firstDayOfWeek = DayOfWeek.Monday) =>
        value.FirstDayOfWeek(firstDayOfWeek).AddDays(6);

    public static DateTime BeginningOfDay(this DateTime date) =>
        new(date.Year, date.Month, date.Day, 0, 0, 0, 0, date.Kind);

    public static DateTime EndOfDay(this DateTime date) =>
        new(date.Year, date.Month, date.Day, 23, 59, 59, 999, date.Kind);

    public static DateTime BeginningOfWeek(this DateTime date, DayOfWeek firstDayOfWeek = DayOfWeek.Monday) =>
        date.FirstDayOfWeek(firstDayOfWeek).BeginningOfDay();

    public static DateTime EndOfWeek(this DateTime date, DayOfWeek firstDayOfWeek = DayOfWeek.Monday) =>
        date.LastDayOfWeek(firstDayOfWeek).EndOfDay();

    public static DateTime BeginningOfMonth(this DateTime date) =>
        date.FirstDayOfMonth().BeginningOfDay();

    public static DateTime EndOfMonth(this DateTime date) =>
        date.LastDayOfMonth().EndOfDay();
}
