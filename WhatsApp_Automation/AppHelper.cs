using System;
using System.Data;
using System.IO;
using System.Windows;
using ExcelDataReader;

namespace WhatsApp_Automation
{
    public class AppHelper
    {
        public static DataTableCollection ImportExcelFile(string fileName)
        {
            try
            {
                using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                        });

                        return result.Tables;
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show(@"Excel file MUST be closed !");
            }
            return null;
        }
    }
}