using ExcelDataReader;
using System.Data;
using System.IO;

namespace Utils
{
    public static class Excel
    {
        public static DataSet ReadExcelToDataSet(string fileNmaePath) {
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
    }
}
