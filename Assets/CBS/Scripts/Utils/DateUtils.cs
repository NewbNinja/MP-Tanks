using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Utils
{
    public class DateUtils
    {
        public static DateTime ConvertFromUnixTimestamp(long timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddMilliseconds(timestamp);
        }

        public static DateTime GetDateFromString(string data)
        {
            return DateTime.Parse(data).ToLocalTime();
        }

        public static int TimestampToHours(int timestamp)
        {
            var timeSpan = TimeSpan.FromMilliseconds(timestamp);
            return (int)timeSpan.TotalHours;
        }

        public static int TournamentDateToHours(TournamentDate date)
        {
            if (date == TournamentDate.Daily)
            {
                var timeSpan = TimeSpan.FromDays(1);
                return (int)timeSpan.TotalHours;
            }
            else if (date == TournamentDate.Hourly)
            {
                var timeSpan = TimeSpan.FromHours(1);
                return (int)timeSpan.TotalHours;
            }
            else if (date == TournamentDate.Weekly)
            {
                var timeSpan = TimeSpan.FromDays(7);
                return (int)timeSpan.TotalHours;
            }
            else if (date == TournamentDate.Monthly)
            {
                var timeSpan = TimeSpan.FromDays(30);
                return (int)timeSpan.TotalHours;
            }
            else if (date == TournamentDate.Yearly)
            {
                var timeSpan = TimeSpan.FromDays(365);
                return (int)timeSpan.TotalHours;
            }
            else
            {
                return 0;
            }
        }

        public static int HoursToMiliseconds(int days)
        {
            var timeSpan = TimeSpan.FromHours(days);
            return (int)timeSpan.TotalMilliseconds;
        }

        public static int ToutnamentDateMiliseconds(TournamentDate date)
        {
            if (date == TournamentDate.Daily)
            {
                var timeSpan = TimeSpan.FromDays(1);
                return (int)timeSpan.TotalMilliseconds;
            }
            else if (date == TournamentDate.Hourly)
            {
                var timeSpan = TimeSpan.FromHours(1);
                return (int)timeSpan.TotalMilliseconds;
            }
            else if (date == TournamentDate.Weekly)
            {
                var timeSpan = TimeSpan.FromDays(7);
                return (int)timeSpan.TotalMilliseconds;
            }
            else if (date == TournamentDate.Monthly)
            {
                var timeSpan = TimeSpan.FromDays(30);
                return (int)timeSpan.TotalMilliseconds;
            }
            else if (date == TournamentDate.Yearly)
            {
                var timeSpan = TimeSpan.FromDays(365);
                return (int)timeSpan.TotalMilliseconds;
            }
            else
            {
                return 0;
            }
        }

        public static int GetZoneOfset()
        {
#if UNITY_2020_2_OR_NEWER
            var curTimeZone = TimeZoneInfo.Local;
#else
            var curTimeZone = TimeZone.CurrentTimeZone;
#endif
            var ofset = curTimeZone.GetUtcOffset(DateTime.Now);
            return (int)ofset.TotalMilliseconds;
        }
    }
}
