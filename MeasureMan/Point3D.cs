using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 三维点基类
    /// </summary>
    public class Point3D
    {
        /// <summary>
        /// 经度或x坐标
        /// </summary>
        public double x;
        /// <summary>
        /// 纬度或y坐标
        /// </summary>
        public double y;
        /// <summary>
        /// 高程
        /// </summary>
        public double z;

        /// <summary>
        /// 三维点默认初始化方法
        /// </summary>
        /// <param name="x">经度或x坐标</param>
        /// <param name="y">纬度或y坐标</param>
        /// <param name="z">高程</param>
        public Point3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// 计算两点间距离
        /// </summary>
        /// <param name="other">另一个点</param>
        /// <returns>欧氏距离</returns>
        public double GetDistance(Point3D other)
        {
            return Math.Sqrt(Math.Pow(this.x - other.x, 2) + Math.Pow(this.y - other.y, 2) + Math.Pow(this.z - other.z, 2));
        }

        /// <summary>
        /// 检验模型点z值是否合格
        /// </summary>
        /// <param name="threshold">阈值</param>
        /// <returns>绝对值小于阈值为合格，否则为不合格</returns>
        public bool Check(double threshold)
        {
            if (Math.Abs(z) > threshold)
                return false;
            else
                return true;
        }

        /// <summary>
        /// 克隆三维点
        /// </summary>
        /// <returns>克隆后的三维点</returns>
        public Point3D Clone()
        {
            return new Point3D(x, y, z);
        }
    }
}
