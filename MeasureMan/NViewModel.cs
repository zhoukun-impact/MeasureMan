using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// N视模型
    /// </summary>
    public class NViewModel
    {
        /// <summary>
        /// 已添加的相机
        /// </summary>
        public List<Camera> cameras;
        /// <summary>
        /// 图像到相机的索引
        /// </summary>
        public Dictionary<int, int> img2cam;
        /// <summary>
        /// 所有同名点
        /// </summary>
        public List<List<MatchPoint>> mps;
        /// <summary>
        /// 同名点轨迹
        /// </summary>
        public List<Track> tracks;

        /// <summary>
        /// N视模型创建方法
        /// </summary>
        public NViewModel()
        {
            cameras = new List<Camera>();
            img2cam = new Dictionary<int, int>();
            mps = new List<List<MatchPoint>>();
            tracks = new List<Track>();
        }

        /// <summary>
        /// 添加相机
        /// </summary>
        /// <param name="img">图像</param>
        public void AddCamera(AddedImage img)
        {
            img2cam.Add(img.order - 1, cameras.Count);
            cameras.Add(new Camera(img));
        }
        
        /// <summary>
        /// 保存out文件
        /// </summary>
        /// <param name="rootDir">文件根目录</param>
        /// <param name="pointCount">点云数量</param>
        public void SaveBundleOutFile(string rootDir, int pointCount)
        {
            StreamWriter sw = new StreamWriter(rootDir + "bundle.rd.out");
            sw.WriteLine("# Bundle file v0.3");
            int cameraCount = cameras.Count;
            sw.WriteLine(cameraCount + " " + pointCount);
            foreach (Camera cam in cameras)
            {
                sw.WriteLine(cam.f + " "+cam.distortion+" 0");
                for (int i = 0; i < 3; i++)
                    sw.WriteLine(cam.R[i, 0] + " " + cam.R[i, 1] + " " + cam.R[i, 2]);
                sw.WriteLine(cam.T[0, 0] + " " + cam.T[1, 0] + " " + cam.T[2, 0]);
            }

            foreach (Track track in tracks)
            {
                if (track!=null&&track._pt != null)
                {
                    sw.Write(track._pt.x + " " + track._pt.y + " " + track._pt.z + "\n0 0 0\n");
                    int mp_Count = 0;
                    string temp = "";
                    foreach (MatchPoint mp in track.matchPoints)
                    {
                        if (img2cam.Keys.Contains(mp.imageIndex))
                        {
                            int camIndex=img2cam[mp.imageIndex];
                            PointF pt = cameras[camIndex].Pixel2Image(mp.ftPoint);
                            temp += camIndex + " " + mp.ftIndex + " " + pt.X + " " + pt.Y + " ";
                            mp_Count++;
                        }
                    }
                    sw.WriteLine(mp_Count + " " + temp.Substring(0, temp.Length - 1));
                }
            }
            sw.Flush();
            sw.Close();
        }
        /*
        /// <summary>
        /// 保存BA文件
        /// </summary>
        /// <param name="rootDir">文件根目录</param>
        public void SaveBAFile(string rootDir)
        {
            StreamWriter sw = new StreamWriter(rootDir + "nvm.ba");
            int cameraCount = cameras.Count;
            sw.WriteLine(cameraCount);
            foreach (Camera cam in cameras)
            {
                ME R = cam.GetRotationVector();
                sw.WriteLine(cam.f + " " + cam.distortion + " " + cam.R[0, 0] + " " + cam.R[1, 0] + " " + cam.R[2, 0] + " " +
                    cam.T[0, 0] + " " + cam.T[1, 0] + " " + cam.T[2, 0]);
            }
            int pt_count = 0;
            string all = "";
            for (int i = 0; i < tracks.Count; i++)
            {
                Track track = tracks[i];
                if (track != null && track._pt != null)
                {
                    string pos = track._pt.x + " " + track._pt.y + " " + track._pt.z + " " + i + " ";
                    string temp = "";
                    int mp_Count = 0;
                    for (int j = 0; j < track.matchPoints.Count;j++ )
                    {
                        MatchPoint mp = track.matchPoints[j];
                        if (img2cam.Keys.Contains(mp.imageIndex))
                        {
                            int camIndex = img2cam[mp.imageIndex];
                            PointF pt = cameras[camIndex].Pixel2Image(mp.ftPoint);
                            temp += camIndex + " " + pt.X + " " + pt.Y + " ";
                            mp_Count++;
                        }
                    }
                    all = all + pos + mp_Count + " " + temp + "\n";
                    pt_count++;
                }
            }
            sw.WriteLine(pt_count);
            sw.Write(all);
            sw.Flush();
            sw.Close();
        }

        /// <summary>
        /// 加载BA文件
        /// </summary>
        /// <param name="rootDir">文件根目录</param>
        public void LoadBAFile(string rootDir)
        {
            StreamReader sr = new StreamReader(rootDir + "nvm.ba");
            int camera_count = int.Parse(sr.ReadLine());
            for (int i = 0; i < camera_count; i++)
            {
                Camera cam = cameras[i];
                string[] camInfo = sr.ReadLine().Split(' ');
                cam.f = double.Parse(camInfo[0]);
                cam.distortion = double.Parse(camInfo[1]);
                ME _R = new ME(3, 1, Emgu.CV.CvEnum.DepthType.Cv64F);
                cam.SetRotationVector(new double[3] { double.Parse(camInfo[2]), double.Parse(camInfo[3]), double.Parse(camInfo[4]) });
                cam.T.SetTo<double>(new double[3] { double.Parse(camInfo[5]), double.Parse(camInfo[6]), double.Parse(camInfo[7]) });
            }
            int pt_count = int.Parse(sr.ReadLine());
            for (int i = 0; i < pt_count; i++)
            {
                string[] ptInfo = sr.ReadLine().Split(' ');
                Point3D pt = new Point3D(double.Parse(ptInfo[0]), double.Parse(ptInfo[1]), double.Parse(ptInfo[2]));
                if (pt.Check(20))
                    tracks[int.Parse(ptInfo[3])]._pt = pt;
                else
                    tracks[int.Parse(ptInfo[3])]._pt = null;
            }
            sr.Close();
        }*/
    }
}
