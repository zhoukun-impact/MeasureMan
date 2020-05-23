using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpGL;
using System.IO;

namespace MeasureMan
{
    /// <summary>
    /// 自定义外包盒
    /// </summary>
    public struct BBox
    {
        public BBox(vec3 min, vec3 max) 
        { 
            this.min = min; 
            this.max = max;
            center = new vec3();
            length = new vec3();
        }

        public vec3 min;
        public vec3 max;
        public vec3 center;
        public vec3 length;
    }

    /// <summary>
    /// 三维模型数据类
    /// </summary>
    public class Model3D:IBufferSource
    {
        public vec3 GetModelSize()
        {
            return new vec3(1,1,1);
        }

        public const string strPosition = "position";
        private VertexBuffer positionBuffer;
        public const string strColor = "color";
        private VertexBuffer colorBuffer;


        private IDrawCommand drawCmd;

        /// <summary>
        /// 点集
        /// </summary>
        private List<vec3> points;
        /// <summary>
        /// 颜色
        /// </summary>
        private List<vec3> colors;
        /// <summary>
        /// 面集
        /// </summary>
        private List<Face> faceList;

        /// <summary>
        /// 获得点数量
        /// </summary>
        /// <returns>点数量</returns>
        public int VertexCount()
        {
            if (points == null)
                return 0;
            else
                return points.Count;
        }

        /// <summary>
        /// 获得面数量
        /// </summary>
        /// <returns>面数量</returns>
        public int FaceCount()
        {
            if (faceList == null)
                return 0;
            else
                return faceList.Count;
        }

        /// <summary>
        /// 获得外包盒
        /// </summary>
        /// <returns>外包盒</returns>
        public BBox GetBBox()
        {
            BBox bbox = new BBox(new vec3(float.MaxValue, float.MaxValue, float.MaxValue), new vec3(float.MinValue, float.MinValue, float.MinValue));
            foreach (vec3 pt in points)
            {
                if (pt.x > bbox.max.x)
                    bbox.max.x = pt.x;
                if (pt.x < bbox.min.x)
                    bbox.min.x = pt.x;
                if (pt.y > bbox.max.y)
                    bbox.max.y = pt.y;
                if (pt.y < bbox.min.y)
                    bbox.min.y = pt.y;
                if (pt.z > bbox.max.z)
                    bbox.max.z = pt.z;
                if (pt.z < bbox.min.z)
                    bbox.min.z = pt.z;
            }
            bbox.center = 0.5f * (bbox.max + bbox.min);
            bbox.length = bbox.max - bbox.min;
            return bbox;
        }

        /// <summary>
        /// 加载三维模型
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        public void LoadModel(string modelPath)
        {
            points = new List<vec3>();
            colors = new List<vec3>();
            faceList = new List<Face>();
            StreamReader sr = new StreamReader(modelPath);
            StringBuilder sb = new StringBuilder();
            while (!sr.EndOfStream)
            {
                sb.Append(sr.ReadLine() + "*");
            }
            sr.Close();
            string[] allInfo = sb.ToString().Split('*');
            int pointCount = int.Parse(allInfo[3].Split(' ')[2]);
            int faceCount = int.Parse(allInfo[14].Split(' ')[2]);
            int i=17;
            int count=i+pointCount;
            for (; i < count; i++)
            {
                string[] singlePt = allInfo[i].Split(' ');
                points.Add(new vec3((float)(double.Parse(singlePt[0])), (float)(double.Parse(singlePt[1])), (float)(double.Parse(singlePt[2]))));
                colors.Add(new vec3(float.Parse(singlePt[6]) / 255, float.Parse(singlePt[7]) / 255, float.Parse(singlePt[8]) / 255));
            }
            count = i + faceCount;
            for (; i < count; i++)
            {
                string[] singleFace = allInfo[i].Split(' ');
                faceList.Add(new Face(int.Parse(singleFace[1]), int.Parse(singleFace[2]), int.Parse(singleFace[3])));//face改为int以增大显示数量
            }
        }

        /// <summary>
        /// 加载稀疏点云
        /// </summary>
        /// <param name="sparseCloud">稀疏点云路径</param>
        public void LoadSparsePC(string sparseCloud)
        {
            points = new List<vec3>();
            colors = new List<vec3>();
            StreamReader sr = new StreamReader(sparseCloud);
            StringBuilder sb = new StringBuilder();
            while (!sr.EndOfStream)
            {
                sb.Append(sr.ReadLine() + "*");
            }
            sr.Close();
            string[] pointInfo = sb.ToString().Split('*');
            int pointCount = int.Parse(pointInfo[2].Split(' ')[2]) + 10;
            for (int i = 10; i < pointCount; i++)
            {
                string[] singlePt = pointInfo[i].Split(' ');
                points.Add(new vec3((float)(double.Parse(singlePt[0])), (float)(double.Parse(singlePt[1])), (float)(double.Parse(singlePt[2]))));
                colors.Add(new vec3(float.Parse(singlePt[3])/255, float.Parse(singlePt[4])/255, float.Parse(singlePt[5])/255));
            }
        }

        /// <summary>
        /// 加载稠密点云
        /// </summary>
        /// <param name="denseCloud">稠密点云路径</param>
        public void LoadDensePC(string denseCloud)
        {
            points = new List<vec3>();
            colors = new List<vec3>();
            StreamReader sr = new StreamReader(denseCloud);
            StringBuilder sb = new StringBuilder();
            while (!sr.EndOfStream)
            {
                sb.Append(sr.ReadLine() + "*");
            }
            sr.Close();
            string[] pointInfo = sb.ToString().Split('*');
            int pointCount = int.Parse(pointInfo[2].Split(' ')[2]) + 13;
            for (int i = 13; i < pointCount; i++)
            {
                string[] singlePt = pointInfo[i].Split(' ');
                points.Add(new vec3((float)(double.Parse(singlePt[0])), (float)(double.Parse(singlePt[1])), (float)(double.Parse(singlePt[2]))));
                colors.Add(new vec3(float.Parse(singlePt[6])/255, float.Parse(singlePt[7])/255, float.Parse(singlePt[8])/255));
            }
        }

        public IEnumerable<VertexBuffer> GetVertexAttribute(string bufferName)
        {
            if (bufferName == strPosition)
            {
                if (this.positionBuffer == null)
                {
                    this.positionBuffer = points.ToArray().GenVertexBuffer(VBOConfig.Vec3, BufferUsage.StaticDraw);
                }

                yield return this.positionBuffer;
            }
            else if (bufferName == strColor)
            {
                if (this.colorBuffer == null)
                {
                    this.colorBuffer = colors.ToArray().GenVertexBuffer(VBOConfig.Vec3, BufferUsage.StaticDraw);
                }

                yield return this.colorBuffer;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public IEnumerable<IDrawCommand> GetDrawCommand()
        {
            if (this.drawCmd == null)
            {
                if (faceList != null)
                {
                    Face[] faces = faceList.ToArray();
                    IndexBuffer buffer = faces.GenIndexBuffer(IndexBufferElementType.UInt, BufferUsage.StaticDraw);//改为int以增大显示数量
                    this.drawCmd = new DrawElementsCmd(buffer, DrawMode.Triangles);
                }
                else
                {
                    DrawArraysCmd buffer = new DrawArraysCmd(DrawMode.Points,VertexCount());
                    this.drawCmd = buffer;
                }
            }

            yield return this.drawCmd;
        }
    }
}
