using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AddWatermark
{
    public partial class Form1 : Form
    {
        private StringBuilder csv = new StringBuilder();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Multiline = true;
            textBox1.ReadOnly = true;
            textBox1.BackColor = this.BackColor;
            textBox1.WordWrap = true;
            textBox1.BorderStyle = 0;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPoint pPoint;

            GetCoordinates changeValues = new GetCoordinates();
            changeValues.BurnValues(dataGridView1);

            string dir = @"C:\Watermark\";
            System.IO.Directory.CreateDirectory(dir);
            pPoint = changeValues.ReturnLocation();
            string fileName = changeValues.ReturnFileName();
            string filePath = changeValues.ReturnFilePath();

            //put coordinates and filepath to CSV
            //X,Y of top left corner, where code is burned into raster
            csv.AppendLine("X,Y,File");
            csv.AppendLine(pPoint.X.ToString() + ","
                + pPoint.Y.ToString() +","
                + filePath + fileName);

            System.IO.File.WriteAllText(dir + 
                fileName.Remove(fileName.Length - 4) + ".csv", csv.ToString());



        }
    }
}
