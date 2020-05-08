using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 控制点
    /// </summary>
    public class GCP :GeoPoint,IComparable<GCP>
    {
        /// <summary>
        /// 控制点所属图像序号，从1开始
        /// </summary>
        public int imageOrder;
        /// <summary>
        /// 像素坐标
        /// </summary>
        public PointF pixelPoint;
        /// <summary>
        /// 控制点对应的模型点
        /// </summary>
        public Point3D modelPoint;

        
        /// <summary>
        /// 读入不携带模型信息的控制点
        /// </summary>
        /// <param name="imageOrder">图像序号</param>
        /// <param name="pixelPointU">像素u坐标</param>
        /// <param name="pixelPointV">像素v坐标</param>
        /// <param name="x">经度</param>
        /// <param name="y">纬度</param>
        /// <param name="z">高程</param>
        public GCP(int imageOrder,float pixelPointU, float pixelPointV,double x, double y, double z): base(x, y, z)
        {
            this.imageOrder = imageOrder;
            pixelPoint = new PointF(pixelPointU, pixelPointV);
        }

        /// <summary>
        /// 读入携带模型信息的控制点
        /// </summary>
        /// <param name="imageOrder">图像序号</param>
        /// <param name="pixelPointU">像素u坐标</param>
        /// <param name="pixelPointV">像素v坐标</param>
        /// <param name="x">经度</param>
        /// <param name="y">纬度</param>
        /// <param name="z">高程</param>
        /// <param name="x2">模型x坐标</param>
        /// <param name="y2">模型y坐标</param>
        /// <param name="z2">模型z坐标</param>
        public GCP(int imageOrder, float pixelPointU, float pixelPointV, double x, double y, double z,double x2,double y2,double z2): base(x, y, z)
        {
            this.imageOrder = imageOrder;
            pixelPoint = new PointF(pixelPointU, pixelPointV);
            modelPoint=new Point3D(x2,y2,z2);
        }

        /// <summary>
        /// 交互生成控制点方法
        /// </summary>
        /// <param name="x">经度</param>
        /// <param name="y">纬度</param>
        /// <param name="z">高程</param>
        public GCP(double x, double y, double z):base(x,y,z)
        {
        }

        /// <summary>
        /// 将控制点信息转化为一条dataRow
        /// </summary>
        /// <returns>信息对象数组</returns>
        public object[] GetDataRow()
        {
            object[] dr = new object[7];
            dr[0] = imageOrder;
            dr[1] = pixelPoint.X;
            dr[2] = pixelPoint.Y;
            dr[3] = x;
            dr[4] = y;
            dr[5] = z;
            return dr;
        }

        public int CompareTo(GCP other)
        {
            if (this.imageOrder > other.imageOrder)
                return 1;
            else if (this.imageOrder == other.imageOrder)
                return 0;
            else
                return -1;
        }
    }
}
