using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace WhatsApp_Automation
{
    public partial class FrmMain : Form
    {
        //declare & initialize data_table to carry data from excel to data_grid_view
        private DataTable _dt = new DataTable();

        public FrmMain() => InitializeComponent();

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog() {Filter = @"Excel Workbook|*.xlsx|Excel 97-2003 Workbook|*.xls"})
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    //empty combobox of sheets name firstly
                    cbxSheetsName.Items.Clear();

                    var excelSheets = AppHelper.ImportExcelFile(ofd.FileName);
                    if (excelSheets == null) return;

                    //Fill text_box with file name and extension
                    txtFileName.Text = Path.GetFileName(ofd.FileName);

                    //Fill combobox with sheets names
                    foreach (var sheetName in excelSheets)
                        cbxSheetsName.Items.Add(sheetName.ToString());
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            //Fill data_grid_view with data_table
            if (txtFileName.Text != null && cbxSheetsName.SelectedItem != null)
            {
                //convert data_table_collection to data_table to fill data_grid_view
                var dtc = AppHelper.ImportExcelFile(txtFileName.Text);
                _dt = dtc[cbxSheetsName.SelectedItem.ToString()];
                dgv.DataSource = _dt;
            }

            else
            {
                MessageBox.Show(@"Please select file and sheet name", @"Error");
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (dgv.RowCount > 0)
            {
                //create list of class ContactInfo
                var person = new List<ContactInfo>();

                //fill list of class members with every contact info in excel sheet
                for (var row = 0; row < _dt.Rows.Count; row++)
                {
                    try
                    {
                        person.Add(new ContactInfo()
                        {
                            ContactName = _dt.Rows[row][0].ToString(),
                            MsgTxt = _dt.Rows[row][1].ToString(),
                            PhotoPath = _dt.Rows[row][2].ToString(),
                            PhotoDesc = _dt.Rows[row][3].ToString()
                        });
                    }
                    catch (IndexOutOfRangeException)
                    {
                        MessageBox.Show(@"Use excel template sheet",@"Error");
                        return;
                    }
                    
                }
                
                //create log file if not exist
                var fileName = ".\\" + "Log - " + DateTime.Now.ToString("dd-MM-yyyy - HH.mm.ss tt") + ".txt";
                var logFile = new StreamWriter($"{fileName}", true);
                logFile.WriteLine("DATE : " + DateTime.Now.ToString("dd-MM-yyyy - HH.mm.ss tt"));

                //create object from SendMessage class
                var objSend = new SendMessage();

                objSend.Send(ref person, ref lblCounter, ref logFile);
                MessageBox.Show(@"Done", @"Congratulations");
                
                //close streaming
                logFile.Close();
            }
            
            else
            {
                MessageBox.Show(@"Please import excel file", @"Error");
            }
        }

        private void btnCopyPath_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(txtPath.Text);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtPath.Clear();
        }

        private void btnGetPath_Click(object sender, EventArgs e)
        {
            //filter to choose photos or any available file to get its path
            var ofd = new OpenFileDialog
            {
                Filter = @"Custom Files|" +
                         @"*.xbm;*.tif;*.pjp;*.svgz;*.jpg;*.jpeg;*.ico;*.tiff;" +
                         @"*.gif;*.svg;*.jfif;*.webp;*.png;*.bmp;*.pjpeg;*.avif;*.m4v;*.mp4;*.3gpp;*.mov" +
                         @"|All Files|*.*"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = ofd.FileName;
            }
        }
    }
}