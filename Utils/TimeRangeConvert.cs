using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Utils.TimeRangeConvert;

namespace Utils
{
    public class TimeRangeConvert
    {
        /// <summary>
        /// 目前仅针对10:51-12:51/14:51-15:30 这种。有需要后续拓展
        /// </summary>
        /// <param name="timeRanges"></param>
        /// <returns></returns>
        public static List<TimeRange> ConvertToTimeRanges(string timeRanges) {
            try {
                string[] parts = timeRanges.Split('/');
                List<TimeRange> result = new List<TimeRange>();
                foreach (string part in parts) {
                    string[] times = part.Split('-');
                    DateTime start = DateTime.Parse(times[0]);
                    DateTime end = DateTime.Parse(times[1]);
                    TimeRange range = new TimeRange(start, end);
                    result.Add(range);
                }
                return result;
            }
            catch (Exception) {
                throw;
            }
        }
    }

    public class TimeRange
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public TimeRange(DateTime start, DateTime end) {
            StartTime = start;
            EndTime = end;
        }
    }
}
