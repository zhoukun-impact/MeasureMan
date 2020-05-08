using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeasureMan
{
    public partial class RasterRenderer : Form
    {
        /// <summary>
        /// 输入的栅格图层
        /// </summary>
        IRasterLayer rasterLayer;
        /// <summary>
        /// 符号样式
        /// </summary>
        ISymbologyStyleClass symbol;
        /// <summary>
        /// 渲染是否设置成功
        /// </summary>
        public bool succeed;
        /// <summary>
        /// 语言系统
        /// </summary>
        private Language lang;

        public RasterRenderer(IRasterLayer rasterLayer,Language lang)
        {
            InitializeComponent();
            this.lang = lang;
            if (lang == Language.English)
            {
                this.Text = "RasterRenderer";
                btRender.Text = "Render";
                cbMethod.Items.Clear();
                cbMethod.Items.AddRange(new object[4] { "equal interval", "natural breaks", "geometrical interval", "quantile" });
                label1.Text = "Layer Name";
                label2.Text = "Band Selection";
                label3.Text = "Class Number";
                label4.Text = "Ribbon Selection";
                label5.Text = "Classification Type";
                this.Width = 371;
                txtLayerName.Location = new Point(174, txtLayerName.Location.Y);
                cbSelectedBand.Location = new Point(174, cbSelectedBand.Location.Y);
                txtNumber.Location = new Point(174, txtNumber.Location.Y);
                cbColorRamp.Location = new Point(174, cbColorRamp.Location.Y);
                cbMethod.Location = new Point(174, cbMethod.Location.Y);
                btRender.Location = new Point(242, btRender.Location.Y);
            }
            axSymbologyControl1.Visible = false;
            this.rasterLayer=rasterLayer;
            txtLayerName.Text = rasterLayer.Name;
            txtLayerName.ReadOnly = true;
            succeed = false;
        }

        private void RasterRenderer_Load(object sender, EventArgs e)
        {          
            IRasterBandCollection bands = rasterLayer.Raster as IRasterBandCollection;
            List<string> bds = new List<string>();
            for(int i=0;i<bands.Count;i++)
                bds.Add(bands.Item(i).Bandname);
            cbSelectedBand.DataSource = bds;
            try
            {
                axSymbologyControl1.LoadStyleFile(System.IO.Path.GetDirectoryName(Application.ExecutablePath)+"\\ESRI\\ESRI.ServerStyle");
                axSymbologyControl1.StyleClass = esriSymbologyStyleClass.esriStyleClassColorRamps;
                symbol = axSymbologyControl1.GetStyleClass(esriSymbologyStyleClass.esriStyleClassColorRamps);
                cbColorRamp.DrawMode=DrawMode.OwnerDrawFixed;
                cbColorRamp.DropDownStyle = ComboBoxStyle.DropDownList;//这是最关键的一步
 
                for (int i = 0; i < symbol.get_ItemCount(symbol); i++)
                {
                    stdole.IPictureDisp picture = symbol.PreviewItem(symbol.GetItem(i), cbColorRamp.Width, cbColorRamp.Height);
                    Image image = Image.FromHbitmap(new IntPtr(picture.Handle));
                    cbColorRamp.Items.Add(image);
                }
            }
            catch
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("色带加载失败！");
                else if (lang == Language.English)
                    MessageBox.Show("Loading color ribbons failed!");
            }
        }

        private void btRender_Click(object sender, EventArgs e)
        {
            int number = 0;
            if (cbColorRamp.SelectedIndex == -1)
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("请先选择一种色带");
                else if (lang == Language.English)
                    MessageBox.Show("Please choose a color ribbon first");
                return;
            }
            try
            {
                number = int.Parse(txtNumber.Text);
                if (number <= 1)
                {
                    if (lang == Language.Chinese)
                        MessageBox.Show("类别数量只能为>1的正整数");
                    else if (lang == Language.English)
                        MessageBox.Show("The class number can only be a positive integer of >1");
                }   
            }
            catch
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("类别数量只能为整数");
                else if (lang == Language.English)
                    MessageBox.Show("The class number can only be an integer");
            }
            IRasterClassifyColorRampRenderer pRasterClassifyRenderer = new RasterClassifyColorRampRendererClass();
            IRasterRenderer pRasterRenderer = pRasterClassifyRenderer as IRasterRenderer;
            IUniqueValues uniVal = new UniqueValuesClass();
            IRasterCalcUniqueValues2 calValues = new RasterCalcUniqueValuesClass();
            calValues.MaxUniqueValueCount = 10000000;
            calValues.AddFromRaster(rasterLayer.Raster,cbSelectedBand.SelectedIndex,uniVal);
            object vValues, vFrequences;
            uniVal.GetHistogram(out vValues, out vFrequences);
            double[] doubleArray = (double[])vValues;
            int[] longArray = (int[])vFrequences;
            int order=SortTool.CheckInOrder(doubleArray,doubleArray.Length);
            if (order == 0)
                SortTool.QuickSort3(doubleArray, 0, doubleArray.Length - 1, longArray);
            else if (order == -1)
                SortTool.Reverse(doubleArray, doubleArray.Length, longArray);
            IClassifyGEN classify = GetMethod(cbMethod.Text);
            classify.Classify((double[])vValues, (int[])vFrequences, ref number);
            double[] classes = classify.ClassBreaks as double[];

            pRasterRenderer.Raster = rasterLayer.Raster;
            pRasterClassifyRenderer.ClassCount = number;
            for (int i = 0; i < classes.Length;i++ )
                pRasterClassifyRenderer.set_Break(i,classes[i]);
            pRasterRenderer.Update();

            IStyleGalleryItem galleryItem = symbol.GetItem(cbColorRamp.SelectedIndex);
            IColorRamp colorRamp = galleryItem.Item as IColorRamp;
            IFillSymbol fill = new SimpleFillSymbolClass() as IFillSymbol;
            int increase =colorRamp.Size/(number-1);
            for (int i = 0; i < pRasterClassifyRenderer.ClassCount-1; i++)
            {
                fill.Color = colorRamp.get_Color(i*increase);
                pRasterClassifyRenderer.set_Symbol(i, fill as ISymbol);
                pRasterClassifyRenderer.set_Label(i, pRasterClassifyRenderer.get_Break(i).ToString() + "-" + pRasterClassifyRenderer.get_Break(i+1).ToString());
            }
            fill.Color = colorRamp.get_Color(colorRamp.Size-1);
            pRasterClassifyRenderer.set_Symbol(pRasterClassifyRenderer.ClassCount - 1, fill as ISymbol);
            pRasterClassifyRenderer.set_Label(pRasterClassifyRenderer.ClassCount - 1, pRasterClassifyRenderer.get_Break(pRasterClassifyRenderer.ClassCount - 1).ToString() + "-" + pRasterClassifyRenderer.get_Break(pRasterClassifyRenderer.ClassCount).ToString());
            rasterLayer.Renderer =pRasterRenderer;
            succeed = true;
            this.Close();
        }

        /// <summary>
        /// 获得分级器
        /// </summary>
        /// <param name="method">分级器名称</param>
        /// <returns>分级器对象</returns>
        private IClassifyGEN GetMethod(string method)
        {
            switch (method)
            {
                case "等间隔分级法":
                case "equal interval":
                    return new EqualIntervalClass();
                case "自然间断点分级法":
                case "natural breaks":
                    return new NaturalBreaksClass();
                case "几何间隔分级法":
                case  "geometrical interval":
                    return new GeometricalIntervalClass();
                case "分位数分级法":
                case "quantile":
                    return new QuantileClass();
                default:
                    return new EqualIntervalClass();
            }
        }

        private void cbColorRamp_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;
            e.DrawBackground();
            e.DrawFocusRectangle();
            Image image = (Image)cbColorRamp.Items[e.Index];
            Rectangle rect = e.Bounds;
            e.Graphics.DrawImage(image, rect);
        }
    }
}
