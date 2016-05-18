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

namespace changePatientAddress
{
   
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        entityImportAddress entityAddressDB = new entityImportAddress();

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.InitialDirectory = @"c:\users\RDCP01\Desktop";
                openFileDialog1.RestoreDirectory = true;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = openFileDialog1.FileName;
                    var excelFile = new ExcelQueryFactory(openFileDialog1.FileName);
                    var sampleImportExcel = excelFile.Worksheet<address>();
                    foreach(address obj in sampleImportExcel.AsEnumerable<address>())
                    {
                        int areaID = entityAddressDB.saveArea(obj.area);
                        int cityID = entityAddressDB.saveiVillage(obj.city);
                        int roadID = entityAddressDB.saveLoad(obj.oldLoad, obj.newLoad);
                        entityAddressDB.saveStreetNumber(areaID, cityID, roadID, obj.oldStreetNumber, obj.newStreetNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            List<address> liAddress = new List<address>()
                    {
                       new address(){
                        area="1",
                         city ="2",
                          newLoad ="3",
                           newStreetNumber ="4",
                            oldLoad="6",
                             oldStreetNumber="7"
                       }    
                    };
            dataGridView1.DataSource = liAddress;
        }

        
    }

    public class address
    {
        public string area
        {
            get;
            set;
        }

        public string city
        {
            get;
            set;
        }

        public string oldLoad
        {
            get;
            set;
        }

        public string oldStreetNumber
        {
            get;
            set;
        }

        public string newLoad
        {
            get;
            set;
        }

        public string newStreetNumber
        {
            get;
            set;
        }

    }    
}
