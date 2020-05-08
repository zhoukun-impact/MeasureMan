using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 单通道Mat扩展类
    /// </summary>
    public class ME : Mat
    {
        public ME()
        {
        }

        public ME(int rows, int cols, DepthType type): base(rows, cols, type, 1)
        {
        }

        /// <summary>
        /// Mat转化为ME
        /// </summary>
        /// <param name="mat">待转化的Mat</param>
        public ME(Mat mat): base(mat, new System.Drawing.Rectangle(0, 0, mat.Width, mat.Height))
        {
        }

        /// <summary>
        /// 获得或设置矩阵的值
        /// </summary>
        /// <param name="row">行号</param>
        /// <param name="col">列号</param>
        /// <returns>获取的值</returns>
        public dynamic this[int row,int col=0]
        {
            get 
            {
                var value1 = CreateElement();
                Marshal.Copy(this.DataPointer + (row * this.Cols + col) * this.ElementSize, value1, 0, 1);
                return value1[0];
            }
            set
            {
                var target = CreateElement();
                target[0] = value;
                Marshal.Copy(target, 0, this.DataPointer + (row * this.Cols + col) * this.ElementSize, 1);
            }
        }

        /// <summary>
        /// 创建大小为1的基本类型数组
        /// </summary>
        /// <param name="depthType">Mat的深度类型</param>
        /// <returns>数组结果</returns>
        private dynamic CreateElement()
        {
            DepthType depthType = this.Depth;
            if (depthType == DepthType.Cv8S)
            {
                return new sbyte[1];
            }
            else if (depthType == DepthType.Cv8U)
            {
                return new byte[1];
            }
            else if (depthType == DepthType.Cv16S)
            {
                return new short[1];
            }
            else if (depthType == DepthType.Cv16U)
            {
                return new ushort[1];
            }
            else if (depthType == DepthType.Cv32S)
            {
                return new int[1];
            }
            else if (depthType == DepthType.Cv64F)
            {
                return new double[1];
            }
            else
            {
                return new float[1];
            }
        }

        /// <summary>
        /// 将矩阵转化为相应类型的数组
        /// </summary>
        /// <returns>数组结果</returns>
        public dynamic ToArray()
        {
            DepthType depthType = this.Depth;
            int length = this.Rows * this.Cols;
            dynamic arr;
            if (depthType == DepthType.Cv8S)
            {
                arr=new sbyte[length];
                this.CopyTo<sbyte>(arr);
            }
            else if (depthType == DepthType.Cv8U)
            {
                arr = new byte[length];
                this.CopyTo<byte>(arr);
            }
            else if (depthType == DepthType.Cv16S)
            {
                arr = new short[length];
                this.CopyTo<short>(arr);
            }
            else if (depthType == DepthType.Cv16U)
            {
                arr = new ushort[length];
                this.CopyTo<ushort>(arr);
            }
            else if (depthType == DepthType.Cv32S)
            {
                arr = new int[length];
                this.CopyTo<int>(arr);
            }
            else if (depthType == DepthType.Cv64F)
            {
                arr = new double[length];
                this.CopyTo<double>(arr);
            }
            else
            {
                arr = new float[length];
                this.CopyTo<float>(arr);
            }
            return arr;
        }

        /// <summary>
        /// 矩阵加法
        /// </summary>
        /// <param name="lhs">左矩阵</param>
        /// <param name="rhs">右矩阵</param>
        /// <returns>结果</returns>
        public static ME operator +(ME lhs, ME rhs)
        {
            if (lhs.Rows != rhs.Rows || lhs.Cols != rhs.Cols)
                return null;
            dynamic data1 = lhs.ToArray();
            dynamic data2 = rhs.ToArray();
            double[] result = new double[lhs.Rows * lhs.Cols];
            for (int i = 0; i < data1.Length; i++)
                result[i] = data1[i] + data2[i];
            ME mat = new ME(lhs.Rows, lhs.Cols, DepthType.Cv64F);
            mat.SetTo<double>(result);
            return mat;
        }

        /// <summary>
        /// 矩阵加法
        /// </summary>
        /// <param name="lhs">矩阵</param>
        /// <param name="rhs">常数</param>
        /// <returns>结果</returns>
        public static ME operator +(ME lhs, double rhs)
        {
            dynamic data1 = lhs.ToArray();
            double[] result = new double[lhs.Rows * lhs.Cols];
            for (int i = 0; i < data1.Length; i++)
                result[i] = data1[i] + rhs;
            ME mat = new ME(lhs.Rows, lhs.Cols, DepthType.Cv64F);
            mat.SetTo<double>(result);
            return mat;
        }

        /// <summary>
        /// 矩阵减法
        /// </summary>
        /// <param name="lhs">左矩阵</param>
        /// <param name="rhs">右矩阵</param>
        /// <returns>结果</returns>
        public static ME operator -(ME lhs, ME rhs)
        {
            if (lhs.Rows != rhs.Rows || lhs.Cols != rhs.Cols)
                return null;
            dynamic data1 = lhs.ToArray();
            dynamic data2 = rhs.ToArray();
            double[] result = new double[lhs.Rows * lhs.Cols];
            for (int i = 0; i < data1.Length; i++)
                result[i] = data1[i]-data2[i];
            ME mat = new ME(lhs.Rows, lhs.Cols, DepthType.Cv64F);
            mat.SetTo<double>(result);
            return mat;
        }

        /// <summary>
        /// 矩阵减法
        /// </summary>
        /// <param name="lhs">矩阵</param>
        /// <param name="rhs">常数</param>
        /// <returns>结果</returns>
        public static ME operator -(ME lhs, double rhs)
        {
            dynamic data1 = lhs.ToArray();
            double[] result = new double[lhs.Rows * lhs.Cols];
            for (int i = 0; i < data1.Length; i++)
                result[i] = data1[i]-rhs;
            ME mat = new ME(lhs.Rows, lhs.Cols, DepthType.Cv64F);
            mat.SetTo<double>(result);
            return mat;
        }

        /// <summary>
        /// 矩阵取负
        /// </summary>
        /// <param name="lhs">矩阵</param>
        /// <returns>结果</returns>
        public static ME operator -(ME lhs)
        {
            dynamic data1 = lhs.ToArray();
            double[] result = new double[lhs.Rows * lhs.Cols];
            for (int i = 0; i < data1.Length; i++)
                result[i] = -data1[i];
            ME mat = new ME(lhs.Rows, lhs.Cols, DepthType.Cv64F);
            mat.SetTo<double>(result);
            return mat;
        }

        /// <summary>
        /// 矩阵乘法
        /// </summary>
        /// <param name="lhs">左矩阵</param>
        /// <param name="rhs">右矩阵</param>
        /// <returns>结果</returns>
        public static ME operator *(ME lhs, ME rhs)
        {
            if (lhs.Cols != rhs.Rows)
                return null;
            double[] result = new double[lhs.Rows * rhs.Cols];
            for (int i = 0; i < lhs.Rows * rhs.Cols; i++)
                result[i] = lhs.Row(i / rhs.Cols).Dot(rhs.Col(i % rhs.Cols).T());
            ME mat = new ME(lhs.Rows, rhs.Cols, DepthType.Cv64F);
            mat.SetTo<double>(result);
            return mat;
        }

        /// <summary>
        /// 矩阵乘法
        /// </summary>
        /// <param name="lhs">常数</param>
        /// <param name="rhs">矩阵</param>
        /// <returns>结果</returns>
        public static ME operator *(double rhs, ME lhs)
        {
            dynamic data1 = lhs.ToArray();
            double[] result = new double[lhs.Rows * lhs.Cols];
            for (int i = 0; i < data1.Length; i++)
                result[i] = data1[i]*rhs;
            ME mat = new ME(lhs.Rows, lhs.Cols, DepthType.Cv64F);
            mat.SetTo<double>(result);
            return mat;
        }

        /// <summary>
        /// 矩阵转置
        /// </summary>
        /// <param name="lhs">原矩阵</param>
        /// <returns>转置矩阵</returns>
        public static ME operator ~(ME lhs)
        {
            return new ME(lhs.T());
        }

        /// <summary>
        /// 矩阵求逆
        /// </summary>
        /// <param name="lhs">原矩阵</param>
        /// <returns>矩阵的逆</returns>
        public static ME operator !(ME lhs)
        {
            if (lhs.Rows != lhs.Cols)
                return null;
            if (lhs.Depth != DepthType.Cv64F)
                return null;
            Mat mat = new Mat();
            CvInvoke.Invert(lhs, mat, DecompMethod.LU);
            return new ME(mat);
        }

        /// <summary>
        /// 将三维点转化为矩阵
        /// </summary>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <param name="z">z坐标</param>
        /// <returns>3*1的矩阵</returns>
        public static ME FromPoint3D(double x,double y,double z)
        {
            ME mat = new ME(3, 1, DepthType.Cv64F);
            mat[0, 0] = x;
            mat[1, 0] = y;
            mat[2, 0] = z;
            return mat;
        }

        /// <summary>
        /// 将三维点转化为矩阵
        /// </summary>
        /// <param name="point">三维点</param>
        /// <returns>3*1的矩阵</returns>
        public static ME FromPoint3D(Point3D point)
        {
            ME mat = new ME(3, 1, DepthType.Cv64F);
            mat[0, 0] = point.x;
            mat[1, 0] = point.y;
            mat[2, 0] = point.z;
            return mat;
        }

        /// <summary>
        /// 将二维同名点集转化为矩阵
        /// </summary>
        /// <param name="points">同名点集</param>
        /// <returns>N*2的矩阵</returns>
        public static ME FromPoints(List<MatchPoint> points)
        {
            ME mat = new ME(points.Count, 2, DepthType.Cv64F);
            for (int i = 0; i < points.Count; i++)
            {
                mat[i, 0] = points[i].ftPoint.X;
                mat[i, 1] = points[i].ftPoint.Y;
            }
            return mat;
        }

        /// <summary>
        /// 将3D点集转为矩阵
        /// </summary>
        /// <param name="points">三维点集</param>
        /// <returns>N*3的矩阵</returns>
        public static ME FromPoints3D(List<Point3D> points)
        {
            ME mat = new ME(points.Count, 3, DepthType.Cv64F);
            for (int i = 0; i < points.Count; i++)
            {
                mat[i, 0] = points[i].x;
                mat[i, 1] = points[i].y;
                mat[i, 2] = points[i].z;
            }
            return mat;
        }

        /// <summary>
        /// 从像素坐标转换为相机坐标
        /// </summary>
        /// <param name="point">匹配点像素坐标</param>
        /// <param name="K">相机内参</param>
        public void PixPointToCamPoint(ME K)
        {
            double fx = K[0, 0];
            double fy = K[1, 1];
            double cx = K[0, 2];
            double cy = K[1, 2];
            for (int i = 0; i < this.Height; i++)
            {
                this[i, 0] = (this[i, 0] - cx) / fx;
                this[i, 1] = (this[i, 1] - cy) / fy;
            }
        }

        /// <summary>
        /// P=[R|T]
        /// </summary>
        /// <param name="_R">旋转矩阵</param>
        /// <param name="_t">平移矩阵</param>
        public void CombineR_t(ME _R, ME _t)
        {
            if (_t.Height != 3)
                _t = ~_t;

            for (int i = 0; i < _R.Height; i++)
                for (int j = 0; j < _R.Width; j++)
                    this[i, j] = _R[i, j];

            for (int i = 0; i < _t.Height; i++)
                this[i, 3] = _t[i, 0];
        }

        /// <summary>
        /// 稀疏重建初始化三维点规范方法
        /// </summary>
        /// <param name="P2">种子匹配对中求解的P矩阵</param>
        /// <param name="mask">根据重投影被筛选的点掩膜</param>
        /// <returns>保留三维点数量</returns>
        public int NormPoint3D(ME P2,ME mask)
        {
            int NotZeroCount = 0;
            if (this.Height == 4)
            {
                for (int i = 0; i < this.Width; i++)
                {
                    double w = this[3, i];
                    bool flag=this[2, i]*w>0;
                    this[0, i] /= w;
                    this[1, i] /= w;
                    this[2, i] /= w;
                    double z2;
                    ME Q2 = P2 * (new ME(this.Col(i)));
                    z2 = Q2[2, 0];
                    if (flag && z2 > 0 && z2 < 10 && this[2, i] < 10)
                    {
                        NotZeroCount++;
                        mask[i, 0] = (byte)1;
                    }
                }
            }
            return NotZeroCount;
        }

        /// <summary>
        /// 稀疏重建增量三维点规范方法
        /// </summary>
        /// <param name="P1">左图P矩阵</param>
        /// <param name="P2">右图P矩阵</param>
        /// <param name="K1">左图相机内参</param>
        /// <param name="K2">右图相机内参</param>
        /// <param name="mask">根据重投影被筛选的点掩膜</param>
        /// <param name="leftPts">左图同名点</param>
        /// <param name="rightPts">右图同名点</param>
        /// <returns>保留三维点数量</returns>
        public int NormPoint3D(ME P1, ME P2, ME K1,ME K2,ME mask, List<MatchPoint> leftPts, List<MatchPoint> rightPts)
        {
            int NotZeroCount = 0;
            if (this.Height == 4)
            {
                for (int i = 0; i < this.Width; i++)
                {
                    double w = this[3, i];
                    if (w == 0) w = 1;
                    this[0, i] /= w;
                    this[1, i] /= w;
                    this[2, i] /= w;
                    this[3, i] /= w;
                    ME objPt=new ME(this.Col(i));
                    ME imgPtOnLeft = P1 * objPt;
                    ME imgPtOnRight = P2 * objPt;
                    double x1 = imgPtOnLeft[0] / imgPtOnLeft[2] * K1[0] + K1[0, 2];
                    double y1 = imgPtOnLeft[1] / imgPtOnLeft[2] * K1[1, 1] + K1[1, 2];
                    double x2 = imgPtOnRight[0] / imgPtOnRight[2] * K2[0] + K2[0, 2];
                    double y2 = imgPtOnRight[1] / imgPtOnRight[2] * K2[1, 1] + K2[1, 2];
                    if (Math.Pow(leftPts[i].ftPoint.X - x1, 2) + Math.Pow(leftPts[i].ftPoint.Y - y1, 2) <1&& Math.Pow(rightPts[i].ftPoint.X - x2, 2) + Math.Pow(rightPts[i].ftPoint.Y - y2, 2) <1)
                    {
                        NotZeroCount++;
                        mask[i, 0] = (byte)1;
                    }
                }
            }
            return NotZeroCount;
        }

        /// <summary>
        /// 将规范化后的三维点加入到track中
        /// </summary>
        /// <param name="tracks">track集合</param>
        /// <param name="right">右图同名点</param>
        /// <param name="mask">规范化生成的掩膜</param>
        public void Added3DInTrack(List<Track> tracks, List<MatchPoint> right,ME mask)
        {
            for (int i = 0; i < this.Width; i++)
            {
                if (mask==null||mask[i, 0] == (byte)1)
                {
                    int trackIndex = right[i].trackIndex;
                    Point3D srcPt = tracks[trackIndex]._pt;
                    Point3D point = new Point3D(this[0, i], this[1, i],this[2, i]);
                    tracks[trackIndex]._pt = CheckPoint(srcPt, point);//根据已有三维点进行筛点
                }
            }
        }

        /// <summary>
        /// 检查新产生的三维点
        /// </summary>
        /// <param name="trackPoint">轨迹上原有三维点</param>
        /// <param name="newPoint">新产生的三维点</param>
        /// <returns>检查后的三维点</returns>
        private Point3D CheckPoint(Point3D trackPoint,Point3D newPoint)
        {
            if (trackPoint == null)
            {
                if (newPoint.Check(20))
                    return newPoint;
                else
                    return trackPoint;
            }
            else
            {
                if(Math.Pow(newPoint.x - trackPoint.x, 2) + Math.Pow(newPoint.y - trackPoint.y, 2) + Math.Pow(newPoint.z - trackPoint.z,2) < 0.1)
                    return new Point3D((newPoint.x + trackPoint.x) / 2, (newPoint.y + trackPoint.y) / 2, (newPoint.z + trackPoint.z) / 2);
                else
                    return trackPoint;
            }
        }
    }
}
