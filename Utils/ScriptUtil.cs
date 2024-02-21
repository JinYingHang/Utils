using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Utils
{
    public class ScriptUtil
    {
        public string ReplaceKeys(string script) {
            // 定义一个正则表达式模式来匹配标记
            string pattern = @"\^([^']+?)\^";
            MatchCollection matches = Regex.Matches(script, pattern);
            // 遍历匹配项并获取标记中的值
            foreach (Match match in matches) {
                string key = match.Groups[1].Value;
                var fullKey = $"^{key}^";
                var replaceStr= GetReplaceStr(fullKey);
                script=script.Replace(fullKey, replaceStr);

            }
            return script;
        }
        private string GetReplaceStr(string key) {
            key = key.Replace("^","");
            if (key.Contains('.')) {
                string[] strings = key.Split('.');
                if (strings[0].ToString().StartsWith("RowCell")) {
                    switch (strings[0]) {
                        case "RowCell":
                            return $"JsonConvert.DeserializeObject<CellObj>(row[\"{strings[1]}\"].ToString())";
                        case "RowCell(double)":
                            return $"double.Parse( JsonConvert.DeserializeObject<CellObj>(row[\"{strings[1]}\"].ToString()).Value)";
                        default:
                            return $"关键字匹配失败，程序不包含:{strings[0]}关键字，请联系管理员进行添加";
                    }
                }
                else if (strings[0].ToString().StartsWith("sys")) {
                    //这里目前没用
                    switch(strings[0]) {
                        case "sysAvgCol":
                            return $"CalcUtil.AvgCol({strings[1]})";
                        case "sysMax":
                            return $"CalcUtil.sysMax({strings[1]})";
                        case "sysMin":
                            return $"CalcUtil.sysMin({strings[1]})";
                        default:
                            return $"关键字匹配失败，程序不包含:{strings[0]}关键字，请联系管理员进行添加";
                    }
                }
                else {
                    return key;
                }
            }
            else {
                return key;
            }
        }
    }
}
