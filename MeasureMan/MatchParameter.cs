using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 用于封装多线程匹配的参数类
    /// </summary>
    public class MatchParameter
    {
        /// <summary>
        /// 起始索引
        /// </summary>
        public int fromIndex;
        /// <summary>
        /// 终止索引
        /// </summary>
        public int toIndex;
        /// <summary>
        /// 距离阈值
        /// </summary>
        public double threshold;

        /// <summary>
        /// 匹配参数初始化方法
        /// </summary>
        /// <param name="fromIndex">起始图像索引</param>
        /// <param name="toIndex">终止图像索引</param>
        /// <param name="threshold">距离阈值，为负值则为无POS匹配，相邻的两幅图，为最大值为全匹配，否则是距离筛选匹配</param>
        public MatchParameter(int fromIndex, int toIndex, double threshold)
        {
            this.fromIndex = fromIndex;
            this.toIndex = toIndex;
            this.threshold = threshold;
        }
    }
}
