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
    public partial class EvenKeyFrameSetting : Form
    {
        /// <summary>
        /// 加载的视频数据
        /// </summary>
        public AddedVideo video;
        /// <summary>
        /// 等距关键帧提取
        /// </summary>
        public EvenKeyFrameTool evenTool;
        /// <summary>
        /// 语言系统
        /// </summary>
        private Language lang;

        public EvenKeyFrameSetting(AddedVideo video,Language lang)
        {
            InitializeComponent();
            this.lang = lang;
            if (lang == Language.English)
            {
                this.Text = "Even Key Frame Capture Setting";
                btEnsure.Text = "Confirm";
                btSkip.Text = "Skip";
                cbClipMode.Items.Clear();
                cbClipMode.Items.AddRange(new object[2] { "skip segment", "select segment" });
                label1.Text = "Capture Mode";
                label10.Text = "Frame Rate";
                label11.Text = "f/s";
                label2.Text = "Total Time";
                label3.Text = "Total Frames";
                label4.Text = "Frame Interval";
                label5.Text = "Capture Period";
                label6.Text = "s  to";
                label7.Text = "s";
                label8.Text = "s";
            }
            this.video = video;
            txtDuration.Text = video.duration.ToString();
            txtFrameNumber.Text = video.frameNumber.ToString();
            txtFps.Text = video.videoFps.ToString();
        }

        private void EvenKeyFrameSettings_Load(object sender, EventArgs e)
        {
            txtDuration.ReadOnly = true;
            txtFrameNumber.ReadOnly = true;
            txtStartTime.ReadOnly = true;
            txtEndTime.ReadOnly = true;
            txtFps.ReadOnly = true;
        }

        private void cbClipMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtStartTime.ReadOnly = false;
            txtEndTime.ReadOnly = false;
        }

        private void btEnsure_Click(object sender, EventArgs e)
        {
            bool deafaultSetting = false;
            int frameInterval, clipMode = 2;//默认为从头到尾的关键帧提取
            double startTime = 0, endTime = video.duration;
            if (txtFrameInter.Text != "")
            {
                frameInterval = int.Parse(txtFrameInter.Text);
                if (frameInterval > video.frameNumber / 2 || frameInterval < 1)//帧间隔大于帧数的一般或少于1
                {
                    frameInterval = DefaultFrameInter();
                    deafaultSetting = true;
                }
            }
            else
            {
                frameInterval = DefaultFrameInter();
                deafaultSetting = true;
            }

            if (cbClipMode.Text != "")
            {
                try
                {
                    if (txtStartTime.Text != "" && txtEndTime.Text != "")
                    {
                        startTime = double.Parse(txtStartTime.Text);
                        endTime = double.Parse(txtEndTime.Text);
                    }
                }
                catch
                {
                    if (lang == Language.Chinese)
                        MessageBox.Show("时段设置不正确！");
                    else if (lang == Language.English)
                        MessageBox.Show("The capture period is not set correctly!");
                    return;
                }
                if (txtStartTime.Text == "" || startTime < 0)
                {
                    startTime = 0;
                    deafaultSetting = true;
                }
                if (txtStartTime.Text == "" || endTime > video.duration)
                {
                    endTime = video.duration;
                    deafaultSetting = true;
                }
                if (startTime > endTime)//起始时间大于结束时间，交换
                {
                    double temp;
                    temp = startTime;
                    startTime = endTime;
                    endTime = temp;
                    deafaultSetting = true;
                }
                clipMode = cbClipMode.SelectedIndex;
            }

            if (deafaultSetting)
            {
                txtFrameInter.Text = frameInterval.ToString();
                txtStartTime.Text = startTime.ToString();
                txtEndTime.Text = endTime.ToString();
                if (lang == Language.Chinese)
                    MessageBox.Show("请再次确认设置！");
                else if (lang == Language.English)
                    MessageBox.Show("Please confirm the settings again!");
            }
            else
            {
                int startFrame = 1, endFrame = video.frameNumber;
                switch (clipMode)
                {
                    case 0:
                    case 1:
                        startFrame = (int)(startTime * video.videoFps) + 1;
                        endFrame = (int)(endTime * video.videoFps);
                        break;
                    case 2:
                        startFrame = 1;
                        endFrame = video.frameNumber;
                        break;
                }
                evenTool = new EvenKeyFrameTool(frameInterval, startFrame, endFrame,clipMode);
                this.Close();
            }
        }

        /// <summary>
        /// 默认设置帧间隔。
        /// </summary>
        /// <returns>默认帧间隔</returns>
        public int DefaultFrameInter()
        {
            int level = video.frameNumber / 10;
            if (level <= 2)//帧数少于30
            {
                return 1;
            }
            else if (level < 5)//帧数多于30少于50
            {
                return 2;
            }
            else if (level < 10)//帧数多于50少于100
            {
                return 3;
            }
            else if (level < 50)//帧数多于100少于500
            {
                return 5;
            }
            else if (level < 100)//帧数多于500少于1000
            {
                return 10;
            }
            else if (level < 1000)//帧数多于1000少于10000
            {
                return 25;
            }
            else//帧数多于10000
            {
                return 50;
            }
        }

        private void btSkip_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
