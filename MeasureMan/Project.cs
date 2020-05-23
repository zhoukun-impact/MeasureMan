using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// msm工程文件
    /// </summary>
    public class Project
    {
        /// <summary>
        /// 工程所在的文件路径
        /// </summary>
        public string path;
        /// <summary>
        /// 工程输入数据的类型，0表示图像数据，1表示影像数据
        /// </summary>
        public int dataType;
        /// <summary>
        /// 工程所使用的特征类型
        /// </summary>
        public FeatureType ftType;
        /// <summary>
        /// 表示工程文件的保存状态，当工程受到修改时，表现为fasle，工程保存后，表现为true
        /// </summary>
        public bool saved;
        /// <summary>
        /// 图像数据
        /// </summary>
        public List<AddedImage> images;
        /// <summary>
        /// 控制点数据
        /// </summary>
        public List<GCP> GCPs;
        /// <summary>
        /// 指示能否进行关键帧精提取
        /// </summary>
        public bool canTrack;
        /// <summary>
        /// 相机参数
        /// </summary>
        public CameraInfo camera;
        /// <summary>
        /// 特征点文件路径
        /// </summary>
        public List<string> ftPaths;
        /// <summary>
        /// 双视模型
        /// </summary>
        public List<TwoViewModel> TVMs;
        /// <summary>
        /// 稀疏点云路径
        /// </summary>
        public string sparseCloud;
        /// <summary>
        /// 稠密点云路径
        /// </summary>
        public string denseCloud;
        /// <summary>
        /// 三维模型路径
        /// </summary>
        public string modelPath;
        /// <summary>
        /// DSM数据路径
        /// </summary>
        public string DSMPath;
        /// <summary>
        /// N视模型
        /// </summary>
        public NViewModel nvm;

        /// <summary>
        /// 工程基本信息初始化方法
        /// </summary>
        /// <param name="path">工程路径</param>
        /// <param name="dataType">使用数据类型</param>
        /// <param name="ftType">使用特征类型</param>
        public Project(string path, int dataType,FeatureType ftType)
        {
            this.path = path;
            this.dataType = dataType;
            this.ftType = ftType;
            this.saved = true;
            if (dataType == 0)
                canTrack = false;
            else
                canTrack = true;
        }

        /// <summary>
        /// 初始化工程文件信息
        /// </summary>
        public void InitiateProject()
        {
            StreamWriter sw = new StreamWriter(path);
            sw.WriteLine("ZKGCPZD");//文件标识码
            sw.WriteLine(dataType);//数据类型
            sw.WriteLine(ftType.ToString());//特征类型
            sw.WriteLine(canTrack);//是否能进行特征点追踪
            sw.WriteLine("--------");
            sw.Flush();
            sw.Close();
            string dir = System.IO.Path.GetDirectoryName(path);
            Directory.CreateDirectory(dir + "\\" + "img");
            Directory.CreateDirectory(dir + "\\" + "ft");
            Directory.CreateDirectory(dir + "\\" + "rel");
        }

        /// <summary>
        /// 查找双视模型是否存在
        /// </summary>
        /// <param name="imgIndex1">双视模型包含的图像1</param>
        /// <param name="imgIndex2">双视模型包含的图像2</param>
        /// <returns>true为存在，false为不存在</returns>
        public bool Exist(int imgIndex1, int imgIndex2)
        {
            foreach (TwoViewModel model in TVMs)
            {
                if ((model.left == imgIndex1&&model.right == imgIndex2)||(model.left == imgIndex2&&model.right == imgIndex1))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 判断多有双视模型的焦距是否相等
        /// </summary>
        public void JudgeEqualF()
        {
            foreach (TwoViewModel model in TVMs)
            {
                if (images[model.left].camera.focalLength == images[model.right].camera.focalLength)
                    model.equalFocalLength = true;
            }
        }

        /// <summary>
        /// 获得包含某一图像所有双视模型的匹配总数(焦距相等)
        /// </summary>
        /// <param name="imgIndex">图像索引</param>
        /// <returns>匹配总数</returns>
        public int GetTotalMatches(int imgIndex)
        {
            int sum=0;
            foreach (TwoViewModel model in TVMs)
            {
                if (model.equalFocalLength&&(model.left == imgIndex || model.right == imgIndex))
                    sum += model.match;
            }
            return sum;
        }

        /// <summary>
        /// 获得包含某一图像所有双视模型在集合中的索引
        /// </summary>
        /// <param name="imgIndex">图像索引</param>
        /// <returns>双视模型索引</returns>
        public List<int> GetModels(int imgIndex)
        {
            List<int> models=new List<int>();
            for (int i = 0; i < TVMs.Count; i++)
            {
                if (TVMs[i].left == imgIndex || TVMs[i].right == imgIndex)
                    models.Add(i);
            }
            return models;
        }


        /// <summary>
        /// 根据控制点或POS分析获取投影坐标系编号
        /// </summary>
        public int GetProjectionCode()
        {
            double lat = 0,lng=0;
            if (GCPs != null && GCPs.Count > 0)
            {
                foreach (GCP gcp in GCPs)
                {
                    if (gcp.proCode != -1)
                        return gcp.proCode;
                    lat += gcp.y;
                    lng += gcp.x;
                }
                lat = lat / GCPs.Count;
                lng = lng / GCPs.Count;
            }
            else if (images != null && images.Count > 0 && images[0]._POS != null)
            {
                foreach (AddedImage img in images)
                {
                    if (img._POS.proCode != -1)
                        return img._POS.proCode;
                    lat += img._POS.y;
                    lng += img._POS.x;
                }
                lat = lat / images.Count;
                lng = lng / images.Count;
            }
            else
                return -1;
            string code = "32";
            if (lat > 0)
                code += "6";
            else
                code += "7";
            int stripe;
            if (lng < 0)
            {
                lng = -lng;
                stripe = 30 - (int)lng / 6;
            }
            else
                stripe = 30 + ((int)lng + 6) / 6;
            code += stripe.ToString("00");
            return int.Parse(code);
        }
    }

    public enum FeatureType
    {
        SIFT=0,
        ORB=1
    }
}
