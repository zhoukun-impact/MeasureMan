using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 添加的图像数据
    /// </summary>
    public class AddedImage
    {
        /// <summary>
        /// 图像名称
        /// </summary>
        public string name;
        /// <summary>
        /// 图像第一次创建的时间
        /// </summary>
        public DateTime time;
        /// <summary>
        /// 图像在集合中的序号,从1开始
        /// </summary>
        public int order;
        /// <summary>
        /// 图像文件路径，用于图像显示和处理
        /// </summary>
        public string path;
        /// <summary>
        /// 无人机POS数据
        /// </summary>
        public POS _POS;
        /// <summary>
        /// 标识是否裁剪
        /// </summary>
        public bool clipped;
        /// <summary>
        /// 当clipped为true时才考虑图像裁剪区域,此时所有检测到的特征点的坐标都要进行相应的变换
        /// </summary>
        public Rectangle ROI;
        /// <summary>
        /// 相机参数
        /// </summary>
        public CameraInfo camera;

        public AddedImage() { }

        /// <summary>
        /// 视频帧初始化方法
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="time">修改时间</param>
        /// <param name="order">序号</param>
        /// <param name="path">路径</param>
        public AddedImage(string name, DateTime time, int order,string path)
        {
            this.name = name;
            this.time = time;
            this.order = order;
            this.path = path;
        }

        /// <summary>
        /// 带相机参数的图像初始化方法
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="time">修改时间</param>
        /// <param name="order">序号</param>
        /// <param name="path">路径</param>
        /// <param name="camera">相机参数</param>
        public AddedImage(string name, DateTime time, int order, string path,CameraInfo camera)
        {
            this.name = name;
            this.time = time;
            this.order = order;
            this.path = path;
            this.camera = camera;
        }
    }
}
