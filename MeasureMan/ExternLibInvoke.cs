using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 外部库方法调用，包括SiftGPU、PCL、VCG、(CERES)和OPALS
    /// </summary>
    public static class ExternLibInvoke
    {
        /*
        /// <summary>
        /// 光束平差法
        /// </summary>
        /// <param name="path">BA文件根目录</param>
        /// <param name="delta">预测偏差阈值</param>
        /// <returns>是否平差成功</returns>
        internal static bool BA(string path, int delta = 10)
        {
            return BundleAdjustment((path + "nvm.ba").ToCharArray(), delta);
        }

        /// <summary>
        /// 局部光束平差
        /// </summary>
        /// <param name="path">BA文件根目录</param>
        /// <param name="new_fd">新的焦距和畸变系数</param>
        /// <param name="new_RT">新的R和T</param>
        /// <param name="delta">是否平差成功</param>
        internal static void LocalBA(string path, double[] new_fd,double[] new_RT,int delta = 10)
        {
            EstimateF((path + "nvm.ba").ToCharArray(), delta,new_fd,new_RT);
        }*/

        /// <summary>
        /// SiftGPU特征检测方法
        /// </summary>
        /// <param name="imgPath">图像路径</param>
        /// <param name="savePath">保存路径</param>
        /// <returns>特征点数量</returns>
        internal static int SIFTDetect(string imgPath,string savePath)
        {
            return DetectSiftFt(imgPath.ToCharArray(), savePath.ToCharArray());
        }

        /// <summary>
        ///基于统计的离群点去除方法（Statistical Outlier Removal）
        /// </summary>
        /// <param name="path">点云路径</param>
        /// <param name="isSparse">是否为稀疏点云</param>
        /// <param name="currentVertexCount">当前点数量</param>
        /// <param name="meanK">邻域点数量</param>
        /// <param name="stddevMulThresh">标准差倍数</param>
        /// <returns>去除的离群点索引</returns>
        internal static int[] SOR(string path, bool isSparse, int currentVertexCount, int meanK = 40, double stddevMulThresh = 3)
        {
            if (isSparse)
            {
                int[] removal = new int[currentVertexCount];
                int removalCount = StOutlierRemove(path.ToCharArray(), isSparse, removal, meanK, stddevMulThresh);
                int[] removal2 = new int[removalCount];
                for (int i = 0; i < removalCount; i++)
                    removal2[i] = removal[i];
                return removal2;
            }
            else
            {
                int[] removal = new int[1];
                StOutlierRemove(path.ToCharArray(), isSparse, removal, meanK, stddevMulThresh);
                return null;
            }
        }

        /// <summary>
        /// 根据SOR结果改变稀疏重建结果
        /// </summary>
        /// <param name="rootDir">根目录</param>
        /// <param name="project">工程</param>
        /// <param name="currentVertexCount">当前点数量</param>
        /// <param name="indicies">去除的离群点索引</param>
        public static void ChangeOutlier(string rootDir, Project project, int currentVertexCount, int[] indicies)
        {
            int trackCount = -1;
            int matchIndex = 0;
            foreach (Track track in project.nvm.tracks)
            {
                if (matchIndex >= indicies.Length)
                    break;
                if (track!=null&&track._pt != null)
                {
                    trackCount++;
                    if (trackCount == indicies[matchIndex])
                    {
                        matchIndex++;
                        track._pt = null;
                    }
                }
            }
            project.nvm.SaveBundleOutFile(rootDir, currentVertexCount - indicies.Length);
        }

        /// <summary>
        /// 滚球法表面重建
        /// </summary>
        /// <param name="openPath">输出点云路径</param>
        /// <param name="savePath">输出模型路径</param>
        /// <param name="ball_radius">滚球半径</param>
        /// <param name="clustering_radius">簇半径</param>
        /// <param name="angle_threshold">角度阈值</param>
        /// <returns>面数量</returns>
        internal static int BallPivoting(string openPath, string savePath, float ball_radius = 0f, float clustering_radius = 20f, float angle_threshold = 90f)
        {
            return BPA(openPath.ToCharArray(), savePath.ToCharArray(), ball_radius, clustering_radius, angle_threshold);
        }

        /// <summary>
        /// 生成DSM
        /// </summary>
        /// <param name="cfgPath">配置文件路径</param>
        /// <returns>生成是否成功</returns>
        internal static bool GenerateDSM(string cfgPath)
        {
            byte[] errorMsg = new byte[1024];
            try
            {
                char[] path = cfgPath.ToCharArray();
                int resultODM = GeneteODM(path, ref errorMsg[0]);
                int resultDEM = GeneteDEM(path, ref errorMsg[0]);
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// 绝对定向
        /// </summary>
        /// <param name="worldPts">物点（已投影）</param>
        /// <param name="modelPts">模型点</param>
        /// <param name="inputPath">稠密点云路径</param>
        /// <param name="savePath">绝对定向后的稠密点云保存路径</param>
        /// <param name="T">模型坐标系原点</param>
        /// <returns>缩放尺度</returns>
        internal static float AbsoluteOrientation(List<GeoPoint> worldPts, List<Point3D> modelPts, string inputPath, string savePath, float[] T)
        {
            int count = worldPts.Count * (worldPts.Count - 1) / 2;
            ME A = new ME(count, 1, Emgu.CV.CvEnum.DepthType.Cv64F);
            ME L = new ME(count, 1, Emgu.CV.CvEnum.DepthType.Cv64F);
            count = 0;
            for (int i = 0; i < worldPts.Count; i++)
            {
                for (int j = i + 1; j < worldPts.Count; j++)
                {
                    A[count] = modelPts[i].GetDistance(modelPts[j]);
                    L[count] = worldPts[i].GetDistance(worldPts[j]);
                    count++;
                }
            }
            ME XMat = (!(~A * A)) * (~A) * L;
            float scale = (float)XMat[0];
            ExternLibInvoke.RigidTransformation(inputPath, savePath, scale, worldPts, modelPts, T);
            return scale;
        }

        /// <summary>
        /// 刚体变换
        /// </summary>
        /// <param name="denseCloud">稠密点云路径</param>
        /// <param name="savePath">结果保存路径</param>
        /// <param name="scale">尺度</param>
        /// <param name="worldPts">物点（已投影）</param>
        /// <param name="modelPts">模型点</param>
        /// <param name="T">模型坐标系原点</param>
        private static void RigidTransformation(string denseCloud,string savePath,float scale,List<GeoPoint> worldPts,List<Point3D> modelPts,float[] T)
        {
            float[] w = new float[worldPts.Count * 3];
            float[] m = new float[modelPts.Count * 3];
            for (int i = 0; i < worldPts.Count*3; i+=3)
            {
                w[i] = (float)worldPts[i/3].x;
                w[i + 1] = (float)worldPts[i/3].y;
                w[i + 2] = (float)worldPts[i/3].z;
                m[i] = (float)modelPts[i/3].x*scale;
                m[i + 1] = (float)modelPts[i/3].y*scale;
                m[i + 2] = (float)modelPts[i/3].z*scale;
            }
            estimateRigid(denseCloud.ToCharArray(), savePath.ToCharArray(), scale, w, m, worldPts.Count, T);
        }
        /*
        [DllImport("BATool.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool BundleAdjustment(char[] path, int delta);

        [DllImport("BATool.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void EstimateF(char[] path, int delta,double[] new_fd,double[] new_RT);*/

        [DllImport("SiftGPU.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DetectSiftFt(char[] imgPath,char[] savePath);

        [DllImport("VcgTool.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int BPA(char[] openPath, char[] savePath, float ball_radius, float clustering_radius, float angle_threshold);

        [DllImport("PclTool.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int StOutlierRemove(char[] path, bool isSparse, int[] removal, int meanK, double stddevMulThresh);

        [DllImport("PclTool.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void estimateRigid(char[] denseCloud, char[] savePath, float scale, float[] worldPts, float[] modelPts, int count, float[] T);

        [DllImport("GeneteODM.dll", EntryPoint = "GeneteDEM", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GeneteODM(char[] path, ref byte errorMsg);

        [DllImport("GeneteDEM.dll", EntryPoint = "GeneteDEM", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GeneteDEM(char[] path, ref byte errorMsg);
    }
}
