using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeasureMan
{
    public partial class DSMTransformation : Form
    {
        /// <summary>
        /// 是否设置成功
        /// </summary>
        public bool succeed;
        /// <summary>
        /// DSM保存路径
        /// </summary>
        public string path;
        /// <summary>
        /// 插值方法
        /// </summary>
        public string method;
        /// <summary>
        /// 配置文件路径
        /// </summary>
        public string cfgPath;
        /// <summary>
        /// ODM文件路径
        /// </summary>
        public string ODMPath;
        /// <summary>
        /// 像元大小
        /// </summary>
        public double pixelSize;
        /// <summary>
        /// 输入稠密点云路径
        /// </summary>
        public string inputPath;
        /// <summary>
        /// 投影编号
        /// </summary>
        private int proCode;
        /// <summary>
        /// 是否检测到AE
        /// </summary>
        private bool hasAE;
        /// <summary>
        /// 语言系统
        /// </summary>
        private Language lang;

        /// <summary>
        /// DSM转换界面初始化方法
        /// </summary>
        /// <param name="denseCloud">稠密点云路径</param>
        /// <param name="proCode">投影编号</param>
        /// <param name="hasAE">是否检测到AE</param>
        public DSMTransformation(string denseCloud, int proCode,bool hasAE,Language lang)
        {
            InitializeComponent();
            this.lang = lang;
            this.hasAE = hasAE;
            if (lang == Language.English)
            {
                this.Text = "DSM Transformation";
                btCancel.Text = "Cancel";
                btEnsure.Text = "Do";
                btExportPath.Text = "Export Path";
                cbxWeightFunc.Items[0] = "none";
                label1.Text = "Search Radius";
                label2.Text = "Neighborhood Size";
                label3.Text = "Interpolation";
                label4.Text = "IDWF";
                lbGridSize.Text = "Pixel Size (m*m)";
                txtGridSize.Location = new System.Drawing.Point(120, txtGridSize.Location.Y);
                txtSearchR.Location = new System.Drawing.Point(120, txtSearchR.Location.Y);
                txtNeighbor.Location = new System.Drawing.Point(120, txtNeighbor.Location.Y);
                cbxInterpol.Location = new System.Drawing.Point(310, cbxInterpol.Location.Y);
                cbxWeightFunc.Location = new System.Drawing.Point(342, cbxWeightFunc.Location.Y);
                btEnsure.Location = new System.Drawing.Point(370, btEnsure.Location.Y);
            }
            if(!hasAE)
                this.cbxInterpol.Items.RemoveAt(7);
            txtGridSize.Text = "1";
            txtNeighbor.Text = "8";
            txtSearchR.Text = "5";
            cbxInterpol.SelectedIndex = 2;
            cbxWeightFunc.SelectedIndex = 0;
            this.inputPath = denseCloud;
            this.proCode = proCode;
            succeed = false;
        }

        private void btEnsure_Click(object sender, EventArgs e)
        {
            if (ParasCheck())
            {
                method = cbxInterpol.Text;
                pixelSize = double.Parse(txtGridSize.Text);
                if (hasAE)
                {
                    if (cbxInterpol.SelectedIndex == 7)
                    {
                        if (lang == Language.Chinese && MessageBox.Show("当前插值方法较为耗时，是否使用？", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.No)
                            return;
                        else if (lang == Language.English && MessageBox.Show("The current interpolation method is time-consuming, whether to use？", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.No)
                            return;
                    }
                    else
                    {
                        if (lang == Language.Chinese && MessageBox.Show("当前插值方法使用后程序将重启，是否使用？", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.No)
                            return;
                        else if (lang == Language.English && MessageBox.Show("The program will restart after the current interpolation method is used, whether to use?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.No)
                            return;
                        WriteCfgFile();
                    }
                }
                else
                    WriteCfgFile();
                    
                succeed = true;
                this.Close();
            }
        }

        /// <summary>
        /// 参数检查
        /// </summary>
        /// <returns>true表示检查通过,false表示不通过</returns>
        private bool ParasCheck()
        {
            if (txtExportPath.Text == "")
            {
                if(lang==Language.Chinese)
                    MessageBox.Show("路径设置不能为空！");
                else if(lang==Language.English)
                    MessageBox.Show("The path cannot be empty!");
                return false;
            }
            if (!SortTool.IsFitNumber(txtGridSize.Text, 1, false))
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("栅格像元大小设置出错！");
                else if (lang == Language.English)
                    MessageBox.Show("Pixel size error!");
                return false;
            }
            if ((!txtNeighbor.ReadOnly)&&(!SortTool.IsFitNumber(txtNeighbor.Text, 1, false,true)))
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("像元邻域大小只能为正整数！");
                else if (lang == Language.English)
                    MessageBox.Show("The neighborhood size can only be a positive integer!");
                return false;
            }
            if ((!txtSearchR.ReadOnly)&&(!SortTool.IsFitNumber(txtSearchR.Text, 1, false,true)))
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("搜索半径只能为正整数！");
                else if (lang == Language.English)
                    MessageBox.Show("The search radius can only be positive integer!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 翻译加权函数
        /// </summary>
        /// <returns>翻译结果</returns>
        private string ParaWeightFuncTrans()
        {
            switch (cbxWeightFunc.SelectedIndex)
            {
                case 0: return "#weightFunc = noSet";
                case 1: return "weightFunc = IDW1";
                case 2: return "weightFunc = IDW2";
                case 3: return "weightFunc = IDW3";
            }
            return null;
        }

        /// <summary>
        /// 创建配置文件
        /// </summary>
        /// <returns>true表示创建成功，false表示未创建成功</returns>
        private bool WriteCfgFile()
        {
            try
            {
                string dsmDir=System.IO.Path.GetDirectoryName(txtExportPath.Text);
                ODMPath = dsmDir + "\\tempODM.odm";
                cfgPath = dsmDir + "\\settings.cfg";
                string path2 = dsmDir + "\\denseCloud.xyz";
                PointCloudTool.Convert2XYZ(inputPath,path2);
                inputPath = path2;

                StreamWriter sw = new StreamWriter(cfgPath);
                sw.WriteLine("# The DSM CfgFile\n");
                sw.WriteLine("# for all modules");
                sw.WriteLine("screenLogLevel = none");
                sw.WriteLine("fileLogLevel = none");
                if (proCode != -1)
                    sw.WriteLine("coord_system = WKT:PROJCS[\"" + GetUTMName() + "\",AUTHORITY[\"EPSG\",\"" + proCode + "\"]]");
                sw.WriteLine("deleteEmptyOutFile = true\n");
                sw.WriteLine("[opalsImport]");
                sw.WriteLine("inFile = "+inputPath);
                sw.WriteLine("outFile = "+ODMPath+"\n");
                sw.WriteLine("[opalsGrid]");
                sw.WriteLine("inFile = "+ODMPath);
                sw.WriteLine("outFile = " + txtExportPath.Text);
                sw.WriteLine("interpolation = " + cbxInterpol.Text);
                sw.WriteLine("gridSize = " + txtGridSize.Text);
                sw.WriteLine("neighbours = " + txtNeighbor.Text);
                sw.WriteLine("searchRadius = " + txtSearchR.Text);
                sw.WriteLine(ParaWeightFuncTrans());
                sw.Flush();
                sw.Close();
                return true;
            }
            catch
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("请检查文件是否占用！");
                else if (lang == Language.English)
                    MessageBox.Show("Please check if the file is occupied!");
                return false;
            }
        }

        /// <summary>
        /// 根据投影坐标系编号获得坐标系名称
        /// </summary>
        /// <returns>坐标系名称</returns>
        public string GetUTMName()
        {
            if (proCode == -1)
                return "Unknown";
            string name = "WGS_1984_UTM_Zone_";
            int strip = proCode % 100;
            name += strip.ToString("00");
            int lat = proCode % 1000;
            if (lat >= 700)
                name += "S";
            else
                name += "N";
            return name;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            succeed = false;
            this.Close();
        }

        private void cbxInterpol_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxInterpol.SelectedIndex == 7)
            {
                txtNeighbor.ReadOnly = true;
                txtSearchR.ReadOnly = true;
                txtNeighbor.Text = "";
                txtSearchR.Text = "";
                cbxWeightFunc.SelectedIndex = 0;
                cbxWeightFunc.Enabled = false;
            }
            else
            {
                txtNeighbor.ReadOnly = false;
                txtSearchR.ReadOnly = false;
                cbxWeightFunc.Enabled = true;
            }
        }

        private void btExportPath_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            if (lang == Language.Chinese)
            {
                save.Filter = "tif数据|*.tif";
                save.Title = "DSM保存到";
            }
            else if (lang == Language.English)
            {
                save.Filter = "TIF Data|*.tif";
                save.Title = "Save DSM To";
            }
            if (save.ShowDialog() == DialogResult.OK)
            {
                path = save.FileName;
                txtExportPath.Text = save.FileName;
            }
        }
    }
}
