using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Data;

namespace Ushahidi.Library
{
    /// <summary>
    /// Converts a DateTime object into a relative time String(eg 10 Seconds ago, 2 Years ago)
    /// </summary>
    public class RelativeDateTimeConverter : IValueConverter
    {

        private const int Minute = 60;

        private const int Hour = Minute * 60;

        private const int Day = Hour * 24;

        private const int Year = Day * 365;



        private readonly Dictionary<long, Func<TimeSpan, string>> thresholds = new Dictionary<long, Func<TimeSpan, string>>

    {

        {2, t => "a second ago"},

        {Minute,  t => String.Format("{0} seconds ago", (int)t.TotalSeconds)},

        {Minute * 2,  t => "a minute ago"},

        {Hour,  t => String.Format("{0} minutes ago", (int)t.TotalMinutes)},

        {Hour * 2,  t => "an hour ago"},

        {Day,  t => String.Format("{0} hours ago", (int)t.TotalHours)},

        {Day * 2,  t => "yesterday"},

        {Day * 30,  t => String.Format("{0} days ago", (int)t.TotalDays)},

        {Day * 60,  t => "last month"},

        {Year,  t => String.Format("{0} months ago", (int)t.TotalDays / 30)},

        {Year * 2,  t => "last year"},

        {Int64.MaxValue,  t => String.Format("{0} years ago", (int)t.TotalDays / 365)}

    };



        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var dateTime = (DateTime)value;

            var difference = DateTime.UtcNow - dateTime.ToUniversalTime();
            return thresholds.First(t => difference.TotalSeconds < t.Key).Value(difference);

        }


        public string Convert(DateTime value)
        {

            var dateTime = (DateTime)value;

            var difference = DateTime.UtcNow - dateTime.ToUniversalTime();
            return thresholds.First(t => difference.TotalSeconds < t.Key).Value(difference);

        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotSupportedException();

        }
        

        public string[] LastSeen(DateTime value)
        {

            var dateTime = (DateTime)value;

            var difference = DateTime.UtcNow - dateTime.ToUniversalTime();
            return LastSeenTimeSpanFixed.First(t => difference.TotalSeconds < t.Key).Value(difference);

        }

        public string[] LastSeen(TimeSpan value)
        {         
            return LastSeenTimeSpanFixed.First(t => value.TotalSeconds < t.Key).Value(value);

        }


        public string[] LastSeen(DateTime value, bool isAgo)
        {
            var dateTime = (DateTime)value;

            var difference = DateTime.UtcNow - dateTime.ToUniversalTime();
            return LastSeenTimeSpanAgo.First(t => difference.TotalSeconds < t.Key).Value(difference);

        }

        private readonly Dictionary<long, Func<TimeSpan, string[]>> LastSeenTimeSpanFixed = new Dictionary<long, Func<TimeSpan, string[]>> {

        {2, t =>new string[]{"second", ((int) t.TotalSeconds)+""}},

        {Minute,  t => new string[]{"seconds",((int) t.TotalSeconds)+""}},

        {Minute * 2,  t => new string[]{ "minute",((int)t.TotalMinutes)+"" }},

        {Hour,  t => new string[]{"minutes", ((int)t.TotalMinutes)+""}},

        {Hour * 2,  t => new string[]{"hour", ((int)t.TotalHours)+""}},

        {Day,  t => new string[]{"hours", ((int)t.TotalHours)+""}},

        {Day * 2,  t =>new string[]{ "day",((int)t.TotalDays)+""}},

        {Day * 30,  t =>new string[]{"days", ((int)t.TotalDays)+""}},

        {Day * 60,  t => new string[]{"month", ((int)t.TotalDays / 30)+""}},

        {Year,  t => new string[]{"months", ((int)t.TotalDays / 30)+""}},

        {Year * 2,  t => new string[]{"year", ((int)t.TotalDays/365)+""}},

        {Int64.MaxValue,  t => new string[]{"years", ((int)t.TotalDays/365)+""}}

    };

        private readonly Dictionary<long, Func<TimeSpan, string[]>> LastSeenTimeSpanAgo = new Dictionary<long, Func<TimeSpan, string[]>> {

        {2, t =>new string[]{"second ago", ((int) t.TotalSeconds)+""}},

        {Minute,  t => new string[]{"seconds ago",((int) t.TotalSeconds)+""}},

        {Minute * 2,  t => new string[]{ "minute ago",((int)t.TotalMinutes)+"" }},

        {Hour,  t => new string[]{"minutes ago", ((int)t.TotalMinutes)+""}},

        {Hour * 2,  t => new string[]{"hour ago", ((int)t.TotalHours)+""}},

        {Day,  t => new string[]{"hours ago", ((int)t.TotalHours)+""}},

        {Day * 2,  t =>new string[]{ "day ago",((int)t.TotalDays)+""}},

        {Day * 30,  t =>new string[]{"days ago", ((int)t.TotalDays)+""}},

        {Day * 60,  t => new string[]{"month ago", ((int)t.TotalDays / 30)+""}},

        {Year,  t => new string[]{"months ago", ((int)t.TotalDays / 30)+""}},

        {Year * 2,  t => new string[]{"year ago", ((int)t.TotalDays/365)+""}},

        {Int64.MaxValue,  t => new string[]{"years ago", ((int)t.TotalDays/365)+""}}

    };

    }

}
