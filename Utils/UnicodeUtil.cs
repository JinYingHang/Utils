using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utils
{
    public class UnicodeUtil
    {
        /// <summary>
        /// Unicode转字符串 (正则形式)
        /// </summary>
        /// <param name="source">Unicode码</param>
        /// <returns></returns>
       public static string UnicodeRegexToString(string source) {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                         source, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }
    }
}
