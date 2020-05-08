using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 匹配的同名点类
    /// </summary>
    public class MatchPoint
    {
        /// <summary>
        /// 图片索引,从0开始
        /// </summary>
        public int imageIndex;
        /// <summary>
        /// 特征点索引,从0开始
        /// </summary>
        public int ftIndex;
        /// <summary>
        /// 轨迹索引，从0开始
        /// </summary>
        public int trackIndex;
        /// <summary>
        /// 特征点像素坐标
        /// </summary>
        public PointF ftPoint;

        /// <summary>
        /// 同名点有参初始化方法
        /// </summary>
        /// <param name="imageindex">图像索引</param>
        /// <param name="ftIndex">特征点索引</param>
        /// <param name="point">特征点像素坐标</param>
        /// <param name="trackIndex">轨迹索引</param>
        public MatchPoint(int imageIndex, int ftIndex, PointF ftPoint,int trackIndex)
        {
            this.imageIndex = imageIndex;
            this.ftIndex = ftIndex;
            this.ftPoint = ftPoint;
            this.trackIndex = trackIndex;
        }
    }
}
