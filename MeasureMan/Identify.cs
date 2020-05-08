using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
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

namespace MeasureMan
{
    public partial class Identify : Form
    {
        public Identify(ILayer layer, IPoint position, IRow row, IRasterIdentifyObj pixel,Language lang)
        {
            InitializeComponent();
            if (lang == Language.English)
            {
                this.Text = "Identify";
                label2.Text = "Layer";
                label1.Text = "Position";
            }
            rtbAttribute.ReadOnly = true;
            txtLayer.ReadOnly = true;
            txtPosition.ReadOnly = true;
            txtPosition.Text = position.X + "," + position.Y;
            if (layer is IFeatureLayer)
            {
                if (lang == Language.Chinese)
                    txtLayer.Text = layer.Name + "（矢量图层）";
                else if (lang == Language.English)
                    txtLayer.Text = layer.Name + "(Feature Layer)";
                IFeatureLayer featureLayer = layer as IFeatureLayer;
                IFeatureClass feature=featureLayer.FeatureClass;
                int shpIndex = feature.FindField(feature.ShapeFieldName);
                string shpType = feature.ShapeType.ToString().Substring(12);
                if (lang == Language.Chinese)
                {
                    switch (shpType)
                    {
                        case "Point":
                            shpType = "点";
                            break;
                        case "Polyline":
                            shpType = "线";
                            break;
                        case "Polygon":
                            shpType = "面";
                            break;
                    }
                }

                for (int i = 0; i < row.Fields.FieldCount; i++)
                {
                    if (i == shpIndex)
                        rtbAttribute.AppendText(feature.Fields.get_Field(i).Name + "*：" + shpType + "\n");
                    else
                        rtbAttribute.AppendText(row.Fields.get_Field(i).Name + "：" + row.get_Value(i).ToString() + "\n");
                }
            }
            else if (layer is IRasterLayer)
            {
                if (lang == Language.Chinese)
                    txtLayer.Text = layer.Name + "（栅格图层）";
                else if (lang == Language.English)
                    txtLayer.Text = layer.Name + "(Raster Layer)";
                string[] values = new string[1];
                if (pixel.MapTip.Contains(","))
                {
                    values = pixel.MapTip.Split(',');
                }
                else
                    values[0] = pixel.MapTip;
                for (int i = 0; i < values.Length; i++)
                {
                    if (i != 0)
                    {
                        if (values[i].Contains("="))
                            rtbAttribute.AppendText(values[i].Replace('=', '：').Substring(1) + "\n");
                        else
                            rtbAttribute.AppendText(values[i] + "\n");
                    }
                    else
                    {
                        if (values[i].Contains("="))
                            rtbAttribute.AppendText(values[i].Replace('=', '：') + "\n");
                        else
                            rtbAttribute.AppendText(values[i] + "\n");
                    }
                    
                }
            }
        }
    }
}
