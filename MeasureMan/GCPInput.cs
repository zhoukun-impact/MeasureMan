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
    public partial class GCPInput : Form
    {
        /// <summary>
        /// 控制点
        /// </summary>
        public GCP gcp;
        /// <summary>
        /// 语言系统
        /// </summary>
        private Language lang;

        public GCPInput(Language lang)
        {
            InitializeComponent();
            this.lang = lang;
            if (lang == Language.English)
            {
                this.Text = "GCP Input";
                btCancel.Text = "Cancel";
                btEnsure.Text = "Confirm";
                label1.Text = "Longitude";
                label4.Text = "Latitude";
                label6.Text = "Elevation";
                this.Width = 324;
                txtLng.Location = new Point(124, txtLng.Location.Y);
                txtLat.Location = new Point(124, txtLat.Location.Y);
                txtElevation.Location = new Point(124, txtElevation.Location.Y);
                btEnsure.Location = new Point(199, btEnsure.Location.Y);
                label2.Location = new Point(263, label2.Location.Y);
                label3.Location = new Point(263, label3.Location.Y);
                label5.Location = new Point(263, label5.Location.Y);
            }
        }

        private void btEnsure_Click(object sender, EventArgs e)
        {
            if (SortTool.IsFitNumber(txtLng.Text) && SortTool.IsFitNumber(txtLat.Text) && SortTool.IsFitNumber(txtElevation.Text))
            {
                double lng = double.Parse(txtLng.Text);
                double lat = double.Parse(txtLat.Text);
                if (Math.Abs(lng) > 180)
                {
                    if (lang == Language.Chinese)
                        MessageBox.Show("经度绝对值不大于180！");
                    else if (lang == Language.English)
                        MessageBox.Show("The absolute longitude is no more than 180!");
                    return;
                }
                if (Math.Abs(lat) > 90)
                {
                    if (lang == Language.Chinese)
                        MessageBox.Show("纬度绝对值不大于90！");
                    else if (lang == Language.English)
                        MessageBox.Show("The absolute latitude is no more than 90!");
                    return;
                }
                double elevation = double.Parse(txtElevation.Text);
                gcp = new GCP(lng, lat, elevation);
                this.Close();
            }
            else
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("坐标都要为数字！");
                else if (lang == Language.English)
                    MessageBox.Show("All the coordinates must be numbers!");
            }
                
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
