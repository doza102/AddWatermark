using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;

namespace AddWatermark
{
    public class GetCoordinates : ESRI.ArcGIS.Desktop.AddIns.Tool
    {

        Form1 form = new Form1();
        public static IRaster2 raster2;
        public static IPixelBlock3 block3;
        public static IPoint pPoint;
        public const int size = 25;
        public static System.Array pixels;
        public static IMxDocument pMxDoc;
        public static ILayer selectedLayer;
        public static IRasterLayer rasterLyr;
        public static IRasterEdit rasterEdit;
        public static string fileName, filePath;

        public GetCoordinates()
        {
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }

        protected override void OnMouseDown(MouseEventArgs arg)
        {
            string data = "";
            pMxDoc = ArcMap.Document;
            pPoint = pMxDoc.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y);

            selectedLayer = pMxDoc.SelectedLayer;


            if (selectedLayer != null)
            {
                if (selectedLayer is IRasterLayer)
                {
                    rasterLyr = selectedLayer as IRasterLayer;
                    int bandCount = rasterLyr.BandCount;
                    IRaster2 selectedRL = rasterLyr.Raster as IRaster2;
                    raster2 = selectedRL;
                    rasterEdit = raster2 as IRasterEdit;
                    IRasterProps props = (IRasterProps)selectedRL;
                    IDataset dataset = selectedLayer as IDataset;
                    fileName = dataset.Name.ToString() ;
                    filePath = dataset.Workspace.PathName.ToString();

                    var cellSize = props.MeanCellSize();
                    rstPixelType type = props.PixelType;

                    int column = 0;
                    int row = 0;


                    pPoint.SpatialReference = pMxDoc.FocusMap.SpatialReference;

                    selectedRL.MapToPixel(pPoint.X, pPoint.Y, out column, out row);

                    if (bandCount != -1)
                    {
                        try
                        {
                            var x = Math.Round((Decimal)pPoint.X, 3, MidpointRounding.AwayFromZero);
                            var y = Math.Round((Decimal)pPoint.Y, 3, MidpointRounding.AwayFromZero);

                            data += "Cell size: " + cellSize.X.ToString() + " x " + cellSize.Y.ToString() + "\r\n";
                            data += "Map X: " + x + "\r\n \tColumn no :" + column + " \r\n";
                            data += "Map Y: " + y + "\r\n \tRow no :" + row + "\r\n";

                            //if value is not null, then print it
                            float value = selectedRL.GetPixelValue(0, column, row);
                            data += "Pixel Value is: " + value.ToString();
                            form.textBox1.Text = data;

                        }
                        catch (Exception e)
                        {
                            //if value of pixel is null, print is null
                            data += "Pixel Value is: null";
                        }


                        GetPixelBlock(size, pPoint, type);
                        //BlockArray(selectedRL, cellSize.X, 5, pPoint);

                        form.textBox1.Text = data;
                        form.ShowDialog();
                    }
                }
                else
                {
                    data += "Layer is not raster!";
                    form.textBox1.Text = data;

                    form.Show();

                }
            }
            else
            {
                data += "Please select a Layer!";
                form.textBox1.Text = data;

                form.Show();
            }

        }
        private void GetPixelBlock(int size, IPoint pPoint, rstPixelType type)
        {
            int x, y;
            IRaster raster = (IRaster)raster2;

            IPixelBlock block = raster2.CreateCursorEx(null).PixelBlock;
            IPixelBlock4 block4 = (IPixelBlock4)block;
            block4.Create(1, size, size, type);


            // read block
            IPnt topLeftCorner = new Pnt();

            raster2.MapToPixel(pPoint.X, pPoint.Y, out x, out y);

            topLeftCorner.SetCoords(x, y);
            raster.Read(topLeftCorner, block);
            block3 = (IPixelBlock3)block4;
            pixels = (System.Array)block3.get_PixelData(0);


            form.dataGridView1.Rows.Clear();
            form.dataGridView1.Refresh();
            form.dataGridView1.ColumnCount = size;

            int lenghtArray = pixels.Length;
            for (int i = 0; i < size; i++)
            {


                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(form.dataGridView1);

                for (int j = 0; j < size; j++)
                {
                    row.Cells[j].Value = pixels.GetValue(j, i).ToString();

                    //Get the pixel value.
                    //value = pixels.GetValue(j, i).ToString();

                    //Do something with the value.
                    //form.dataGridView1.Rows.Add(value, typeof(float));
                }
                form.dataGridView1.Rows.Add(row);

            }

            form.dataGridView1.AutoResizeColumns();
            form.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            form.button1.Enabled = true;
            //form.textBox2.Text = pPoint.X.ToString() + ", " + pPoint.Y.ToString();


        }

        // public void BurnValues(IRaster2 raster2, IPoint pPoint, IPixelBlock3 block3, int size)
        public void BurnValues(DataGridView grid)
        {
            QR qR_code = new QR();
            qR_code.QR_ImpactForecasting(grid);
            Type type = pixels.GetValue(0, 0).GetType();

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    //value = Convert.ToUInt32(grid.Rows[i].Cells[j].Value);
                    dynamic value = grid.Rows[i].Cells[j].Value;
                    pixels.SetValue(Convert.ChangeType(value, type), j, i);

                }
            }

            block3.set_PixelData(0, pixels);

            IPnt topLeftCorner = new Pnt();
            raster2.MapToPixel(pPoint.X, pPoint.Y, out int x, out int y);
            topLeftCorner.SetCoords(x, y);

            IPixelBlock block = (IPixelBlock)block3;

            //form.dataGridView1.Data

            rasterEdit.Write(topLeftCorner, block);
            rasterEdit.Refresh();

            pMxDoc.ActiveView.Refresh();

        }
        public IPoint ReturnLocation()
        {
            return pPoint;
        }

        public string ReturnFileName()
        {
            return fileName;
        }
        public string ReturnFilePath()
        {
            return filePath;
        }

    }
    

}
