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
    public partial class CreateProject : Form
    {
        /// <summary>
        /// 工程文件
        /// </summary>
        public Project project;
        /// <summary>
        /// 语言系统
        /// </summary>
        private Language lang;

        public CreateProject(Language lang)
        {
            InitializeComponent();
            this.lang = lang;
            if (lang == Language.English)
            {
                this.Width = 350;
                this.Text = "Create Project";
                btCancel.Text = "Cancel";
                btEnsure.Text = "Confirm";
                cbDataType.Items.Clear();
                cbDataType.Items.AddRange(new object[2]{"image data","video data"});
                cbFeatureType.Items.Clear();
                cbFeatureType.Items.AddRange(new object[2] { "fine modeling", "rapid modeling" });
                label1.Text = "Data Type";
                label2.Text = "Modeling Method";
                label3.Text = "Save Path";
                cbDataType.Location = new Point(128, cbDataType.Location.Y);
                cbFeatureType.Location = new Point(128, cbFeatureType.Location.Y);
                txtPath.Location = new Point(128, txtPath.Location.Y);
                btGetPath.Location = new Point(282, btGetPath.Location.Y);
                btEnsure.Location = new Point(238, btEnsure.Location.Y);
            }
        }

        private void btEnsure_Click(object sender, EventArgs e)
        {
            if (cbDataType.SelectedIndex < 0||cbFeatureType.SelectedIndex<0||txtPath.Text=="")
            {
                if(lang==Language.Chinese)
                    MessageBox.Show("请正确设置工程信息！");
                else if(lang==Language.English)
                    MessageBox.Show("Please set the project information correctly!");

            }
            else
            {
                project = new Project(txtPath.Text, cbDataType.SelectedIndex,(FeatureType)cbFeatureType.SelectedIndex);
                this.Close();
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btGetPath_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            if (lang == Language.Chinese)
            {
                save.Title = "工程保存到";
                save.Filter = "MeasureMan工程文件|*.msm";
            }
            else if (lang == Language.English)
            {
                save.Title = "Save Project To";
                save.Filter = "MeasureMan Project File|*.msm";
            }
            if (save.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = save.FileName;
            }
        }
    }
}
