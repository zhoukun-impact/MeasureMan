using Emgu.CV;
using Emgu.CV.Structure;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using System.Data.OleDb;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using ESRI.ArcGIS.DataSourcesFile;
using MetadataExtractor;
using System.Threading;
using CSharpGL;

namespace MeasureMan
{
    /// <summary>
    /// 语言系统
    /// </summary>
    public enum Language
    {
        /// <summary>
        /// 中文简体
        /// </summary>
        Chinese = 0,
        /// <summary>
        /// 英语
        /// </summary>
        English = 1
    }

    public partial class MeasureMan : Form
    {
        /// <summary>
        /// 加载的图片集
        /// </summary>
        List<AddedImage> images;
        /// <summary>
        /// 视频读写工具
        /// </summary>
        VideoIO videoIO;
        /// <summary>
        /// 加载的视频数据
        /// </summary>
        AddedVideo video;
        /// <summary>
        /// 系统正打开的工程
        /// </summary>
        Project project;
        /// <summary>
        /// 用于记录处理过程的处理时长
        /// </summary>
        System.Diagnostics.Stopwatch watch;
        /// <summary>
        /// 用于显示标识结果窗口
        /// </summary>
        Identify identify;
        /// <summary>
        /// 添加的控制点信息
        /// </summary>
        DataGridView GCPsView;
        /// <summary>
        /// 图像裁剪范围的第一个点
        /// </summary>
        System.Drawing.Point rectFirPoint;
        /// <summary>
        /// 地图结果显示的POS图层
        /// </summary>
        List<IFeatureLayer> layers;
        /// <summary>
        ///  地图结果显示的GCP图层
        /// </summary>
        IFeatureLayer layer;
        /// <summary>
        /// 相机方向
        /// </summary>
        CameraDirection cameraDirection;
        /// <summary>
        /// 模型外包盒
        /// </summary>
        BBox box;
        /// <summary>
        /// 三维视图
        /// </summary>
        CSharpGL.Scene scene;
        /// <summary>
        /// 控制视图旋转
        /// </summary>
        ArcBallManipulater ball;
        /// <summary>
        /// 控制视图平移
        /// </summary>
        TranslateManipulater tran;
        /// <summary>
        /// 操作集
        /// </summary>
        ActionList actionList;
        /// <summary>
        /// 特指三维模型
        /// </summary>
        Model3DNode model;
        /// <summary>
        /// 选择操作
        /// </summary>
        Picking pickingAction;
        /// <summary>
        /// 被选中的几何图形
        /// </summary>
        PickedGeometry pickedGeometry;
        /// <summary>
        /// 高亮点
        /// </summary>
        LegacyPointNode highlightPt;
        /// <summary>
        /// 选中的模型点
        /// </summary>
        List<vec3> modelPts;
        /// <summary>
        /// 面积测量结果
        /// </summary>
        double area;
        /// <summary>
        /// 选中的窗体点（用于绘制）
        /// </summary>
        List<vec3> windowPts;
        /// <summary>
        /// 长度测量结果
        /// </summary>
        double length;
        /// <summary>
        /// 量测类型
        /// </summary>
        MeasureType measure;
        /// <summary>
        /// 地图点大小
        /// </summary>
        double pointScale;
        /// <summary>
        /// 地图范围
        /// </summary>
        BoundingBox bbox;
        /// <summary>
        /// 是否检测到AE
        /// </summary>
        bool hasAE;
        /// <summary>
        /// 当前语言系统
        /// </summary>
        Language lang;
        /// <summary>
        /// 最近打开文件
        /// </summary>
        List<string> latestPaths;
        
        public MeasureMan(string proPath,Language lang)
        {
            this.lang = lang;
            hasAE = false;
            try
            {
                ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.Engine);
                ESRI.ArcGIS.RuntimeManager.BindLicense(ESRI.ArcGIS.ProductCode.Engine);
                hasAE = true;
            }
            catch(Exception e)
            {
                if (e.HResult == -2146233052)
                {
                    if (lang == Language.Chinese)
                        MessageBox.Show("未找到Arcgis Engine运行时，将自动切换到无AE模式！");
                    else if (lang == Language.English)
                        MessageBox.Show("ArcGIS Engine Runtime is not found!");
                }
                else if (e.HResult == -2146233088)
                {
                    if (lang == Language.Chinese)
                        MessageBox.Show("未打开ArcGIS许可服务，将自动切换到无AE模式！");
                    else if (lang == Language.English)
                        MessageBox.Show("ArcGIS license service is not open!");
                }
            }
            InitializeComponent();
            InitLanguage();
            if (hasAE)
            {
                if (lang == Language.Chinese)
                    axMapControl1.Map.Name = "图层";
                else if (lang == Language.English)
                    axMapControl1.Map.Name = "Layers";
                axTOCControl1.SetBuddyControl(axMapControl1);
                axToolbarControl1.SetBuddyControl(axMapControl1);
                axTOCControl1.EnableLayerDragDrop = true; 
            }   

            StreamWriter sw = new StreamWriter(Application.StartupPath+"\\Doc\\language.config");
            sw.WriteLine(lang.ToString());
            sw.WriteLine("false");
            sw.Flush();
            sw.Close();
            watch = new System.Diagnostics.Stopwatch();
            添加图像数据ToolStripMenuItem.Enabled = false;
            关键帧提取ToolStripMenuItem.Enabled = false;
            特征点检测ToolStripMenuItem1.Enabled = false;
            特征点匹配ToolStripMenuItem1.Enabled = false;
            稀疏重建ToolStripMenuItem1.Enabled = false;
            稠密重建ToolStripMenuItem.Enabled = false;
            dSM转换ToolStripMenuItem.Enabled = false;
            表面重建ToolStripMenuItem1.Enabled = false;
            this.AllowDrop = true;
            this.DragEnter+=MeasureMan_DragEnter;
            this.DragDrop+=MeasureMan_DragDrop;
            InitiateGLWindow();
            imageBox1.Cursor = Cursors.Cross;
            progressBar1.Visible = false;
            ReadLatestProjects();
            if (proPath != null)
            {
                if (OpenProject(proPath)&&hasAE)
                {
                    ShowGCPLayer();
                    ShowRoute();
                }
            }        
        }

        #region 帮助模块
        private void 查看帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Application.StartupPath + "\\Doc\\help.chm");
        }

        private void 作者ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutAuthor form = new AboutAuthor(lang);
            form.ShowDialog();
        }

        private void 中文简体ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            中文简体ToolStripMenuItem.Enabled = false;
            英语ToolStripMenuItem.Enabled = true;
            lang = Language.Chinese;
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\Doc\\language.config");
            sw.WriteLine("Chinese");
            sw.WriteLine("true");
            sw.Flush();
            sw.Close();
            Application.Restart();
        }

        private void 英语ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            中文简体ToolStripMenuItem.Enabled = true;
            英语ToolStripMenuItem.Enabled = false;
            lang = Language.English;
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\Doc\\language.config");
            sw.WriteLine("English");
            sw.WriteLine("true");
            sw.Flush();
            sw.Close();
            Application.Restart();
        }

        /// <summary>
        /// 初始化语言系统
        /// </summary>
        private void InitLanguage()
        {
            if (lang == Language.Chinese)
                中文简体ToolStripMenuItem.Enabled = false;
            else if (lang == Language.English)
            {
                最近打开ToolStripMenuItem.Text = "Latest Project";
                英语ToolStripMenuItem.Enabled = false;
                btAddLog.Text = "Import";
                btClearLog.Text = "Clear";
                btCreatePro.Text = "Create Project";
                btEnd.Text = "Bottom Picture";
                btExit.Text = "Exit";
                btFeaturePtsTrack.Text = "Feature Point Tracking";
                btGetArea.Text = "Area Measurement";
                btGetLength.Text = "Length Measurement";
                btLast.Text = "Last Picture";
                btNext.Text = "Next Picture";
                btOpenPro.Text = "Open Project";
                btOutputLog.Text = "Export";
                btPickPoint.Text = "Point Query";
                btRefresh.Text = "Flush";
                btSavePro.Text = "Save Project";
                btSearch.Text = "Search";
                btStart.Text = "Head Picture";
                dSM转换ToolStripMenuItem.Text = "DSM Transformation";
                Play.Text = "Add Video";
                tabPage1.Text = "Data View";
                tabPage3.Text = "Result View";
                tabPage4.Text = "Project Directory";
                tabPage5.Text = "Log Window";
                tabPage6.Text = "Image Result";
                tabPage7.Text = "3D Result";
                tabPage9.Text = "Map Result";
                toolStripDropDownButton1.Text = "Toolbox";
                toolStripDropDownButton3.Text = "POS";
                toolStripDropDownButton4.Text = "Camera";
                toolStripLabel1.Text = "Name";
                toolStripLabel2.Text = "Creation Time";
                toolStripLabel3.Text = "Project Path";
                toolStripLabel4.Text = "Vertex";
                toolStripLabel5.Text = "Face";
                toolStripLabel6.Text = "Model Path";
                ddbView.Text = "Camera";
                ddbView.ToolTipText = "Camera";
                topToolStripMenuItem.Text = "Top View";
                bottomToolStripMenuItem.Text = "Bottom View";
                leftToolStripMenuItem.Text = "Left View";
                rightToolStripMenuItem.Text = "Right View";
                frontToolStripMenuItem.Text = "Front View";
                backToolStripMenuItem.Text = "Back View";
                fullToolStripMenuItem.Text = "Full View";
                if (hasAE)
                {
                    toolStripStatusLabel1.Text = "Coord. X:";
                    toolStripStatusLabel3.Text = "Coord. Y:";
                    toolStripStatusLabel5.Text = "Coord. System:";
                }
                else
                {

                    toolStripStatusLabel9.Text = "Longitude:";
                    toolStripStatusLabel11.Text = "Latitude:";
                }
                按名称添加ToolStripMenuItem.Text = "Add By Name";
                按时间添加ToolStripMenuItem.Text = "Add By Time";
                帮助ToolStripMenuItem.Text = "Help";
                表面重建ToolStripMenuItem1.Text = "Surface Reconstruction";
                查看帮助ToolStripMenuItem.Text = "View Help";
                稠密重建ToolStripMenuItem.Text = "Dense Reconstruction";
                产品ToolStripMenuItem.Text = "Product";
                处理日志窗口ToolStripMenuItem.Text = "Log Window";
                地图结果ToolStripMenuItem.Text = "Map Result";
                翻滚角ToolStripMenuItem.Text = "Roll:";
                分级渲染ToolStripMenuItem.Text = "Render";
                俯仰角ToolStripMenuItem.Text = "Pitch:";
                高程ToolStripMenuItem.Text = "Elevation:";
                工程目录ToolStripMenuItem.Text = "Project Directory";
                关键帧提取ToolStripMenuItem.Text = "Video";
                焦距ToolStripMenuItem.Text = "Focal Length(pixel):";
                经度ToolStripMenuItem.Text = "Longitude:";
                稀疏重建ToolStripMenuItem1.Text = "Sparse Reconstruction";
                偏航角ToolStripMenuItem.Text = "Yaw:";
                三维结果ToolStripMenuItem.Text = "3D Result";
                重建ToolStripMenuItem.Text = "Reconstruction";
                视图ToolStripMenuItem.Text = "View";
                数据视图ToolStripMenuItem.Text = "Data View";
                特征点检测ToolStripMenuItem1.Text = "Feature Point Detection";
                特征点匹配ToolStripMenuItem1.Text = "Feature Point Matching";
                添加POSToolStripMenuItem.Text = "Add POS";
                添加控制点ToolStripMenuItem.Text = "Add GCP";
                添加图像数据ToolStripMenuItem.Text = "Image";
                图像裁剪ToolStripMenuItem.Text = "Clip Image";
                图像结果ToolStripMenuItem1.Text = "Image Result";
                纬度ToolStripMenuItem.Text = "Latitude:";
                文件ToolStripMenuItem.Text = "File";
                相机型号ToolStripMenuItem.Text = "Camera Type:";
                移除图像ToolStripMenuItem.Text = "Remove Image";
                栅格还原ToolStripMenuItem.Text = "Restore";
                主点坐标ToolStripMenuItem.Text = "Principal Point(pixel):";
                作者ToolStripMenuItem.Text = "About Author";
                语言设置ToolStripMenuItem.Text = "Language Setting";
                中文简体ToolStripMenuItem.Text = "Chinese";
                英语ToolStripMenuItem.Text = "English";
            }
        }
        #endregion

        #region 多线程控件操作
        private delegate void imgBoxDelegate(AddedImage image);
        private delegate void treeDelegate();
        private delegate void pbDelegate(int value);
        private delegate void rtbDelegate(string text);
        private delegate void tabDelegate(int selectedIndex1, int selectedIndex3);

        /// <summary>
        /// 多线程设置进度条的值
        /// </summary>
        /// <param name="value">值</param>
        private void SetPbValue(int value)
        {
            if (progressBar1.InvokeRequired)
                Invoke(new pbDelegate(SetPbValue), new object[] { value });
            else
            {
                if (value != -1)
                    progressBar1.Value = value;
                else
                    progressBar1.Visible = false;
            }
        }

        /// <summary>
        /// 多线程添加日志
        /// </summary>
        /// <param name="text">文本</param>
        private void AppendText(string text)
        {
            if (rtbLog.InvokeRequired)
                Invoke(new rtbDelegate(AppendText), new object[] { text });
            else
                rtbLog.AppendText(text);
        }

        /// <summary>
        /// 改变窗口
        /// </summary>
        /// <param name="selectedIndex1">数据视图/结果视图索引</param>
        /// <param name="selectedIndex3">图像结果/三维结果/地图结果视图索引</param>
        private void ChangeTab(int selectedIndex1, int selectedIndex3)
        {
            if (tabControl1.InvokeRequired)
                Invoke(new tabDelegate(ChangeTab), new object[] { selectedIndex1, selectedIndex3 });
            else
                tabControl1.SelectedIndex = selectedIndex1;
            if (tabControl3.InvokeRequired)
                Invoke(new tabDelegate(ChangeTab), new object[] { selectedIndex1, selectedIndex3 });
            else
                tabControl3.SelectedIndex = selectedIndex3;
        }
        #endregion

        #region 添加图像数据
        private void 按时间添加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddImages(false);
        }

        private void 按名称添加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddImages(true);
        }

        /// <summary>
        /// 添加多幅图像
        /// </summary>
        /// <param name="type">=false表示按时间添加，=true表示按名称添加</param>
        private void AddImages(bool type)
        {
            FolderBrowserDialog openFileDialog = new FolderBrowserDialog();
            if (lang == Language.Chinese)
                openFileDialog.Description = "选择需要添加图片的文件夹";
            else if (lang == Language.English)
                openFileDialog.Description = "Select a folder to add images";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                watch.Restart();
                imageBox1.Image = null;
                images = new List<AddedImage>();
                DirectoryInfo theFolder = new DirectoryInfo(openFileDialog.SelectedPath);
                FileInfo[] files = theFolder.GetFiles();
                List<FileInfo> filteredFiles = new List<FileInfo>();
                foreach (FileInfo file in files)
                {
                    string ext = file.Extension.ToUpper();
                    if (ext == ".JPG" || ext == ".PNG" || ext == ".JPEG")
                    {
                        images.Add(new AddedImage(file.Name, file.LastWriteTime, 0, file.FullName));
                    }
                }
                if (images.Count < 2)
                {
                    if (lang == Language.Chinese)
                        MessageBox.Show("图像数据过少，无法添加！");
                    else if (lang == Language.English)
                        MessageBox.Show("Images are too few to add!");
                    return;
                }
                CheckImages(images);
                try
                {
                    if (type)
                        SortTool.SortByName(images);
                    else
                        SortTool.SortByTime(images);
                }
                catch
                {
                    if (lang == Language.Chinese&&MessageBox.Show("图像命名非\"1.jpg\"的形式，不适合按名称添加，是否选择按时间添加？", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        type = false;
                        SortTool.SortByTime(images);
                    }
                    else if (lang == Language.English && MessageBox.Show("Image name is not like \"1.jpg\", not suitable for adding by name, whether to add by time?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        type = false;
                        SortTool.SortByTime(images);
                    }
                    else
                        return;
                }
                if (lang == Language.Chinese)
                    rtbLog.AppendText("- - - -图像数据添加- - - -\n");
                else if (lang == Language.English)
                    rtbLog.AppendText("- - - -Add Image Data- - - -\n");
                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = images.Count+1;
                progressBar1.Visible = true;
                Thread thread1 = new Thread(new ParameterizedThreadStart(DealWithImages));
                thread1.IsBackground = true;
                thread1.Start(type);
            }
        }

        /// <summary>
        /// 图像预处理
        /// </summary>
        /// <param name="type">图像添加方式</param>
        private void DealWithImages(object type)
        {
            int current = 0;
            for (int i = 0; i < images.Count; i++)
                images[i].order = i + 1;
            //判断是否有POS的GPS数据，有则自动添加
            if (lang == Language.Chinese)
                AppendText("正在检查图像自带的GPS数据...\n");
            else if (lang == Language.English)
                AppendText("Checking GPS data...\n");
            if (AutoAddGPS(images))
            {
                if (lang == Language.Chinese)
                    AppendText("已检测到自带GPS数据,正在绘制POS航线...\n");
                else if (lang == Language.English)
                    AppendText("GPS data detected, drawing POS route...\n");
                DrawRoute();
                ChangeTab(1, 2);
            }
            else
            {
                if (lang == Language.Chinese)
                    AppendText("未检测到自带GPS数据，需利用工具箱添加POS\n");
                else if (lang == Language.English)
                    AppendText("No GPS data detected, should Add POS with the toolbox\n");
            }
                
            current++;
            SetPbValue(current);
            if (lang == Language.Chinese)
                AppendText("正在进行图像降采样...\n");
            else if (lang == Language.English)
                AppendText("Image downsampling...\n");
            string dir = System.IO.Path.GetDirectoryName(project.path) + "\\img";
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            foreach (AddedImage img in images)
            {
                Image<Bgr, byte> image = new Image<Bgr, byte>(img.path);
                string[] info=img.name.Split('.');
                if ((!info[1].Equals("jpg")) && (!info[1].Equals("JPG")))
                    img.name = info[0] + ".jpg";
                if (img.name.Contains(" "))
                    img.name = img.name.Replace(" ", "");
                if (image.Width < 2000 && image.Height < 2000)//尺寸过小便不降采样
                {
                    image.Save(dir + "\\" + img.name);
                    image.Dispose();
                }
                else
                {
                    float ratio;
                    if (image.Width > image.Height)
                        ratio = 2000f / image.Width;
                    else
                        ratio = 2000f / image.Height;
                    Image<Bgr, byte> image2 = image.Resize(ratio, Inter.Area);
                    image.Dispose();
                    image2.Save(dir + "\\" + img.name);
                    image2.Dispose();
                    img.camera.ModifyParamrters(ratio);
                }
                img.path = dir + "\\" + img.name;
                current++;
                SetPbValue(current);
            }    
            project.images = images;
            ShowImageInfo(images[0]);
            watch.Stop();
            project.saved = false;
            ProjectToFile();
            BuildDirectoryTree();
            if (lang == Language.Chinese)
            {
                AppendText("图像数量：共" + images.Count + "张\n");
                if ((bool)type)
                    AppendText("图像添加方式：按名称排序添加\n");
                else
                    AppendText("图像添加方式：按时间排序添加\n");
                Clock();
                MessageBox.Show("图像添加成功！");
            }
            else if (lang == Language.English)
            {
                AppendText("Number of images:" + images.Count + " in total\n");
                if ((bool)type)
                    AppendText("The way to add images: by name\n");
                else
                    AppendText("The way to add images: by time\n");
                Clock();
                MessageBox.Show("Images were added successfully!");
            }
            添加图像数据ToolStripMenuItem.Enabled = false;
            特征点检测ToolStripMenuItem1.Enabled = true;
            SetPbValue(-1);
        }

        /// <summary>
        /// 检查添加的图像
        /// </summary>
        /// <param name="images">添加的图像</param>
        private void CheckImages(List<AddedImage> images)
        {
            string cameraName="unknown";
            double f=0,pixelSize=0;//mm/像素
            int imageWidth=0,imageHeight=0;
            for (int i = 0; i < images.Count; i++)
            {
                var rmd = ImageMetadataReader.ReadMetadata(images[i].path);
                foreach (var rd in rmd)
                {
                    if (rd.Name.Equals("JPEG"))
                    {
                        foreach (var tag in rd.Tags)
                        {
                            if (tag.Name.Equals("Image Width"))
                                imageWidth = int.Parse(tag.Description.Split(' ')[0]);
                            else if (tag.Name.Equals("Image Height"))
                                imageHeight = int.Parse(tag.Description.Split(' ')[0]);
                        }
                    }
                    else if (rd.Name.Equals("Exif IFD0"))
                    {
                        foreach (var tag in rd.Tags)
                        {
                            if (tag.Name.Equals("Model"))
                            {
                                cameraName = tag.Description;
                                break;
                            }
                        }
                    }
                    else if (rd.Name.Equals("Exif SubIFD"))
                    {
                        foreach (var tag in rd.Tags)
                        {
                            if (tag.Name.Equals("Focal Plane X Resolution"))
                            {
                                string[] info = tag.Description.Split(' ');
                                string[] info2 = info[0].Split('/');
                                pixelSize = double.Parse(info2[0]) * 25.4 / double.Parse(info2[1]);
                            }
                            else if (tag.Name.Equals("Focal Length"))
                                f = double.Parse(tag.Description.Split(' ')[0]);
                        }
                    }

                }

                if (cameraName.Equals("unknown") || pixelSize == 0)//无Exif信息或未知像元大小
                    f = (imageWidth > imageHeight ? imageWidth : imageHeight) * 1.2;
                else
                    f = f / pixelSize;
                images[i].camera = new CameraInfo(cameraName, f, imageWidth, imageHeight);
            }
        }

        #region 自动添加GPS
        /// <summary>
        /// 自动添加图像GPS数据，角元素默认为-361
        /// </summary>
        /// <param name="images">添加的图像</param>
        /// <returns>是否自动添加成功</returns>
        private bool AutoAddGPS(List<AddedImage> images)
        {
            List<List<double>> gpses = new List<List<double>>();
            foreach (AddedImage image in images)
            {
                List<double> gps = GetGPSofExif(image.path);
                if (gps == null)
                    return false;
                else
                    gpses.Add(gps);
            }
            for (int i = 0; i < images.Count; i++)
                images[i]._POS = new POS(gpses[i][0], gpses[i][1], gpses[i][2], -361, -361, -361);
            return true;
        }

        /// <summary>
        /// 获取图像Exif信息中的GPS数据
        /// </summary>
        /// <param name="imgPath">图像路径</param>
        /// <returns>纬度,经度,高程</returns>
        private List<double> GetGPSofExif(string imgPath)
        {
            var rmd = ImageMetadataReader.ReadMetadata(imgPath);
            List<double> gps = new List<double>();//添加元素必须为3个:纬度,经度,高程
            foreach (var rd in rmd)
            {
                if (rd.Name.Equals("GPS"))
                {
                    if (rd.Tags.Count != 7)
                        return null;
                    Dictionary<string, string> GPSInfo = new Dictionary<string, string>();
                    foreach (var tag in rd.Tags)
                        GPSInfo.Add(tag.Name, tag.Description);
                    gps.Add(ToDemicalDegree(GPSInfo["GPS Latitude Ref"], GPSInfo["GPS Latitude"]));
                    gps.Add(ToDemicalDegree(GPSInfo["GPS Longitude Ref"], GPSInfo["GPS Longitude"]));
                    gps.Add(double.Parse(GPSInfo["GPS Altitude"].Split(' ')[0]));
                    return gps;
                }
            }
            return null;
        }

        /// <summary>
        /// 将度分秒转化为带符号的十进制度
        /// </summary>
        /// <param name="reference">方向（N/E为正，S/W为负）</param>
        /// <param name="latOrLng">度分秒形式的纬度或经度</param>
        /// <returns>带符号的十进制度形式的纬度或经度</returns>
        private double ToDemicalDegree(string reference, string latOrLng)
        {
            string[] latOrLngInfo = latOrLng.Split(' ');
            double degree = double.Parse(latOrLngInfo[0].Substring(0, latOrLngInfo[0].Length - 1));
            double minute = double.Parse(latOrLngInfo[1].Substring(0, latOrLngInfo[1].Length - 1));
            double second = double.Parse(latOrLngInfo[2].Substring(0, latOrLngInfo[2].Length - 1));
            double demicalDegree = degree + minute / 60 + second / 3600;
            if (reference.Equals("S") || reference.Equals("W"))
                demicalDegree = -demicalDegree;
            return demicalDegree;
        }
        #endregion
        #endregion

        #region 添加视频数据
        #region 粗提取
        private void Play_Click(object sender, EventArgs e)
        {
            imageBox1.Image = null;
            tabControl1.SelectedIndex = 0;
            if (Play.Text == "添加视频数据" || Play.Text == "Add Video")
            {//开启播放模式
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (lang == Language.Chinese)
                {
                    openFileDialog.Title = "添加视频数据";
                    openFileDialog.Filter = "MP4文件|*.mp4|MOV文件|*.mov|AVI文件|*.avi|所有文件|*.*";
                }
                else if (lang == Language.English)
                {
                    openFileDialog.Title = "Add Video Data";
                    openFileDialog.Filter = "MP4 File|*.mp4|MOV File|*.mov|AVI File|*.avi|All File|*.*";
                }
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    videoIO = new VideoIO(new VideoCapture(openFileDialog.FileName));
                    video = videoIO.GetVideo();
                    Size si = video.resolution;
                    double fx = (si.Width > si.Height ? si.Width : si.Height) * 1.2;
                    project.camera = new CameraInfo("unknown", fx, si.Width, si.Height);

                    string savePath = System.IO.Path.GetDirectoryName(project.path) + "\\img";
                    if (!System.IO.Directory.Exists(savePath))
                        System.IO.Directory.CreateDirectory(savePath);
                    EvenKeyFrameSetting form4 = new EvenKeyFrameSetting(video,lang);
                    form4.ShowDialog();
                    if (form4.evenTool != null)
                    {
                        watch.Restart();
                        videoIO.evenTool = form4.evenTool;
                        videoIO.savePath = savePath;
                        if (videoIO.evenTool.clipMode == 1)
                            videoIO.limitFrame = videoIO.evenTool.endFrame;
                        images = new List<AddedImage>();
                    }
                    if (lang == Language.Chinese)
                        Play.Text = "停止";
                    else if (lang == Language.English)
                        Play.Text = "Stop";
                    imageBox1.Image = null;
                    Application.Idle += new EventHandler(ProcessFrame);
                }
            }
            else
            {
                videoIO.capture.Dispose();
                Application.Idle -= new EventHandler(ProcessFrame);
                if (lang == Language.Chinese)
                    Play.Text = "添加视频数据";
                else if (lang == Language.English)
                    Play.Text = "Add Video";
            }
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (Play.Text != "添加视频数据" && Play.Text != "Add Video")//正在播放视频
            {
                videoIO.frameCount++;
                if (videoIO.frameCount <= videoIO.limitFrame)//判断视频是否结束
                {
                    videoIO.frame = videoIO.capture.QueryFrame();
                    imageBox1.Image = videoIO.frame;
                    if (videoIO.evenTool != null)
                    {
                        if (videoIO.evenTool.clipMode == 1 || videoIO.evenTool.clipMode == 2)
                        {
                            if (videoIO.frameCount >= videoIO.evenTool.startFrame && videoIO.frameCount <= videoIO.evenTool.endFrame && (videoIO.frameCount - videoIO.evenTool.startFrame) % videoIO.evenTool.frameInterval == 0)
                            {
                                videoIO.frameOrder++;
                                videoIO.frame.Save(videoIO.savePath + "\\" + videoIO.frameOrder + ".jpg"); //帧保存
                                FileInfo file = new FileInfo(videoIO.savePath + "\\" + videoIO.frameOrder + ".jpg");
                                images.Add(new AddedImage(file.Name, file.LastWriteTime, videoIO.frameOrder, file.FullName,project.camera.Clone()));
                            }
                        }
                        else if (videoIO.evenTool.clipMode == 0)
                        {
                            if ((videoIO.frameCount < videoIO.evenTool.startFrame || videoIO.frameCount > videoIO.evenTool.endFrame) && ((videoIO.frameCount - 1) % videoIO.evenTool.frameInterval == 0 || (videoIO.frameCount - videoIO.evenTool.endFrame - 1) % videoIO.evenTool.frameInterval == 0))
                            {
                                videoIO.frameOrder++;
                                videoIO.frame.Save(videoIO.savePath + "\\" + videoIO.frameOrder + ".jpg"); //帧保存
                                FileInfo file = new FileInfo(videoIO.savePath + "\\" + videoIO.frameOrder + ".jpg");
                                images.Add(new AddedImage(file.Name, file.LastWriteTime, videoIO.frameOrder, file.FullName,project.camera.Clone()));
                            }
                        }
                    }
                    else
                    {
                        //为使播放顺畅，添加以下延时
                        System.Threading.Thread.Sleep((int)(1000.0 / video.videoFps + 20));
                    }
                }
                else
                {
                    if (lang == Language.Chinese)
                        Play.Text = "添加视频数据";
                    else if (lang == Language.English)
                        Play.Text = "Add Video";
                    imageBox1.Image = null;
                    tabControl1.SelectedIndex = 0;
                    project.images = images;
                    if (project.images != null && project.images.Count != 0)
                    {
                        ShowImageInfo(images[0]);
                        BuildDirectoryTree();
                        project.saved = false;
                        ProjectToFile();
                        watch.Stop();
                        if (lang == Language.Chinese)
                        {
                            rtbLog.AppendText("- - - -视频数据等距关键帧粗提取- - - -\n");
                            rtbLog.AppendText("粗提取关键帧数量：共" + images.Count + "帧\n");
                            rtbLog.AppendText("关键帧帧间隔：" + videoIO.evenTool.frameInterval + "\n");
                        }
                        else if (lang == Language.English)
                        {
                            rtbLog.AppendText("- - - -Even Key Frame Capture of Video Data- - - -\n");
                            rtbLog.AppendText("Key frame number:" + images.Count + "\n");
                            rtbLog.AppendText("Key frame interval:" + videoIO.evenTool.frameInterval + "\n");
                        }
                        Clock();
                        Play.Enabled = false;
                        btFeaturePtsTrack.Enabled = true;
                    }
                }
            }
        }
        #endregion

        #region 精提取
        private void btFeaturePtsTrack_Click(object sender, EventArgs e)
        {
            watch.Restart();
            images = new List<AddedImage>();
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;
            progressBar1.Visible = true;
            progressBar1.Maximum = project.images.Count + 5;
            关键帧提取ToolStripMenuItem.Enabled = false;
            if (lang == Language.Chinese)
            {
                rtbLog.AppendText("- - - 视频数据特征点追踪关键帧精提取- - -\n");
                rtbLog.AppendText("正在进行特征点追踪...\n");
            }
            else if (lang == Language.English)
            {
                rtbLog.AppendText("- - - Feature Point Tracking of Video Data- - -\n");
                rtbLog.AppendText("Tracking feature points...\n");
            }
            Thread thread1 = new Thread(new ThreadStart(FeatureTrack));
            thread1.IsBackground = true;
            thread1.Start();
        }

        /// <summary>
        /// 特征点追踪(基于航线的连续性)
        /// </summary>
        private void FeatureTrack()
        {
            int count = 1;
            int i = 0;
            for (; i < project.images.Count; i++)
            {
                project.images[i].order = count;
                images.Add(project.images[i]);
                Image<Gray, byte> preFrame = new Image<Gray, byte>(project.images[i].path);
                VectorOfKeyPoint featurePoints0 = new VectorOfKeyPoint();
                Mat descriptors0 = new Mat();
                FtPointAnalyseTool.OrbDetect(preFrame, featurePoints0, descriptors0, 1000);//利用快速检测的orb特征点
                for (int j = i + 1; j < project.images.Count; j++)
                {
                    Image<Gray, byte> currentFrame = new Image<Gray, byte>(project.images[j].path);
                    VectorOfKeyPoint featurePoints1 = new VectorOfKeyPoint();
                    Mat descriptors1 = new Mat();
                    int currentFrame_Width = currentFrame.Width;
                    FtPointAnalyseTool.OrbDetect(currentFrame, featurePoints1, descriptors1, 1000);
                    VectorOfVectorOfDMatch matches = FtPointAnalyseTool.TraditionalFtPointsMatch(descriptors0, descriptors1, DistanceType.Hamming);//汉明距离,每一个描述子有32个字节，每一个字节的每一位（0或1）不同则+1
                    Mat mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                    mask.SetTo(new MCvScalar(1));
                    FtPointAnalyseTool.VoteForRightMatchesByDis(matches, mask);
                    bool isChanged = FtPointAnalyseTool.AngleChange(featurePoints0, featurePoints1, matches, mask, currentFrame_Width, 10);
                    if (isChanged || j == project.images.Count - 1)
                    {
                        i = j - 1;
                        count++;
                        break;
                    }
                }
                SetPbValue(i + 1);
            }
            project.images = images;
            ChangeTab(0, 0);
            if (project.images.Count > 1)
            {
                Thread thread2 = new Thread(new ParameterizedThreadStart(DealWithFrames));
                thread2.IsBackground = true;
                thread2.Start(i);
            }
            else
            {
                watch.Stop();
                关键帧提取ToolStripMenuItem.Enabled = true;
                if (lang == Language.Chinese)
                {
                    AppendText("特征点追踪失败\n");
                    Clock();
                    MessageBox.Show("精提取关键帧数量<2，无法正常进行后续操作！");
                }
                else if (lang == Language.English)
                {
                    AppendText("Feature point tracking failed\n");
                    Clock();
                    MessageBox.Show("The number of key frames is <2 and the following operation cannot be carried out normally!");
                }
                SetPbValue(-1);
            }
        }

        /// <summary>
        /// 视频帧预处理
        /// </summary>
        /// <param name="cur">当前进度</param>
        private void DealWithFrames(object cur)
        {
            if (lang == Language.Chinese)
                AppendText("正在进行图像降采样...\n");
            else if (lang == Language.English)
                AppendText("Image downsampling...\n");
            foreach (AddedImage img in images)
            {
                Image<Bgr, byte> image = new Image<Bgr, byte>(img.path);
                if (image.Width < 2000&&image.Height < 2000)
                    image.Dispose();
                else
                {
                    float ratio;
                    if (image.Width > image.Height)
                        ratio = 2000f / image.Width;
                    else
                        ratio = 2000f / image.Height;
                    Image<Bgr, byte> image2 = image.Resize(ratio, Inter.Area);
                    image.Dispose();
                    File.Delete(img.path);
                    image2.Save(img.path);
                    image2.Dispose();
                    img.camera.ModifyParamrters(ratio);
                    FileInfo file = new FileInfo(img.path);
                    img.time = file.LastWriteTime;
                }
            }
            project.images = images;    
            ShowImageInfo(images[0]);
            SetPbValue((int)cur + 5);
            watch.Stop();
            ShowImageInfo(images[0]);
            BuildDirectoryTree();
            project.canTrack = false;
            project.saved = false;
            ProjectToFile();
            watch.Stop();
            if (lang == Language.Chinese)
            {
                AppendText("精提取关键帧数量：共" + images.Count + "帧\n");
                Clock();
                MessageBox.Show("特征点追踪成功！");
            }
            else if (lang == Language.English)
            {
                AppendText("Key frame number:" + images.Count + "\n");
                Clock();
                MessageBox.Show("Feature point tracking succeeded!");
            }
            btFeaturePtsTrack.Enabled = false;
            特征点检测ToolStripMenuItem1.Enabled = true;
            SetPbValue(-1);
        }
        #endregion
        #endregion

        #region 查看图像数据
        /// <summary>
        /// 显示图像信息。
        /// </summary>
        /// <param name="image">添加的图像</param>
        private void ShowImageInfo(AddedImage image)
        {
            if (imageBox1.InvokeRequired)
            {
                Invoke(new imgBoxDelegate(ShowImageInfo), new object[] { image });
            }
            else
            {
                if (imageBox1.Image != null)
                    imageBox1.Image.Dispose();
                imageBox1.Image = null;
                try
                {
                    Image<Bgr, byte> img = new Image<Bgr, byte>(image.path);
                    if (image.clipped)
                        img.Draw(image.ROI, new Bgr(0, 0, 255), img.Height / 240 + 1);
                    imageBox1.Image = img;
                }
                catch 
                {
                    if (lang == Language.Chinese)
                        MessageBox.Show("未找到当前需要打开的图像文件！");
                    else if (lang == Language.English)
                        MessageBox.Show("The current image file to open was not found!");
                    return; 
                }
                if (imageBox2.Image != null)
                    imageBox2.Image.Dispose();
                imageBox2.Image = null;
                txtImageOrder.Text = image.order.ToString();
                txtImageName.Text = image.name;
                txtImageTime.Text = image.time.ToString();
                if (image._POS != null)
                {
                    if (lang == Language.Chinese)
                    {
                        经度ToolStripMenuItem.Text = "经度：" + image._POS.x + "°";
                        纬度ToolStripMenuItem.Text = "纬度：" + image._POS.y + "°";
                        高程ToolStripMenuItem.Text = "高程：" + image._POS.z + "m";
                        俯仰角ToolStripMenuItem.Text = "俯仰角：" + image._POS.pitch + "°";
                        偏航角ToolStripMenuItem.Text = "偏航角：" + image._POS.yaw + "°";
                        翻滚角ToolStripMenuItem.Text = "翻滚角：" + image._POS.roll + "°";
                    }
                    else if (lang == Language.English)
                    {
                        经度ToolStripMenuItem.Text = "Longitude:" + image._POS.x + "°";
                        纬度ToolStripMenuItem.Text = "Latitude:" + image._POS.y + "°";
                        高程ToolStripMenuItem.Text = "Elevation:" + image._POS.z + "m";
                        俯仰角ToolStripMenuItem.Text = "Pitch:" + image._POS.pitch + "°";
                        偏航角ToolStripMenuItem.Text = "Yaw:" + image._POS.yaw + "°";
                        翻滚角ToolStripMenuItem.Text = "Roll:" + image._POS.roll + "°";
                    }
                }
                ShowCameraInfo(image.camera);
                if (GCPsView != null)
                    ShowGCPs();
                if (图像裁剪ToolStripMenuItem.Text.Equals("结束图像裁剪") || 图像裁剪ToolStripMenuItem.Text.Equals("End Image Clipping"))
                    rectFirPoint = System.Drawing.Point.Empty;
                tabControl1.SelectedIndex =0;
            }
        }

        /// <summary>
        /// 显示相机参数信息
        /// </summary>
        /// <param name="camera">图像的相机信息</param>
        private void ShowCameraInfo(CameraInfo camera)
        {
            if (lang == Language.Chinese)
            {
                相机型号ToolStripMenuItem.Text = "相机型号：";
                焦距ToolStripMenuItem.Text = "焦距（像素）：";
                主点坐标ToolStripMenuItem.Text = "主点坐标（像素）：";
            }
            else if (lang == Language.English)
            {
                相机型号ToolStripMenuItem.Text = "Camera Type:";
                焦距ToolStripMenuItem.Text = "Focal Length(pixel):";
                主点坐标ToolStripMenuItem.Text = "Principal Point(pixel):";
            }
            if (camera != null)
            {
                相机型号ToolStripMenuItem.Text +=camera.name;
                焦距ToolStripMenuItem.Text +=camera.focalLength;
                主点坐标ToolStripMenuItem.Text += "(" + camera.principalPoint.X + "," + camera.principalPoint.Y + ")";
            }
        }

        private void btLast_Click(object sender, EventArgs e)
        {
            if (imageBox1.Image==null||images==null||images.Count == 0)
                return;
            int image_order = int.Parse(txtImageOrder.Text);
            if (image_order == 1)
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("查看到头！");
                else if (lang == Language.English)
                    MessageBox.Show("Check to head picture!");
            }  
            else
            {
                ShowImageInfo(images[image_order-2]);
            }
        }

        private void btNext_Click(object sender, EventArgs e)
        {
            if (imageBox1.Image == null || images == null || images.Count == 0)
                return;
            int image_order = int.Parse(txtImageOrder.Text);
            if (image_order == images.Count)
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("查看到底！");
                else if (lang == Language.English)
                    MessageBox.Show("Check to bottom picture!");
            }
            else
            {
                ShowImageInfo(images[image_order]);
            }
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            if (imageBox1.Image == null || images == null || images.Count == 0)
                return;
            ShowImageInfo(images[0]);
        }

        private void btEnd_Click(object sender, EventArgs e)
        {
            if (imageBox1.Image == null || images == null || images.Count == 0)
                return;
            ShowImageInfo(images.Last());
        }
        #endregion

        #region 视图显示
        private void 工程目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 0;
        }

        private void 处理日志窗口ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 1;
        }

        private void 数据视图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 0;
        }

        private void 图像结果ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
            tabControl3.SelectedIndex = 0;
        }

        private void 三维结果ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
            tabControl3.SelectedIndex = 1;
        }

        private void 地图结果ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
            tabControl3.SelectedIndex = 2;
        }

        #endregion

        #region 日志模块
        private void btClearLog_Click(object sender, EventArgs e)
        {
            if (lang == Language.Chinese&&MessageBox.Show("是否要清空日志?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                rtbLog.Text = "";
            else if(lang == Language.English&&MessageBox.Show("Whether to clear the log?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                rtbLog.Text = "";
        }

        private void btOutputLog_Click(object sender, EventArgs e)
        {
            if (rtbLog.Text == "")
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("日志内容为空！");
                else if (lang == Language.English)
                    MessageBox.Show("The log is empty!");
                return;
            }
            SaveFileDialog save = new SaveFileDialog();
            if (lang == Language.Chinese)
            {
                save.Title = "日志存储为";
                save.Filter = "文本文件|*.txt";
            }
            else if (lang == Language.English)
            {
                save.Title = "Store Log As";
                save.Filter = "Text File|*.txt";
            }
            if (save.ShowDialog() == DialogResult.OK)
            {
                StreamWriter sw = new StreamWriter(save.FileName);
                string[] text = rtbLog.Text.Split('\n');
                foreach (string s in text)
                {
                    sw.WriteLine(s);
                }
                sw.Flush();
                sw.Close();
                if (lang == Language.Chinese)
                    MessageBox.Show("日志输出成功！");
                else if (lang == Language.English)
                    MessageBox.Show("The log was exported successfully!");
            }
        }

        private void btAddLog_Click(object sender, EventArgs e)
        {
            if ((lang == Language.Chinese&&MessageBox.Show("是否要用新日志覆盖当前日志?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)||
                (lang == Language.English && MessageBox.Show("Whether to overwrite the current log with a new log?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes))
            {
                OpenFileDialog open = new OpenFileDialog();
                if (lang == Language.Chinese)
                {
                    open.Filter = "日志文本|*.txt";
                    open.Title = "打开日志文件";
                }
                else if(lang==Language.English)
                {
                    open.Filter = "Log Text|*.txt";
                    open.Title = "Open Log File";
                }
                if (open.ShowDialog() == DialogResult.OK)
                {
                    rtbLog.Text="";
                    StreamReader sr = new StreamReader(open.FileName);
                    StringBuilder sb = new StringBuilder();
                    while (!sr.EndOfStream)
                    {
                        sb.Append(sr.ReadLine() + "*");
                    }
                    sr.Close();
                    string[] logs = sb.ToString().Split('*');
                    for (int i = 0; i < logs.Length - 1; i++)
                        rtbLog.AppendText(logs[i] + '\n');
                }
            }
        }
        #endregion

        #region 目录树模块
        /// <summary>
        /// 创建工程目录树，一般是在创建工程或打开工程后
        /// </summary>
        private void BuildDirectoryTree()
        {
            if (DirectoryTree.InvokeRequired)
                Invoke(new treeDelegate(BuildDirectoryTree));
            else
            {
                DirectoryTree.Nodes.Clear();
                if (project == null)
                {
                    toolStripTextBox1.Text = "";
                    return;
                } 
                toolStripTextBox1.Text = project.path;
                if (lang == Language.Chinese)
                {
                    DirectoryTree.Nodes.Add("图像数据");//图像数据视图
                    DirectoryTree.Nodes.Add("特征点集");//图像结果
                    DirectoryTree.Nodes.Add("匹配关系");//图像结果
                    DirectoryTree.Nodes.Add("辅助信息");//地图结果
                    DirectoryTree.Nodes.Add("重建点云");//模型在三维结果，DEM在地图结果
                    DirectoryTree.Nodes.Add("建模产品");//三维结果
                }
                else if (lang == Language.English)
                {
                    DirectoryTree.Nodes.Add("Image Data");
                    DirectoryTree.Nodes.Add("Feature Point");
                    DirectoryTree.Nodes.Add("Matching Relation");
                    DirectoryTree.Nodes.Add("Auxiliary Information");
                    DirectoryTree.Nodes.Add("Reconstructed Cloud");
                    DirectoryTree.Nodes.Add("Modeling Product");
                }
                if (project.images != null && project.images.Count != 0)
                {
                    foreach (AddedImage img in project.images)
                        DirectoryTree.Nodes[0].Nodes.Add(img.name);
                    if (project.ftPaths != null)
                    {
                        foreach (string ftPath in project.ftPaths)
                            DirectoryTree.Nodes[1].Nodes.Add(System.IO.Path.GetFileName(ftPath));
                    }
                    if (project.TVMs != null)
                    {
                        foreach (TwoViewModel model in project.TVMs)
                            DirectoryTree.Nodes[2].Nodes.Add(System.IO.Path.GetFileName(model.path));
                    }
                    if (lang == Language.Chinese)
                    {
                        if (project.images[0]._POS != null)
                            DirectoryTree.Nodes[3].Nodes.Add("POS航线");
                        if (project.GCPs != null && project.GCPs.Count != 0)
                            DirectoryTree.Nodes[3].Nodes.Add("控制点");
                        if (project.sparseCloud != null)
                            DirectoryTree.Nodes[4].Nodes.Add("稀疏点云");
                        if (project.denseCloud != null)
                            DirectoryTree.Nodes[4].Nodes.Add("稠密点云");
                        if (project.DSMPath != null)
                            DirectoryTree.Nodes[5].Nodes.Add("DSM");
                        if (project.modelPath != null)
                            DirectoryTree.Nodes[5].Nodes.Add("三维模型");
                    }
                    else if (lang == Language.English)
                    {
                        if (project.images[0]._POS != null)
                            DirectoryTree.Nodes[3].Nodes.Add("POS Route");
                        if (project.GCPs != null && project.GCPs.Count != 0)
                            DirectoryTree.Nodes[3].Nodes.Add("GCP");
                        if (project.sparseCloud != null)
                            DirectoryTree.Nodes[4].Nodes.Add("Sparse Cloud");
                        if (project.denseCloud != null)
                            DirectoryTree.Nodes[4].Nodes.Add("Dense Cloud");
                        if (project.DSMPath != null)
                            DirectoryTree.Nodes[5].Nodes.Add("DSM");
                        if (project.modelPath != null)
                            DirectoryTree.Nodes[5].Nodes.Add("3D Model");
                    }
                }
                DirectoryTree.ExpandAll();
                DirectoryTree.Refresh();
            }
        }

        private void btSearch_Click(object sender, EventArgs e)
        {
            if (txtSearch.Text == "")
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("请在搜索框内输入文件名称");
                else if (lang == Language.English)
                    MessageBox.Show("Please enter the file name in the search box");
                return;
            }
            if (project != null)
                BuildDirectoryTree();
            List<TreeNode> nodeList = new List<TreeNode>();
            SearchNode(DirectoryTree.Nodes, txtSearch.Text.ToUpper(), nodeList);
            if (nodeList.Count == 0)
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("未搜索到指定文件!");
                else if (lang == Language.English)
                    MessageBox.Show("The specified file was not searched!");
            }
            else
            {
                TreeNode[] parents = new TreeNode[DirectoryTree.Nodes.Count];
                List<TreeNode>[] childrenGroups = new List<TreeNode>[DirectoryTree.Nodes.Count];
                foreach (TreeNode node in nodeList)
                {
                    if (node.Parent == null)
                        parents[node.Index] = node;
                    else
                    {
                        if (childrenGroups[node.Parent.Index] == null)
                            childrenGroups[node.Parent.Index] = new List<TreeNode>();
                        childrenGroups[node.Parent.Index].Add(node);
                    }
                }
                DirectoryTree.Nodes.Clear();
                for (int i = 0; i < parents.Length; i++)
                {
                    if (parents[i] != null)
                        DirectoryTree.Nodes.Add(parents[i]);
                    else if (childrenGroups[i] != null)
                    {
                        TreeNode parent = childrenGroups[i][0].Parent;
                        parent.Nodes.Clear();
                        foreach (TreeNode child in childrenGroups[i])
                            parent.Nodes.Add(child);
                        DirectoryTree.Nodes.Add(parent);
                    }
                }
                DirectoryTree.ExpandAll();
                DirectoryTree.Refresh();
            }

        }

        /// <summary>
        /// 搜索文件结点，即叶节点
        /// </summary>
        /// <param name="nodes">搜索树结点</param>
        /// <param name="name">搜索名称大写形式</param>
        /// <param name="nodeList">保存搜索结果的结点列表</param>
        private void SearchNode(TreeNodeCollection nodes, string name, List<TreeNode> nodeList)
        {
            if (nodes.Count != 0)
            {
                foreach (TreeNode node in nodes)
                {
                    if (node.Text.ToUpper().Contains(name))
                        nodeList.Add(node);
                    else
                        SearchNode(node.Nodes, name, nodeList);
                }
            }
        }

        private void btRefresh_Click(object sender, EventArgs e)
        {
            BuildDirectoryTree();
        }

        private void DirectoryTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                string existInfo="";
                if (lang == Language.Chinese)
                    existInfo = "文件不存在！";
                else if(lang==Language.English)
                    existInfo = "The file does not exist!";
                if (e.Node.Parent == null || DirectoryTree.SelectedNode == null)//解决了双击父节点加号的错误
                    return;
                TreeNode treeNode = DirectoryTree.SelectedNode;
                if (treeNode.Parent.Text.Equals("图像数据") || treeNode.Parent.Text.Equals("Image Data"))//打开图像数据
                {
                    tabControl1.SelectedIndex = 0;
                    foreach (AddedImage img in images)
                    {
                        if (img.name.Equals(treeNode.Text))
                        {
                            ShowImageInfo(img);
                            break;
                        }
                    }
                }
                else
                {
                    if (GCPsView != null)
                    {
                        if (lang == Language.Chinese)
                            MessageBox.Show("当前处于控制点编辑状态！");
                        else if (lang == Language.English)
                            MessageBox.Show("Currently in editting state of GCPs!");
                        return;
                    }
                    if (图像裁剪ToolStripMenuItem.Text.Equals("结束图像裁剪") || 图像裁剪ToolStripMenuItem.Text.Equals("End Image Clipping"))
                    {
                        if (lang == Language.Chinese)
                            MessageBox.Show("当前处于图像裁剪模式！");
                        else if (lang == Language.English)
                            MessageBox.Show("Currently in image clipping mode!");
                        return;
                    }
                    if (treeNode.Parent.Text.Equals("特征点集") || treeNode.Parent.Text.Equals("Feature Point"))
                    {
                        if (imageBox1.Image != null)
                            imageBox1.Image.Dispose();
                        imageBox1.Image = null;
                        if (imageBox2.Image != null)
                            imageBox2.Image.Dispose();
                        imageBox2.Image = null;
                        string ftPath = System.IO.Path.GetDirectoryName(project.ftPaths[0]) + "\\" + treeNode.Text;
                        if (!File.Exists(ftPath))
                        {
                            MessageBox.Show(existInfo);
                            return;
                        }

                        VectorOfKeyPoint featurePoints = new VectorOfKeyPoint();
                        int index = int.Parse(treeNode.Text.Split('.')[0]) - 1;
                        if (project.ftType == FeatureType.SIFT)
                            FtPointAnalyseTool.LoadSiftFile(ftPath, featurePoints, null, project.images[index]);
                        else
                            FtPointAnalyseTool.LoadOrbFile(ftPath, featurePoints, null);
                        int featurePoints_size = featurePoints.Size;
                        imageBox2.Image = FtPointAnalyseTool.ShowFeaturePoints(project.images, treeNode.Text, featurePoints);
                        tabControl1.SelectedIndex = 1;
                        tabControl3.SelectedIndex = 0;
                        InitiateImageDataWindow();
                        if (lang == Language.Chinese)
                            MessageBox.Show("图像" + project.images[int.Parse(treeNode.Text.Split('.')[0]) - 1].name + "的特征点数量共有：" + featurePoints_size + "个");
                        else if (lang == Language.English)
                            MessageBox.Show("The number of feature points detected from image " + project.images[int.Parse(treeNode.Text.Split('.')[0]) - 1].name + ":" + featurePoints_size + " in total");
                    }
                    else if (treeNode.Parent.Text.Equals("匹配关系") || treeNode.Parent.Text.Equals("Matching Relation"))
                    {
                        if (imageBox1.Image != null)
                            imageBox1.Image.Dispose();
                        imageBox1.Image = null;
                        if (imageBox2.Image != null)
                            imageBox2.Image.Dispose();
                        imageBox2.Image = null;
                        string relPath = System.IO.Path.GetDirectoryName(project.path) + "\\rel\\" + treeNode.Text;
                        if (!File.Exists(relPath))
                        {
                            MessageBox.Show(existInfo);
                            return;
                        }

                        VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
                        int matchesCount=FtPointAnalyseTool.LoadRelFile(relPath, matches);
                        imageBox2.Image = FtPointAnalyseTool.ShowMatches(project, treeNode.Text, System.IO.Path.GetDirectoryName(project.ftPaths[0]), matches);
                        tabControl1.SelectedIndex = 1;
                        tabControl3.SelectedIndex = 0;
                        InitiateImageDataWindow();
                        string[] twoImages = treeNode.Text.Split(new char[2] { '-', '.' });
                        if (lang == Language.Chinese)
                            MessageBox.Show("图像" + project.images[int.Parse(twoImages[0]) - 1].name + "和图像" + project.images[int.Parse(twoImages[1]) - 1].name + "的匹配点对共：" + matchesCount + "对");
                        else if (lang == Language.English)
                            MessageBox.Show("The number of matches between image " + project.images[int.Parse(twoImages[0]) - 1].name + " and image " + project.images[int.Parse(twoImages[1]) - 1].name + ":" + matchesCount + " in total");
                    }
                    else if (treeNode.Text.Equals("POS航线") || treeNode.Text.Equals("POS Route"))
                    {
                        tabControl1.SelectedIndex = 1;
                        tabControl3.SelectedIndex = 2;
                        if (hasAE)
                        {
                            for (int i = 0; i < 2; i++)
                                layers[i].Visible = true;
                            SwitchLayer(layers[0]);
                            axMapControl1.Refresh();
                        }
                    }
                    else if (treeNode.Text.Equals("控制点") || treeNode.Text.Equals("GCP"))
                    {
                        tabControl1.SelectedIndex = 1;
                        tabControl3.SelectedIndex = 2;
                        if (hasAE)
                        {
                            layer.Visible = true;
                            SwitchLayer(layer);
                            axMapControl1.Refresh();
                        }
                    }
                    else if (treeNode.Text.Equals("稀疏点云") || treeNode.Text.Equals("Sparse Cloud"))
                    {
                        tabControl1.SelectedIndex = 1;
                        tabControl3.SelectedIndex = 1;
                        if (txtModelPath.Text.Equals(project.sparseCloud))
                            return;
                        if (!File.Exists(project.sparseCloud))
                        {
                            MessageBox.Show(existInfo);
                            return;
                        }
                        Model3DNode node = Model3DNode.Create(0, project.sparseCloud);
                        BeginDrawing(node);
                        txtModelPath.Text = project.sparseCloud;
                    }
                    else if (treeNode.Text.Equals("稠密点云") || treeNode.Text.Equals("Dense Cloud"))
                    {
                        tabControl1.SelectedIndex = 1;
                        tabControl3.SelectedIndex = 1;
                        if (txtModelPath.Text.Equals(project.denseCloud))
                            return;
                        if (!File.Exists(project.denseCloud))
                        {
                            MessageBox.Show(existInfo);
                            return;
                        }
                        Model3DNode node = Model3DNode.Create(1, project.denseCloud);
                        BeginDrawing(node);  
                        txtModelPath.Text = project.denseCloud;
                    }
                    else if (treeNode.Text.Equals("DSM"))
                    {
                        if (!hasAE)
                        {
                            if (lang == Language.Chinese)
                                MessageBox.Show("无AE模式不支持DSM的显示！");
                            else if (lang == Language.English)
                                MessageBox.Show("DSM display is not supported with no ArcGIS Engine！");
                            return;
                        } 
                        tabControl1.SelectedIndex = 1;
                        tabControl3.SelectedIndex = 2;
                        if (!File.Exists(project.DSMPath))
                        {
                            MessageBox.Show(existInfo);
                            return;
                        }

                        for (int i = 0; i < axMapControl1.LayerCount; i++)
                        {
                            if (axMapControl1.get_Layer(i) is IRasterLayer)
                            {
                                axMapControl1.get_Layer(i).Visible = true;
                                SwitchLayer(axMapControl1.get_Layer(i));
                                return;
                            }
                        }
                        ShowDSM(project.DSMPath);
                    }
                    else if (treeNode.Text.Equals("三维模型") || treeNode.Text.Equals("3D Model"))
                    {
                        tabControl1.SelectedIndex = 1;
                        tabControl3.SelectedIndex = 1;
                        if (txtModelPath.Text.Equals(project.modelPath))
                            return;
                        if (model== null)
                        {
                            if (!File.Exists(project.modelPath))
                            {
                                MessageBox.Show(existInfo);
                                return;
                            }
                            Model3DNode node = Model3DNode.Create(2, project.modelPath);
                            model = node;
                        }
                        BeginDrawing(model);
                        txtModelPath.Text = project.modelPath;
                    }
                }
            }
            catch { }
        }
        #endregion

        #region 工程及系统模块

        private void MeasureMan_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (s[0].Split('.').Last().ToUpper().Equals("MSM"))
            {
                if (s[0].Equals(toolStripTextBox1.Text))
                {
                    if (lang == Language.Chinese)
                        MessageBox.Show("该工程已在系统中打开！");
                    else if (lang == Language.English)
                        MessageBox.Show("This project has been opened!");
                    return;
                }
                if (project != null)
                {
                    if (!project.saved)
                    {
                        if (lang == Language.Chinese && MessageBox.Show("原工程未保存，是否要保存原工程后再打开新的工程?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            ProjectToFile();
                        else if (lang == Language.English && MessageBox.Show("The original project is not saved, whether to save the original project and then open the new project?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            ProjectToFile();
                    }
                    if (rtbLog.Text != "")
                    {
                        if (lang == Language.Chinese && MessageBox.Show("是否保存原工程的日志?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            btOutputLog_Click(sender, e);
                        else if (lang == Language.English && MessageBox.Show("Whether to save the log of the original project?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            btOutputLog_Click(sender, e);
                        rtbLog.Text = "";
                    }
                }
                if (OpenProject(s[0]))
                {
                    if (lang == Language.Chinese)
                        MessageBox.Show("工程打开成功！");
                    else if (lang == Language.English)
                        MessageBox.Show("The project was opened successfully!");
                }
            }
        }

        private void MeasureMan_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        /// <summary>
        /// 读取最近打开的工程
        /// </summary>
        private void ReadLatestProjects()
        {
            latestPaths = new List<string>();
            if (File.Exists(Application.StartupPath+"\\Doc\\latest.txt"))
            {
                int count = 10;
                StreamReader sr = new StreamReader(Application.StartupPath + "\\Doc\\latest.txt");
                for (int i = 0; (!sr.EndOfStream) && i < count; i++)
                {
                    string path = sr.ReadLine();
                    if (path != "")
                    {
                        latestPaths.Add(path);
                        ToolStripMenuItem item = new ToolStripMenuItem(path);
                        item.Name = "latest" + count;
                        item.Click+=item_Click;
                        最近打开ToolStripMenuItem.DropDownItems.Add(item);
                    }     
                }
                sr.Close();
            }
            else
                File.Create(Application.StartupPath + "\\Doc\\latest.txt").Close();
        }

        private void item_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            if (!File.Exists(item.Text))
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("文件不存在！");
                else if (lang == Language.English)
                    MessageBox.Show("The file does not exist!");
                return;
            }
            if (item.Text.Equals(toolStripTextBox1.Text))
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("该工程已在系统中打开！");
                else if (lang == Language.English)
                    MessageBox.Show("This project has been opened!");
                return;
            }
            if (project != null)
            {
                if (!project.saved)
                {
                    if (lang == Language.Chinese && MessageBox.Show("原工程未保存，是否要保存原工程后再打开新的工程?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        ProjectToFile();
                    else if (lang == Language.English && MessageBox.Show("The original project is not saved, whether to save the original project and then open the new project?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        ProjectToFile();
                }
                if (rtbLog.Text != "")
                {
                    if (lang == Language.Chinese && MessageBox.Show("是否保存原工程的日志?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        btOutputLog_Click(sender, e);
                    else if (lang == Language.English && MessageBox.Show("Whether to save the log of the original project?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        btOutputLog_Click(sender, e);
                    rtbLog.Text = "";
                }
            }
            if (OpenProject(item.Text))
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("工程打开成功！");
                else if (lang == Language.English)
                    MessageBox.Show("The project was opened successfully!");
            }
        }

        /// <summary>
        /// 将路径添加到最近打开工程列表中
        /// </summary>
        /// <param name="path">路径</param>
        private void AddToLatest(string path)
        {
            for (int i = 0; i < latestPaths.Count; i++)
            {
                if (latestPaths.ElementAt(i) == path)
                {
                    最近打开ToolStripMenuItem.DropDownItems.RemoveAt(i);
                    latestPaths.RemoveAt(i);
                    break;
                }
            }
            ToolStripMenuItem item = new ToolStripMenuItem(path);
            item.Name = "latest" + latestPaths.Count;
            item.Click += item_Click;
            latestPaths.Insert(0,path);
            最近打开ToolStripMenuItem.DropDownItems.Insert(0, item);
            if (latestPaths.Count > 10)
            {
                最近打开ToolStripMenuItem.DropDownItems.RemoveAt(10);
                latestPaths.RemoveAt(10);
            }
        }

        /// <summary>
        /// 保存最近打开的工程
        /// </summary>
        private void SaveLatestProjects()
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\Doc\\latest.txt");
            for (int i = 0; i < latestPaths.Count;i++ )
                sw.WriteLine(latestPaths[i]);
            sw.Flush();
            sw.Close();
        }

        /// <summary>
        /// 操作计时
        /// </summary>
        private void Clock()
        {
            if (lang == Language.Chinese)
            {
                AppendText("处理结束时间：" + DateTime.Now.ToString() + "\n");
                AppendText("处理时长：" + (int)watch.Elapsed.TotalSeconds + "s\n");
            }
            else if (lang == Language.English)
            {
                AppendText("End time:" + DateTime.Now.ToString() + "\n");
                AppendText("Processing time:" + (int)watch.Elapsed.TotalSeconds + "s\n");
            }
        }

        private void btExit_Click(object sender, EventArgs e)
        {
            this.Close(); 
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (project != null)
            {
                if (!project.saved)
                {
                    if (lang == Language.Chinese&&MessageBox.Show("工程未保存，是否要保存工程后再退出系统?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        ProjectToFile();
                    else if(lang == Language.English&&MessageBox.Show("The project has not been saved, whether to save the project and then exit the system?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        ProjectToFile();
                }
                if (rtbLog.Text != "")
                {
                    if (lang == Language.Chinese && MessageBox.Show("是否保存原工程的日志?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        btOutputLog_Click(sender, e);
                    else if (lang == Language.English && MessageBox.Show("Whether to save the log of the original project?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        btOutputLog_Click(sender, e);
                    rtbLog.Text = "";
                }
                if (hasAE)
                {
                    ClearRoute();
                    ClearGCPLayer();
                }
                string proDir=System.IO.Path.GetDirectoryName(project.path);
                string outPath =  proDir+ "\\bundle.rd.out";
                if (File.Exists(outPath))
                    File.Delete(outPath);
                if (System.IO.Directory.Exists(proDir + "\\data"))
                {
                    DirectoryInfo dir = new DirectoryInfo(proDir + "\\data");
                    dir.Delete(true);
                }

                DirectoryInfo exeDir = new DirectoryInfo(Application.StartupPath);
                foreach (FileInfo file in exeDir.GetFiles())
                {
                    if (file.Extension.ToUpper().Equals(".LOG"))
                        file.Delete();
                }

                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.StandardInput.WriteLine("taskkill /im pmvs2.exe /f&exit");
                p.StandardInput.WriteLine("taskkill /im genOption.exe&exit");
                p.StandardInput.WriteLine("taskkill /im SiftGPU.exe&exit");
                p.StandardInput.AutoFlush = true;
                p.WaitForExit();
                p.Close();
            }
            SaveLatestProjects();
        }

        private void btCreatePro_Click(object sender, EventArgs e)
        {
            CreateProject create = new CreateProject(lang);
            create.ShowDialog();
            if (create.project!=null)
            {
                if (project!=null)
                {
                    if (!project.saved)
                    {
                        if (lang == Language.Chinese && MessageBox.Show("原工程未保存，是否要保存原工程后再创建新的工程?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            ProjectToFile();
                        else if (lang == Language.English && MessageBox.Show("The original project is not saved, whether to save the original project and then create a new project?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            ProjectToFile();
                    }
                    if (rtbLog.Text != "")
                    {
                        if (lang == Language.Chinese && MessageBox.Show("是否保存原工程的日志?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            btOutputLog_Click(sender, e);
                        else if (lang == Language.English && MessageBox.Show("Whether to save the log of the original project?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            btOutputLog_Click(sender, e);
                        rtbLog.Text = "";
                    }
                }
                project = create.project;
                project.InitiateProject();
                InitiateImageDataWindow();
                ClearPreviousProject();
                CanUse();
                数据视图ToolStripMenuItem_Click(sender, e);
                images = new List<AddedImage>();
                BuildDirectoryTree();
                ShowCameraInfo(null);
                AddToLatest(project.path);
                if (lang == Language.Chinese)
                {
                    if (project.ftType == FeatureType.SIFT)
                        this.Text = "MeasureMan->精细建模";
                    else
                        this.Text = "MeasureMan->快速建模";
                    MessageBox.Show("工程创建成功！");
                }
                else if (lang == Language.English)
                {
                    if (project.ftType == FeatureType.SIFT)
                        this.Text = "MeasureMan->fine modeling";
                    else
                        this.Text = "MeasureMan->rapid modeling";
                    MessageBox.Show("The project was created successfully!");
                }
            }
        }

        private void btSavePro_Click(object sender, EventArgs e)
        {
            if (ProjectToFile())
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("工程保存成功！"); 
                else if (lang == Language.English)
                    MessageBox.Show("The project was saved successfully!"); 
            }
                
        }

        /// <summary>
        /// 将Project信息保存到文件中
        /// </summary>
        /// <returns>工程是否保存成功</returns>
        private bool ProjectToFile()
        {
            if (project == null)
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("系统未打开任何工程文件！");
                else if (lang == Language.English)
                    MessageBox.Show("The system did not open any project file!");
                return false;
            }
            if (project.saved)
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("工程文件已保存！");
                else if (lang == Language.English)
                    MessageBox.Show("The project file has been saved!");
                return false;
            }
            StreamWriter sw = new StreamWriter(project.path);
            sw.WriteLine("ZKGCPZD");
            sw.WriteLine(project.dataType);
            sw.WriteLine(project.ftType.ToString());
            sw.WriteLine(project.canTrack);
            sw.WriteLine("--------");
            if (project.images != null && project.images.Count != 0 && project.images[0].camera != null)
            {
                foreach (AddedImage img in project.images)
                {
                    if (img.clipped)
                        sw.WriteLine(img.name + "|" + img.time +"|"+img.camera.GetAllInfo()+"|" + img.ROI.Left + "|" + img.ROI.Top + "|" + img.ROI.Width + "|" + img.ROI.Height);
                    else
                        sw.WriteLine(img.name + "|" + img.time + "|" +img.camera.GetAllInfo());
                }
                sw.WriteLine("--------");
                foreach (AddedImage img in project.images)
                {
                    if (img._POS != null)
                        sw.WriteLine(img._POS.y + "|" + img._POS.x + "|" + img._POS.z + "|" + img._POS.pitch + "|" + img._POS.yaw + "|" + img._POS.roll);
                }
                sw.WriteLine("--------");
                if (project.GCPs != null && project.GCPs.Count != 0)
                {
                    foreach (GCP gcp in project.GCPs)
                    {
                        if(gcp.modelPoint==null)
                            sw.WriteLine(gcp.imageOrder + "|" + gcp.pixelPoint.X + "|" + gcp.pixelPoint.Y + "|" + gcp.x + "|" + gcp.y + "|" + gcp.z);
                        else
                            sw.WriteLine(gcp.imageOrder + "|" + gcp.pixelPoint.X + "|" + gcp.pixelPoint.Y + "|" + gcp.x + "|" + gcp.y + "|" + gcp.z + "|" + gcp.modelPoint.x + "|" + gcp.modelPoint.y + "|" + gcp.modelPoint.z);
                    }
                        
                }
                sw.WriteLine("--------");
                if (project.sparseCloud!=null)
                    sw.WriteLine(project.sparseCloud);
                sw.WriteLine("--------");
                if (project.denseCloud != null)
                    sw.WriteLine(project.denseCloud);   
                sw.WriteLine("--------");
                if(project.DSMPath!=null)
                    sw.WriteLine(project.DSMPath);
                sw.WriteLine("--------");
                if(project.modelPath != null)
                    sw.WriteLine(project.modelPath);
                sw.WriteLine("--------");
            }
            sw.Flush();
            sw.Close();
            project.saved = true;
            return true;
        }

        private void btOpenPro_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            if (lang == Language.Chinese)
            {
                open.Title = "打开工程文件";
                open.Filter = "MeasureMan工程文件|*.msm";
            }
            else if (lang == Language.English)
            {
                open.Title = "Open Project File";
                open.Filter = "MeasureMan Project File|*.msm";
            }
            if (open.ShowDialog() == DialogResult.OK)
            {
                if (open.FileName.Equals(toolStripTextBox1.Text))
                {
                    if (lang == Language.Chinese)
                        MessageBox.Show("该工程已在系统中打开！");
                    else if (lang == Language.English)
                        MessageBox.Show("This project has been opened!");
                    return;
                }
                if (project != null)
                {
                    if (!project.saved)
                    {
                        if (lang == Language.Chinese&&MessageBox.Show("原工程未保存，是否要保存原工程后再打开新的工程?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            ProjectToFile();
                        else if(lang == Language.English&&MessageBox.Show("The original project is not saved, whether to save the original project and then open the new project?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            ProjectToFile();
                    }
                    if (rtbLog.Text != "")
                    {
                        if (lang == Language.Chinese && MessageBox.Show("是否保存原工程的日志?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            btOutputLog_Click(sender, e);
                        else if (lang == Language.English && MessageBox.Show("Whether to save the log of the original project?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            btOutputLog_Click(sender, e);
                        rtbLog.Text = "";
                    }
                }
                if (OpenProject(open.FileName))
                {
                    if (lang == Language.Chinese)
                        MessageBox.Show("工程打开成功！");
                    else if (lang == Language.English)
                        MessageBox.Show("The project was opened successfully!");
                }
            }
        }

        /// <summary>
        /// 打开工程文件
        /// </summary>
        /// <param name="proPath">工程文件路径</param>
        ///<returns>是否打开成功</returns>
        private bool OpenProject(string proPath)
        {
            StreamReader sr = new StreamReader(proPath);
            StringBuilder sb = new StringBuilder();
            while (!sr.EndOfStream)
            {
                sb.Append(sr.ReadLine() + "*");
            }
            sr.Close();
            string[] text = sb.ToString().Split('*');
            if (!text[0].Equals("ZKGCPZD"))
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("工程文件格式不正确！");
                else if (lang == Language.English)
                    MessageBox.Show("Incorrect format of project file!");
                return false;
            }
            images = new List<AddedImage>();
            InitiateImageDataWindow();
            ClearPreviousProject();
            if (FileToProject(text, proPath))
            {
                project.JudgeEqualF();
                CanUse();
                BuildDirectoryTree();
                ShowCameraInfo(project.camera);
                if (lang == Language.Chinese)
                {
                    if (project.ftType == FeatureType.SIFT)
                        this.Text = "MeasureMan->精细建模";
                    else
                        this.Text = "MeasureMan->快速建模";
                }
                else if (lang == Language.English)
                {
                    if (project.ftType == FeatureType.SIFT)
                        this.Text = "MeasureMan->fine modeling";
                    else
                        this.Text = "MeasureMan->rapid modeling";
                }
                AddToLatest(project.path);
                return true;
            }
            project = null;
            BuildDirectoryTree();
            return false;
        }

        /// <summary>
        /// 将文件信息读入到Project中
        /// </summary>
        /// <param name="text">文件信息</param>
        /// <param name="path">文件路径</param>
        /// <returns>工程打开是否成功</returns>
        private bool FileToProject(string[] text, string path)
        {
            try
            {
                project = new Project(path, int.Parse(text[1]), (FeatureType)Enum.Parse(typeof(FeatureType), text[2]));
                project.canTrack = bool.Parse(text[3]);
                int j = 5;
                string proDir = System.IO.Path.GetDirectoryName(project.path);

                if (System.IO.Directory.Exists(proDir + "\\ZKGCPZD_POS"))//防止程序意外退出
                {
                    DirectoryInfo dir = new DirectoryInfo(proDir + "\\ZKGCPZD_POS");
                    dir.Delete(true);
                }

                if (System.IO.Directory.Exists(proDir + "\\ZKGCPZD_GCP"))//防止程序意外退出
                {
                    DirectoryInfo dir = new DirectoryInfo(proDir + "\\ZKGCPZD_GCP");
                    dir.Delete(true);
                }

                if (System.IO.Directory.Exists(proDir + "\\data"))//防止程序意外退出
                {
                    DirectoryInfo dir = new DirectoryInfo(proDir + "\\data");
                    dir.Delete(true);
                }
                //读入图片
                string imgDir = proDir + "\\img";
                if (!System.IO.Directory.Exists(imgDir))
                    System.IO.Directory.CreateDirectory(imgDir);
                for (int i = j; i < text.Length - 1; i++)
                {
                    if (text[i].Equals("--------"))
                    {
                        j = i + 1;
                        break;
                    }
                    else
                    {
                        string[] imageInfo = text[i].Split('|');
                        string imgPath = imgDir + "\\" + imageInfo[0];
                        if (!System.IO.File.Exists(imgPath))
                        {
                            if (lang == Language.Chinese)
                                MessageBox.Show("未找到工程文件中的某些图像数据！");
                            else if (lang == Language.English)
                                MessageBox.Show("Some images in the project file was not found！");
                            return false;
                        }
                        images.Add(new AddedImage(imageInfo[0], DateTime.Parse(imageInfo[1]), i - j + 1, imgPath,new CameraInfo(imageInfo[2],double.Parse(imageInfo[3]),
                            float.Parse(imageInfo[4]),float.Parse(imageInfo[5]))));
                        if (imageInfo.Length>6)
                        {
                            images.Last().clipped = true;
                            images.Last().ROI = new Rectangle(int.Parse(imageInfo[6]), int.Parse(imageInfo[7]), int.Parse(imageInfo[8]), int.Parse(imageInfo[9]));
                        }
                    }
                }
                if (images.Count != 0 && project.canTrack == false)
                    特征点检测ToolStripMenuItem1.Enabled = true;
                //读入.ft特征点文件
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(proDir + "\\ft");
                    FileInfo[] files = dir.GetFiles();
                    project.ftPaths = new List<string>();
                    foreach (FileInfo file in files)
                    {
                        if (file.Extension.Equals(".ft"))
                            project.ftPaths.Add(file.FullName);
                    }
                    if (project.ftPaths.Count > 0 && project.ftPaths.Count == images.Count)
                    {
                        SortTool.SortByName(project.ftPaths);
                        特征点检测ToolStripMenuItem1.Enabled = false;
                        图像裁剪ToolStripMenuItem.Enabled = false;
                        移除图像ToolStripMenuItem.Enabled = false;
                        特征点匹配ToolStripMenuItem1.Enabled = true;
                    }
                    else if (project.ftPaths.Count > 0)
                    {
                        if (lang == Language.Chinese)
                            MessageBox.Show("未找到某些图像数据对应的特征点数据，请重新打开工程！");
                        else if (lang == Language.English)
                            MessageBox.Show("Unable to find the corresponding feature point data of some images, please reopen the project!");
                        foreach (FileInfo file in files)
                            file.Delete();
                        return false;
                    }
                }
                catch
                {
                    if (lang == Language.Chinese)
                        MessageBox.Show("未找到与工程文件同目录的特征点文件夹，请重新打开工程！");
                    else if (lang == Language.English)
                        MessageBox.Show("Unable to find the feature point folder in the same directory as the project file, please reopen the project!");
                    System.IO.Directory.CreateDirectory(proDir + "\\ft");
                    return false;
                }

                //读入.rel图像匹配关系（无限制）
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(proDir + "\\rel");
                    FileInfo[] files = dir.GetFiles();
                    project.TVMs = new List<TwoViewModel>();
                    foreach (FileInfo file in files)
                    {
                        if (file.Extension.Equals(".rel"))
                        {
                            StreamReader sr = new StreamReader(file.FullName);
                            project.TVMs.Add(new TwoViewModel(file.FullName, int.Parse(sr.ReadLine())));
                            sr.Close();
                        }    
                    }
                    if (project.TVMs.Count > 0)
                    {
                        project.TVMs.Sort();
                        特征点匹配ToolStripMenuItem1.Enabled = false;
                        稀疏重建ToolStripMenuItem1.Enabled = true;
                        添加POSToolStripMenuItem.Enabled = false;
                    }
                }
                catch
                {
                    string relDir = proDir + "\\rel";
                    if (System.IO.Directory.Exists(relDir))
                    {
                        if (lang == Language.Chinese)
                            MessageBox.Show("有匹配文件但无法找到相应特征点，请重新打开工程！");
                        else if (lang == Language.English)
                            MessageBox.Show("There exists matching files but cannot find the corresponding feature point files, please reopen the project!");
                        DirectoryInfo dir = new DirectoryInfo(relDir);
                        FileInfo[] files = dir.GetFiles();
                        foreach (FileInfo file in files)
                            file.Delete();
                    }
                    else
                    {
                        if (lang == Language.Chinese)
                            MessageBox.Show("未找到与工程文件同目录的匹配关系文件夹，请重新打开工程！");
                        else if (lang == Language.English)
                            MessageBox.Show("Unable to find the matching relation folder in the same directory as the project file, please reopen the project!");
                        System.IO.Directory.CreateDirectory(relDir);
                    }
                    return false;
                }

                //读入POS
                for (int i = j; i < text.Length - 1; i++)
                {
                    if (text[i].Equals("--------"))
                    {
                        j = i + 1;
                        break;
                    }
                    else
                    {
                        string[] posInfo = text[i].Split('|');
                        images[i - j]._POS = new POS(double.Parse(posInfo[0]), double.Parse(posInfo[1]), double.Parse(posInfo[2]), double.Parse(posInfo[3]), double.Parse(posInfo[4]), double.Parse(posInfo[5]));
                    }
                }
                project.images = images;
                if (hasAE&&project.images.Count != 0 && project.images[0]._POS != null)
                    DrawRoute();
                //读入控制点
                project.GCPs = new List<GCP>();
                for (int i = j; i < text.Length - 1; i++)
                {
                    if (text[i].Equals("--------"))
                    {
                        j = i + 1;
                        break;
                    }
                    else
                    {
                        string[] gcpInfo = text[i].Split('|');
                        if(gcpInfo.Length==6)
                            project.GCPs.Add(new GCP(int.Parse(gcpInfo[0]),float.Parse(gcpInfo[1]), float.Parse(gcpInfo[2]), double.Parse(gcpInfo[3]), double.Parse(gcpInfo[4]), double.Parse(gcpInfo[5])));
                        else
                            project.GCPs.Add(new GCP(int.Parse(gcpInfo[0]), float.Parse(gcpInfo[1]), float.Parse(gcpInfo[2]), double.Parse(gcpInfo[3]), double.Parse(gcpInfo[4]), double.Parse(gcpInfo[5]), 
                                double.Parse(gcpInfo[6]),double.Parse(gcpInfo[7]),double.Parse(gcpInfo[8])));
                    }
                }
                if (hasAE && project.GCPs.Count != 0)
                    DrawGCPLayer();
                //读入点云路径
                for (int i = j; i < text.Length - 1; i++)
                {
                    if (text[i].Equals("--------"))
                    {
                        j = i + 1;
                        break;
                    }
                    else
                    {
                        if (File.Exists(text[i]))
                        {
                            project.sparseCloud = text[i];
                            稀疏重建ToolStripMenuItem1.Enabled = false;
                            添加控制点ToolStripMenuItem.Enabled = false;
                            稠密重建ToolStripMenuItem.Enabled = true;
                        }
                    }   
                }

                for (int i = j; i < text.Length - 1; i++)
                {
                    if (text[i].Equals("--------"))
                    {
                        j = i + 1;
                        break;
                    }
                    else
                    {
                        if (File.Exists(text[i]))
                        {
                            project.denseCloud = text[i];
                            稠密重建ToolStripMenuItem.Enabled = false;
                            dSM转换ToolStripMenuItem.Enabled = true;
                            表面重建ToolStripMenuItem1.Enabled = true;
                        }
                    }
                        
                }

                for (int i = j; i < text.Length - 1; i++)
                {
                    if (text[i].Equals("--------"))
                    {
                        j = i + 1;
                        break;
                    }
                    else
                    {
                        if (File.Exists(text[i]))
                        {
                            project.DSMPath = text[i];
                            dSM转换ToolStripMenuItem.Enabled = false;
                        }
                    }
                }

                for (int i = j; i < text.Length - 1; i++)
                {
                    if (text[i].Equals("--------"))
                    {
                        j = i + 1;
                        break;
                    }
                    else
                    {
                        if (File.Exists(text[i]))
                        {
                            project.modelPath = text[i];
                            表面重建ToolStripMenuItem1.Enabled = false;
                        }
                    }
                }

                if (project.images != null && project.images.Count != 0)
                {
                    if (hasAE)
                    {
                        tabControl1.SelectedIndex = 1;
                        tabControl3.SelectedIndex = 2;
                    }
                    ShowImageInfo(images[0]);
                    project.camera = images[0].camera;
                }
                return true;
            }
            catch
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("工程文件损毁，无法正确打开！");
                else if (lang == Language.English)
                    MessageBox.Show("The project file corrupted, cannot open correctly!");
                return false;
            }

        }

        /// <summary>
        /// 初始化图像数据视图，一般发生在视图跳转或工程初始化
        /// </summary>
        private void InitiateImageDataWindow()
        {
            txtImageOrder.Text = "";
            txtImageName.Text = "";
            txtImageTime.Text = "";
            if (lang == Language.Chinese)
            {
                经度ToolStripMenuItem.Text = "经度：";
                纬度ToolStripMenuItem.Text = "纬度：";
                高程ToolStripMenuItem.Text = "高程：";
                俯仰角ToolStripMenuItem.Text = "俯仰角：";
                偏航角ToolStripMenuItem.Text = "偏航角：";
                翻滚角ToolStripMenuItem.Text = "翻滚角：";
            }
            else if (lang == Language.English)
            {
                经度ToolStripMenuItem.Text = "Longitude:";
                纬度ToolStripMenuItem.Text = "Latitude:";
                高程ToolStripMenuItem.Text = "Elevation:";
                俯仰角ToolStripMenuItem.Text = "Pitch:";
                偏航角ToolStripMenuItem.Text = "Yaw:";
                翻滚角ToolStripMenuItem.Text = "Roll:";
            }
        }

        /// <summary>
        /// 清除前一个工程文件的内容
        /// </summary>
        private void ClearPreviousProject()
        {
            if (imageBox1.Image != null)
                imageBox1.Image.Dispose();
            imageBox1.Image = null;
            if (imageBox2.Image != null)
                imageBox2.Image.Dispose();
            imageBox2.Image = null;
            if (hasAE)
            {
                for (int i = axMapControl1.LayerCount - 1; i >= 0; i--)
                    axMapControl1.DeleteLayer(i);
                ClearRoute();
                ClearGCPLayer();
            }
            ClearLastMeasurement();
            InitiateGLWindow();
            model = null;
            特征点检测ToolStripMenuItem1.Enabled = false;
            特征点匹配ToolStripMenuItem1.Enabled = false;
            稀疏重建ToolStripMenuItem1.Enabled = false;
            稠密重建ToolStripMenuItem.Enabled = false;
            dSM转换ToolStripMenuItem.Enabled = false;
            表面重建ToolStripMenuItem1.Enabled = false;
            添加POSToolStripMenuItem.Enabled = true;
            添加控制点ToolStripMenuItem.Enabled = true;
            移除图像ToolStripMenuItem.Enabled = true;
            图像裁剪ToolStripMenuItem.Enabled = true;
        }

        /// <summary>
        /// 工程读入或创建后数据添加功能的使用状态
        /// </summary>
        private void CanUse()
        {
            if (project.dataType == 0)
            {
                添加图像数据ToolStripMenuItem.Visible = true;
                添加图像数据ToolStripMenuItem.Enabled = true;
                if (project.images != null && project.images.Count > 0)
                    添加图像数据ToolStripMenuItem.Enabled = false;
                关键帧提取ToolStripMenuItem.Visible = false;
            }
            else
            {
                添加图像数据ToolStripMenuItem.Visible = false;
                关键帧提取ToolStripMenuItem.Visible = true;
                关键帧提取ToolStripMenuItem.Enabled = true;
                if(project.canTrack)
                {
                    if (project.images != null && project.images.Count > 0)
                    {
                        btFeaturePtsTrack.Enabled = true;
                        Play.Enabled = false;
                    }
                    else
                    {
                        Play.Enabled = true;
                        btFeaturePtsTrack.Enabled = false;
                    }

                }
                else
                    关键帧提取ToolStripMenuItem.Enabled = false;
            }
        }

        #endregion

        #region 工具箱模块
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            txtLat.Text = "";
            txtLng.Text = "";
            bool drawRoute, drawGCPs;
            bbox = GetBBox(out drawRoute, out drawGCPs);
            if (drawRoute)
                DrawRoute(e.Graphics);
            if (drawGCPs)
                DrawGCPLayer(e.Graphics);
        }

        /// <summary>
        /// 获得航线和控制点的地图范围
        /// </summary>
        /// <param name="drawRoute">是否绘制航线</param>
        /// <param name="drawGCPs">是否绘制控制点</param>
        /// <returns>地图范围</returns>
        private BoundingBox GetBBox(out bool drawRoute, out bool drawGCPs)
        {
            float XMin = 180, YMin = 90, XMax = -180, YMax = -90;

            if (images != null && images.Count > 1 && images[0]._POS != null)
            {
                foreach (AddedImage img in images)
                {
                    POS pos = img._POS;
                    if (pos.x < XMin)
                        XMin = (float)(pos.x);
                    if (pos.x > XMax)
                        XMax = (float)(pos.x);
                    if (pos.y < YMin)
                        YMin = (float)(pos.y);
                    if (pos.y > YMax)
                        YMax = (float)(pos.y);
                }
                drawRoute = true;
            }
            else
                drawRoute = false;
            if (project != null && project.GCPs != null && project.GCPs.Count > 0)
            {
                foreach (GCP gcp in project.GCPs)
                {
                    if (gcp.x < XMin)
                        XMin = (float)(gcp.x);
                    if (gcp.x > XMax)
                        XMax = (float)(gcp.x);
                    if (gcp.y < YMin)
                        YMin = (float)(gcp.y);
                    if (gcp.y > YMax)
                        YMax = (float)(gcp.y);
                }
                drawGCPs = true;
            }
            else
                drawGCPs = false;
            if (drawGCPs || drawRoute)
                pointScale = pictureBox1.Size.Height / 50.0;
            else
                pointScale = 0;
            return new BoundingBox(new vec3(XMin, YMin, 0), new vec3(XMax, YMax, 0));
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (pointScale != 0)
            {
                PointF pt = GetWGS84Point(e.X, e.Y);
                txtLng.Text = pt.X + "°";
                txtLat.Text = pt.Y + "°";
            }
            else
            {
                txtLat.Text = "";
                txtLng.Text = "";
            }
        }

        /// <summary>
        /// 鼠标坐标转WGS84坐标
        /// </summary>
        /// <param name="x">鼠标坐标x</param>
        /// <param name="y">鼠标坐标y</param>
        /// <returns>WGS84坐标</returns>
        private PointF GetWGS84Point(int x, int y)
        {
            double dFieldHeigth = bbox.MaxPosition.y - bbox.MinPosition.y;
            double dFieldwidth = bbox.MaxPosition.x - bbox.MinPosition.x;
            double dHeigthRatio = pictureBox1.Size.Height / dFieldHeigth;
            double dWidthRatio = pictureBox1.Size.Width / dFieldwidth;

            float ptx = (float)(x / dWidthRatio + bbox.MinPosition.x);
            float pty = (float)(dFieldHeigth + bbox.MinPosition.y - y / dHeigthRatio);
            return new PointF(ptx, pty);
        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (pointScale == 0)
                return;
            double dFieldHeigth = bbox.MaxPosition.y - bbox.MinPosition.y;
            double dFieldwidth = bbox.MaxPosition.x - bbox.MinPosition.x;
            double dHeigthRatio = pictureBox1.Size.Height / dFieldHeigth;
            double dWidthRatio = pictureBox1.Size.Width / dFieldwidth;

            if (images != null && images.Count > 1 && images[0]._POS != null)
            {
                foreach (AddedImage img in images)
                {
                    double tx = (img._POS.x - bbox.MinPosition.x) * dWidthRatio;
                    double ty = (dFieldHeigth - img._POS.y + bbox.MinPosition.y) * dHeigthRatio;
                    if (Math.Sqrt(Math.Pow(e.X - tx, 2) + Math.Pow(e.Y - ty, 2)) <= pointScale)
                    {
                        ShowImageInfo(images[img.order - 1]);
                        return;
                    }
                }
            }

            if (project != null && project.GCPs != null && project.GCPs.Count > 0)
            {
                foreach (GCP gcp in project.GCPs)
                {
                    double tx = (gcp.x - bbox.MinPosition.x) * dWidthRatio;
                    double ty = (dFieldHeigth - gcp.y + bbox.MinPosition.y) * dHeigthRatio;
                    if (Math.Sqrt(Math.Pow(e.X - tx, 2) + Math.Pow(e.Y - ty, 2)) <= pointScale)
                    {
                        if (lang == Language.Chinese)
                            MessageBox.Show("经度：" + gcp.x + "°\n纬度：" + gcp.y + "°\n高程：" + gcp.z + "m");
                        else if (lang == Language.English)
                            MessageBox.Show("Longitude:" + gcp.x + "°\nLatitude:" + gcp.y + "°\nElevation:" + gcp.z + "m");
                        return;
                    }
                }
            }
        }


        /// <summary>
        /// 读出Excel数据
        /// </summary>
        /// <param name="filePath">Excel文件路径</param>
        /// <returns>数据表</returns>
        private DataTable SelectDataFromExcel(string filePath)
        {
            string strConn = "Provider=Microsoft.Jet.Oledb.4.0;Data Source=" + filePath + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
            string sql = "SELECT * FROM [Sheet1$]";
            OleDbConnection OleConn = new OleDbConnection(strConn);
            OleConn.Open();
            OleDbDataAdapter OleDa = new OleDbDataAdapter(sql, OleConn);
            DataSet OleDs = new DataSet();
            OleDa.Fill(OleDs, "Sheet1");
            OleConn.Close();
            return OleDs.Tables[0];
        }

        private void imageBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0 && imageBox1.Image != null && e.Button == MouseButtons.Left && (GCPsView != null || 图像裁剪ToolStripMenuItem.Text.Equals("结束图像裁剪")||
                图像裁剪ToolStripMenuItem.Text.Equals("End Image Clipping")))
            {
                //先将控件坐标转化为真实鼠标坐标
                //e为控件坐标，mousePoint为鼠标坐标
                PointF mousePoint = new PointF();
                mousePoint.X = (float)(e.Location.X / imageBox1.ZoomScale);
                mousePoint.Y = (float)(e.Location.Y / imageBox1.ZoomScale);
                int horizontalScrollBarValue = imageBox1.HorizontalScrollBar.Visible ? imageBox1.HorizontalScrollBar.Value : 0;
                int verticalScrollBarValue = imageBox1.VerticalScrollBar.Visible ? imageBox1.VerticalScrollBar.Value : 0;
                mousePoint.X += horizontalScrollBarValue;
                mousePoint.Y += verticalScrollBarValue;
                Size realSize = new Size();
                PointF upperLeft = GetUpperLeftPoint(ref realSize);
                Size imageSize = imageBox1.Image.Size;
                float pixelPointU = (mousePoint.X - upperLeft.X + 1) * imageSize.Width / realSize.Width - 1;
                float pixelPointV = (mousePoint.Y - upperLeft.Y + 1) * imageSize.Height / realSize.Height - 1;
                if (pixelPointU < 0 || pixelPointV < 0 || pixelPointU > imageSize.Width - 1 || pixelPointV > imageSize.Height - 1)
                {
                    if (lang == Language.Chinese)
                        MessageBox.Show("点不在图像范围内，请重新选择！");
                    else if (lang == Language.English)
                        MessageBox.Show("The mouse point is not in the image area, please select again!");
                    return;
                }
                if (GCPsView != null)
                {
                    GCPInput input = new GCPInput(lang);
                    input.ShowDialog();
                    GCP gcp = input.gcp;
                    input.Dispose();
                    if (gcp != null)
                    {
                        Image<Bgr, byte> img = imageBox1.Image as Image<Bgr, byte>;
                        gcp.imageOrder = int.Parse(txtImageOrder.Text);
                        gcp.pixelPoint = new PointF(pixelPointU, pixelPointV);
                        byte[, ,] data = img.Data;
                        int row = (int)pixelPointV;
                        int col = (int)pixelPointU;
                        if (GCPRepeat(gcp))
                        {
                            if (lang == Language.Chinese)
                                MessageBox.Show("该控制点已添加，请添加其他控制点！");
                            else if (lang == Language.English)
                                MessageBox.Show("This GCP has been added, please add other GCPs!");
                            return;
                        }
                        DataTable dt = (DataTable)GCPsView.DataSource;
                        dt.Rows.Add(gcp.GetDataRow());
                        GCPsView.Rows[dt.Rows.Count - 1].Cells[6].Style.BackColor = Color.FromArgb(img.Data[row, col, 2], img.Data[row, col, 1], img.Data[row, col, 0]);
                        project.GCPs.Add(gcp);
                        CircleF circle = new CircleF(gcp.pixelPoint, 5);
                        img.Draw(circle, new Bgr(0, 255, 255), 3);
                    }
                }
                else if (图像裁剪ToolStripMenuItem.Text.Equals("结束图像裁剪") || 图像裁剪ToolStripMenuItem.Text.Equals("End Image Clipping"))
                {
                    if (rectFirPoint == System.Drawing.Point.Empty)
                    {
                        rectFirPoint = new System.Drawing.Point((int)pixelPointU, (int)pixelPointV);
                    }
                    else
                    {
                        System.Drawing.Point rectSecPoint = new System.Drawing.Point((int)pixelPointU, (int)pixelPointV);
                        int imageIndex = int.Parse(txtImageOrder.Text) - 1;
                        project.images[imageIndex].clipped = true;
                        project.images[imageIndex].ROI = GetRectBy2Pts(rectFirPoint, rectSecPoint);
                        ShowImageInfo(project.images[imageIndex]);
                        project.saved = false;
                    }
                }
            }
        }

        /// <summary>
        /// 获取左上角的鼠标坐标
        /// </summary>
        /// <param name="realSize">返回的图像显示大小</param>
        /// <returns>左上角的鼠标坐标</returns>
        private PointF GetUpperLeftPoint(ref Size realSize)
        {
            Size boxSize = imageBox1.Size;//显示图片最大的宽和高
            double boxRatio = boxSize.Width / (boxSize.Height + 0.0);//容器宽高比
            Size imageSize = imageBox1.Image.Size;
            double imageRatio = imageSize.Width / (imageSize.Height + 0.0);//图像宽高比
            PointF upperLeft = new PointF();//左上角的像素坐标
            if (imageRatio < boxRatio)
            {
                realSize.Height = boxSize.Height;
                realSize.Width = (int)(realSize.Height * imageRatio);
                upperLeft.Y = 0;
                upperLeft.X = boxSize.Width / 2f - realSize.Width / 2f - 1;
                if (upperLeft.X < 0)
                    upperLeft.X = 0;
            }
            else
            {
                realSize.Width = boxSize.Width;
                realSize.Height = (int)(realSize.Width / imageRatio);
                upperLeft.X = 0;
                upperLeft.Y = boxSize.Height / 2f - realSize.Height / 2f - 1;
                if (upperLeft.Y < 0)
                    upperLeft.Y = 0;
            }
            return upperLeft;
        }

        /// <summary>
        /// 切换显示图层
        /// </summary>
        /// <param name="layer">图层</param>
        private void SwitchLayer(ILayer layer)
        {
            IGeoDataset geo = layer as IGeoDataset;
            axMapControl1.SpatialReference = geo.SpatialReference;
            axMapControl1.Extent = geo.Extent;
            if (geo.SpatialReference is IGeographicCoordinateSystem)
                toolStripStatusLabel7.Text = "°";
            else
                toolStripStatusLabel7.Text = "m";
            toolStripStatusLabel8.Text = toolStripStatusLabel7.Text;
            toolStripStatusLabel6.Text = geo.SpatialReference.Name;
        }

        #region 添加POS
        private void 添加POSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (project==null||project.images == null || project.images.Count == 0)
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("请先添加图像数据，再添加相应的POS数据！");
                else if (lang == Language.English)
                    MessageBox.Show("Please add image data first, then add corresponding POS data!");
                return;
            }
            OpenFileDialog open = new OpenFileDialog();
            if (lang == Language.Chinese)
            {
                open.Filter = "xls表格|*.xls";
                open.Title = "打开POS数据";
            }
            else if (lang == Language.English)
            {
                open.Filter = "xls Table|*.xls";
                open.Title = "Open POS Data";
            }
            if (open.ShowDialog() == DialogResult.OK)
            {
                watch.Restart();
                string failWarning = "";
                string succeedInfo = "";   
                if (lang == Language.Chinese)
                {
                    rtbLog.AppendText("- - - -添加POS数据- - - -\n");
                    rtbLog.AppendText("POS数据路径：" + open.FileName + "\n");
                    failWarning = "POS数据添加失败\n";
                    succeedInfo = "POS数据添加成功";
                }
                else if (lang == Language.English)
                {
                    rtbLog.AppendText("- - - -Add POS Data- - - -\n");
                    rtbLog.AppendText("POS data path:" + open.FileName + "\n");
                    failWarning = "Failed to add POS data\n";
                    succeedInfo = "Add POS data successfully";
                }
                DataTable dt = SelectDataFromExcel(open.FileName);
                if (dt.Rows.Count< project.images.Count)
                {
                    watch.Stop();
                    rtbLog.AppendText(failWarning);
                    Clock();
                    if (lang == Language.Chinese)
                        MessageBox.Show("POS数据不足！");
                    else if (lang == Language.English)
                        MessageBox.Show("Insufficient POS data!");
                    return;
                }
                string[] cols = new string[] { "NAME", "LAT", "LNG", "ALT", "PITCH", "YAW", "ROLL" };
                if (cols.Length != dt.Columns.Count)
                {
                    watch.Stop();
                    rtbLog.AppendText(failWarning);
                    Clock();
                    if (lang == Language.Chinese)
                        MessageBox.Show("POS数据列数不为"+cols.Length+"！");
                    else if (lang == Language.English)
                        MessageBox.Show("The number of POS data columns is not equal to " + cols.Length + "!");
                    return;
                }
                int i = 0;
                for (; i < dt.Columns.Count; i++)
                {
                    if (!dt.Columns[i].ColumnName.ToUpper().Equals(cols[i]))
                        break;
                }
                if (i != dt.Columns.Count)
                {
                    watch.Stop();
                    rtbLog.AppendText(failWarning);
                    Clock();
                    if (lang == Language.Chinese)
                        MessageBox.Show("POS数据每列的名称和顺序要与{NAME,LAT,LNG,ALT,PITCH,YAW,ROLL}一致！");
                    else if (lang == Language.English)
                        MessageBox.Show("The name and order of each column of POS data should be the same as {NAME,LAT,LNG,ALT,PITCH,YAW,ROLL}!");
                    return;
                }
                List<string> posNames = new List<string>();
                List<int> indexes = new List<int>();
                List<POS> poses = new List<POS>();
                i = 0;
                for (; i < dt.Rows.Count; i++)
                {
                    posNames.Add(dt.Rows[i][0].ToString().ToUpper());
                    indexes.Add(i);
                }
                int order = SortTool.CheckInOrder(posNames,posNames.Count);
                if (order == 0)
                    SortTool.QuickSort2(posNames, 0, posNames.Count - 1, indexes);
                else if (order == -1)
                    SortTool.Reverse(posNames,posNames.Count,indexes);
                try
                {
                    for (int j = 0; j < project.images.Count; j++)
                    {
                        int index = BinarySearch(posNames, project.images[j].name.ToUpper());
                        if (index == -1)
                        {
                            watch.Stop();
                            rtbLog.AppendText(failWarning);
                            Clock();
                            if (lang == Language.Chinese)
                                MessageBox.Show("有的图像无相应的POS数据！");
                            else if (lang == Language.English)
                                MessageBox.Show("Some images have no corresponding POS data!");
                            return;
                        }
                        else
                        {
                            i = indexes[index];
                            double y = double.Parse(dt.Rows[i][1].ToString());
                            double x = double.Parse(dt.Rows[i][2].ToString());
                            double z = double.Parse(dt.Rows[i][3].ToString());
                            double pitch = double.Parse(dt.Rows[i][4].ToString());
                            double yaw = double.Parse(dt.Rows[i][5].ToString());
                            double roll = double.Parse(dt.Rows[i][6].ToString());
                            project.images[j]._POS = new POS(y, x, z, pitch, yaw, roll);
                            poses.Add(project.images[j]._POS);
                        }
                    }
                }
                catch
                {
                    watch.Stop();
                    rtbLog.AppendText(failWarning);
                    Clock();
                    if (lang == Language.Chinese)
                        MessageBox.Show("POS数据都要为数字,经纬度、角元素单位为°,高程单位为m！");
                    else if (lang == Language.English)
                        MessageBox.Show("POS data should be numbers. The unit of longitude, latitude and angular elements is °, that of elevation is m!");
                    return;
                }
                if (hasAE)
                {
                    ClearRoute();
                    DrawRoute();
                }
                else
                    pictureBox1.Refresh();
                ShowImageInfo(project.images[0]);
                project.saved = false;
                watch.Stop();
                移除图像ToolStripMenuItem.Enabled = false;
                rtbLog.AppendText(succeedInfo+"\n");
                Clock();
                地图结果ToolStripMenuItem_Click(sender, e);
                BuildDirectoryTree();
                MessageBox.Show(succeedInfo);
            }
        }

        /// <summary>
        /// 二分查找
        /// </summary>
        /// <param name="posNames">从小到大排序的pos名称数据</param>
        /// <param name="imgName">待查找的图像名称（大写）</param>
        /// <returns>找到的pos名称数据索引，-1为未找到</returns>
        private int BinarySearch(List<string> posNames, string imgName)
        {
            int left = 0, right = posNames.Count,middle=(left+right)/2,repeat=-1;
            while (repeat != middle)
            {
                repeat = middle;
                if (imgName.CompareTo(posNames[middle]) > 0)
                    left = middle;
                else if (imgName.CompareTo(posNames[middle]) < 0)
                    right = middle;
                else
                    return middle;
                middle = (left + right) / 2;
            }
            return -1;
        }

        /// <summary>
        /// 添加或打开POS后绘制航线
        /// </summary>
        private void DrawRoute()
        {
            string folder = System.IO.Path.GetDirectoryName(project.path) + "\\ZKGCPZD_POS";
            if (layers!=null&&layers.Count==2)
                return;
            layers = new List<IFeatureLayer>();
            System.IO.Directory.CreateDirectory(folder);
            ISpatialReferenceFactory srf = new SpatialReferenceEnvironmentClass();
            ISpatialReference sr;          
            if (images[0]._POS.proCode == -1)
                sr = srf.CreateGeographicCoordinateSystem((int)ESRI.ArcGIS.Geometry.esriSRGeoCSType.esriSRGeoCS_WGS1984);
            else
                sr = srf.CreateProjectedCoordinateSystem(project.images[0]._POS.proCode);
            IFeatureLayer fl1 = AEOperation.CreateFeatureLayer(folder + "\\POS.shp", sr, null, null, "P");
            IFeatureLayer fl2 = AEOperation.CreateFeatureLayer(folder + "\\Route.shp", sr, new string[] { "from","to","length" }, new string[] { "I","I","D" }, "L");
            IWorkspaceEdit wsEdit = AEOperation.GetEdit(folder);
            wsEdit.StartEditing(false);
            wsEdit.StartEditOperation();
            foreach (AddedImage img in images)
                AEOperation.AddFeature(fl1.FeatureClass, new Point3D[] { img._POS }, null);
            for (int i = 1; i < images.Count; i++)
                AEOperation.AddFeature(fl2.FeatureClass, new Point3D[] { images[i - 1]._POS, images[i]._POS }, new object[] { i,i+1,images[i - 1]._POS.GetDistance(images[i]._POS) });
            wsEdit.StopEditing(true);
            wsEdit.StopEditOperation();
        }

        /// <summary>
        /// 添加或打开POS后绘制航线
        /// </summary>
        private void DrawRoute(Graphics g)
        {
            SolidBrush pointBrush = new SolidBrush(Color.Red);
            Pen linePen = new Pen(Color.Green, 3);

            double dFieldHeigth = bbox.MaxPosition.y - bbox.MinPosition.y;
            double dFieldwidth = bbox.MaxPosition.x - bbox.MinPosition.x;
            double dHeigthRatio = pictureBox1.Size.Height / dFieldHeigth;
            double dWidthRatio = pictureBox1.Size.Width / dFieldwidth;

            PointF[] points = new PointF[images.Count];
            int count = 0;
            foreach (AddedImage img in images)
            {
                double tx = (img._POS.x - bbox.MinPosition.x) * dWidthRatio;
                double ty = (dFieldHeigth - img._POS.y + bbox.MinPosition.y) * dHeigthRatio;
                g.FillEllipse(pointBrush, (float)(tx - pointScale / 2), (float)(ty - pointScale / 2), (float)(pointScale), (float)(pointScale));
                points[count] = new PointF((float)tx, (float)ty);
                count++;
            }
            g.DrawLines(linePen, points);
        }

        /// <summary>
        /// 清除航线
        /// </summary>
        private void ClearRoute()
        {
            if (layers != null && layers.Count != 0)
            {
                IDataLayer lay = layers[0] as IDataLayer;
                IWorkspaceName name = ((IDatasetName)lay.DataSourceName).WorkspaceName;
                string path = name.PathName;
                for (int i = 0; i < layers.Count; i++)
                    (layers[i].FeatureClass as IDataset).Delete();
                System.IO.Directory.Delete(path);
            }
            layers = null;
        }

        /// <summary>
        /// 显示航线
        /// </summary>
        private void ShowRoute()
        {
            if (project.images==null||project.images.Count==0||project.images[0]._POS == null)
                return;
            IWorkspaceFactory xjWsF = new ShapefileWorkspaceFactoryClass();
            IWorkspaceFactoryLockControl control = xjWsF as IWorkspaceFactoryLockControl;
            control.DisableSchemaLocking();
            IFeatureWorkspace xjFWs = (IFeatureWorkspace)xjWsF.OpenFromFile(System.IO.Path.GetDirectoryName(project.path) + "\\ZKGCPZD_POS", 0);
            IFeatureClass fc1 = xjFWs.OpenFeatureClass("POS.shp");
            IFeatureLayer fl1 = new FeatureLayerClass();
            fl1.FeatureClass = fc1;
            fl1.Name = "POS";
            IFeatureClass fc2 = xjFWs.OpenFeatureClass("Route.shp");
            IFeatureLayer fl2 = new FeatureLayerClass();
            fl2.FeatureClass = fc2;
            fl2.Name = "Route";
            layers.Add(fl1); layers.Add(fl2);
            AEOperation.SetSymbol(fl1, new object[] { 255, 0, 0, ESRI.ArcGIS.Display.esriSimpleMarkerStyle.esriSMSSquare, 5.0 });
            AEOperation.SetSymbol(fl2, new object[] { 0, 255, 0, 2.5 });
            for (int i = layers.Count - 1; i >= 0; i--)
                axMapControl1.AddLayer(layers[i]);
            SwitchLayer(fl1);
            axMapControl1.Refresh();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (hasAE&&project != null && tabControl1.SelectedIndex == 1 && tabControl3.SelectedIndex == 2)
            {
                if (layers != null && layers.Count == 0)
                    ShowRoute();
                if(layer!=null&&layer.Name=="")
                    ShowGCPLayer();
            }
        }

        private void tabControl3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (hasAE&&project != null && tabControl1.SelectedIndex == 1 && tabControl3.SelectedIndex == 2)
            {
                if (layers != null && layers.Count == 0)
                    ShowRoute();
                if (layer != null && layer.Name == "")
                    ShowGCPLayer();
            }
        }
        #endregion

        #region 添加控制点
        private void 添加控制点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (图像裁剪ToolStripMenuItem.Text.Equals("结束图像裁剪") || 图像裁剪ToolStripMenuItem.Text.Equals("End Image Clipping"))
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("当前处于图像裁剪模式！");
                else if (lang == Language.English)
                    MessageBox.Show("Currently in image clipping mode!");
                return;
            }
            if (project == null || project.images == null || project.images.Count == 0)
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("请先添加图像数据，再添加控制点！");
                else if (lang == Language.English)
                    MessageBox.Show("Please add image data first, then add GCPs!");
                return;
            }
            if (添加控制点ToolStripMenuItem.Text.Equals("添加控制点") || 添加控制点ToolStripMenuItem.Text.Equals("Add GCP"))
            {
                if ((lang == Language.Chinese&&MessageBox.Show("开启控制点编辑功能将会覆盖之前添加的控制点，是否开启？", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.No)||
                    lang == Language.English && MessageBox.Show("Starting GCP editing function will overwrite the GCPs added before. Is it enabled?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
                project.GCPs = new List<GCP>();
                文件ToolStripMenuItem.Enabled = false;
                重建ToolStripMenuItem.Enabled = false;
                imageBox1.Dock = DockStyle.None;
                GCPsView = new DataGridView();
                tabControl1.TabPages[0].Controls.Add(GCPsView);
                GCPsView.Dock = DockStyle.Bottom;
                imageBox1.Dock = DockStyle.Fill;
                GCPsView.RowHeaderMouseDoubleClick += GCPsView_RowHeaderMouseDoubleClick;
                GCPsView.MouseDown += GCPsView_MouseDown;
                this.WindowState = FormWindowState.Maximized;
                this.MaximizeBox = false;
                DataTable dt = new DataTable();
                if (lang == Language.Chinese)
                {
                    添加控制点ToolStripMenuItem.Text = "停止添加控制点";
                    dt.Columns.Add("图像序号");
                    dt.Columns.Add("像素u坐标");
                    dt.Columns.Add("像素v坐标");
                    dt.Columns.Add("经度");
                    dt.Columns.Add("纬度");
                    dt.Columns.Add("高程");
                    dt.Columns.Add("颜色");
                }
                else if (lang == Language.English)
                {
                    添加控制点ToolStripMenuItem.Text = "Stop Adding GCP";
                    dt.Columns.Add("Order");
                    dt.Columns.Add("Pixel u");
                    dt.Columns.Add("Pixel v");
                    dt.Columns.Add("Longitude");
                    dt.Columns.Add("Latitude");
                    dt.Columns.Add("Elevation");
                    dt.Columns.Add("Pixel color");
                }
                GCPsView.DataSource = null;
                GCPsView.DataSource = dt;
                GCPsView.ReadOnly = true;
                for (int j = 0; j < GCPsView.Columns.Count; j++)
                    GCPsView.Columns[j].SortMode = DataGridViewColumnSortMode.Programmatic;
                GCPsView.Refresh();
                ShowImageInfo(project.images[0]);
                tabControl1.SelectedIndex = 0;
            }
            else
            {
                if ((lang == Language.Chinese && MessageBox.Show("未在两幅或以上图像中出现的控制点视为无效控制点，是否要继续添加控制点？", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)||
                    (lang == Language.English && MessageBox.Show("GCPs that do not appear in two or more images are regarded as invalid GCPs, whether to continue adding GCPs?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes))
                    return;
                文件ToolStripMenuItem.Enabled = true;
                重建ToolStripMenuItem.Enabled = true;
                tabControl1.TabPages[0].Controls.Remove(GCPsView);
                imageBox1.Dock = DockStyle.Fill;
                this.WindowState = FormWindowState.Normal;
                this.MaximizeBox = true;
                if (lang == Language.Chinese)
                    添加控制点ToolStripMenuItem.Text = "添加控制点";
                else if (lang == Language.English)
                    添加控制点ToolStripMenuItem.Text = "Add GCP";
                GCPsView = null;
                if (project.GCPs.Count > 0)
                {
                    if (imageBox1.Image != null)
                        imageBox1.Image.Dispose();
                    imageBox1.Image = null;
                    if (hasAE)
                    {
                        ClearGCPLayer();
                        DrawGCPLayer();
                    }
                    else
                        pictureBox1.Refresh();
                    InitiateImageDataWindow();
                    project.saved = false;
                    BuildDirectoryTree();
                    地图结果ToolStripMenuItem_Click(sender, e);
                }
            }
        }

        /// <summary>
        /// 绘制控制点
        /// </summary>
        private void DrawGCPLayer()
        {
            string folder = System.IO.Path.GetDirectoryName(project.path) + "\\ZKGCPZD_GCP";
            if (layer != null)
                return;
            System.IO.Directory.CreateDirectory(folder);
            ISpatialReferenceFactory srf = new SpatialReferenceEnvironmentClass();
            ISpatialReference sr;
            if (project.GCPs[0].proCode == -1)
                sr = srf.CreateGeographicCoordinateSystem((int)ESRI.ArcGIS.Geometry.esriSRGeoCSType.esriSRGeoCS_WGS1984);
            else
                sr = srf.CreateProjectedCoordinateSystem(project.GCPs[0].proCode); 
            IFeatureLayer fl = AEOperation.CreateFeatureLayer(folder + "\\GCP.shp", sr, new string[] { "x", "y", "z" }, new string[] { "D", "D", "D" }, "P");
            IWorkspaceEdit wsEdit = AEOperation.GetEdit(folder);
            wsEdit.StartEditing(false);
            wsEdit.StartEditOperation();
            foreach (GCP gcp in project.GCPs)
                AEOperation.AddFeature(fl.FeatureClass, new Point3D[] { gcp }, new object[] { gcp.x, gcp.y, gcp.z });  
            wsEdit.StopEditing(true);
            wsEdit.StopEditOperation();
            layer = new FeatureLayerClass();
        }

        /// <summary>
        /// 绘制控制点
        /// </summary>
        private void DrawGCPLayer(Graphics g)
        {
            SolidBrush pointBrush = new SolidBrush(Color.Blue);

            double dFieldHeigth = bbox.MaxPosition.y - bbox.MinPosition.y;
            double dFieldwidth = bbox.MaxPosition.x - bbox.MinPosition.x;
            double dHeigthRatio = pictureBox1.Size.Height / dFieldHeigth;
            double dWidthRatio = pictureBox1.Size.Width / dFieldwidth;

            foreach (GCP gcp in project.GCPs)
            {
                double tx = (gcp.x - bbox.MinPosition.x) * dWidthRatio;
                double ty = (dFieldHeigth - gcp.y + bbox.MinPosition.y) * dHeigthRatio;
                g.FillEllipse(pointBrush, (float)(tx - pointScale / 2), (float)(ty - pointScale / 2), (float)(pointScale), (float)(pointScale));
            }
        }

        /// <summary>
        /// 清除控制点
        /// </summary>
        private void ClearGCPLayer()
        {
            if (layer != null)
            {
                IDataLayer lay = layer as IDataLayer;
                if ((IDatasetName)lay.DataSourceName == null)
                    return;
                IWorkspaceName name = ((IDatasetName)lay.DataSourceName).WorkspaceName;
                string path = name.PathName;
                (layer.FeatureClass as IDataset).Delete();
                System.IO.Directory.Delete(path);
            }
            layer = null;
        }

        /// <summary>
        /// 显示控制点
        /// </summary>
        private void ShowGCPLayer()
        {
            if (project.GCPs == null || project.GCPs.Count == 0)
                return;
            IWorkspaceFactory xjWsF = new ShapefileWorkspaceFactoryClass();
            IFeatureWorkspace xjFWs = (IFeatureWorkspace)xjWsF.OpenFromFile(System.IO.Path.GetDirectoryName(project.path) + "\\ZKGCPZD_GCP", 0);
            IFeatureClass fc= xjFWs.OpenFeatureClass("GCP.shp");
            IFeatureLayer fl = new FeatureLayerClass();
            fl.FeatureClass = fc;
            fl.Name = "GCP";
            AEOperation.SetSymbol(fl, new object[] { 0,0,255, ESRI.ArcGIS.Display.esriSimpleMarkerStyle.esriSMSCircle, 3.0 });
            layer = fl;
            SwitchLayer(layer);
            axMapControl1.AddLayer(layer);
            axMapControl1.Refresh();
        }

        private void GCPsView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if ((lang == Language.Chinese&&MessageBox.Show("是否确定删除选中的控制点？", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.No)||
                    (lang == Language.English&&MessageBox.Show("Are you sure to delete the selected GCPs?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.No))
                    return;
                DataTable dt = (DataTable)GCPsView.DataSource;
                List<int> deletingRows = new List<int>();
                for (int i = 0; i < GCPsView.Rows.Count; i++)
                {
                    if (GCPsView.Rows[i].Selected && i < dt.Rows.Count)
                        deletingRows.Add(i);
                }
                int count = 0;
                foreach (int index in deletingRows)
                {
                    dt.Rows[index - count].Delete();
                    project.GCPs.RemoveAt(index - count);
                    count++;
                }
                ShowImageInfo(project.images[0]);
            }
        }

        private void GCPsView_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataTable dt = (DataTable)GCPsView.DataSource;
            if (e.RowIndex > dt.Rows.Count - 1)
                return;
            int order = project.GCPs[e.RowIndex].imageOrder;
            if (!txtImageOrder.Text.Equals(order.ToString()))
                ShowImageInfo(project.images[order - 1]);
            Size boxSize = imageBox1.Size;
            imageBox1.SetZoomScale(0.5, new System.Drawing.Point(boxSize.Width / 2, boxSize.Height / 2));
            Size realSize = new Size();
            PointF upperLeft = GetUpperLeftPoint(ref realSize);
            Size imageSize = imageBox1.Image.Size;
            PointF pixelPoint = project.GCPs[e.RowIndex].pixelPoint;
            PointF mousePoint = new PointF((pixelPoint.X + 1) * realSize.Width / imageSize.Width + upperLeft.X - 1, (pixelPoint.Y + 1) * realSize.Height / imageSize.Height + upperLeft.Y - 1);
            int horizontalScrollBarValue = imageBox1.HorizontalScrollBar.Visible ? imageBox1.HorizontalScrollBar.Value : 0;
            int verticalScrollBarValue = imageBox1.VerticalScrollBar.Visible ? imageBox1.VerticalScrollBar.Value : 0;
            mousePoint.X -= horizontalScrollBarValue;
            mousePoint.Y -= verticalScrollBarValue;
            imageBox1.SetZoomScale(10, new System.Drawing.Point((int)(mousePoint.X * imageBox1.ZoomScale), (int)(mousePoint.Y * imageBox1.ZoomScale)));
        }

        /// <summary>
        /// 显示图像的控制点
        /// </summary>
        private void ShowGCPs()
        {
            int order = int.Parse(txtImageOrder.Text);
            Image<Bgr, byte> img = imageBox1.Image as Image<Bgr, byte>;
            foreach (GCP gcp in project.GCPs)
            {
                if (gcp.imageOrder == order)
                {
                    CircleF circle = new CircleF(gcp.pixelPoint, 5);
                    img.Draw(circle, new Bgr(0, 255, 255), 3);
                }
            }
        }

        /// <summary>
        /// 检验控制点是否重复
        /// </summary>
        /// <param name="gcp">待检验控制点</param>
        /// <returns>true为重复，false为不重复</returns>
        private bool GCPRepeat(GCP gcp)
        {
            foreach (GCP gcp2 in project.GCPs)
            {
                if (gcp.imageOrder == gcp2.imageOrder && gcp.x == gcp2.x && gcp.y == gcp2.y && gcp.z == gcp2.z)
                    return true;
            }
            return false;
        }
        #endregion

        #region 图像裁剪
        private void 图像裁剪ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GCPsView != null)
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("当前处于控制点编辑状态！");
                else if (lang == Language.English)
                    MessageBox.Show("Currently in editting state of GCPs!");
                return;
            }
            if (project == null || project.images == null || project.images.Count == 0)
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("未打开任何工程文件或工程文件中不包含任何图像文件！");
                else if (lang == Language.English)
                    MessageBox.Show("No project file is opened or no image files are included in the project file!");
                return;
            }
            if (图像裁剪ToolStripMenuItem.Text.Equals("图像裁剪") || 图像裁剪ToolStripMenuItem.Text.Equals("Clip Image"))
            {
                ShowImageInfo(project.images[0]);
                if (lang == Language.Chinese)
                {
                    MessageBox.Show("已开启手动拾取模式，请在图像上点击两个点来确定图像裁剪范围！");
                    图像裁剪ToolStripMenuItem.Text = "结束图像裁剪";
                }
                else if (lang == Language.English)
                {
                    MessageBox.Show("Manual pickup mode is enabled, please click two points on the image to determine the image clipping range!");
                    图像裁剪ToolStripMenuItem.Text = "End Image Clipping";
                }
                重建ToolStripMenuItem.Enabled = false;
                文件ToolStripMenuItem.Enabled = false;
                移除图像ToolStripMenuItem.Enabled = false;
            }
            else
            {
                重建ToolStripMenuItem.Enabled = true;
                文件ToolStripMenuItem.Enabled = true;
                移除图像ToolStripMenuItem.Enabled = true;
                if (lang == Language.Chinese)
                {
                    图像裁剪ToolStripMenuItem.Text = "图像裁剪";
                    MessageBox.Show("已关闭手动拾取模式！");
                }
                else if (lang == Language.English)
                {
                    图像裁剪ToolStripMenuItem.Text = "Clip Image";
                    MessageBox.Show("Manual pickup mode is off！");
                }
            } 
        }

        /// <summary>
        /// 两点确定一个矩形
        /// </summary>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <returns>矩形范围</returns>
        private Rectangle GetRectBy2Pts(System.Drawing.Point point1, System.Drawing.Point point2)
        {
            Rectangle rect;
            if (point1.X > point2.X)
            {
                if (point1.Y > point2.Y)
                    rect = new Rectangle(point2, new Size(point1.X - point2.X + 1, point1.Y - point2.Y + 1));
                else
                    rect = new Rectangle(point2.X, point1.Y, point1.X - point2.X + 1, point2.Y - point1.Y + 1);
            }
            else
            {
                if (point1.Y > point2.Y)
                    rect = new Rectangle(point1.X, point2.Y, point2.X - point1.X + 1, point1.Y - point2.Y + 1);
                else
                    rect = new Rectangle(point1, new Size(point2.X - point1.X + 1, point2.Y - point1.Y + 1));
            }
            return rect;
        }       
        #endregion

        #region 移除图像
        private void 移除图像ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GCPsView != null)
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("当前处于控制点编辑状态！");
                else if (lang == Language.English)
                    MessageBox.Show("Currently in editting state of GCPs!");
                return;
            }
            if (imageBox1.Image == null)
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("当前图像为空，无法移除！");
                else if (lang == Language.English)
                    MessageBox.Show("The current image is empty, cannot be removed!");
                return;
            }
            if(project.GCPs!=null&&project.GCPs.Count!=0)
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("已添加控制点，无法移除！");
                else if (lang == Language.English)
                    MessageBox.Show("GCPs have been added, cannot be removed!");
                return;
            }
            if (project.images != null && project.images.Count == 2)
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("图像数量达到最小值，无法移除！");
                else if (lang == Language.English)
                    MessageBox.Show("The number of images has reached the minimum value, cannot be removed!");
                return;
            }
            if (project.images[0]._POS != null)
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("已添加POS，无法移除！");
                else if (lang == Language.English)
                    MessageBox.Show("POS data has been added, cannot be removed!");
                return;
            }
            int index = int.Parse(txtImageOrder.Text) - 1;
            string name = project.images[index].name;
            string path = project.images[index].path;
            if ((lang == Language.Chinese&&MessageBox.Show("是否确定移除图像\"" + name + "\"？", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.No)||
                (lang == Language.English) && MessageBox.Show("Whether to remove the image \"" + name + "\"？", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            project.images.RemoveAt(index);
            FileInfo file = new FileInfo(path);
            file.Delete();
            for (int i = 0; i < project.images.Count; i++)
                project.images[i].order = i + 1;
            BuildDirectoryTree();
            ShowImageInfo(project.images[0]);
            project.saved = false;
            ProjectToFile();
            if (lang == Language.Chinese)
                MessageBox.Show("图像\"" + name + "\"移除成功！");
            else if (lang == Language.English)
                MessageBox.Show("The image \"" + name + "\" was removed successfully！");
        }
        #endregion

        #endregion

        #region 三维模型绘制
        /// <summary>
        /// 相机方向
        /// </summary>
        public enum CameraDirection
        {
            Top,
            Bottom,
            Left,
            Right,
            Front,
            Back
        }

        /// <summary>
        /// 初始化三维视图
        /// </summary>
        private void InitiateGLWindow()
        {
            RemoveGLWindowControl();
            var position = new vec3(0, 0, 1);
            var center = new vec3(0, 0, 0);
            var up = new vec3(0, 1, 0);
            var camera = new CSharpGL.Camera(position, center, up, CameraType.Ortho, this.winGLCanvas1.Width, this.winGLCanvas1.Height);
            scene = new CSharpGL.Scene(camera);
            cameraDirection = CameraDirection.Top;
        }

        /// <summary>
        /// 绘制三维模型
        /// </summary>
        /// <param name="node">三维模型</param>
        private void BeginDrawing(Model3DNode node)
        {
            ResizeGLWindow();
            ClearLastMeasurement();
            RemoveGLWindowControl();
            txtVertex.Text = node.GetVertCount().ToString();
            txtFace.Text = node.GetFaceCount().ToString();
            box = node.GetBBox();
            node.bboxCenter = box.center;
            float zoom = ComputeZoom();
            node.Scale = new vec3(zoom, zoom, zoom);
            scene.RootNode = node;
            AddGLWindowControl();
            var list = new ActionList();
            var transformAction = new TransformAction(scene);
            list.Add(transformAction);
            var renderAction = new RenderAction(scene);
            list.Add(renderAction);
            actionList = list;
        }

        /// <summary>
        /// 添加三维视图控制
        /// </summary>
        private void AddGLWindowControl()
        {
            if (scene != null && scene.RootNode != null)
            {
                ball = new ArcBallManipulater(GLMouseButtons.Left);
                ball.MouseSensitivity = 2;
                ball.Bind(scene.Camera, this.winGLCanvas1);
                ball.Rotated += manipulater_Rotated;
                tran = new TranslateManipulater(scene.RootNode, GLMouseButtons.Right);
                tran.MouseSensitivity = 2;
                tran.Bind(scene.Camera, this.winGLCanvas1);
                winGLCanvas1.MouseWheel += openGLControl1_MouseWheel;
            }
        }

        /// <summary>
        /// 移除三维视图控制
        /// </summary>
        private void RemoveGLWindowControl()
        {
            if (scene != null && scene.RootNode != null&&ball!=null&&tran!=null)
            {
                ball.Unbind();
                tran.Unbind();
                GL.Instance.Clear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT | GL.GL_STENCIL_BUFFER_BIT);
                winGLCanvas1.MouseWheel -= openGLControl1_MouseWheel;
                scene.RootNode = null;
                actionList = null;
            }
            txtFace.Text = "";
            txtVertex.Text = "";
        }

        private void manipulater_Rotated(object sender, CSharpGL.ArcBallManipulater.Rotation e)
        {
            SceneNodeBase node = scene.RootNode;
            node.RotationAngle = e.angleInDegree;
            node.RotationAxis = e.axis;
        }

        private void winGLCanvas1_Resize_1(object sender, EventArgs e)
        {
            ResizeGLWindow();
        }

        /// <summary>
        /// 缩放三维视图
        /// </summary>
        private void ResizeGLWindow()
        {
            if (scene != null)
            {
                if (scene.RootNode != null)
                {
                    float zoom = ComputeZoom();
                    scene.RootNode.Scale = new vec3(zoom, zoom, zoom);
                }
                if (scene.Camera != null)
                {
                    scene.Camera.Bottom = -this.winGLCanvas1.Height / 2.0f;
                    scene.Camera.Left = -this.winGLCanvas1.Width / 2.0f;
                    scene.Camera.Top = this.winGLCanvas1.Height / 2.0f;
                    scene.Camera.Right = this.winGLCanvas1.Width / 2.0f;
                }
            }
        }

        private void winGLCanvas1_OpenGLDraw(object sender, PaintEventArgs e)
        {
            ActionList list = this.actionList;
            if (list != null)
            {
                GL.Instance.ClearColor(0, 0, 0, 255);
                GL.Instance.Clear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT | GL.GL_STENCIL_BUFFER_BIT);
                list.Act(new ActionParams(Viewport.GetCurrent()));
            }
        }

        private void openGLControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            var scene = this.scene;
            if (scene != null&&scene.RootNode!=null)
            {
                float zoom=scene.RootNode.Scale[0];
                float delta = e.Delta / 8.0f;
                float c_defaultDeg2Zoom = 20.0f;
                float zoomFactor = (float)Math.Pow(1.1f, delta/ c_defaultDeg2Zoom);
                if (zoomFactor > 0.0f && zoomFactor != 1.0f)
                    zoom=zoom*zoomFactor;
                scene.RootNode.Scale = new vec3(zoom, zoom, zoom);
            }
        }

        /// <summary>
        /// 计算相机缩放比例
        /// </summary>
        /// <returns>缩放比例</returns>
        private float ComputeZoom()
        {
            float rx=0, ry=0;
            int width=this.winGLCanvas1.Width,height=this.winGLCanvas1.Height;
            switch (cameraDirection)
            {
                case CameraDirection.Front:
                case CameraDirection.Back:
                    rx = width / (box.length.y * 1.01f);
                    ry = height / (box.length.z * 1.01f);
                    break;
                case CameraDirection.Left:
                case CameraDirection.Right:
                    rx = width / (box.length.x * 1.01f);
                    ry = height / (box.length.z * 1.01f);
                    break;
                case CameraDirection.Top:
                case CameraDirection.Bottom:
                    rx = width / (box.length.x * 1.01f);
                    ry = height / (box.length.y * 1.01f);
                    break;
            }
            return rx < ry ? rx : ry;
        }

        /// <summary>
        /// 切换相机位置
        /// </summary>
        /// <param name="direction">相机方向</param>
        private void SwitchCameraView(CameraDirection direction)
        {
            if (scene != null && scene.RootNode != null)
            {
                cameraDirection = direction;
                float zoom=ComputeZoom();
                scene.RootNode.Scale = new vec3(zoom, zoom, zoom);
                mat4 totalRotation = mat4.identity();
                switch (direction)
                {
                    case CameraDirection.Front:
                        totalRotation = glm.rotate(totalRotation, -90, new vec3(0, 0, 1));
                        totalRotation = glm.rotate(totalRotation, -90, new vec3(0, 1, 0));
                        break;
                    case CameraDirection.Back:
                        totalRotation = glm.rotate(totalRotation, 90, new vec3(0, 0, 1));
                        totalRotation = glm.rotate(totalRotation, 90, new vec3(0, 1, 0));
                        break;
                    case CameraDirection.Left:
                        totalRotation = glm.rotate(totalRotation, -90, new vec3(1, 0, 0));
                        break;
                    case CameraDirection.Right:
                        totalRotation = glm.rotate(totalRotation, 180, new vec3(0, 0, 1));
                        totalRotation = glm.rotate(totalRotation, 90, new vec3(1, 0, 0));
                        break;
                    case CameraDirection.Bottom:
                        totalRotation = glm.rotate(totalRotation, 180, new vec3(0, 1, 0));
                        break;
                }
                scene.RootNode.WorldPosition = new vec3(0, 0, 0);
                ball.SetRotationMatrix(totalRotation);
                float angleInDegree;
                vec3 axis;
                totalRotation.ToQuaternion().Parse(out angleInDegree, out axis);
                scene.RootNode.RotationAngle = angleInDegree;
                scene.RootNode.RotationAxis = axis;
            }
        }

        private void topToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SwitchCameraView(CameraDirection.Top);
        }

        private void bottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SwitchCameraView(CameraDirection.Bottom);
        }

        private void leftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SwitchCameraView(CameraDirection.Left);
        }

        private void rightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SwitchCameraView(CameraDirection.Right);
        }

        private void frontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SwitchCameraView(CameraDirection.Front);
        }

        private void backToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SwitchCameraView(CameraDirection.Back);
        }

        private void fullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (scene != null && scene.RootNode != null)
            {
                float zoom = ComputeZoom();
                scene.RootNode.Scale = new vec3(zoom, zoom, zoom);
                scene.RootNode.WorldPosition = new vec3(0, 0, 0);
            }
        }
        #endregion

        #region 三维重建
        #region 特征检测
        private void 特征点检测ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!project.images[0].clipped)
            {
                if ((lang == Language.Chinese&&MessageBox.Show("工具箱中的图像裁剪工具可以加快后续操作速度及提高精度，是否需要使用？", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)||
                    (lang == Language.English && MessageBox.Show("The Clip Image tool in the toolbox can speed up the follow-up operation and improve the accuracy, whether to use？", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes))
                {
                    图像裁剪ToolStripMenuItem_Click(sender, e);
                    return;
                }
            }
            watch.Restart();
            图像裁剪ToolStripMenuItem.Enabled = false;
            特征点检测ToolStripMenuItem1.Enabled = false;
            移除图像ToolStripMenuItem.Enabled = false;
            if (lang == Language.Chinese)
            {
                rtbLog.AppendText("- - - -图像特征点检测- - - -\n");
                if (project.ftType == FeatureType.SIFT)
                    rtbLog.AppendText("采用 ChangChang Wu->SiftGPU\n");
            }
            else if (lang == Language.English)
            {
                rtbLog.AppendText("- - - -Feature Point Detection- - - -\n");
                if (project.ftType == FeatureType.SIFT)
                    rtbLog.AppendText("Use ChangChang Wu->SiftGPU\n");
            }
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = project.images.Count;
            progressBar1.Visible = true;
            string dir = System.IO.Path.GetDirectoryName(project.path) + "\\ft";
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            project.ftPaths = new List<string>();
            Thread thread1 = new Thread(new ThreadStart(FeatureDetect));
            thread1.IsBackground = true;
            thread1.Start();
        }

        /// <summary>
        /// 特征检测
        /// </summary>
        private void FeatureDetect()
        {
            int current = 0;
            string dir = System.IO.Path.GetDirectoryName(project.path) + "\\ft";
            string imgDir = System.IO.Path.GetDirectoryName(project.path) + "\\img";
            if (project.ftType == FeatureType.SIFT)
            {
                foreach (AddedImage image in project.images)
                {
                    string savePath = dir + "\\" + image.order + ".ft";
                    project.ftPaths.Add(savePath);
                    int featureCount;
                    if (image.clipped)
                    {
                        Image<Gray, byte> img2 = new Image<Gray, byte>(image.path);
                        img2.ROI = image.ROI;
                        string imgPath = imgDir + "\\" + System.IO.Path.GetFileNameWithoutExtension(image.path) + "_clipped.jpg";
                        img2.Save(imgPath);
                        img2.Dispose();
                        featureCount=ExternLibInvoke.SIFTDetect(imgPath, savePath);
                    }
                    else
                        featureCount = ExternLibInvoke.SIFTDetect(image.path, savePath);
                    if (lang == Language.Chinese)
                        AppendText("图像" + image.name + "成功检测：" + featureCount + "个特征点\n");
                    else if (lang == Language.English)
                        AppendText("Image " + image.name + " detected:" + featureCount + " feature points\n");
                    current++;
                    SetPbValue(current);
                }
            }
            else
            {
                foreach (AddedImage image in project.images)
                {
                    string savePath = dir + "\\" + image.order + ".ft";
                    project.ftPaths.Add(savePath);
                    Image<Gray, byte> img = new Image<Gray, byte>(image.path);
                    if (image.clipped)
                        img.ROI = image.ROI;
                    VectorOfKeyPoint points = new VectorOfKeyPoint();
                    Mat dp = new Mat();
                    FtPointAnalyseTool.OrbDetect(img, points, dp);
                    if (image.clipped)
                        points=FtPointAnalyseTool.FeaturePointsCorrection(points, image.ROI);
                    int featureCount = points.Size;
                    FtPointAnalyseTool.SaveOrbFile(savePath, points, dp);
                    if (lang == Language.Chinese)
                        AppendText("图像" + image.name + "成功检测：" + featureCount + "个特征点\n");
                    else if (lang == Language.English)
                        AppendText("Image " + image.name + " detected:" + featureCount + " feature points\n");
                    current++;
                    SetPbValue(current);
                }
            }
            watch.Stop();
            Clock();
            BuildDirectoryTree();
            特征点匹配ToolStripMenuItem1.Enabled = true;
            if (lang == Language.Chinese)
                MessageBox.Show("特征点检测成功！");
            else if (lang == Language.English)
                MessageBox.Show("The feature points were detected successfully！");
            SetPbValue(-1);
        }
        #endregion

        #region 特征匹配
        private void 特征点匹配ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (project.images[0]._POS==null&&
                ((lang == Language.Chinese&&MessageBox.Show("工具箱中的添加POS工具(无角元素角元素设置为-361)可以加快后续匹配速度，是否需要使用？", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)||
                (lang == Language.English && MessageBox.Show("The Add POS tool in the toolbox (the angular elements are set to -361 if no) can speed up the subsequent matching, whether to use it?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)))
            {
                添加POSToolStripMenuItem_Click(sender, e);
                return;
            }
            watch.Restart();
            添加POSToolStripMenuItem.Enabled = false;
            特征点匹配ToolStripMenuItem1.Enabled = false;
            if (lang == Language.Chinese)
                rtbLog.AppendText("- - - -图像特征点匹配- - - -\n");
            else if (lang == Language.English)
                rtbLog.AppendText("- - - -Feature Point Matching- - - -\n");
            project.TVMs = new List<TwoViewModel>();
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = project.images.Count;
            progressBar1.Visible = true;
            Thread thread = new Thread(new ThreadStart(SetMatches));
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 将所有相关图像匹配并保存在选定路径
        /// </summary>
        private void SetMatches()
        {
            string dir = System.IO.Path.GetDirectoryName(project.path) + "\\rel";
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            if (project.images.Count >= 7)
            {
                double threshold;
                if (project.images[0]._POS != null)
                    threshold = FtPointAnalyseTool.GetThreshold(project);
                else
                    threshold = -1;
                Thread thread1 = new Thread(new ParameterizedThreadStart(PMatch));
                Thread thread2 = new Thread(new ParameterizedThreadStart(PMatch));
                Thread thread3 = new Thread(new ParameterizedThreadStart(PMatch));
                thread1.IsBackground = true;
                MatchParameter param1 = new MatchParameter(0, project.images.Count / 3, threshold);
                thread1.Start(param1);
                thread2.IsBackground = true;
                MatchParameter param2 = new MatchParameter(project.images.Count / 3 + 1, project.images.Count * 2 / 3, threshold);
                thread2.Start(param2);
                thread3.IsBackground = true;
                MatchParameter param3 = new MatchParameter(project.images.Count * 2 / 3 + 1, project.images.Count - 1, threshold);
                thread3.Start(param3);
                while (thread1.IsAlive || thread2.IsAlive || thread3.IsAlive) { }
                project.TVMs.Sort();
            }
            else
            {
                MatchParameter param = new MatchParameter(0, project.images.Count - 1, double.MaxValue);
                PMatch(param);
            }
            watch.Stop();   
            BuildDirectoryTree();
            if (lang == Language.Chinese)
            {
                AppendText("一共匹配：" + project.TVMs.Count + "对\n");
                Clock();
                MessageBox.Show("特征点匹配成功！");
            }
            else if (lang == Language.English)
            {
                AppendText("Matching " + project.TVMs.Count + " pairs in total\n");
                Clock();
                MessageBox.Show("The feature points were matched successfully!");
            }
            稀疏重建ToolStripMenuItem1.Enabled = true;         
            SetPbValue(-1);
        }

        /// <summary>
        /// 多线程匹配
        /// </summary>
        /// <param name="param">匹配参数</param>
        private void PMatch(object param)
        {
            MatchParameter match = (MatchParameter)param;
            for (int i = match.fromIndex; i <= match.toIndex; i++)
            {
                List<int> matchpic_index = FtPointAnalyseTool.GetMatchPicture(project,i, match.threshold);
                for (int j = 0; j < matchpic_index.Count; j++)
                {
                    int image1 = i + 1;
                    int image2 = matchpic_index[j] + 1;
                    string savePath = System.IO.Path.GetDirectoryName(project.path) + "\\rel\\" + image1 + "-" + image2 + ".rel";
                    int matchesCount = FtPointAnalyseTool.MatchPictures(project,i, matchpic_index[j], savePath);
                    if (matchesCount > 0)
                    {
                        lock (rtbLog)
                        {
                            if (lang == Language.Chinese)
                                AppendText(project.images[i].name + "-" + project.images[matchpic_index[j]].name + "成功匹配：" + matchesCount + "对特征点对\n");
                            else if (lang == Language.English)
                                AppendText(project.images[i].name + "-" + project.images[matchpic_index[j]].name + " matched:" + matchesCount + " pairs\n");
                        }
                        bool equal = project.images[i].camera.focalLength == project.images[matchpic_index[j]].camera.focalLength;
                        project.TVMs.Add(new TwoViewModel(savePath, matchesCount, equal));
                    }    
                }
                lock(progressBar1)
                    SetPbValue(progressBar1.Value+1);
            }
        }

        #endregion

        #region 稀疏重建
        private void 稀疏重建ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (project.GCPs == null || project.GCPs.Count<6)
            {
                if ((lang == Language.Chinese&&MessageBox.Show("控制点数量<3将会严重影响模型尺度，是否需要使用工具箱中的添加控制点工具？", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes)||
                    (lang == Language.English && MessageBox.Show("The number of GCPs <3 will seriously affect the scale of the model. Is it necessary to use Add GCP tool in the toolbox?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.Yes))
                {
                    添加控制点ToolStripMenuItem_Click(sender, e);
                    return;
                }
            }
            SaveFileDialog save = new SaveFileDialog();
            if (lang == Language.Chinese)
            {
                save.Filter = "ply文件|*.ply";
                save.Title = "稀疏点云导出到";
            }
            else if (lang == Language.English)
            {
                save.Filter = "ply File|*.ply";
                save.Title = "Export Sparse Cloud To";
            }
            if (save.ShowDialog() == DialogResult.OK)
            {
                watch.Restart();
                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 3;
                progressBar1.Visible = true;
                添加控制点ToolStripMenuItem.Enabled = false;
                稀疏重建ToolStripMenuItem1.Enabled = false;
                if (lang == Language.Chinese)
                    rtbLog.AppendText("- - - -稀疏重建- - - -\n");
                else if (lang == Language.English)
                    rtbLog.AppendText("- - - -Sparse Reconstruction- - - -\n");
                Thread thread1 = new Thread(new ParameterizedThreadStart(RelativeOrientation));
                thread1.IsBackground = true;
                thread1.Start(save.FileName);
            }
        }

        /// <summary>
        /// 相对定向
        /// </summary>
        /// <param name="savePath">点云保存路径</param>
        private void RelativeOrientation(object savePath)
        {
            if (lang == Language.Chinese)
                AppendText("正在进行相对定向...\n");
            else if (lang == Language.English)
                AppendText("Relative orientation in progress...\n");
            SetPbValue(1);
            string dir = System.IO.Path.GetDirectoryName(project.path);

            if (!FtPointAnalyseTool.SparseReconstruction(project, (string)savePath))
            {
                watch.Stop();
                if (lang == Language.Chinese)
                    AppendText("相对定向初始化失败" + "\n");
                else if (lang == Language.English)
                    AppendText("Initialization failed" + "\n");
                Clock();
                添加控制点ToolStripMenuItem.Enabled = true;
                特征点匹配ToolStripMenuItem1.Enabled = true;
                DirectoryInfo relDir = new DirectoryInfo(dir + "\\rel");
                foreach (FileInfo file in relDir.GetFiles())
                    file.Delete();
                if (lang == Language.Chinese)
                    MessageBox.Show("匹配文件残缺，无法进行相对定向！");
                else if (lang == Language.English)
                    MessageBox.Show("The matching files are incomplete, relative orientation is interrupted!");
                SetPbValue(-1);
                return;
            }
            if (lang == Language.Chinese)
                AppendText("正在删除中间文件...\n");
            else if (lang == Language.English)
                AppendText("Deleting temporary files...\n");
            SetPbValue(2);
            if (lang == Language.Chinese)
                AppendText("正在加载稀疏点云数据...\n");
            else if (lang == Language.English)
                AppendText("Loading sparse cloud...\n");
            project.sparseCloud=(string)savePath;
            SetPbValue(3);
            watch.Stop();
            BuildDirectoryTree();
            if (lang == Language.Chinese)
                AppendText("点云保存路径：" + project.sparseCloud + "\n");
            else if (lang == Language.English)
                AppendText("Save path of cloud:" + project.sparseCloud + "\n");
            Clock();
            project.saved = false;
            ProjectToFile();
            稠密重建ToolStripMenuItem.Enabled = true;
            if (lang == Language.Chinese)
                MessageBox.Show("稀疏重建成功！");
            else if (lang == Language.English)
                MessageBox.Show("Sparse reconstruction succeeded!");
            SetPbValue(-1);
        }
        #endregion

        #region 稠密重建
        private void 稠密重建ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string outPath = System.IO.Path.GetDirectoryName(project.path) + "\\bundle.rd.out";
            if (!File.Exists(outPath))
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("未找到\"" + outPath + "\"文件，无法进行稠密重建！");
                else if (lang == Language.English)
                    MessageBox.Show("File \"" + outPath + "\"is not found, dense reconstruction is disable!");
                return;
            }
            SaveFileDialog save = new SaveFileDialog();
            if (lang == Language.Chinese)
            {
                save.Filter = "ply文件|*.ply";
                save.Title = "稠密点云导出到";
            }
            else if (lang == Language.English)
            {
                save.Filter = "ply File|*.ply";
                save.Title = "Export Dense Cloud To";
            }
            if (save.ShowDialog() == DialogResult.OK)
            {
                watch.Restart();
                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 4;
                progressBar1.Visible = true;
                project.nvm.mps=null;
                project.nvm.tracks = null;
                稠密重建ToolStripMenuItem.Enabled = false;
                if (lang == Language.Chinese)
                {
                    rtbLog.AppendText("- - - -稠密重建- - - -\n");
                    rtbLog.AppendText("正在创建稠密重建环境...\n");
                }
                else if (lang == Language.English)
                {
                    rtbLog.AppendText("- - - -Dense Reconstruction- - - -\n");
                    rtbLog.AppendText("Creating a dense reconstruction environment...\n");
                }
                string proDir = System.IO.Path.GetDirectoryName(project.path);
                string dataDir = proDir + "\\data";
                string rootDir = dataDir + "\\";
                System.IO.Directory.CreateDirectory(dataDir);
                System.IO.Directory.CreateDirectory(rootDir + "models");
                System.IO.Directory.CreateDirectory(rootDir + "txt");
                System.IO.Directory.CreateDirectory(rootDir + "visualize");
                FileInfo file = new FileInfo(proDir + "\\bundle.rd.out");
                file.CopyTo(rootDir + "bundle.rd.out");
                file.Delete();
                int count = 0;
                foreach (int ind in project.nvm.img2cam.Keys)
                {
                    AddedImage img = project.images[ind];
                    FileInfo imgFile = new FileInfo(img.path);
                    imgFile.CopyTo(rootDir + "visualize\\" + count.ToString("00000000") + ".jpg");//采用畸变参数进行畸变校正
                    StreamWriter sw = new StreamWriter(rootDir + "txt\\" + count.ToString("00000000") + ".txt");
                    sw.WriteLine("CONTOUR");
                    ME P = new ME(3, 4, DepthType.Cv64F);
                    Camera cam = project.nvm.cameras[project.nvm.img2cam[ind]];
                    P.CombineR_t(cam.R, cam.T);
                    ME H = cam.GetIntrinsicMatrix()* P;
                    for (int i = 0; i < 3; i++)
                        sw.WriteLine(H[i, 0] + " " + H[i, 1] + " " + H[i, 2] + " " + H[i, 3]);
                    sw.Flush();
                    sw.Close();
                    count++;
                }
                SetPbValue(progressBar1.Value + 1);
                if (lang == Language.Chinese)
                {
                    rtbLog.AppendText("正在进行密集匹配...\n");
                    rtbLog.AppendText("采用 Yasutaka Furukawa->CMVS/PMVS\n");
                }
                else if (lang == Language.English)
                {
                    rtbLog.AppendText("Dense matching in progress...\n");
                    rtbLog.AppendText("Use Yasutaka Furukawa->CMVS/PMVS\n");
                }
                Thread thread1 = new Thread(new ParameterizedThreadStart(DenseReconstruction));
                thread1.IsBackground = true;
                thread1.Start(save.FileName);
            }
        }

        /// <summary>
        /// 稠密重建
        /// </summary>
        /// <param name="savePath">点云保存路径</param>
        private void DenseReconstruction(object savePath)
        {
            string dataDir = System.IO.Path.GetDirectoryName(project.path) + "\\data";
            string rootDir = dataDir + "\\";
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.StandardInput.WriteLine("cd " + Application.StartupPath + "\\CMVS_PMVS");
            p.StandardInput.WriteLine("cmvs " + rootDir + " 50 4");
            p.StandardInput.WriteLine("genOption " + rootDir + " 1 2 0.700000 7 2 4");
            p.StandardInput.WriteLine("pmvs2 " + rootDir + " option-0000&exit");
            p.StandardInput.AutoFlush = true;
            p.WaitForExit();
            p.Close();
            SetPbValue(progressBar1.Value + 1);
            string GCPwarning = "";
            if (lang == Language.Chinese)
            {
                AppendText("正在进行绝对定向...\n");
                GCPwarning = "控制点信息不足，导致无法进行绝对定向\n";
            }

            else if (lang == Language.English)
            {
                AppendText("Absolute orientation in progress...\n");
                GCPwarning = "Insufficient GCPs, absolute orientation failed\n";
            }
            int proCode = project.GetProjectionCode();
            if (proCode == -1)
            {
                AppendText(GCPwarning);
                FileInfo plyFile = new FileInfo(rootDir + "models\\option-0000.ply");
                plyFile.CopyTo((string)savePath, true);
            }
            else
            {
                ISpatialReference geo =null;
                ISpatialReference prj=null;
                if (hasAE)
                {
                    ISpatialReferenceFactory srf = new SpatialReferenceEnvironmentClass();
                    geo= srf.CreateGeographicCoordinateSystem((int)ESRI.ArcGIS.Geometry.esriSRGeoCSType.esriSRGeoCS_WGS1984);
                    prj = srf.CreateProjectedCoordinateSystem(proCode);
                }
                List<GeoPoint> worldPts = new List<GeoPoint>();
                List<Point3D> modelPts = new List<Point3D>();
                if (project.GCPs != null)
                {
                    foreach (GCP gcp in project.GCPs)
                    {
                        if (gcp.modelPoint != null)
                        {
                            GeoPoint gcp2 = gcp.Clone();
                            if (hasAE)
                                gcp2.ProjectPoint(geo, prj, proCode);
                            else
                                gcp2.ProjectPoint(proCode);
                            worldPts.Add(gcp2);
                            modelPts.Add(gcp.modelPoint.Clone());
                        }
                    }
                }
                string outPath = rootDir + "models\\option-0000.ply";

                if (worldPts.Count >= 3)
                    ExternLibInvoke.AbsoluteOrientation(worldPts, modelPts, outPath, (string)savePath);
                else
                {
                    AppendText(GCPwarning);
                    FileInfo plyFile = new FileInfo(rootDir + "models\\option-0000.ply");
                    plyFile.CopyTo((string)savePath, true);
                }
            }
            SetPbValue(progressBar1.Value + 1);
            if (lang == Language.Chinese)
                AppendText("正在加载稠密点云数据...\n");
            else if (lang == Language.English)
                AppendText("Loading dense cloud...\n");
            DirectoryInfo dir = new DirectoryInfo(dataDir);
            dir.Delete(true);
            project.denseCloud = (string)savePath;
            SetPbValue(progressBar1.Value + 1);
            watch.Stop();
            BuildDirectoryTree();
            if (lang == Language.Chinese)
                AppendText("点云保存路径：" + project.denseCloud + "\n");
            else if (lang == Language.English)
                AppendText("Save path of cloud:" + project.denseCloud + "\n");
            Clock();
            project.saved = false;
            ProjectToFile();
            dSM转换ToolStripMenuItem.Enabled = true;
            表面重建ToolStripMenuItem1.Enabled = true;
            if (lang == Language.Chinese)
                MessageBox.Show("稠密重建成功！");
            else if (lang == Language.English)
                MessageBox.Show("Dense reconstruction succeeded!");
            SetPbValue(-1);
        }

        private void btSOR_Click(object sender, EventArgs e)
        {
            if (txtFace.Text != "" && int.Parse(txtFace.Text) == 0)
            {
                if ((lang == Language.Chinese && MessageBox.Show("该工具将去除部分离群点，是否使用？", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.No) ||
                    (lang == Language.English && MessageBox.Show("This tool will remove some outliers, whether to use it?", "MeasureMan", MessageBoxButtons.YesNo) == DialogResult.No))
                    return;
                Model3DNode node;
                if (txtModelPath.Text.Equals(project.sparseCloud))
                {
                    if (project.nvm==null)
                    {
                        if (lang == Language.Chinese)
                            MessageBox.Show("重新打开的工程文件因缺少信息无法去除稀疏点云离群点！");
                        else if (lang == Language.English)
                            MessageBox.Show("The reopened project file cannot remove the outliers of sparse point cloud due to lack of information！");
                        return;
                    }
                    int[] removal = ExternLibInvoke.SOR(project.sparseCloud, true, int.Parse(txtVertex.Text));
                    ExternLibInvoke.ChangeOutlier(System.IO.Path.GetDirectoryName(project.path) + "\\", project, int.Parse(txtVertex.Text), removal);
                    node = Model3DNode.Create(0, project.sparseCloud);
                }
                else
                {
                    ExternLibInvoke.SOR(project.denseCloud, false, int.Parse(txtVertex.Text), 100);
                    node = Model3DNode.Create(1, project.denseCloud);
                }
                BeginDrawing(node);
                if (lang == Language.Chinese)
                    MessageBox.Show("离群点去除成功！");
                else if (lang == Language.English)
                    MessageBox.Show("Outliers were removed successfully！");
            }
        }
        #endregion
        #endregion

        #region 建模产品
        #region DSM转换
        private void dSM转换ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!File.Exists(project.denseCloud))
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("未找到工程保存的稠密点云文件！");
                else if (lang == Language.English)
                    MessageBox.Show("The dense point cloud file saved in the project is not found!");
                return;
            }
            DSMTransformation transform;
            int proCode = project.GetProjectionCode();
            transform = new DSMTransformation(project.denseCloud, proCode,hasAE,lang);
            transform.ShowDialog();
            if (!transform.succeed)
                return;
            dSM转换ToolStripMenuItem.Enabled = false;
            watch.Restart();
            if (lang == Language.Chinese)
                rtbLog.AppendText("- - - -DSM转换- - - -\n");
            else if (lang == Language.English)
                rtbLog.AppendText("- - - -DSM Transformation- - - -\n");
            string method = transform.method;

            if (method.Equals("naturalNeighbour"))
            {
                string path = transform.path;
                double pixelSize = transform.pixelSize;
                string proName = transform.GetUTMName();
                transform.Dispose();
                //点云转要素
                PointCloudTool pcTool = new PointCloudTool(project.denseCloud);
                ISpatialReference sr = null;
                ISpatialReferenceFactory srf = new SpatialReferenceEnvironmentClass();
                if (proCode != -1)
                    sr = srf.CreateProjectedCoordinateSystem(proCode);
                IFeatureClass featureClass = AEOperation.CreatePointCloudFeature(path, sr, pcTool.pointCloud);
                //要素转TIN
                ITin tin = AEOperation.CreateTIN(featureClass, path);
                (featureClass as IDataset).Delete();
                //TIN转DSM
                AEOperation.TIN2DEM(tin, path, pixelSize);
                (tin as IDataset).Delete();
                ShowDSM(path);
                ChangeTab(1, 2);
                project.DSMPath = path;
                BuildDirectoryTree();
                watch.Stop();
                if (lang == Language.Chinese)
                {
                    AppendText("DSM保存路径：" + path + "\n");
                    AppendText("DSM插值方法：" + method + "\n");
                    AppendText("DSM坐标系：" +  proName+ "\n");
                    AppendText("DSM空间分辨率：" + pixelSize + "m * " + pixelSize + "m\n");
                }
                else if (lang == Language.English)
                {
                    AppendText("Save path:" + path + "\n");
                    AppendText("Interpolation method:" + method + "\n");
                    AppendText("Coordinates system:" + proName + "\n");
                    AppendText("Spatial resolution:" + pixelSize + "m * " + pixelSize + "m\n");
                }
                Clock();
                project.saved = false;
                ProjectToFile();
                if (lang == Language.Chinese)
                    MessageBox.Show("DSM转换成功！");
                else if (lang == Language.English)
                    MessageBox.Show("DSM transformation succeeded!");
            }
            else
            {
                if (lang == Language.Chinese)
                {
                    AppendText("正在生成DSM...\n");
                    AppendText("采用 Vienna University of Technology->OPALS库插值方法");
                }
                else if (lang == Language.English)
                {
                    AppendText("Generating DSM\n");
                    AppendText("use Vienna University of Technology->OPALS interpolation methods");
                }

                progressBar1.Value = 1;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 3;
                progressBar1.Visible = true;
                Thread thread = new Thread(new ParameterizedThreadStart(GetDSM));
                thread.IsBackground = true;
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start(transform);
            }
        }

        private void GetDSM(object obj)
        {
            DSMTransformation transform = (DSMTransformation)obj;
            string path = transform.path;
            double pixelSize = transform.pixelSize;
            string method = transform.method;
            string proName=transform.GetUTMName();
            bool succeed = ExternLibInvoke.GenerateDSM(transform.cfgPath);
            SetPbValue(progressBar1.Value + 1);
            if (lang == Language.Chinese)
                AppendText("正在删除中间文件...\n");
            else if (lang == Language.English)
                AppendText("Deleting temporary files...\n");
            if (File.Exists(transform.cfgPath))
                File.Delete(transform.cfgPath);
            if (File.Exists(transform.inputPath))
                File.Delete(transform.inputPath);
            if (File.Exists(transform.ODMPath))
                File.Delete(transform.ODMPath);
            SetPbValue(progressBar1.Value + 1);
            if (succeed)
            {
                project.DSMPath = path;
                BuildDirectoryTree();
                watch.Stop();
                if (lang == Language.Chinese)
                {
                    AppendText("DSM保存路径：" + path + "\n");
                    AppendText("DSM插值方法：" + method + "\n");
                    AppendText("DSM坐标系：" + proName + "\n");
                    AppendText("DSM空间分辨率：" + pixelSize + "m * " + pixelSize + "m\n");
                }
                else if (lang == Language.English)
                {
                    AppendText("Save path:" + path + "\n");
                    AppendText("Interpolation method:" + method + "\n");
                    AppendText("Coordinates system:" + proName + "\n");
                    AppendText("Spatial resolution:" + pixelSize + "m * " + pixelSize + "m\n");
                }
                Clock();
                project.saved = false;
                ProjectToFile();
                if (lang == Language.Chinese)
                    MessageBox.Show("DSM转换成功！");
                else if (lang == Language.English)
                    MessageBox.Show("DSM transformation succeeded!");
                SetPbValue(-1);
                if(hasAE)
                    Application.Restart();
            }
            else
            {
                dSM转换ToolStripMenuItem.Enabled = true;
                if (lang == Language.Chinese)
                    AppendText("文件占用导致DSM生成失败！\n");
                else if (lang == Language.English)
                    AppendText("The file is occupied!");
                Clock();
                SetPbValue(-1);
            }  
        }

        /// <summary>
        /// 展示DSM数据
        /// </summary>
        /// <param name="path">DSM数据路径</param>
        private void ShowDSM(string path)
        {
            if (!File.Exists(path))
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("文件不存在！");
                else if (lang == Language.English)
                    MessageBox.Show("The file does not exist!");
                return;
            }

            string filePath = System.IO.Path.GetDirectoryName(path);
            //获得文件名称
            string fileName = System.IO.Path.GetFileName(path);
            IWorkspaceFactory workspcFac = new RasterWorkspaceFactoryClass();
            IRasterWorkspace rasterWorkspc = workspcFac.OpenFromFile(filePath, 0) as IRasterWorkspace;
            IRasterDataset rasterDataset = rasterWorkspc.OpenRasterDataset(fileName);
            //构建金字塔
            IRasterPyramid3 pyramid = rasterDataset as IRasterPyramid3;
            if (pyramid != null)//无金字塔则创建金字塔
            {
                if (!pyramid.Present)
                    pyramid.Create();
            }
            IRasterLayer rasterLay = new RasterLayerClass();
            rasterLay.CreateFromDataset(rasterDataset);
            SwitchLayer(rasterLay);
            axMapControl1.AddLayer(rasterLay as ILayer);
            axMapControl1.Refresh();
        }

        private void axMapControl1_OnMouseMove(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseMoveEvent e)
        {
            if (axMapControl1.LayerCount > 0)
            {
                toolStripStatusLabel2.Text = e.mapX.ToString();
                toolStripStatusLabel4.Text = e.mapY.ToString();
            }
            else
            {
                toolStripStatusLabel2.Text = "";
                toolStripStatusLabel4.Text = "";
                toolStripStatusLabel6.Text = "";
                toolStripStatusLabel7.Text = "";
                toolStripStatusLabel8.Text = "";
            }
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
        }

        private void axTOCControl1_OnMouseDown(object sender, ESRI.ArcGIS.Controls.ITOCControlEvents_OnMouseDownEvent e)
        {
            if (e.button == 2)
            {
                esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap basicMap = null;
                ILayer layer = null;
                System.Object obj = null;
                System.Object ind = null;
                axTOCControl1.HitTest(e.x, e.y, ref item, ref basicMap, ref layer, ref obj, ref ind);
                if (item == esriTOCControlItem.esriTOCControlItemLayer)
                {
                    if (layer is IRasterLayer)
                        contextMenuStrip1.Show(axTOCControl1, e.x, e.y);
                }
            }
        }

        private void axTOCControl1_OnDoubleClick(object sender, ITOCControlEvents_OnDoubleClickEvent e)
        {
            if (e.button == 1)
            {
                esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap basicMap = null;
                ILayer layer = null;
                System.Object obj = null;
                System.Object ind = null;
                axTOCControl1.HitTest(e.x, e.y, ref item, ref basicMap, ref layer, ref obj, ref ind);
                if (item == esriTOCControlItem.esriTOCControlItemLayer)
                    SwitchLayer(layer);         
            }
        }

        private void 分级渲染ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IRasterLayer lay = null;
            for (int i = 0; i < axMapControl1.LayerCount; i++)
            {
                if (axMapControl1.get_Layer(i) is IRasterLayer)
                {
                    lay = axMapControl1.get_Layer(i) as IRasterLayer;
                    break;
                }
            }
            RasterRenderer pRasterRenderer = new RasterRenderer(lay,lang);
            pRasterRenderer.ShowDialog();
            if (pRasterRenderer.succeed)
            {
                axMapControl1.ActiveView.ContentsChanged();
                axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            }
        }

        private void 栅格还原ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = (axMapControl1.Map.get_Layer(0) as IRasterLayer).FilePath;
            for (int i = 0; i < axMapControl1.LayerCount; i++)
            {
                if (axMapControl1.get_Layer(i) is IRasterLayer)
                {
                    axMapControl1.DeleteLayer(i);
                    break;
                }
            }
            ShowDSM(path);
        }

        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (axMapControl1.CurrentTool == null)
            {
                if (e.button == 1)//标识功能
                {
                    IPoint selectedPoint = new PointClass();
                    selectedPoint.PutCoords(e.mapX, e.mapY);
                    for (int i = 0; i < axMapControl1.LayerCount; i++)
                    {
                        IGeometry geom;
                        if (axMapControl1.get_Layer(i).Visible)
                        {
                            IIdentify iden = axMapControl1.get_Layer(i) as IIdentify;
                            string name = axMapControl1.get_Layer(i).Name;
                            if (axMapControl1.get_Layer(i) is IFeatureLayer && name.Equals("GCP"))
                            {
                                ITopologicalOperator topo = selectedPoint as ITopologicalOperator;
                                double dis = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.FromPoints(5);
                                geom = topo.Buffer(dis);
                                geom.SpatialReference = axMapControl1.Map.SpatialReference;
                                IArray arr = iden.Identify(geom);
                                if (arr != null)
                                {
                                    for (int j = 0; j < arr.Count; j++)
                                    {
                                        IIdentifyObj obj = (IIdentifyObj)arr.get_Element(j);
                                        IRowIdentifyObject row = obj as IRowIdentifyObject;
                                        IRow r = row.Row;
                                        if (identify != null)
                                            identify.Close();
                                        identify = new Identify(axMapControl1.get_Layer(i), selectedPoint, r, null,lang);
                                        identify.Show();
                                    }
                                    break;
                                }
                            }
                            else if (axMapControl1.get_Layer(i) is IRasterLayer)
                            {
                                geom = selectedPoint as IGeometry;
                                IArray arr = iden.Identify(geom);
                                if (arr != null)
                                {
                                    for (int j = 0; j < arr.Count; j++)
                                    {
                                        IIdentifyObj obj = (IIdentifyObj)arr.get_Element(j);
                                        IRasterIdentifyObj pixel = obj as IRasterIdentifyObj;
                                        if (identify != null)
                                            identify.Close();
                                        identify = new Identify(axMapControl1.get_Layer(i), selectedPoint, null, pixel,lang);
                                        identify.Show();
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (e.button == 2)
                    axMapControl1.Extent = axMapControl1.TrackRectangle();
            }

        }

        private void axMapControl1_OnDoubleClick(object sender, IMapControlEvents2_OnDoubleClickEvent e)
        {
            if (e.button == 2)
                axMapControl1.CurrentTool = null;
            else if (e.button == 1 && axMapControl1.CurrentTool == null)
            {
                IPoint selectedPoint = new PointClass();
                selectedPoint.PutCoords(e.mapX, e.mapY);
                for (int i = 0; i < axMapControl1.LayerCount; i++)
                {
                    IGeometry geom;
                    if (axMapControl1.get_Layer(i).Visible)
                    {
                        IIdentify iden = axMapControl1.get_Layer(i) as IIdentify;
                        string name = axMapControl1.get_Layer(i).Name;
                        if (axMapControl1.get_Layer(i) is IFeatureLayer && (name.Equals("POS") || name.Equals("Route")))
                        {
                            ITopologicalOperator topo = selectedPoint as ITopologicalOperator;
                            double dis = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.FromPoints(5);
                            geom = topo.Buffer(dis);
                            geom.SpatialReference = axMapControl1.Map.SpatialReference;
                            IArray arr = iden.Identify(geom);
                            if (arr != null)
                            {
                                for (int j = 0; j < arr.Count; j++)
                                {
                                    IIdentifyObj obj = (IIdentifyObj)arr.get_Element(j);
                                    IRowIdentifyObject row = obj as IRowIdentifyObject;
                                    IRow r = row.Row;
                                    if (name.Equals("POS"))
                                        ShowImageInfo(images[(int)r.get_Value(0)]);
                                    else
                                    {
                                        string relPath=null;
                                        if(project.TVMs!=null&&project.TVMs.Count!=0)
                                            relPath = System.IO.Path.GetDirectoryName(project.path) + "\\rel\\" + (int)r.get_Value(2) + "-" + (int)r.get_Value(3) + ".rel";
                                        if (relPath != null && File.Exists(relPath))
                                        {
                                            if (imageBox1.Image != null)
                                                imageBox1.Image.Dispose();
                                            imageBox1.Image = null;
                                            InitiateImageDataWindow();
                                            if (imageBox2.Image != null)
                                                imageBox2.Image.Dispose();
                                            imageBox2.Image = null;
                                            string relName = System.IO.Path.GetFileName(relPath);
                                            VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
                                            int matchesCount =FtPointAnalyseTool.LoadRelFile(relPath, matches);
                                            imageBox2.Image = FtPointAnalyseTool.ShowMatches(project, relName, System.IO.Path.GetDirectoryName(project.ftPaths[0]), matches);
                                            tabControl1.SelectedIndex = 1;
                                            tabControl3.SelectedIndex = 0;
                                            string[] twoImages = relName.Split(new char[2] { '-', '.' });
                                            if (lang == Language.Chinese)
                                                MessageBox.Show("图像" + project.images[int.Parse(twoImages[0]) - 1].name + "和图像" + project.images[int.Parse(twoImages[1]) - 1].name + "的匹配点对共：" + matchesCount + "对，间距为" + (double)r.get_Value(4) + "米");
                                            else if (lang == Language.English)
                                                MessageBox.Show("Image " + project.images[int.Parse(twoImages[0]) - 1].name + " and image " + project.images[int.Parse(twoImages[1]) - 1].name + " are matched " + matchesCount + " pairs, their distance is " + (double)r.get_Value(4) + " m");
                                        }
                                        else
                                        {
                                            if (lang == Language.Chinese)
                                                MessageBox.Show("该航线段间距为" + (double)r.get_Value(4) + "米");
                                            else if (lang == Language.English)
                                                MessageBox.Show("The distance between the route segment is " + (double)r.get_Value(4) + " m");
                                        }  
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void axTOCControl1_OnBeginLabelEdit(object sender, ITOCControlEvents_OnBeginLabelEditEvent e)
        {
            e.canEdit = false;
        }
        #endregion

        #region 表面重建
        private void 表面重建ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!File.Exists(project.denseCloud))
            {
                if (lang == Language.Chinese)
                    MessageBox.Show("未找到工程保存的稠密点云文件！");
                else if (lang == Language.English)
                    MessageBox.Show("The dense point cloud file saved in the project is not found!");
                return;
            }

            SaveFileDialog save = new SaveFileDialog();
            if (lang == Language.Chinese)
            {
                save.Filter = "ply文件|*.ply";
                save.Title = "三维模型导出到";
            }
            else if (lang == Language.English)
            {
                save.Filter = "ply File|*.ply";
                save.Title = "Export 3D Model To";
            }
            if (save.ShowDialog() == DialogResult.OK)
            {
                watch.Restart();
                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 2;
                progressBar1.Visible = true;
                表面重建ToolStripMenuItem1.Enabled = false;
                if (lang == Language.Chinese)
                {
                    rtbLog.AppendText("- - - -表面重建- - - -\n");
                    rtbLog.AppendText("采用 Bernardini et al.->Ball Pivoting\n");
                    rtbLog.AppendText("正在进行表面重建...\n");
                }
                else if (lang == Language.English)
                {
                    rtbLog.AppendText("- - - -Surface Reconstruction- - - -\n");
                    rtbLog.AppendText("Use Bernardini et al.->Ball Pivoting\n");
                    rtbLog.AppendText("Surface reconstruction in progress...\n");
                }
                Thread thread1 = new Thread(new ParameterizedThreadStart(SurfaceReconstruction));
                thread1.IsBackground = true;
                thread1.Start(save.FileName);
            }
        }

        /// <summary>
        /// 表面重建
        /// </summary>
        /// <param name="path">模型保存路径</param>
        private void SurfaceReconstruction(object path)
        {
            SetPbValue(progressBar1.Value + 1);
            int faceCount = ExternLibInvoke.BallPivoting(project.denseCloud, (string)path);
            if (lang == Language.Chinese)
                AppendText("正在加载三维模型...\n");
            else if (lang == Language.English)
                AppendText("Loading 3D model...\n");
            project.modelPath = (string)path;
            SetPbValue(progressBar1.Value + 1);
            watch.Stop();
            BuildDirectoryTree();
            if (lang == Language.Chinese)
            {
                AppendText("三维模型保存路径：" + project.modelPath + "\n");
                AppendText("面片数量：共" + faceCount + "个\n");
            }
            else if (lang == Language.English)
            {
                AppendText("Save path of model:" + project.modelPath + "\n");
                AppendText("Faces of model: " + faceCount + "\n");
            }
            Clock();
            project.saved = false;
            ProjectToFile();
            if (lang == Language.Chinese)
                MessageBox.Show("表面重建成功！");
            else if (lang == Language.English)
                MessageBox.Show("Surface reconstruction is succeeded！");
            SetPbValue(-1);
        }
        #endregion  
        #endregion

        #region 量测模块
        /// <summary>
        /// 量测类型
        /// </summary>
        enum MeasureType
        {
            /// <summary>
            /// 无量测
            /// </summary>
            Empty=0,
            /// <summary>
            /// 点的位置
            /// </summary>
            Location=1,
            /// <summary>
            /// 长度测量
            /// </summary>
            Length=2,
            /// <summary>
            /// 面积测量
            /// </summary>
            Area=3
        }


        private void btPickPoint_Click(object sender, EventArgs e)
        {
            if (scene != null&&scene.RootNode!=null &&measure!=MeasureType.Location)
            {
                ClearLastMeasurement();
                this.pickingAction = new Picking(scene);
                highlightPt = new LegacyPointNode();
                measure = MeasureType.Location;
            }
        }

        private void btGetLength_Click(object sender, EventArgs e)
        {
            if (scene != null && scene.RootNode != null && measure!=MeasureType.Length)
            {
                ClearLastMeasurement();
                this.pickingAction = new Picking(scene);
                highlightPt = new LegacyPointNode();
                length = 0;
                measure = MeasureType.Length;
                处理日志窗口ToolStripMenuItem_Click(sender, e);
                if (lang == Language.Chinese)
                    rtbLog.AppendText("- - - -长度测量- - - -\n");
                else if (lang == Language.English)
                    rtbLog.AppendText("- - - -Length Measurement- - - -\n");
            }
        }

        private void btGetArea_Click(object sender, EventArgs e)
        {
            if (scene != null && scene.RootNode != null && measure != MeasureType.Area)
            {
                ClearLastMeasurement();
                this.pickingAction = new Picking(scene);
                highlightPt = new LegacyPointNode();
                area = 0;
                measure = MeasureType.Area;
                处理日志窗口ToolStripMenuItem_Click(sender, e);
                if (lang == Language.Chinese)
                    rtbLog.AppendText("- - - -面积测量- - - -\n");
                else if (lang == Language.English)
                    rtbLog.AppendText("- - - -Area Measurement- - - -\n");
            }
        }

        private void winGLCanvas1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.pickingAction != null)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (this.pickedGeometry != null)
                    {
                        IGLCanvas canvas = this.winGLCanvas1;
                        int x = e.X;
                        int y = canvas.Height - e.Y - 1;
                        var viewport = new vec4(0, 0, canvas.Width, canvas.Height);
                        var lastWindowSpacePos = new vec3(x, y, pickedGeometry.PickedPosition.z);
                        mat4 projectionMat = this.scene.Camera.GetProjectionMatrix();
                        mat4 viewMat = this.scene.Camera.GetViewMatrix();
                        mat4 modelMat = (pickedGeometry.FromObject as PickableNode).GetModelMatrix();
                        var pickedPt = glm.unProject(lastWindowSpacePos, viewMat * modelMat, projectionMat, viewport);
                        switch (measure)
                        {
                            case MeasureType.Location:
                                if (lang == Language.Chinese)
                                    MessageBox.Show("选中的点坐标为(" + pickedPt.x + "," + pickedPt.y + "," + pickedPt.z + ")");
                                else if (lang == Language.English)
                                    MessageBox.Show("The selected point is (" + pickedPt.x + "," + pickedPt.y + "," + pickedPt.z + ")");
                                break;
                            case MeasureType.Length:
                                处理日志窗口ToolStripMenuItem_Click(sender, e);
                                if (modelPts==null)
                                {
                                    if (lang == Language.Chinese)
                                        rtbLog.AppendText("选中第一个点(" + pickedPt.x + "," + pickedPt.y + "," + pickedPt.z + ")\n");
                                    else if (lang == Language.English)
                                        rtbLog.AppendText("Selected the first point (" + pickedPt.x + "," + pickedPt.y + "," + pickedPt.z + ")\n");
                                    modelPts = new List<vec3>();
                                    windowPts = new List<vec3>();
                                }
                                else
                                {
                                    LegacyLineNode line = new LegacyLineNode();
                                    line.Vertex0 = windowPts.Last();
                                    line.Vertex1 = pickedGeometry.Positions[0];
                                    line.Parent=this.pickedGeometry.FromObject as SceneNodeBase;
                                    length += Math.Sqrt(Math.Pow(pickedPt.x - modelPts.Last().x, 2) + Math.Pow(pickedPt.y - modelPts.Last().y, 2) + Math.Pow(pickedPt.z - modelPts.Last().z, 2));
                                    if (lang == Language.Chinese)
                                    {
                                        rtbLog.AppendText("选中下一个点(" + pickedPt.x + "," + pickedPt.y + "," + pickedPt.z + ")\n");
                                        rtbLog.AppendText("当前长度为" + length + "\n");
                                    }
                                    else if (lang == Language.English)
                                    {
                                        rtbLog.AppendText("Selected the next point (" + pickedPt.x + "," + pickedPt.y + "," + pickedPt.z + ")\n");
                                        rtbLog.AppendText("The current length is " + length + "\n");
                                    }
                                }
                                modelPts.Add(pickedPt);
                                windowPts.Add(pickedGeometry.Positions[0]);
                                break;
                            case MeasureType.Area:
                                处理日志窗口ToolStripMenuItem_Click(sender, e);
                                if (modelPts==null)
                                {
                                    if (lang == Language.Chinese)
                                        rtbLog.AppendText("选中第一个点(" + pickedPt.x + "," + pickedPt.y + "," + pickedPt.z + ")\n");
                                    else if (lang == Language.English)
                                        rtbLog.AppendText("Selected the first point (" + pickedPt.x + "," + pickedPt.y + "," + pickedPt.z + ")\n");
                                    modelPts = new List<vec3>();
                                    windowPts = new List<vec3>();
                                }
                                else if (modelPts.Count==1)
                                {
                                    if (lang == Language.Chinese)
                                        rtbLog.AppendText("选中第二个点(" + pickedPt.x + "," + pickedPt.y + "," + pickedPt.z + ")\n");
                                    else if (lang == Language.English)
                                        rtbLog.AppendText("Selected the second point( " + pickedPt.x + "," + pickedPt.y + "," + pickedPt.z + ")\n");
                                }
                                else
                                {
                                    double triArea = GetArea(modelPts[0], modelPts.Last(), pickedPt);
                                    if (triArea == 0)
                                    {
                                        if (lang == Language.Chinese)
                                            rtbLog.AppendText("该点不符合要求，请重新选择\n");
                                        else if (lang == Language.English)
                                            rtbLog.AppendText("This point does not meet the requirements, please select again\n");
                                        return;
                                    }
                                    else
                                    {
                                        LegacyTriangleNode tri = new LegacyTriangleNode();
                                        tri.Vertex0 = windowPts[0]; tri.Vertex1 = windowPts.Last(); tri.Vertex2 = pickedGeometry.Positions[0];
                                        tri.Color0 = new vec3(1, 0, 0); tri.Color1 = new vec3(1, 0, 0); tri.Color2 = new vec3(1, 0, 0);
                                        tri.PolygonMode = PolygonMode.Fill;
                                        tri.Parent = this.pickedGeometry.FromObject as SceneNodeBase;
                                        area += triArea;
                                        if (lang == Language.Chinese)
                                        {
                                            rtbLog.AppendText("选中下一个点(" + pickedPt.x + "," + pickedPt.y + "," + pickedPt.z + ")\n");
                                            rtbLog.AppendText("当前面积为" + area + "\n");
                                        }
                                        else if (lang == Language.English)
                                        {
                                            rtbLog.AppendText("Selected the next point (" + pickedPt.x + "," + pickedPt.y + "," + pickedPt.z + ")\n");
                                            rtbLog.AppendText("The current area is " + area + "\n");
                                        }
                                    }
                                }
                                modelPts.Add(pickedPt);
                                windowPts.Add(pickedGeometry.Positions[0]);
                                break;
                        }
                    }
                }
                else if (e.Button == MouseButtons.Right)
                    ClearLastMeasurement();
            }
        }

        /// <summary>
        /// 求三点构成的三角形面积
        /// </summary>
        /// <param name="pt1">顶点1</param>
        /// <param name="pt2">顶点2</param>
        /// <param name="pt3">顶点3</param>
        /// <returns>三角形面积，为0表示无法构成三角形</returns>
        private double GetArea(vec3 pt1, vec3 pt2, vec3 pt3)
        {
            double[] sides = new double[3];
            sides[0] = Math.Sqrt(Math.Pow(pt1.x - pt2.x, 2) + Math.Pow(pt1.y - pt2.y, 2) + Math.Pow(pt1.z - pt2.z, 2));
            sides[1] = Math.Sqrt(Math.Pow(pt1.x - pt3.x, 2) + Math.Pow(pt1.y - pt3.y, 2) + Math.Pow(pt1.z - pt3.z, 2));
            sides[2] = Math.Sqrt(Math.Pow(pt3.x - pt2.x, 2) + Math.Pow(pt3.y - pt2.y, 2) + Math.Pow(pt3.z - pt2.z, 2));
            if (sides[0] + sides[1] <= sides[2] || sides[0] + sides[2] <= sides[1] || sides[1] + sides[2] <= sides[0])
                return 0;
            double p = (sides[0] + sides[1] + sides[2]) / 2;
            return Math.Sqrt(p * (p - sides[0]) * (p - sides[1]) * (p - sides[2]));
        }

        private void winGLCanvas1_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.pickingAction != null)
            {
                IGLCanvas canvas = this.winGLCanvas1;
                int x = e.X;
                int y = canvas.Height - e.Y - 1;
                this.pickedGeometry = this.pickingAction.Pick(x, y, PickingGeometryTypes.Point, canvas.Width, canvas.Height);
                if (this.pickedGeometry != null)
                {
                    highlightPt.Color = new vec3(1, 0, 0);
                    highlightPt.Vertex = this.pickedGeometry.Positions[0];
                    highlightPt.Parent = this.pickedGeometry.FromObject as SceneNodeBase;
                }
            }
            else
            {
                this.pickedGeometry = null;
            }
        }

        /// <summary>
        /// 清除上一次测量
        /// </summary>
        private void ClearLastMeasurement()
        {
            if (measure != MeasureType.Empty && scene.RootNode!=null)
            {
                switch (measure)
                {
                    case MeasureType.Length:
                        modelPts = null;
                        windowPts = null;
                        if (lang == Language.Chinese)
                        {
                            rtbLog.AppendText("总长度为" + length + "\n");
                            rtbLog.AppendText("长度测量结束\n");
                        }
                        else if (lang == Language.English)
                        {
                            rtbLog.AppendText("The total length: " + length + "\n");
                            rtbLog.AppendText("Length measurement ended\n");
                        }
                        break;
                    case MeasureType.Area:
                        modelPts=null;
                        windowPts = null;
                        if (lang == Language.Chinese)
                        {
                            rtbLog.AppendText("总面积为" + area + "\n");
                            rtbLog.AppendText("面积测量结束\n");
                        }
                        else if (lang == Language.English)
                        {
                            rtbLog.AppendText("The total area: " + area + "\n");
                            rtbLog.AppendText("Area measurement ended\n");
                        }
                        break;
                }
                pickingAction = null;
                highlightPt = null;
                measure = MeasureType.Empty;
                scene.RootNode.Children.Clear();
            }
        }
        #endregion
    }
}
