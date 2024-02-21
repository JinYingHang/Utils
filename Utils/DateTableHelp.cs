using ExcelDataReader;
using System.Collections.Generic;
using System;
using System.Data;
using System.IO;
using System.Linq;

namespace Utils
{
    public static class DataTableHelp
    {
        /// <summary>
        /// .XLS||.XLSX转换成DataSet
        /// </summary>
        /// <param name="fileNmaePath"></param>
        /// <returns></returns>
        public static DataSet ParseExcelToDataSet(string fileNmaePath) {
            try {
                FileStream stream = null;
                IExcelDataReader excelReader = null;
                DataSet dataSet = null;
                try {
                    //stream = File.Open(fileNmaePath, FileMode.Open, FileAccess.Read);
                    stream = new FileStream(fileNmaePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                catch {
                    return null;
                }
                string extension = Path.GetExtension(fileNmaePath);
                if (extension.ToUpper() == ".XLS") {
                    excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else if (extension.ToUpper() == ".XLSX") {
                    excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }
                else {
                    return null;
                }
                //dataSet = excelReader.AsDataSet();//第一行当作数据读取
                dataSet = excelReader.AsDataSet(new ExcelDataSetConfiguration() {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration() {
                        UseHeaderRow = true
                    }
                });//第一行当作列名读取
                excelReader.Close();
                return dataSet;
            }
            catch (System.Exception) {
                throw;
            }
        }

        /// <summary>
        /// CSV转换成DataTable
        /// </summary>
        /// <param name="fileNmaePath"></param>
        /// <returns></returns>
        public static DataTable ParseCSVToDataTable(string fileNmaePath) {
            try {
                // 假设你有一个从 CSV 文件中读取数据的方法
                List<string[]> csvData = ReadCSVFile(fileNmaePath);
                if (csvData != null && csvData.Any()) {
                    // 创建 DataTable
                    DataTable dataTable = new DataTable();

                    // 使用第一行数据作为列名
                    string[] headers = csvData[0];
                    foreach (string header in headers) {
                        dataTable.Columns.Add(new DataColumn(header));
                    }

                    // 添加数据到 DataTable
                    for (int i = 1; i < csvData.Count; i++) {
                        DataRow row = dataTable.NewRow();
                        row.ItemArray = csvData[i];
                        dataTable.Rows.Add(row);
                    }
                    return dataTable;
                }
                else {
                    return null;
                }
            }
            catch (Exception) {
                throw;
            }
        }

        // 读取 CSV
        private static List<string[]> ReadCSVFile(string filePath) {
            List<string[]> data = new List<string[]>();
            try {
                using (StreamReader sr = new StreamReader(filePath)) {
                    string line;
                    while ((line = sr.ReadLine()) != null) {
                        string[] values = line.Split(','); 
                        data.Add(values);
                    }
                }
            }
            catch (Exception) {
                throw;
            }
            return data;
        }
    }
}
