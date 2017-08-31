using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace DDDReader_0._4._0_renewed
{
    public class ShiftElement
    {
        public int shiftNumber;
        public string StartingDateTime;
        public string EndingDateTime;

    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            XmlReader xml = XmlReader.Create(new FileStream("_561 - 2006ReportDEBUG.xml",FileMode.Open));
            while (xml.Read())
            {
                //if ((xml.NodeType==XmlNodeType.Element) && (xml.Name))
            }
        }
    }
}
