using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class Config
    {
        /// <summary>
        /// 根据KeyName读取指定位置config文件值
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static string ReadConfigValue(string keyName) {
            string value = "";
            try {
                ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                map.ExeConfigFilename = @"Solution Item/DBContext.config";
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                value = config.AppSettings.Settings[keyName].Value;
                return value;
            }
            catch (Exception) {
                throw;
            }
        }
    }
}
