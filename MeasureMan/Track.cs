using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 同名点轨迹类
    /// </summary>
    public class Track
    {
        /// <summary>
        /// 轨迹包含的同名点列表
        /// </summary>
        public List<MatchPoint> matchPoints;
        /// <summary>
        /// 同名三维点
        /// </summary>
        public Point3D _pt;

        /// <summary>
        /// 轨迹默认初始化方法
        /// </summary>
        public Track()
        {
            matchPoints = new List<MatchPoint>();
        }

        /// <summary>
        /// 携带同名点的轨迹初始化方法
        /// </summary>
        /// <param name="matchPoints">同名点集合</param>
        public Track(List<MatchPoint> matchPoints)
        {
            this.matchPoints = matchPoints;
        }

        /// <summary>
        /// 根据图像索引查找同名点
        /// </summary>
        /// <param name="imageIndex">图像索引</param>
        /// <returns>同名点，不包含返回null</returns>
        public MatchPoint FindMatchPoint(int imageIndex)
        {
            foreach (MatchPoint mp in matchPoints)
            {
                if (mp.imageIndex == imageIndex)
                    return mp;
            }
            return null;
        }
    }
}
