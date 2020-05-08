using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;

namespace MeasureMan
{
    /// <summary>
    /// 相机参数
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// 相机信息
        /// </summary>
        CameraInfo info;
        /// <summary>
        /// 旋转矩阵
        /// </summary>
        public ME R;
        /// <summary>
        /// 平移矩阵
        /// </summary>
        public ME T;
        /// <summary>
        /// 畸变系数
        /// </summary>
        public double distortion;
        /// <summary>
        /// 焦距
        /// </summary>
        public double f
        {
            get
            {
                return info.focalLength;
            }
            set
            {
                info.focalLength = value;
            }
        }
        /// <summary>
        /// 主点x坐标
        /// </summary>
        public float cx
        {
            get
            {
                return info.principalPoint.X;
            }
            set
            {
                info.principalPoint.X = value;
            }
        }
        /// <summary>
        /// 主点y坐标
        /// </summary>
        public float cy
        {
            get
            {
                return info.principalPoint.Y;
            }
            set
            {
                info.principalPoint.Y = value;
            }
        }

        /// <summary>
        /// 相机创建方法
        /// </summary>
        /// <param name="img">图像</param>
        public Camera(AddedImage img)
        {
            info = img.camera;
            distortion = 0;
        }

        /// <summary>
        /// 像素坐标转图像坐标
        /// </summary>
        /// <param name="pixelPoint">像素坐标</param>
        /// <returns>图像坐标</returns>
        public PointF Pixel2Image(PointF pixelPoint)
        {
            return new PointF(pixelPoint.X - cx, pixelPoint.Y - cy);
        }

        /// <summary>
        /// 获得3*3相机内参矩阵
        /// </summary>
        //
        public ME GetIntrinsicMatrix()
        {
            ME intrinsicMatrix = new ME(3, 3, DepthType.Cv64F);
            double[] data = new double[]
            {
                f,0,cx,
                0,f,cy,
                0,0,1
            };
            intrinsicMatrix.SetTo<double>(data);
            return intrinsicMatrix;
        }

        /// <summary>
        /// 获取相机位置
        /// </summary>
        /// <returns></returns>
        public ME GetPosition()
        {
            return -(~R) * T;
        }
        /*
        /// <summary>
        /// 由旋转矩阵转化为旋转向量
        /// </summary>
        /// <returns></returns>
        public ME GetRotationVector()
        {
            ME _R = new ME(3, 1, DepthType.Cv64F);
            CvInvoke.Rodrigues(R, _R);
            return _R;
        }

        /// <summary>
        /// 由旋转向量转化为旋转矩阵
        /// </summary>
        /// <param name="vector">数据</param>
        public void SetRotationVector(double[] vector)
        {
            ME _R = new ME(3, 1, DepthType.Cv64F);
            _R.SetTo<double>(vector);
            CvInvoke.Rodrigues(_R, R);
        }

        /// <summary>
        /// 局部BA，优化PnP的结果
        /// </summary>
        /// <param name="pts">三维点</param>
        /// <param name="mps">二维点</param>
        /// <param name="rootDir">文件根目录</param>
        public void LocalBA(List<Point3D> pts, List<MatchPoint> mps, string rootDir)
        {
            StreamWriter sw = new StreamWriter(rootDir + "nvm.ba");
            ME R = GetRotationVector();
            sw.WriteLine(f + " " + distortion + " " + R[0, 0] + " " + R[1, 0] + " " + R[2, 0] + " " +
                T[0, 0] + " " + T[1, 0] + " " + T[2, 0]);
            sw.WriteLine(pts.Count);
            for (int i = 0; i < pts.Count; i++)
            {
                PointF p = Pixel2Image(mps[i].ftPoint);
                sw.WriteLine(pts[i].x + " " + pts[i].y + " " + pts[i].z + " " + p.X + " " + p.Y);
            }
            sw.Flush();
            sw.Close();
            double[] new_fd = new double[2];
            double[] new_RT = new double[6];
            ExternLibInvoke.LocalBA(rootDir, new_fd, new_RT);
            f = new_fd[0]; distortion = new_fd[1];
            SetRotationVector(new double[] { new_RT[0], new_RT[1], new_RT[2] });
            T[0, 0] = new_RT[3]; T[1, 0] = new_RT[4]; T[2, 0]=new_RT[5];
        }

        
        /// <summary>
        /// 由非畸变坐标系转为畸变坐标系（原始坐标系）by Dan Costin
        /// </summary>
        /// <param name="x">非畸变x</param>
        /// <param name="y">非畸变y</param>
        /// <returns>原始坐标系点</returns>
        public PointF DistortionCorrection(double x,double y)
        {
            if (distortion == 0)
                return new PointF((float)x, (float)y);
            double t2 = y * y;
            double t3 = t2 * t2 * t2;
            double t4 = x * x;
            double t7 = distortion * (t2 + t4);
            if (distortion > 0)
            {
                double t8 = 1.0 / t7;
                double t10 = t3 / (t7 * t7);
                double t14 = Math.Sqrt(t10 * (0.25 + t8 / 27.0));
                double t15 = t2 * t8 * y * 0.5;
                double t17 = Math.Pow(t14 + t15, 1.0 / 3.0);
                double t18 = t17 - t2 * t8 / (t17 * 3.0);
                return new PointF((float)(t18 * x / y), (float)t18);
            }
            else
            {
                double t9 = t3 / (t7 * t7 * 4.0);
                double t11 = t3 / (t7 * t7 * t7 * 27.0);
                Complex t12 = t9 + t11;
                Complex t13 = Complex.Sqrt(t12);
                double t14 = t2 / t7;
                double t15 = t14 * y * 0.5;
                Complex t16 = t13 + t15;
                Complex t17 = Complex.Pow(t16, 1.0 / 3.0);
                Complex t18 = (t17 + t14 / (t17 * 3.0))*(new Complex(0.0,Math.Sqrt(3.0)));
                Complex t19 = -0.5 * (t17 + t18) + t14 / (t17 * 6.0);
                return new PointF((float)(t19.Real * x / y), (float)t19.Real);
            }
        }*/
    }
}
