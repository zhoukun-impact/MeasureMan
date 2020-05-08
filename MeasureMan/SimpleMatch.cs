using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 简化的匹配类，映射.rel文件
    /// </summary>
    public class SimpleMatch : IComparable<SimpleMatch>
    {
        /// <summary>
        /// 查询特征点索引
        /// </summary>
        public int queryIdx;
        /// <summary>
        /// 训练特征点索引
        /// </summary>
        public int trainIdx;
        /// <summary>
        /// 匹配最短距离
        /// </summary>
        public float distance;

        /// <summary>
        /// 匹配信息初始化方法
        /// </summary>
        /// <param name="QueryIdx">查询特征点索引</param>
        /// <param name="TrainIdx">训练特征点索引</param>
        /// <param name="Distance">匹配最短距离</param>
        public SimpleMatch(int QueryIdx, int TrainIdx, float Distance)
        {
            this.queryIdx = QueryIdx;
            this.trainIdx = TrainIdx;
            this.distance = Distance;
        }

        public int CompareTo(SimpleMatch other)
        {
            if (trainIdx > other.trainIdx)
                return 1;
            else if (trainIdx == other.trainIdx)
            {
                if (distance < other.distance)
                    other.distance = float.MaxValue;
                else
                    distance = float.MaxValue;
                return 0;
            }
            else
                return -1;
        }
    }
}
