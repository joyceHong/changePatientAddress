using LinqToExcel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WriteEvent;

namespace changePatientAddress
{
    public partial class Form2 : Form
    {
        contrReplaceAddress controlAddress = new contrReplaceAddress();
        WrittingEventLog writeObj = new WrittingEventLog();
        int totalProgress = 0;
        public Form2()
        {
            InitializeComponent();
        }

        

        private void btnOpenPath_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog openFileDialog1 = new FolderBrowserDialog();               
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = openFileDialog1.SelectedPath.ToString();                    
                }
            }
            catch (Exception ex)
            {
                writeObj.writeToFile(ex.Message);
                //throw new Exception(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                if (string.IsNullOrEmpty(textBox1.Text.Trim()))
                {
                    MessageBox.Show("請輸入cooper的路徑");
                }
                else
                {
                    controlAddress._cooperPath = textBox1.Text.Trim();
                    controlAddress.replacePatientAddress();
                    while (controlAddress.currentProgress < 105)
                    {
                        progressBar1.Value = controlAddress.currentProgress;
                        progressBar1.Update();
                    }
                    progressBar1.Value = controlAddress.currentProgress;
                    progressBar1.Update();
                    MessageBox.Show("轉檔完畢");
                }
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.Message);
                writeObj.writeToFile(ex.Message);
            }
        }       
    }
}
