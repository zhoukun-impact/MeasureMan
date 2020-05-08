using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 带有投影坐标的地理点
    /// </summary>
    public class GeoPoint:Point3D
    {
        /// <summary>
        /// -1表示未投影，其他表示投影编号
        /// </summary>
        public int proCode;

        /// <summary>
        /// 无投影坐标系的地理点初始化方法
        /// </summary>
        /// <param name="x">经度或x坐标</param>
        /// <param name="y">纬度或y坐标</param>
        /// <param name="z">高程</param>
        public GeoPoint(double x, double y, double z)
            : base(x, y, z)
        {
            proCode = -1;
        }

        /// <summary>
        /// 携带投影坐标系的地理点初始化方法
        /// </summary>
        /// <param name="x">经度或x坐标</param>
        /// <param name="y">纬度或y坐标</param>
        /// <param name="z">高程</param>
        /// <param name="proCode">投影坐标系编号</param>
        public GeoPoint(double x, double y, double z, int proCode):base(x,y,z)
        {
            this.proCode = proCode;
        }

        /// <summary>
        /// 点投影
        /// </summary>
        /// <param name="geo">地理坐标系</param>
        /// <param name="pro">投影坐标系</param>
        /// <param name="prj">投影坐标系编号</param>
        public void ProjectPoint(ISpatialReference geo, ISpatialReference prj, int proCode)
        {
            IPoint point = new PointClass();
            point.PutCoords(x, y);
            point.SpatialReference = geo;
            point.Project(prj);
            x = point.X;
            y = point.Y;
            this.proCode = proCode;
        }

        /// <summary>
        /// WGS84转UTM
        /// </summary>
        /// /// <param name="proCode">投影坐标系编号</param>
        public void ProjectPoint(int proCode)
        {
            double sm_a = 6378137, sm_b = 6356752.314, UTMScaleFactor = 0.9996;
            int zone = proCode % 100;
            double cmeridian = GetRad(-183.0 + (zone * 6.0));//中央子午线
            double phi = GetRad(y);//纬度弧度
            double lamda = GetRad(x);//经度弧度

            double N, nu2, ep2, t, t2, l;
            double l3coef, l4coef, l5coef, l6coef, l7coef, l8coef;
            double tmp;
            ep2 = (Math.Pow(sm_a, 2.0) - Math.Pow(sm_b, 2.0)) / Math.Pow(sm_b, 2.0);
            nu2 = ep2 * Math.Pow(Math.Cos(phi), 2.0);
            N = Math.Pow(sm_a, 2.0) / (sm_b * Math.Sqrt(1 + nu2));
            t = Math.Tan(phi);
            t2 = t * t;
            tmp = (t2 * t2 * t2) - Math.Pow(t, 6.0);
            l = lamda - cmeridian;

            /* Precalculate coefficients for l**n in the equations below
               so a normal human being can read the expressions for easting
               and northing
               -- l**1 and l**2 have coefficients of 1.0 */
            l3coef = 1.0 - t2 + nu2;
            l4coef = 5.0 - t2 + 9 * nu2 + 4.0 * (nu2 * nu2);
            l5coef = 5.0 - 18.0 * t2 + (t2 * t2) + 14.0 * nu2 - 58.0 * t2 * nu2;
            l6coef = 61.0 - 58.0 * t2 + (t2 * t2) + 270.0 * nu2 - 330.0 * t2 * nu2;
            l7coef = 61.0 - 479.0 * t2 + 179.0 * (t2 * t2) - (t2 * t2 * t2);
            l8coef = 1385.0 - 3111.0 * t2 + 543.0 * (t2 * t2) - (t2 * t2 * t2);

            double eastx = N * Math.Cos(phi) * l
                + (N / 6.0 * Math.Pow(Math.Cos(phi), 3.0) * l3coef * Math.Pow(l, 3.0))
                + (N / 120.0 * Math.Pow(Math.Cos(phi), 5.0) * l5coef * Math.Pow(l, 5.0))
                + (N / 5040.0 * Math.Pow(Math.Cos(phi), 7.0) * l7coef * Math.Pow(l, 7.0));

            /*计算中央经线弧线长度*/
            double alpha, beta, gamma, delta, epsilon, n;
            n = (sm_a - sm_b) / (sm_a + sm_b);
            alpha = ((sm_a + sm_b) / 2.0) * (1.0 + (Math.Pow(n, 2.0) / 4.0) + (Math.Pow(n, 4.0) / 64.0));
            beta = (-3.0 * n / 2.0) + (9.0 * Math.Pow(n, 3.0) / 16.0) + (-3.0 * Math.Pow(n, 5.0) / 32.0);
            gamma = (15.0 * Math.Pow(n, 2.0) / 16.0) + (-15.0 * Math.Pow(n, 4.0) / 32.0);
            delta = (-35.0 * Math.Pow(n, 3.0) / 48.0) + (105.0 * Math.Pow(n, 5.0) / 256.0);
            epsilon = (315.0 * Math.Pow(n, 4.0) / 512.0);
            double arcLength = alpha * (phi + (beta * Math.Sin(2.0 * phi)) + (gamma * Math.Sin(4.0 * phi))
                    + (delta * Math.Sin(6.0 * phi)) + (epsilon * Math.Sin(8.0 * phi)));

            double northy = arcLength
                + (t / 2.0 * N * Math.Pow(Math.Cos(phi), 2.0) * Math.Pow(l, 2.0))
                + (t / 24.0 * N * Math.Pow(Math.Cos(phi), 4.0) * l4coef * Math.Pow(l, 4.0))
                + (t / 720.0 * N * Math.Pow(Math.Cos(phi), 6.0) * l6coef * Math.Pow(l, 6.0))
                + (t / 40320.0 * N * Math.Pow(Math.Cos(phi), 8.0) * l8coef * Math.Pow(l, 8.0));


            x = eastx * UTMScaleFactor + 500000.0;
            y = northy * UTMScaleFactor;
            if (y < 0)
                y += 10000000.0;

            this.proCode = proCode;
        }

        /// <summary>
        /// 计算两点间距离
        /// </summary>
        /// <param name="other">另一个点</param>
        /// <returns>经纬度平面距离/投影坐标欧氏距离,-1表示计算条件出错</returns>
        public double GetDistance(GeoPoint other)
        {
            if (this.proCode != -1 && other.proCode != -1)
            {
                return Math.Sqrt(Math.Pow(this.x - other.x, 2) + Math.Pow(this.y - other.y, 2) + Math.Pow(this.z - other.z, 2));
            }
            else if (this.proCode == -1 && other.proCode == -1)
            {
                double earth_radius = 6378137;//地球半径
                double f = 1 / 298.257;//焦距
                double lat_plus = GetRad((this.y + other.y) / 2);
                double lat_minus = GetRad((this.y - other.y) / 2);
                double lng_minus = GetRad((this.x - other.x) / 2);
                double sin2_lat_plus = Math.Pow(Math.Sin(lat_plus), 2);
                double sin2_lat_minus = Math.Pow(Math.Sin(lat_minus), 2);
                double sin2_lng_minus = Math.Pow(Math.Sin(lng_minus), 2);
                double s = sin2_lat_minus * (1 - sin2_lng_minus) + (1 - sin2_lat_plus) * sin2_lng_minus;
                double c = (1 - sin2_lat_minus) * (1 - sin2_lng_minus) + sin2_lat_plus * sin2_lng_minus;
                double w = Math.Atan(Math.Sqrt(s / c));
                double r = Math.Sqrt(s * c) / w;
                double d = 2 * w * earth_radius;
                double h1 = (3 * r - 1) / (2 * c);
                double h2 = (3 * r + 1) / (2 * s);
                return d * (1 + f * (h1 * sin2_lat_plus * (1 - sin2_lat_minus) - h2 * (1 - sin2_lat_plus) * sin2_lat_minus));
            }
            else
                return -1;
        }

        /// <summary>
        /// 角度制向弧度制转换
        /// </summary>
        /// <param name="degree">角度</param>
        /// <returns>弧度</returns>
        private double GetRad(double degree)
        {
            return degree * Math.PI / 180;
        }

        /// <summary>
        /// 克隆地理点
        /// </summary>
        /// <returns>克隆后的地理点</returns>
        public new  GeoPoint Clone()
        {
            return new GeoPoint(x, y, z,proCode);
        }
    }
}
