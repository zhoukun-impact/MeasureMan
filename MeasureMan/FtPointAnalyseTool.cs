using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Flann;
using Emgu.CV.Structure;
using Emgu.CV.Util;
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
    /// 特征点分析工具，包括特征点检测和匹配方法。
    /// </summary>
    public static class FtPointAnalyseTool
    {
        #region ORB特征点分析
        /// <summary>
        /// 检测ORB特征点及计算二进制描述子,特点：32维，检测速度快，具有旋转、光照不变性
        /// </summary>
        /// <param name="image">待检测图像</param>
        /// <param name="featurePoints">特征点向量</param>
        /// <param name="descriptors">特征点描述子，一行对应一个特征点</param>
        /// <param name="orbNumber">orb特征点个数约束</param>
        public static void OrbDetect(Image<Gray, byte> image, VectorOfKeyPoint featurePoints, Mat descriptors,int orbNumber = 5000)
        {
            ORBDetector orb = new ORBDetector(orbNumber, 1.2f, 8, 31, 0, 2, ORBDetector.ScoreType.Fast, 31, 20);
            orb.DetectAndCompute(image, null, featurePoints, descriptors, false);
            image.Dispose();
            orb.Dispose();
        }


        /// <summary>
        /// 将特征点和描述子保存到orb文件中
        /// </summary>
        /// <param name="path">保存路径</param>
        /// <param name="featurePoints">特征点</param>
        /// <param name="descriptors">描述子</param>
        public static void SaveOrbFile(string path, VectorOfKeyPoint featurePoints, Mat descriptors)
        {
            StreamWriter sw = new StreamWriter(path);
            ME dp = new ME(descriptors);
            for (int i = 0; i < featurePoints.Size; i++)
            {
                string onePoint = "";
                onePoint += featurePoints[i].Point.X + " " + featurePoints[i].Point.Y + " ";
                for (int j = 0; j < descriptors.Cols; j++)
                    onePoint += dp[i, j] + " ";
                sw.WriteLine(onePoint.Trim());
            }
            sw.Flush();
            sw.Close();
            featurePoints.Dispose();
            descriptors.Dispose();
        }

        /// <summary>
        /// 从orb文件中加载特征点和描述子
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="featurePoints">特征点,忽略为null</param>
        /// <param name="descriptors">描述子，忽略为null</param>
        public static void LoadOrbFile(string path, VectorOfKeyPoint featurePoints, Mat descriptors)
        {
            StreamReader sr = new StreamReader(path);
            StringBuilder sb = new StringBuilder();
            while (!sr.EndOfStream)
                sb.Append(sr.ReadLine() + "*");
            sr.Close();
            string[] featPtsInfo = sb.ToString().Split('*');
            if (featurePoints != null)
            {
                MKeyPoint[] keyPoints = new MKeyPoint[featPtsInfo.Length - 1];
                for (int i = 0; i < featPtsInfo.Length - 1; i++)
                {
                    string[] onePoint = featPtsInfo[i].Split(' ');
                    keyPoints[i] = new MKeyPoint();
                    keyPoints[i].Point = new PointF(float.Parse(onePoint[0]), float.Parse(onePoint[1]));
                }
                featurePoints.Push(keyPoints);
            }
            if (descriptors != null)
            {
                for (int i = 0; i < featPtsInfo.Length - 1; i++)
                {
                    string[] onePoint = featPtsInfo[i].Split(' ');
                    ME descriptor = new ME(1, 32, DepthType.Cv8U);
                    for (int j = 0; j < 32; j++)
                        descriptor[0, j] = byte.Parse(onePoint[j + 2]);
                    descriptors.PushBack(descriptor);
                }
            }
        }

        /// <summary>
        /// 按照汉明距离选出正确匹配，用于特征追踪。
        /// </summary>
        /// <param name="matches">原匹配</param>
        /// <param name="mask">匹配标识</param>
        /// <param name="disError">汉明距离值差异</param>
        /// <param name="disThresh">汉明距离差值阈值</param>
        public static void VoteForRightMatchesByDis(VectorOfVectorOfDMatch matches, Mat mask, byte disError = 32, byte disThresh = 2)
        {
            ME mk = new ME(mask);
            for (int i = 0; i < matches.Size; i++)
            {
                if (mk[i, 0] == 1)
                {
                    float dis1 = matches[i][0].Distance;
                    float dis2 = matches[i][1].Distance;
                    if (dis1 > disError || dis2 - dis1 > disThresh)
                    {
                        mk[i, 0] = (byte)0;
                    }
                }
            }
        }

        /*
        /// <summary>
        /// 按照汉明距离选出正确匹配，用于特征匹配。
        /// </summary>
        /// <param name="matches">原匹配</param>
        /// <param name="mask">匹配标识</param>
        /// <param name="disError">汉明距离值差异</param>
        /// <param name="disThresh">汉明距离差值阈值</param>
        private static Mat VoteForRightMatchesByDis2(VectorOfVectorOfDMatch matches, Mat mask, byte disError = 40, byte disThresh = 4)
        {
            Mat temp_mask = null;
            int matchesCount;
            do
            {
                if (temp_mask != null)
                    temp_mask.Dispose();
                temp_mask = mask.Clone();
                VoteForRightMatchesByDis(matches, temp_mask, disError, disThresh);
                disError += 5;
                matchesCount = CvInvoke.CountNonZero(temp_mask);
            } while (matchesCount < 1000 && disError <= 50);//当匹配数量少于1000个时，增加错误阈值上限。最多不超过50.
            if (matchesCount < 1000)//如果错误阈值此时仍小于1000，则增加差值阈值。
            {
                temp_mask.Dispose();
                VoteForRightMatchesByDis(matches, mask, 50, 5);
                return mask;
            }
            else
            {
                mask.Dispose();
                return temp_mask;
            }
        }*/

        /// <summary>
        /// 裁剪后orb特征点坐标的校正
        /// </summary>
        /// <param name="featurePoints">校正前特征点</param>
        /// <param name="rect">裁剪范围，未裁剪设为null</param>
        /// <returns>校正后的特征点</returns>
        public static VectorOfKeyPoint FeaturePointsCorrection(VectorOfKeyPoint featurePoints, Rectangle rect)
        {
            if (rect != null&&rect.X!=0&&rect.Y!=0)
            {
                MKeyPoint[] newPoints = featurePoints.ToArray();
                featurePoints.Dispose();
                for (int i = 0; i < newPoints.Length; i++)
                {
                    newPoints[i].Point.X += rect.Left;
                    newPoints[i].Point.Y += rect.Top;
                }
                return new VectorOfKeyPoint(newPoints);
            }
            else
                return featurePoints;
        }


        /// <summary>
        /// 检测匹配点连线角度的变化（较粗略的匹配筛选方式）。
        /// </summary>
        /// <param name="modelFeatures">模型特征点</param>
        /// <param name="observedFeatures">观察特征点</param>
        /// <param name="matches">原匹配</param>
        /// <param name="mask">匹配标识</param>
        /// <param name="observedImageWidth">观察图像宽度</param>
        /// <param name="angleTolenrance">角度（角度制）变化阈值，该值越大说明角度变化容忍度就越高</param>
        /// <returns>true表示角度变化大，false表示角度变化小</returns>
        public static bool AngleChange(VectorOfKeyPoint modelFeatures, VectorOfKeyPoint observedFeatures, VectorOfVectorOfDMatch matches, Mat mask, int observedImageWidth, double angleTolenrance)
        {
            double tanThresh = Math.Tan(angleTolenrance * Math.PI / 180);
            double maxTan = -1;
            ME mk = new ME(mask);
            for (int i = 0; i < mask.Rows; i++)
            {
                if (mk[i, 0] != 0)
                {
                    PointF point1 = modelFeatures[matches[i][0].TrainIdx].Point;
                    PointF point2 = observedFeatures[matches[i][0].QueryIdx].Point;
                    double tempTan = Math.Abs(point2.Y - point1.Y) / (observedImageWidth - point2.X + point1.X);
                    if (tempTan > maxTan)
                        maxTan = tempTan;
                }
            }
            if (maxTan > 0 & maxTan < tanThresh)
                return false;
            else
                return true;
        }

        #endregion

        #region SIFT特征点分析
        /// <summary>
        /// 加载SIFT特征点
        /// </summary>
        /// <param name="path">加载路径</param>
        /// <param name="featurePoints">特征点数据，忽略设为null</param>
        /// <param name="descriptors">描述子数据，忽略设为null</param>
        /// <param name="image">所属图像</param>
        /// <param name="maxFeatureCount">最大特征点数量</param>
        public static void LoadSiftFile(string path, VectorOfKeyPoint featurePoints, Mat descriptors,AddedImage image,int maxFeatureCount=5000)
        {
            StreamReader sr = new StreamReader(path);
            dynamic synsr = StreamReader.Synchronized(sr);
            int featureCount = int.Parse(synsr.ReadLine().Split(' ')[0]);
            featureCount = featureCount < maxFeatureCount ? featureCount : maxFeatureCount;
            MKeyPoint[] keyPoints = new MKeyPoint[featureCount];
            for (int i = 0; i < featureCount; i++)
            {
                string info1 = synsr.ReadLine();
                string info2 = synsr.ReadLine();
                if (featurePoints != null)
                {
                    string[] onePoint = info1.Split(' ');
                    keyPoints[i] = new MKeyPoint();
                    if (image.clipped)
                        keyPoints[i].Point = new PointF(float.Parse(onePoint[1]) + image.ROI.X, float.Parse(onePoint[0]) + image.ROI.Y);
                    else
                        keyPoints[i].Point = new PointF(float.Parse(onePoint[1]), float.Parse(onePoint[0]));
                }
                if (descriptors != null)
                {
                    string[] onePoint = info2.Split(' ');
                    ME descriptor = new ME(1, 128, DepthType.Cv32F);
                    for (int j = 0; j < 128; j++)
                        descriptor[0, j] = float.Parse(onePoint[j]);
                    descriptors.PushBack(descriptor);
                }
            }
            featurePoints.Push(keyPoints);
            synsr.Close();
            sr.Close();
        }

        /// <summary>
        /// 只读取SIFT特征点文件的坐标信息
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="locations">特征点坐标</param>
        /// <param name="image">所属图像</param>
        /// <param name="maxFeatureCount">最大特征点数量</param>
        private static void LoadLocations(string path, List<PointF> locations, AddedImage image, int maxFeatureCount = 5000)
        {
            StreamReader sr = new StreamReader(path);
            int featureCount = int.Parse(sr.ReadLine().Split(' ')[0]);
            featureCount = featureCount < maxFeatureCount ? featureCount : maxFeatureCount;
            for (int i = 0; i < featureCount; i++)
            {
                string[] onePoint = sr.ReadLine().Split(' ');
                if (image.clipped)
                    locations.Add(new PointF(float.Parse(onePoint[1]) + image.ROI.X, float.Parse(onePoint[0]) + image.ROI.Y));
                else
                    locations.Add(new PointF(float.Parse(onePoint[1]), float.Parse(onePoint[0])));
                sr.ReadLine();
            }
            sr.Close();
        }

        /// <summary>
        /// 只读取ORB特征点文件的坐标信息
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="locations">特征点坐标</param>
        private static void LoadLocations(string path, List<PointF> locations)
        {
            StreamReader sr = new StreamReader(path);
            StringBuilder sb = new StringBuilder();
            while (!sr.EndOfStream)
                sb.Append(sr.ReadLine() + "*");
            sr.Close();
            string[] allPts = sb.ToString().Split('*');
            for (int i = 0; i < allPts.Length - 1; i++)
            {
                string[] onePoint = allPts[i].Split(' ');
                locations.Add(new PointF(float.Parse(onePoint[0]), float.Parse(onePoint[1])));
            }
            sr.Close();
        }

        /// <summary>
        /// 显示图像特征点
        /// </summary>
        /// <param name="images">图像集</param>
        /// <param name="fileName">特征点文件名称</param>
        /// <param name="featurePoints">特征点</param>
        /// <returns>含特征点的图像</returns>
        public static Image<Bgr, byte> ShowFeaturePoints(List<AddedImage> images, string fileName, VectorOfKeyPoint featurePoints)
        {
            Image<Bgr, byte> image = new Image<Bgr, byte>(images[int.Parse(fileName.Split('.')[0]) - 1].path);
            Features2DToolbox.DrawKeypoints(image, featurePoints, image, new Bgr(0, 0, 255));
            featurePoints.Dispose();
            return image;
        }
        #endregion

        #region 特征匹配
        /// <summary>
        /// 将当前两幅图匹配并存储在指定路径
        /// </summary>
        /// <param name="project">工程文件</param>
        /// <param name="imageindex1">第一幅图索引</param>
        /// <param name="imageindex2">第二幅图索引</param>
        /// <param name="savePath">存储路径</param>
        /// <returns>正确匹配数量</returns>
        public static int MatchPictures(Project project, int imageindex1, int imageindex2, string savePath)
        {
            string ftPath1 = project.ftPaths[imageindex1];
            string ftPath2 = project.ftPaths[imageindex2];

            VectorOfKeyPoint queryFeaturePoints = new VectorOfKeyPoint();
            Mat queryDescriptors = new Mat();
            VectorOfKeyPoint modelFeaturePoints = new VectorOfKeyPoint();
            Mat modelDescriptors = new Mat();
            VectorOfVectorOfDMatch matches;
            if (project.ftType == FeatureType.SIFT)
            {
                LoadSiftFile(ftPath1, queryFeaturePoints, queryDescriptors, project.images[imageindex1]);
                LoadSiftFile(ftPath2, modelFeaturePoints, modelDescriptors, project.images[imageindex2]);
                matches = TraditionalFtPointsMatch(modelDescriptors, queryDescriptors, DistanceType.L1);
            }
            else
            {
                LoadOrbFile(ftPath1, queryFeaturePoints, queryDescriptors);
                LoadOrbFile(ftPath2, modelFeaturePoints, modelDescriptors);
                matches = TraditionalFtPointsMatch(modelDescriptors, queryDescriptors, DistanceType.Hamming);
            }
           
            ME mask = new ME(matches.Size, 1, DepthType.Cv8U);
            mask.SetTo(new MCvScalar(1));
            VoteForRightMatchesByMatirc(matches, mask, queryFeaturePoints, modelFeaturePoints,1);//如果只使用此方法只需找最近邻
            int matchCount = CvInvoke.CountNonZero(mask);
            if (matchCount > 3)
                SaveRelFile(savePath, matches, mask,matchCount);
            else
                matchCount = 0;
            return matchCount;
        }

        /// <summary>
        /// 传统特征匹配
        /// </summary>
        /// <param name="modelDescriptors">训练描述子</param>
        /// <param name="queryDescriptors">查询描述子</param>
        /// <param name="distanceType">距离类型</param>
        /// <returns>匹配点对</returns>
        public static VectorOfVectorOfDMatch TraditionalFtPointsMatch(Mat modelDescriptors, Mat queryDescriptors,DistanceType distanceType)
        {
            //40000个特征点,欧氏距离，线性匹配时间为110s，不能筛选,结果完全正确;40000个特征点,hamming距离，线性匹配时间为25.64s,不能筛选,结果完全正确
            BFMatcher matcher = new BFMatcher(distanceType, false);//欧氏距离
            VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
            matcher.Add(modelDescriptors);
            if (distanceType == DistanceType.L1)
                matcher.KnnMatch(queryDescriptors, matches, 1, null);//SIFT只需找最近邻
            else if (distanceType == DistanceType.Hamming)
            {
                if(modelDescriptors.Rows>3000)
                    matcher.KnnMatch(queryDescriptors, matches, 1, null);//用于特征匹配
                else
                    matcher.KnnMatch(queryDescriptors, matches, 2, null);//用于关键帧提取
            }
            matcher.Dispose();
            queryDescriptors.Dispose();
            return matches;
        }

        /*
        /// <summary>
        /// 基于随机kd树或LSH的特征匹配（适用于特征点较多的情况）
        /// </summary>
        /// <param name="modelDescriptors">训练描述子</param>
        /// <param name="queryDescriptors">查询描述子</param>
        /// <param name="distanceType">距离类型</param>
        /// <param name="time">耗时（s）</param>
        /// <returns>匹配点对</returns>
        private static VectorOfVectorOfDMatch MoreAdvancedFtPointsMatch(Mat modelDescriptors, Mat queryDescriptors, DistanceType distanceType, out double time)
        {
            watch.Restart();
            VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
            FlannBasedMatcher flannMatcher=null;
            if (distanceType == DistanceType.L1) //40000个特征点,仅适用于SURF或SIFT,时间为20.21s，正确率为90.97%,96.06%,不能筛选
                flannMatcher = new FlannBasedMatcher(new KdTreeIndexParams(10), new SearchParams(700));//影响时间主要是后一个参数,为迭代次数
            else if(distanceType == DistanceType.Hamming)//40000个特征点，仅适用于ORB，时间为20s,正确率为84.84%，96.48%,不能筛选
                flannMatcher= new FlannBasedMatcher(new LshIndexParams(25, 8, 0), new SearchParams());
            flannMatcher.Add(modelDescriptors);
            flannMatcher.KnnMatch(queryDescriptors, matches, 1, null);//只需找最近邻
            flannMatcher.Dispose();
            modelDescriptors.Dispose();
            queryDescriptors.Dispose();
            watch.Stop();
            time = watch.Elapsed.TotalSeconds;
            return matches;
        }*/

        /// <summary>
        /// 显示匹配关系
        /// </summary>
        ///<param name="project">工程</param>
        /// <param name="fileName">匹配关系文件名称</param>
        /// <param name="ftDir">特征点文件目录</param>
        /// <param name="matches">匹配关系数据</param>
        /// <returns>含有匹配关系的图像</returns>
        public static Mat ShowMatches(Project project, string fileName, string ftDir, VectorOfVectorOfDMatch matches)
        {
            List<AddedImage> images = project.images;
            Mat result = new Mat();
            string[] twoImages = fileName.Split(new char[2] { '-', '.' });
            Image<Bgr, byte> modelImage = new Image<Bgr, byte>(images[int.Parse(twoImages[1]) - 1].path);
            Image<Bgr, byte> queryImage = new Image<Bgr, byte>(images[int.Parse(twoImages[0]) - 1].path);
            VectorOfKeyPoint modelKeyPoints = new VectorOfKeyPoint();
            string modelFtPath = ftDir + "\\" + twoImages[1] + ".ft";
            VectorOfKeyPoint queryKeyPoints = new VectorOfKeyPoint();
            string queryFtPath = ftDir + "\\" + twoImages[0] + ".ft";
            if (project.ftType == FeatureType.SIFT)
            {
                LoadSiftFile(modelFtPath, modelKeyPoints, null, images[int.Parse(twoImages[1]) - 1]);
                LoadSiftFile(queryFtPath, queryKeyPoints, null, images[int.Parse(twoImages[0]) - 1]);
            }
            else
            {
                LoadOrbFile(modelFtPath, modelKeyPoints, null);
                LoadOrbFile(queryFtPath, queryKeyPoints, null);
            }
            Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, queryImage, queryKeyPoints, matches, result, new MCvScalar(255, 0, 255), new MCvScalar(0, 0, 255), null);
            modelImage.Dispose();
            queryImage.Dispose();
            modelKeyPoints.Dispose();
            queryKeyPoints.Dispose();
            matches.Dispose();
            return result;
        }

        /// <summary>
        /// 使用单应矩阵或基础矩阵优化匹配
        /// </summary>
        /// <param name="matches">匹配结果</param>
        /// <param name="mask">掩膜</param>
        /// <param name="modelFeatures">图像一特征点</param>
        /// <param name="observedFeatures">图像二特征点</param>
        /// <param name="type">特矩阵类型，0表示单应矩阵，1表示基础矩阵</param>
        private static void VoteForRightMatchesByMatirc(VectorOfVectorOfDMatch matches, Mat mask, VectorOfKeyPoint observedFeatures, VectorOfKeyPoint modelFeatures, int type)
        {
            VectorOfPointF srcPoints = new VectorOfPointF();
            VectorOfPointF dstPoints = new VectorOfPointF();
            List<PointF> p1 = new List<PointF>();
            List<PointF> p2 = new List<PointF>();
            for (int i = 0; i < matches.Size; i++)
            {
                p1.Add(modelFeatures[matches[i][0].TrainIdx].Point);
                p2.Add(observedFeatures[matches[i][0].QueryIdx].Point);
            }
            srcPoints.Push(p1.ToArray());
            dstPoints.Push(p2.ToArray());
            Mat homography = new Mat();
            if (type == 0)
                CvInvoke.FindHomography(srcPoints, dstPoints, HomographyMethod.Ransac, 3, mask);//单应矩阵
            else
                CvInvoke.FindFundamentalMat(dstPoints, srcPoints, FmType.Ransac, 3, 0.99, mask);//基本矩阵
            srcPoints.Dispose();
            dstPoints.Dispose();
        }

        /// <summary>
        ///将匹配关系数据保存到rel文件中 
        /// </summary>
        /// <param name="path">保存路径</param>
        /// <param name="matches">匹配关系数据</param>
        /// <param name="mask">掩膜</param>
        /// <param name="matchesCount">匹配数量</param>
        public static void SaveRelFile(string path, VectorOfVectorOfDMatch matches, Mat mask,int matchesCount)
        {
            StreamWriter sw = new StreamWriter(path);
            ME mk = new ME(mask);
            sw.WriteLine(matchesCount);
            for (int i = 0; i < matches.Size; i++)
            {
                if(mk[i]!=0)
                    sw.WriteLine(matches[i][0].QueryIdx + " " + matches[i][0].TrainIdx + " " + matches[i][0].Distance);
            }
            sw.Flush();
            sw.Close();
            matches.Dispose();
            mask.Dispose();
        }

        
        /// <summary>
        /// 从rel文件中加载匹配关系数据
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="matches">匹配关系数据</param>
        /// <returns>匹配数量</returns>
        public static int LoadRelFile(string path, VectorOfVectorOfDMatch matches)
        {
            StreamReader sr = new StreamReader(path);
            StringBuilder sb = new StringBuilder();
            while (!sr.EndOfStream)
                sb.Append(sr.ReadLine() + "*");
            sr.Close();
            string[] matchesInfo = sb.ToString().Split('*');
            for (int i = 1; i < matchesInfo.Length - 1; i++)
            {
                string[] onePairMatch = matchesInfo[i].Split(' ');//顺序为query，train，distance,mask
                VectorOfDMatch match = new VectorOfDMatch();
                MDMatch[] singleMatch = new MDMatch[1];
                singleMatch[0].QueryIdx = int.Parse(onePairMatch[0]);
                singleMatch[0].TrainIdx = int.Parse(onePairMatch[1]);
                singleMatch[0].Distance = float.Parse(onePairMatch[2]);
                match.Push(singleMatch);
                matches.Push(match);
            }
            return int.Parse(matchesInfo[0]);
        }

        /// <summary>
        /// 匹配文件的只读方式
        /// </summary>
        /// <param name="path">匹配关系文件路径</param>
        /// <returns>有效匹配列表</returns>
        private static List<SimpleMatch> LoadRelFile(string path)
        {
            StreamReader sr = new StreamReader(path);
            StringBuilder sb = new StringBuilder();
            while (!sr.EndOfStream)
                sb.Append(sr.ReadLine() + "*");
            sr.Close();
            string[] matchesInfo = sb.ToString().Split('*');
            List<SimpleMatch> matches = new List<SimpleMatch>();
            for (int i = 1; i < matchesInfo.Length - 1; i++)
            {
                string[] onePairMatch = matchesInfo[i].Split(' ');//顺序为query，train，distance,mask
                matches.Add(new SimpleMatch(int.Parse(onePairMatch[0]), int.Parse(onePairMatch[1]), float.Parse(onePairMatch[2])));
            }
            return matches;
        }

        /// <summary>
        /// 获得综合的距离阈值
        /// </summary>
        /// <param name="project">工程文件</param>
        /// <returns>距离阈值</returns>
        public static double GetThreshold(Project project)
        {
            double threshold1 = GetThresholdByHist(project);
            double threshold2 = GetThresholdByRatio(project);
            if (CheckAllMatch(project, threshold1))
                return threshold1 > threshold2 ? threshold1 : threshold2;
            else
                return threshold2;
        }


        /// <summary>
        /// 通过直方图获得距离阈值
        /// </summary>
        /// <param name="project">工程文件</param>
        private static double GetThresholdByHist(Project project)
        {
            double threshold = 0;
            List<double> data = new List<double>();
            double dis = 0;
            int splitcount = (int)(1.5 + Math.Log(project.images.Count) * 3.322);//加上0.5，以四舍五入
            int[] number = new int[splitcount];//记录每一块的数量
            POS pos1, pos2;
            double min = double.MaxValue, max = 0;
            for (int j = 0; j < project.images.Count - 1; j++)//首先获取最大值最小值，并将距离数据录入。
            {
                pos1 = project.images[j]._POS;
                for (int i = j + 1; i < project.images.Count; i++)
                {
                    pos2 = project.images[i]._POS;
                    dis = pos1.GetDistance(pos2);
                    data.Add(dis);
                    if (dis > max)
                        max = dis;
                    if (dis < min)
                        min = dis;
                }
            }
            max += 1;
            double span = (max - min) / splitcount;//区间间隔
            for (int i = 0; i < data.Count; i++)
                number[(int)((data[i] - min) / span)]++;
            for (int i = 0; i < number.Length - 1; i++)
            {
                if (number[i + 1] < number[i])
                {
                    return min + span * (i + 1);
                }
            }
            return threshold;
        }

        /// <summary>
        /// 按比例获得距离阈值
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private static double GetThresholdByRatio(Project project)
        {
            List<double> data = new List<double>();
            for (int j = 0; j < project.images.Count - 1; j++)
            {
                POS pos1 = project.images[j]._POS;
                for (int i = j + 1; i < project.images.Count; i++)
                {
                    POS pos2 = project.images[i]._POS;
                    double dis = pos1.GetDistance(pos2);
                    data.Add(dis);
                }
            }
            data.Sort();
            double ratio = 0.3;
            double threshold = 0;
            while (ratio <= 0.35)
            {
                threshold = data[(int)(data.Count * ratio)];
                if (CheckAllMatch(project, threshold))
                    break;
                else
                    ratio += 0.005;
            }
            return threshold;
        }


        /// <summary>
        /// （获取阈值辅助方法）查看按当前阈值所有图片能否有匹配
        /// </summary>
        /// <param name="project">工程文件</param>
        /// <param name="threshold">距离阈值（单位/米）</param>
        private static bool CheckAllMatch(Project project, double threshold)
        {
            bool answer = false;
            POS pos1, pos2;
            for (int i = 0; i < project.images.Count; i++)
            {
                answer = false;
                for (int j = 0; j < project.images.Count; j++)
                {
                    if (i == j)
                    {
                        j++;
                        continue;
                    }
                    pos1 = project.images[i]._POS;
                    pos2 = project.images[j]._POS;
                    double dis = pos1.GetDistance(pos2);
                    if (dis < threshold)
                    {
                        answer = true;
                        break;
                    }

                }
                if (!answer)//如果在某一项中answer为false，表示该图片没有匹配到图，返回false
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 寻找所有距离当前图像距离在一定距离范围内的所有图像索引。
        /// </summary>
        /// <param name="project">工程文件</param>
        /// <param name="index">当前图像索引</param>
        /// <param name="threshold">距离阈值（单位/米）</param>
        public static List<int> GetMatchPicture(Project project, int index, double threshold)
        {
            List<int> picture = new List<int>();
            if (threshold < 0)
            {
                if (index < project.images.Count - 2)
                {
                    picture.Add(index + 1);
                    picture.Add(index + 2);
                }
                else if (index == project.images.Count - 2)
                    picture.Add(index + 1);
            }
            else if (threshold == double.MaxValue)
            {
                for (int i = index + 1; i < project.images.Count; i++)
                    picture.Add(i);
            }
            else
            {
                POS pos1 = project.images[index]._POS;
                POS pos2 = null;
                double dis = 0;
                for (int i = index + 1; i < project.images.Count; i++)
                {
                    pos2 = project.images[i]._POS;
                    dis = pos1.GetDistance(pos2);
                    if (dis < threshold)
                        picture.Add(i);
                }
            }
            return picture;
        }
        #endregion

        #region 获得Tracks
        /// <summary>
        /// 获取所有track
        /// </summary>
        /// <param name="project">打开的项目</param>
        private static void GetTrack(Project project)
        {
            ConvertToMP(project);
            try
            {
                for (int i = 0; i < project.TVMs.Count; i++)
                    GetMatch(project, project.TVMs[i].left, project.TVMs[i].right);
            }
            catch
            {
                project.nvm.mps.Clear();
                project.nvm.tracks.Clear();
                return;
            }
        }

        /// <summary>
        /// 将特征点转换为matchpoint。
        /// </summary>
        /// <param name="project">工程文件</param>
        private static void ConvertToMP(Project project)
        {
            for (int i = 0; i < project.images.Count; i++)
            {
                string ftPath = project.ftPaths[i];
                List<PointF> locations = new List<PointF>();
                if (project.ftType == FeatureType.SIFT)
                    LoadLocations(ftPath, locations, project.images[i]);
                else
                    LoadLocations(ftPath, locations);
                project.nvm.mps.Add(new List<MatchPoint>());
                for (int j = 0; j < locations.Count; j++)
                    project.nvm.mps[i].Add(new MatchPoint(i, j, locations[j], -1));
            }
        }

        /// <summary>
        /// 通过两幅图像的基本信息，为输入的matchpoint和track赋值。
        /// </summary>
        /// <param name="project">工程文件</param>
        /// <param name="imageindex1">图像一在project中的索引</param>
        /// <param name="imageindex2">图像二在project中的索引</param>
        /// <param name="tracks">最终输出结果所有track</param>
        private static void GetMatch(Project project, int imageindex1, int imageindex2)
        {
            string dir = Path.GetDirectoryName(project.path);
            string relPath = dir + "\\rel\\" + (imageindex1 + 1) + "-" + (imageindex2 + 1) + ".rel";
            List<SimpleMatch> matches = LoadRelFile(relPath);
            matches.Sort();
            List<MatchPoint> matchpoint1 = project.nvm.mps[imageindex1];
            List<MatchPoint> matchpoint2 = project.nvm.mps[imageindex2];
            List<Track> tracks = project.nvm.tracks;
            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].distance!=float.MaxValue)
                {
                    MatchPoint p1=matchpoint1[matches[i].queryIdx];
                    MatchPoint p2=matchpoint2[matches[i].trainIdx];
                    if (p2.trackIndex == -1)//说明该点未被匹配
                    {
                        if (p1.trackIndex == -1)//说明这对匹配是一对起始匹配
                        {
                            Track track = new Track();
                            p1.trackIndex = tracks.Count;
                            p2.trackIndex = tracks.Count;
                            track.matchPoints.Add(p1);
                            track.matchPoints.Add(p2);
                            tracks.Add(track);
                        }
                        else//说明该点需要加到已有的track上
                        {
                            p2.trackIndex = p1.trackIndex;
                            List<MatchPoint> mps = tracks[p1.trackIndex].matchPoints;
                            mps.Add(p2);
                        }
                    }
                    else
                    {
                        if (p1.trackIndex == -1)//说明该点需要加到已有的track上
                        {
                            p1.trackIndex = p2.trackIndex;
                            List<MatchPoint> mps = tracks[p2.trackIndex].matchPoints;
                            mps.Add(p1);
                        }
                        else if(p1.trackIndex!=p2.trackIndex)
                        {
                            List<MatchPoint> mps1 = tracks[p1.trackIndex].matchPoints;
                            int ind=p2.trackIndex;
                            List<MatchPoint> mps2 = tracks[ind].matchPoints;
                            foreach (MatchPoint mp in mps2)
                                mp.trackIndex = p1.trackIndex;
                            tracks[p1.trackIndex].matchPoints = mps1.Union(mps2).ToList();
                            tracks[ind] = null;
                        }
                    } 
                }
            }
        }

        /*
        /// <summary>
        /// 获得控制点轨迹
        /// </summary>
        /// <param name="gcps">控制点</param>
        /// <param name="camera">相机</param>
        /// <returns>同名控制点组成的轨迹</returns>
        public static List<List<GCP>> GetGCPTracks(List<GCP> gcps)
        {
            gcps.Sort();
            List<List<GCP>> gcpGroups = new List<List<GCP>>();
            List<GCP> gcpGroup1 = new List<GCP>();
            gcpGroup1.Add(gcps[0]);
            gcpGroups.Add(gcpGroup1);
            for (int i = 1; i < gcps.Count; i++)
            {
                if (gcps[i].imageOrder == gcps[i - 1].imageOrder)
                    gcpGroups.Last().Add(gcps[i]);
                else
                {
                    List<GCP> gcpGroup = new List<GCP>();
                    gcpGroup.Add(gcps[i]);
                    gcpGroups.Add(gcpGroup);
                }
            }
            int head = 0;
            List<List<GCP>> gcpTracks = new List<List<GCP>>();
            while (head < gcpGroups.Count - 1)
            {
                foreach (GCP headGCP in gcpGroups[head])
                {
                    List<GCP> gcpTrack = new List<GCP>();
                    gcpTrack.Add(headGCP);
                    for (int i = head + 1; i < gcpGroups.Count; i++)
                    {
                        for (int j = 0; j < gcpGroups[i].Count; j++)
                        {
                            if (headGCP.x == gcpGroups[i][j].x && headGCP.y == gcpGroups[i][j].y && headGCP.z == gcpGroups[i][j].z)
                            {
                                gcpTrack.Add(gcpGroups[i][j]);
                                gcpGroups[i].RemoveAt(j);
                                break;
                            }
                        }
                    }
                    gcpTracks.Add(gcpTrack);
                }
                head++;
            }
            return gcpTracks;
        }*/
        #endregion

        #region 稀疏重建
        /// <summary>
        /// 稀疏重建
        /// </summary>
        /// <param name="project">工程</param>
        /// <param name="savePath">点云保存路径</param>
        /// <returns>是否重建成功</returns>
        public static bool SparseReconstruction(Project project, string savePath)
        {
            project.nvm = new NViewModel();
            GetTrack(project);
            if (project.nvm.tracks.Count==0)
                return false;
            List<Camera> cams = project.nvm.cameras;
            Dictionary<int, int> dic = project.nvm.img2cam;
            if (!Initiate(project))
                return false;
            string dir=Path.GetDirectoryName(project.path);
            
            //project.nvm.SaveBAFile(dir + "\\");
            //if (ExternLibInvoke.BA(dir + "\\"))
                //project.nvm.LoadBAFile(dir + "\\");
            while (cams.Count < project.images.Count)
            {
                int secondImage = GetNextImage(project);
                if (secondImage == -1)
                    break;
                foreach (int firstImage in dic.Keys)
                {
                    if (project.Exist(firstImage, secondImage))
                    {
                        //还原新的3D点
                        List<MatchPoint> leftPoints = new List<MatchPoint>();
                        List<MatchPoint> rightPoints = new List<MatchPoint>();
                        FindPointPair(firstImage, secondImage, leftPoints, rightPoints, project);
                        ME _R1 = cams[dic[firstImage]].R; ME _t1 = cams[dic[firstImage]].T;
                        ME _R2 = cams[dic[secondImage]].R; ME _t2 = cams[dic[secondImage]].T;

                        ME P1 = new ME(3, 4, DepthType.Cv64F); ME P2 = new ME(3, 4, DepthType.Cv64F);
                        P1.CombineR_t(cams[dic[firstImage]].R, cams[dic[firstImage]].T);
                        P2.CombineR_t(cams[dic[secondImage]].R, cams[dic[secondImage]].T);
                        ME Q = new ME();
                        //调整格式
                        ME ins1 = cams[dic[firstImage]].GetIntrinsicMatrix();
                        ME ins2 = cams[dic[secondImage]].GetIntrinsicMatrix();
                        ME _left = ME.FromPoints(leftPoints); _left.PixPointToCamPoint(ins1);
                        ME _right = ME.FromPoints(rightPoints); _right.PixPointToCamPoint(ins2);

                        CvInvoke.TriangulatePoints(P1, P2, ~_left, ~_right, Q);

                        ME mask = new ME(_left.Height, 1, DepthType.Cv8U);
                        Q.NormPoint3D(P1, P2, ins1, ins2, mask, leftPoints, rightPoints);
                        Q.Added3DInTrack(project.nvm.tracks, rightPoints, mask);
                    }
                }
                
                //project.nvm.SaveBAFile(dir + "\\");
                //if (ExternLibInvoke.BA(dir + "\\"))
                    //project.nvm.LoadBAFile(dir + "\\");
            }

            List<Point3D> pointClouds = new List<Point3D>();
            foreach (Track track in project.nvm.tracks)
            {
                if (track != null)
                {
                    Point3D point = track._pt;
                    if (point != null)
                    {
                        if (point.Check(20))
                            pointClouds.Add(point);
                        else
                            track._pt = null;
                    } 
                }  
            }
            PointCloudTool tool = new PointCloudTool(pointClouds);
            tool.OutputPointCloud(savePath);
            project.nvm.SaveBundleOutFile(dir + "\\", tool.pointCloud.Count);

            /*测试，可删*/
            List<Point3D> pts = new List<Point3D>();
            foreach (Camera c in cams)
            {
                double[] CData = c.GetPosition().ToArray();
                pts.Add(new Point3D(CData[0], CData[1], CData[2]));   
            }
            PointCloudTool tool2 = new PointCloudTool(pts);
            tool2.OutputPointCloud(System.IO.Path.GetDirectoryName(project.path) + "\\cameras.ply");

            //控制点模型点获取
            if (project.GCPs != null&&project.GCPs.Count>=6)
                GetGCPTracks(project);

            return true;
        }

        /// <summary>
        /// 获得控制点轨迹
        /// </summary>
        /// <param name="project">工程</param>
        private static void GetGCPTracks(Project project)
        {
            List<Camera> cams = project.nvm.cameras;
            Dictionary<int, int> dic = project.nvm.img2cam;
            List<GCP> gcps = project.GCPs;
            gcps.Sort();
            List<List<GCP>> gcpGroups = new List<List<GCP>>();
            List<GCP> gcpGroup1 = new List<GCP>();
            gcpGroup1.Add(gcps[0]);
            gcpGroups.Add(gcpGroup1);
            for (int i = 1; i < gcps.Count; i++)
            {
                if (gcps[i].imageOrder == gcps[i - 1].imageOrder)
                    gcpGroups.Last().Add(gcps[i]);
                else
                {
                    List<GCP> gcpGroup = new List<GCP>();
                    gcpGroup.Add(gcps[i]);
                    gcpGroups.Add(gcpGroup);
                }
            }
            for (int i = gcpGroups.Count - 1; i >= 0; i--)
            {
                if (cams[dic[gcpGroups[i][0].imageOrder - 1]].R == null)
                    gcpGroups.RemoveAt(i);
            }

            int head = 0;
            List<List<GCP>> gcpTracks = new List<List<GCP>>();
            while (head < gcpGroups.Count - 1)
            {
                foreach (GCP headGCP in gcpGroups[head])
                {
                    List<GCP> gcpTrack = new List<GCP>();
                    gcpTrack.Add(headGCP);
                    for (int i = head + 1; i < gcpGroups.Count; i++)
                    {
                        for (int j = 0; j < gcpGroups[i].Count; j++)
                        {
                            if (headGCP.x == gcpGroups[i][j].x && headGCP.y == gcpGroups[i][j].y && headGCP.z == gcpGroups[i][j].z)
                            {
                                gcpTrack.Add(gcpGroups[i][j]);
                                gcpGroups[i].RemoveAt(j);
                                break;
                            }
                        }
                    }
                    if(gcpTrack.Count>1)
                        gcpTracks.Add(gcpTrack);
                }
                head++;
            }

            foreach (List<GCP> gcpTrack in gcpTracks)
            {
                Camera cam0=cams[dic[gcpTrack[0].imageOrder - 1]];
                Camera cam1=cams[dic[gcpTrack[1].imageOrder - 1]];
                ME P1 = new ME(3, 4, DepthType.Cv64F); ME P2 = new ME(3, 4, DepthType.Cv64F);
                P1.CombineR_t(cam0.R, cam0.T);
                P2.CombineR_t(cam1.R, cam1.T);
                ME _left = new ME(1, 2, DepthType.Cv64F);
                _left[0, 0] = gcpTrack[0].pixelPoint.X; _left[0, 1] = gcpTrack[0].pixelPoint.Y;
                _left.PixPointToCamPoint(cam0.GetIntrinsicMatrix());
                ME _right = new ME(1, 2, DepthType.Cv64F);
                _right[0, 0] = gcpTrack[1].pixelPoint.X; _right[0, 1] = gcpTrack[1].pixelPoint.Y;
                _right.PixPointToCamPoint(cam1.GetIntrinsicMatrix());
                ME Q = new ME();
                CvInvoke.TriangulatePoints(P1, P2, ~_left, ~_right, Q);
                double w = Q[3];
                if (w == 0) w = 1;
                gcpTrack[0].modelPoint = new Point3D(Q[0] / w, Q[1] / w, Q[2] / w);
            }    
        }

        /// <summary>
        /// 获得下一幅添加的图
        /// </summary>
        /// <param name="project">工程</param>
        /// <returns>图像索引</returns>
        private static int GetNextImage(Project project)
        {
            int max = 0;
            int optimal = -1;
            List<int> wrong = new List<int>();
            NViewModel nvm = project.nvm;

            while(true)
            {
                for (int i = 0; i < project.images.Count; i++)
                {
                    if ((!nvm.img2cam.Keys.Contains(i)) && (!wrong.Contains(i)))
                    {
                        List<int> all = project.GetModels(i);
                        int temp = 0;
                        foreach (int ind in all)
                        {
                            if (nvm.img2cam.Keys.Contains(project.TVMs[ind].left) || nvm.img2cam.Keys.Contains(project.TVMs[ind].right))
                                temp += project.TVMs[ind].match;
                        }
                        if (temp > max)
                        {
                            optimal = i;
                            max = temp;
                        }
                    }
                }

                int maxVal = -1, minVal = int.MaxValue;
                if (optimal == -1)
                {
                    foreach (int ind in nvm.img2cam.Keys)
                    {
                        if (ind < minVal)
                            minVal = ind;
                        if (ind > maxVal)
                            maxVal = ind;
                    }
                    if (minVal == 0)
                        optimal = maxVal + 1;
                    else
                        optimal = minVal - 1;
                }
                if (Enough3DPoints(project, optimal))
                    break;
                else
                {
                    if (wrong.Contains(optimal))
                        return -1;
                    wrong.Add(optimal);
                    max = 0;
                    optimal = -1;
                }
                    
            }
            return optimal;
        }

        /// <summary>
        /// 判断是否有足够的三维点
        /// </summary>
        /// <param name="project">工程</param>
        /// <param name="secondImage">待添加下一幅图的索引</param>
        /// <returns>是否足够</returns>
        private static bool Enough3DPoints(Project project, int secondImage)
        {
            List<MatchPoint> points_2D_temp =project.nvm.mps[secondImage];
            List<Point3D> points_3D = new List<Point3D>();
            List<MatchPoint> points_2D = new List<MatchPoint>();
            foreach (MatchPoint mp in points_2D_temp)
            {
                if (mp.trackIndex != -1 && project.nvm.tracks[mp.trackIndex]._pt != null)
                {
                    points_3D.Add(project.nvm.tracks[mp.trackIndex]._pt);
                    points_2D.Add(mp);
                }
            }
            if (points_3D.Count < 4)
                return false;

            Mat objectPoints = ME.FromPoints3D(points_3D).Reshape(3);
            Mat pixelPoints = ME.FromPoints(points_2D).Reshape(2);

            project.nvm.AddCamera(project.images[secondImage]);
            Camera cam = project.nvm.cameras.Last();
            ME K = cam.GetIntrinsicMatrix();

            Mat _R = new Mat(3, 1, DepthType.Cv64F, 1);
            Mat t = new Mat(3, 1, DepthType.Cv64F, 1);
            int iterationsCount = 100; float error = 2f; double confident = 0.99; Mat inliers = new Mat();
            CvInvoke.SolvePnPRansac(objectPoints, pixelPoints, K as Mat, null, _R, t, false, iterationsCount, error, confident, inliers, SolvePnpMethod.EPnP);           

            ME R = new ME(3, 3, DepthType.Cv64F);
            CvInvoke.Rodrigues(_R, R);
            cam.R = new ME(R);
            cam.T = new ME(t);
            /*
            if(points_3D.Count>100)
                cam.LocalBA(points_3D, points_2D,Path.GetDirectoryName(project.path)+"\\");*/
            return true;
        }

        /// <summary>
        /// 从轨迹组中寻找匹配点对
        /// </summary>
        /// <param name="baseImageIndex">基准影像索引</param>
        /// <param name="relaImageIndex">相对影像索引</param>
        /// <param name="lv">基准影像点</param>
        /// <param name="rv">相对影像点</param>
        private static void FindPointPair(int baseImageIndex, int relaImageIndex, List<MatchPoint> ll, List<MatchPoint> rl, Project project)
        {
            List<MatchPoint> mps = project.nvm.mps[baseImageIndex];
            foreach (MatchPoint mp in mps)
            {
                if (mp.trackIndex != -1)
                {
                    MatchPoint mp2 = project.nvm.tracks[mp.trackIndex].FindMatchPoint(relaImageIndex);

                    if (mp2 != null)
                    {
                        ll.Add(mp);
                        rl.Add(mp2);
                    }
                }
            }
        }

        /// <summary>
        /// 稀疏重建初始化
        /// </summary>
        /// <param name="project">工程</param>
        /// <returns>初始化是否成功</returns>
        private static bool Initiate(Project project)
        {
            int[] initImgPairs=InitImgPairs(project);
            if (initImgPairs == null)
                return false;
            project.nvm.AddCamera(project.images[initImgPairs[0]]);
            project.nvm.AddCamera(project.images[initImgPairs[1]]);
            ME R, t;
            RecoverPose(initImgPairs, project, out R, out t);

            //修改左图参数
            project.nvm.cameras[0].R = new ME(ME.Eye(3, 3, DepthType.Cv64F, 1));
            project.nvm.cameras[0].T = new ME(ME.Zeros(3, 1, DepthType.Cv64F, 1));

            //修改右图参数
            project.nvm.cameras[1].R = R;
            project.nvm.cameras[1].T = t;
            return true;
        }

        /// <summary>
        /// 获得种子匹配对
        /// </summary>
        /// <param name="project">工程</param>
        /// <returns>种子匹配对图像索引</returns>
        private static int[] InitImgPairs(Project project)
        {
            /*先匹配总数最多的图,选择匹配数量在平均数处的图*/
            int[] indicies = new int[2];

            int maxParis = 0;
            int left = -1;
            for (int i = 0; i < project.images.Count; i++)
            {
                int sum=project.GetTotalMatches(i);
                if (sum > maxParis)
                {
                    left = i;
                    maxParis = sum;
                }
            }
            List<int> models = project.GetModels(left);
            double avg = 0;
            foreach (int ind in models)
            {
                if(project.TVMs[ind].equalFocalLength)
                    avg += project.TVMs[ind].match;
            }
                
            avg /= models.Count;

            double diff=double.MaxValue;
            int optimal = -1;
            foreach (int ind in models)
            {
                double temp=project.TVMs[ind].match-avg;
                if (temp >= 0 && temp < diff && project.TVMs[ind].equalFocalLength)
                {
                    diff = temp;
                    optimal = ind;
                }
            }
            if (optimal == -1)
                return null;
            indicies[0] = project.TVMs[optimal].left;
            indicies[1] = project.TVMs[optimal].right;
            return indicies;
        }

        /// <summary>
        /// 从本质矩阵中恢复相机姿态
        /// </summary>
        /// <param name="initPairs">种子匹配对</param>
        /// <param name="project">工程</param>
        /// <param name="_R">输出旋转矩阵</param>
        /// <param name="_t">输出平移向量</param>
        private static void RecoverPose(int[] initPairs, Project project,out ME _R,out ME _t)
        {
            int l=initPairs[0],r=initPairs[1];
            List<MatchPoint> leftPoints = new List<MatchPoint>();
            List<MatchPoint> rightPoints = new List<MatchPoint>();
            FindPointPair(l, r, leftPoints, rightPoints, project);
            ME left = ME.FromPoints(leftPoints);
            ME right = ME.FromPoints(rightPoints);
            //FindEssentialMat()函数必须转为Mat，且一行为一个2D点，否则报错
            ME ins=project.nvm.cameras[0].GetIntrinsicMatrix();
            ME essentialMat = new ME(CvInvoke.FindEssentialMat(left as Mat, right as Mat,ins));
            //转成相机坐标
            left.PixPointToCamPoint(ins);
            right.PixPointToCamPoint(ins);
            left = ~left; right = ~right;//调整格式
            //svd分解
            ME _R1, _R2, t;
            decomposeEssentialMat(essentialMat, out _R1, out _R2, out t);

            ME P0 = new ME(ME.Eye(3, 4, _R1.Depth, 1));
            ME P1 = new ME(3, 4, _R1.Depth); ME P2 = new ME(3, 4, _R1.Depth); ME P3 = new ME(3, 4, _R1.Depth); ME P4 = new ME(3, 4, _R1.Depth);
            P1.CombineR_t(_R1, t);
            P2.CombineR_t(_R2, t);
            P3.CombineR_t(_R1, -t);
            P4.CombineR_t(_R2, -t);

            ME Q1 = new ME(); ME Q2 = new ME(); ME Q3 = new ME(); ME Q4 = new ME();
            ME mask1 = new ME(left.Width, 1, DepthType.Cv8U); mask1.SetTo(new MCvScalar(0));
            ME mask2 = new ME(left.Width, 1, DepthType.Cv8U); mask2.SetTo(new MCvScalar(0));
            ME mask3 = new ME(left.Width, 1, DepthType.Cv8U); mask3.SetTo(new MCvScalar(0));
            ME mask4 = new ME(left.Width, 1, DepthType.Cv8U); mask4.SetTo(new MCvScalar(0));

            CvInvoke.TriangulatePoints(P0, P1, left, right, Q1);
            int good1 = Q1.NormPoint3D(P1, mask1);
            CvInvoke.TriangulatePoints(P0, P2, left, right, Q2);
            int good2 = Q2.NormPoint3D(P2, mask2);
            CvInvoke.TriangulatePoints(P0, P3, left, right, Q3);
            int good3 = Q3.NormPoint3D(P3, mask3);
            CvInvoke.TriangulatePoints(P0, P4, left, right, Q4);
            int good4 = Q4.NormPoint3D(P4, mask4);

            //挑选计算出来的四种结果，大于0（即在相机前方的点）数目最多的为最佳结果
            if (good1 >= good2 && good1 >= good3 && good1 >= good4)
            {
                _R = _R1; _t = t;
                Q1.Added3DInTrack(project.nvm.tracks, rightPoints, mask1);
            }
            else if (good2 >= good1 && good2 >= good3 && good2 >= good4)
            {
                _R = _R2; _t = t;
                Q2.Added3DInTrack(project.nvm.tracks, rightPoints, mask2);//反向投影筛点
            }
            else if (good3 >= good1 && good3 >= good2 && good3 >= good4)
            {
                _R = _R1; _t = -t;
                Q3.Added3DInTrack(project.nvm.tracks, rightPoints, mask3);
            }
            else
            {
                _R = _R2; _t = -t;
                Q4.Added3DInTrack(project.nvm.tracks, rightPoints, mask4);
            }
        }

        
        /// <summary>
        /// SVD分解出R，t
        /// </summary>
        /// <param name="E">本质矩阵</param>
        /// <param name="_R1">R1</param>
        /// <param name="_R2">R2</param>
        /// <param name="_t">up-to-scale的T</param>
        private static void decomposeEssentialMat(ME E, out ME _R1, out ME _R2, out ME _t)
        {
            ME U = new ME();
            ME Vt = new ME();
            ME D = new ME();
            _t = new ME(3, 1, DepthType.Cv64F);
            CvInvoke.SVDecomp(E as Mat, D, U, Vt, SvdFlag.ModifyA | SvdFlag.FullUV);

            if (CvInvoke.Determinant(U as Mat) < 0)
                U = -U;
            if (CvInvoke.Determinant(Vt as Mat) < 0)
                Vt = -Vt;

            ME W = new ME(ME.Zeros(3, 3, E.Depth, 1));
            W[0, 1] = 1; W[1, 0] = -1; W[2, 2] = 1;

            _R1 = U * W * Vt;
            _R2 = U * ~W * Vt;
            for (int i = 0; i < 3; i++)
            {
                _t[i, 0] = U[i, 2];   //取最后一列
            }
        }
        #endregion

    }
}
