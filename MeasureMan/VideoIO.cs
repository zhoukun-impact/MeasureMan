using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 视频读写工具
    /// </summary>
    public class VideoIO
    {
        /// <summary>
        /// 抓取视频流的工具
        /// </summary>
        public VideoCapture capture;
        /// <summary>
        /// 当前视频帧
        /// </summary>
        public Mat frame;
        /// <summary>
        /// 视频帧计数
        /// </summary>
        public int frameCount;
        /// <summary>
        /// 输出帧的顺序
        /// </summary>
        public int frameOrder;
        /// <summary>
        /// 视频播放结束帧
        /// </summary>
        public int limitFrame;
        /// <summary>
        /// 等距关键帧提取工具
        /// </summary>
        public EvenKeyFrameTool evenTool;
        /// <summary>
        /// 关键帧保存路径
        /// </summary>
        public string savePath;

        /// <summary>
        /// 视频读写工具的初始化方法
        /// </summary>
        /// <param name="capture">抓取视频流的工具</param>
        public VideoIO(VideoCapture capture)
        {
            this.capture = capture;
            frameCount = 0;
            frameOrder = 0;
            limitFrame = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
        }

        /// <summary>
        /// 从视频流中获取视频信息
        /// </summary>
        /// <returns>视频信息</returns>
        public AddedVideo GetVideo()
        {
            return new AddedVideo(capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps), (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount),new Size(capture.Width,capture.Height));
        }
    }
}
