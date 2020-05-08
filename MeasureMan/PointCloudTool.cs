using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 三维点云处理工具
    /// </summary>
    public class PointCloudTool
    {
        /// <summary>
        /// 待处理三维点云，包括稀疏点云和稠密点云
        /// </summary>
        public List<Point3D> pointCloud;

        /// <summary>
        /// 通用点云工具初始化方法
        /// </summary>
        /// <param name="points"></param>
        public PointCloudTool(List<Point3D> points)
        {
            pointCloud = points;
        }

        /// <summary>
        /// 由文件对点云工具初始化
        /// </summary>
        /// <param name="path">ply文件路径</param>
        public PointCloudTool(string path)
        {
            StreamReader sr = new StreamReader(path);
            StringBuilder sb = new StringBuilder();
            while(!sr.EndOfStream)
            {
                sb.Append(sr.ReadLine() + "*");
            }
            sr.Close();
            string[] pointInfo = sb.ToString().Split('*');
            pointCloud = new List<Point3D>();
            if (pointInfo[9].Equals("end_header"))
            {
                int pointCount = int.Parse(pointInfo[2].Split(' ')[2]) + 10;
                for (int i = 10; i < pointCount; i++)
                {
                    string[] singlePt = pointInfo[i].Split(' ');
                    pointCloud.Add(new Point3D(double.Parse(singlePt[0]), double.Parse(singlePt[1]), double.Parse(singlePt[2])));
                }
            }
            else//包含法线信息
            {
                int pointCount = int.Parse(pointInfo[2].Split(' ')[2]) + 13;
                for (int i = 13; i < pointCount; i++)
                {
                    string[] singlePt = pointInfo[i].Split(' ');
                    pointCloud.Add(new Point3D(double.Parse(singlePt[0]), double.Parse(singlePt[1]), double.Parse(singlePt[2])));
                }
            }
            
        }

        /// <summary>
        /// ply转为xyz
        /// </summary>
        /// <param name="denseCloudPath">稠密点云路径</param>
        /// <param name="savePath">保存路径</param>
        public static void Convert2XYZ(string denseCloudPath,string savePath,Point3D origin)
        {
            StreamReader sr = new StreamReader(denseCloudPath);
            StringBuilder sb = new StringBuilder();
            while (!sr.EndOfStream)
            {
                sb.Append(sr.ReadLine() + "*");
            }
            sr.Close();
            string[] pointInfo = sb.ToString().Split('*');
            StreamWriter sw = new StreamWriter(savePath);
            int pointCount = int.Parse(pointInfo[2].Split(' ')[2]) + 13;
            for (int i = 13; i < pointCount; i++)
            {
                string[] singlePt = pointInfo[i].Split(' ');
                if(origin!=null)
                    sw.WriteLine((double.Parse(singlePt[0]) + origin.x) + " " + (double.Parse(singlePt[1]) + origin.y) + " " + (double.Parse(singlePt[2])+origin.z));
                else
                    sw.WriteLine(double.Parse(singlePt[0]) + " " + double.Parse(singlePt[1]) + " " + double.Parse(singlePt[2]));
            }
            sw.Close();
        }

        /// <summary>
        /// 导出三维点云数据
        /// </summary>
        /// <param name="path">导出路径</param>
        public void OutputPointCloud(string path)
        {
            StreamWriter sw = new StreamWriter(path, false, Encoding.ASCII);//meshlab只认ascii编码和text默认编码
            sw.WriteLine("ply");
            sw.WriteLine("format ascii 1.0");
            sw.WriteLine("element vertex " + pointCloud.Count);
            sw.WriteLine("property float x");
            sw.WriteLine("property float y");
            sw.WriteLine("property float z");
            sw.WriteLine("property uchar red");
            sw.WriteLine("property uchar green");
            sw.WriteLine("property uchar blue");
            sw.WriteLine("end_header");
            foreach (Point3D point in pointCloud)
                sw.WriteLine(point.x+ " " + point.y + " " + point.z + " 255 255 0" );
            sw.Flush();
            sw.Close();
        }
    }
}
