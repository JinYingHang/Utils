using System;
using System.Configuration;

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
                map.ExeConfigFilename = @"Solution Item/autoof.config";
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                value = config.AppSettings.Settings[keyName].Value;
                return value;
            }
            catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// 根据KeyName读取指定位置config文件值
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static string ReadConfigValue(string keyName,string filePath) {
            string value = "";
            try {
                ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                map.ExeConfigFilename = filePath;
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
