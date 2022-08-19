namespace GreenVerticalBot.EntityFramework
{
    using System;

    public static class DateTimeExtension
    {
        /// <summary>
        /// Безопасное сравнение DateTime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="value"></param>
        /// <remarks>
        /// Unspecified трактуется как время в UTC
        /// </remarks>
        /// <returns></returns>
        public static int SafeCompare(this DateTime dateTime, DateTime? value)
        {
            if (value == null)
            {
                return 1;
            }

            var dateTimeUniversal = dateTime.ToUniversalSafe();
            var valueUniversal = ((DateTime)value).ToUniversalSafe();

            return dateTimeUniversal.CompareTo(valueUniversal);
        }

        /// <summary>
        /// Приводит время к UTC безопасным для Unspecified образом
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static DateTime ToUniversalSafe(this DateTime datetime)
        {
            if (datetime.Kind != DateTimeKind.Unspecified)
            {
                return datetime.ToUniversalTime();
            }

            return DateTime.SpecifyKind(datetime, DateTimeKind.Utc);
        }

        /// <summary>
        /// Округление DateTime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        /// <remarks>
        /// https://stackoverflow.com/questions/1004698/how-to-truncate-milliseconds-off-of-a-net-datetime
        /// </remarks>
        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue) return dateTime; // do not modify "guard" values
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }
    }
}