using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 等距关键帧提取工具
    /// </summary>
    public class EvenKeyFrameTool
    {
        /// <summary>
        /// 关键帧间隔
        /// </summary>
        public int frameInterval;
        /// <summary>
        /// 关键帧提取开始帧
        /// </summary>
        public int startFrame;
        /// <summary>
        /// 关键帧提取结束帧
        /// </summary>
        public int endFrame;
        /// <summary>
        /// 截取视频模式,0为跳过，1为选择,2为全选
        /// </summary>
        public int clipMode;

        /// <summary>
        /// 关键帧提取工具初始化方法
        /// </summary>
        /// <param name="frameInterval">帧间隔</param>
        /// <param name="startFrame">开始帧</param>
        /// <param name="endFrame">结束帧</param>
        /// <param name="clipMode">截取模式</param>
        public EvenKeyFrameTool(int frameInterval, int startFrame, int endFrame, int clipMode)
        {
            this.frameInterval = frameInterval;
            this.startFrame = startFrame;
            this.endFrame = endFrame;
            this.clipMode = clipMode;
        }
    }
}
