using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 添加的影像数据
    /// </summary>
    public class AddedVideo
    {
        /// <summary>
        /// 视频帧率
        /// </summary>
        public double videoFps;
        /// <summary>
        /// 视频总帧数
        /// </summary>
        public int frameNumber;
        /// <summary>
        /// 视频时长
        /// </summary>
        public double duration;
        /// <summary>
        /// 分辨率
        /// </summary>
        public Size resolution;

        /// <summary>
        /// 影像初始化方法
        /// </summary>
        /// <param name="videoFps">视频帧率</param>
        /// <param name="frameNumber">总帧数</param>
        /// <param name="resolution">分辨率，长*宽</param>
        public AddedVideo(double videoFps, int frameNumber,Size resolution)
        {
            this.videoFps = videoFps;
            this.frameNumber = frameNumber;
            this.duration = double.Parse((frameNumber / videoFps).ToString());
            this.resolution = resolution;
        }
    }
}
