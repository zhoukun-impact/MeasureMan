using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 相机信息，用于显示
    /// </summary>
    public class CameraInfo
    {
        /// <summary>
        /// 相机型号
        /// </summary>
        public readonly string name;
        /// <summary>
        /// 焦距（像素）
        /// </summary>
        public double focalLength;
        /// <summary>
        /// 主点像素坐标，图像宽高的一半
        /// </summary>
        public PointF principalPoint;

        /// <summary>
        /// 相机信息初始化方法
        /// </summary>
        /// <param name="name">型号</param>
        /// <param name="focalLength">焦距</param>
        /// <param name="imageWidth">图像宽度</param>
        /// <param name="imageHeight">图像高度</param>
        public CameraInfo(string name, double focalLength, int imageWidth, int imageHeight)
        {
            this.name = name;
            this.focalLength = focalLength;
            principalPoint = new PointF(imageWidth / 2.0f, imageHeight / 2.0f);
        }

        /// <summary>
        /// 相机信息读取方法
        /// </summary>
        /// <param name="name">型号</param>
        /// <param name="focalLength">焦距</param>
        /// <param name="principalPointU">主点u坐标</param>
        /// <param name="principalPointV">主点v坐标</param>
        public CameraInfo(string name, double focalLength, float principalPointU, float principalPointV)
        {
            this.name = name;
            this.focalLength = focalLength;
            principalPoint = new PointF(principalPointU, principalPointV);
        }

        /// <summary>
        /// 获得相机信息描述
        /// </summary>
        /// <returns>描述文本</returns>
        public string GetAllInfo()
        {
            return name + "|" + focalLength + "|" + principalPoint.X + "|" + principalPoint.Y ;
        }

        /// <summary>
        /// 修改信息
        /// </summary>
        /// <param name="scale">缩小比例(小于1)</param>
        public void ModifyParamrters(float scale)
        {
            focalLength = focalLength * scale;
            principalPoint.X = principalPoint.X * scale;
            principalPoint.Y = principalPoint.Y * scale;
        }

        /// <summary>
        /// 复制相机信息
        /// </summary>
        /// <returns>复制的相机信息</returns>
        public CameraInfo Clone()
        {
            return new CameraInfo(name, focalLength, principalPoint.X, principalPoint.Y);
        }
    }
}
