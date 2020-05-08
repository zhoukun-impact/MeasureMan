using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// POS数据
    /// </summary>
    public class POS:GeoPoint
    {
        /// <summary>
        /// 俯仰角（绕x方向，正东方向，旋转，废弃）
        /// </summary>
        public double pitch;
        /// <summary>
        /// 偏航角（绕z方向，天方向，旋转，废弃）
        /// </summary>
        public double yaw;
        /// <summary>
        /// 翻滚角（绕y方向，正北方向，旋转，废弃）
        /// </summary>
        public double roll;

        /// <summary>
        /// 初始化POS点方法
        /// </summary>
        /// <param name="y">纬度</param>
        /// <param name="x">经度</param>
        /// <param name="z">高程</param>
        /// <param name="pitch">俯仰角</param>
        /// <param name="yaw">偏航角</param>
        /// <param name="roll">翻滚角</param>
        public POS(double y,double x,double z, double pitch, double yaw, double roll):base(x,y,z)
        {
            this.pitch = pitch;
            this.yaw = yaw;
            this.roll = roll;
        }

    }
}
