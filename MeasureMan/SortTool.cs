using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 排序工具
    /// </summary>
    public static class SortTool
    {
        /// <summary>
        /// 将加载文件按名称排序。
        /// </summary>
        /// <param name="files">图像集合</param>
        public static void SortByName(List<AddedImage> images)
        {
            //插入排序
            for (int i = 1; i < images.Count; i++)
            {
                for (int j = i; j > 0; j--)
                {
                    if (int.Parse(images[j].name.Split('.')[0]) < int.Parse(images[j - 1].name.Split('.')[0]))
                        Swap(images, j, j - 1);
                    else
                        break;
                }
            }
        }

        /// <summary>
        /// 将加载文件按名称排序。
        /// </summary>
        /// <param name="ftPaths">特征点文件列表</param>
        public static void SortByName(List<string> ftPaths)
        {
            for (int i = 1; i < ftPaths.Count; i++)
            {
                for (int j = i; j > 0; j--)
                {
                    int path1 = int.Parse(System.IO.Path.GetFileNameWithoutExtension(ftPaths[j-1]));
                    int path2 = int.Parse(System.IO.Path.GetFileNameWithoutExtension(ftPaths[j]));
                    if (path2 < path1)
                        Swap(ftPaths, j, j - 1);
                    else
                        break;
                }
            }
        }


        /// <summary>
        /// 将加载文件按创建时间排序。
        /// </summary>
        /// <param name="files">图像集合</param>
        public static void SortByTime(List<AddedImage> images)
        {
            //插入排序
            for (int i = 1; i < images.Count; i++)
            {
                for (int j = i; j > 0; j--)
                {
                    if (images[j].time < images[j - 1].time)
                        Swap(images, j, j - 1);
                    else
                        break;
                }
            }
        }
        /*
        /// <summary>
        /// 递归快速排序
        /// </summary>
        /// <param name="data">源数据</param>
        /// <param name="L">头索引</param>
        /// <param name="R">尾索引</param>
        /// <param name="indexes">伴随源数据排序的数据，忽略则设为null</param>
        public static void QuickSort(dynamic data, int L, int R, dynamic indexes)
        {
            int P = Partition(data, L, R, indexes);
            if (L < P)
            {
                Swap(data, L, P);
                Swap(indexes, L, P);
            }
            if (P - 1 > L)
                QuickSort(data, L, P - 1, indexes);
            if (P + 1 < R)
                QuickSort(data, P + 1, R, indexes);
        }*/

        /// <summary>
        /// 非递归快速排序(更安全)
        /// </summary>
        /// <param name="data">源数据</param>
        /// <param name="L0">头索引</param>
        /// <param name="R0">尾索引</param>
        /// <param name="indexes">伴随源数据排序的数据，忽略则设为null</param>
        public static void QuickSort2(dynamic data, int L0, int R0, dynamic indexes)
        {
            Queue<int> queue1 = new Queue<int>(), queue2 = new Queue<int>();
            queue1.Enqueue(L0);
            queue2.Enqueue(R0);
            while (queue1.Count() != 0 && queue2.Count() != 0)
            {
                int L, R, P;
                L = queue1.Dequeue();
                R = queue2.Dequeue();
                P = Partition(data, L, R, indexes);
                if (P > L)
                {
                    Swap(data, L, P);
                    Swap(indexes, L, P);
                }
                if (P - 1 > L)
                {
                    queue1.Enqueue(L);
                    queue2.Enqueue(P - 1);
                }
                if (P + 1 < R)
                {
                    queue1.Enqueue(P + 1);
                    queue2.Enqueue(R);
                }
            }
        }

        /// <summary>
        /// 普遍数组快速排序（更快）
        /// </summary>
        /// <param name="data">源数据</param>
        /// <param name="L0">头索引</param>
        /// <param name="R0">尾索引</param>
        /// <param name="indexes">伴随源数据排序的数据</param>
        public static void QuickSort3(double[] data, int L0, int R0, int[] indexes)
        {
            Queue<int> queue1 = new Queue<int>(), queue2 = new Queue<int>();
            queue1.Enqueue(L0);
            queue2.Enqueue(R0);
            while (queue1.Count() != 0 && queue2.Count() != 0)
            {
                int L, R, P;
                L = queue1.Dequeue();
                R = queue2.Dequeue();
                P = Partition2(data, L, R, indexes);
                if (P > L)
                {
                    Swap2(data, L, P);
                    Swap3(indexes, L, P);
                }
                if (P - 1 > L)
                {
                    queue1.Enqueue(L);
                    queue2.Enqueue(P - 1);
                }
                if (P + 1 < R)
                {
                    queue1.Enqueue(P + 1);
                    queue2.Enqueue(R);
                }
            }
        }

        /// <summary>
        /// 获得基准值需要交换到的索引
        /// </summary>
        /// <param name="data">源数据</param>
        /// <param name="L">头索引</param>
        /// <param name="R">尾索引</param>
        /// <param name="indexes">伴随源数据排序的数据</param>
        /// <returns>索引</returns>
        private static int Partition(dynamic data, int L, int R, dynamic indexes)
        {
            int P = L;
            while (L <= R)
            {
                if (data[L].CompareTo(data[P]) < 0 || L == P)
                    L++;
                else if (data[R].CompareTo(data[P]) > 0 || R == P)
                    R--;
                else
                {
                    Swap(data, L, R);
                    Swap(indexes, L, R);
                    L++;
                    R--;
                }
            }
            return R;
        }

        /// <summary>
        /// 获得基准值需要交换到的索引（更快）
        /// </summary>
        /// <param name="data">源数据</param>
        /// <param name="L">头索引</param>
        /// <param name="R">尾索引</param>
        /// <param name="indexes">伴随源数据排序的数据</param>
        /// <returns>索引</returns>
        private static int Partition2(double[] data, int L, int R, int[] indexes)
        {
            int P = L;
            while (L <= R)
            {
                if (data[L]<data[P]|| L == P)
                    L++;
                else if (data[R]>data[P]|| R == P)
                    R--;
                else
                {
                    Swap2(data, L, R);
                    Swap3(indexes, L, R);
                    L++;
                    R--;
                }
            }
            return R;
        }

        /// <summary>
        /// 交换两个数据（通用）
        /// </summary>
        /// <param name="data">源数据</param>
        /// <param name="index1">索引1</param>
        /// <param name="index2">索引2</param>
        private static void Swap(dynamic data, int index1, int index2)
        {
            if (data == null)
                return;
            var temp = data[index1];
            data[index1] = data[index2];
            data[index2] = temp;
        }

        /// <summary>
        /// 交换两个数据(针对double数组)
        /// </summary>
        /// <param name="data">源数据</param>
        /// <param name="index1">索引1</param>
        /// <param name="index2">索引2</param>
        private static void Swap2(double[] data, int index1, int index2)
        {
            double temp = data[index1];
            data[index1] = data[index2];
            data[index2] = temp;
        }

        /// <summary>
        /// 交换两个数据(针对int数组)
        /// </summary>
        /// <param name="data">源数据</param>
        /// <param name="index1">索引1</param>
        /// <param name="index2">索引2</param>
        private static void Swap3(int[] data, int index1, int index2)
        {
            int temp = data[index1];
            data[index1] = data[index2];
            data[index2] = temp;
        }

        /// <summary>
        /// 判断文本是否为合适的数字
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="isPositive">0为负数,1为正数,其他为正负数</param>
        /// <param name="containZero">是否包含0</param>
        /// <param name="integer">是否为整数</param>
        /// <returns>是否为满足条件的数字</returns>
        public static bool IsFitNumber(string text,int isPositive=2,bool containZero=true,bool integer=false)
        {
            if (text == "")
                return false;
            try
            {
                double number;
                if (integer)
                    number=int.Parse(text);
                else
                    number= double.Parse(text);
                switch (isPositive)
                {
                    case 0:
                        if (containZero)
                        {
                            if (number > 0)
                                return false;
                        }
                        else
                        {
                            if (number >= 0)
                                return false;
                        }
                        break;
                    case 1:
                        if (containZero)
                        {
                            if (number < 0)
                                return false;
                        }
                        else
                        {
                            if (number <= 0)
                                return false;
                        }
                        break;
                    default:
                        if (!containZero && number == 0)
                            return false;
                        break;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 判断数据是否有序
        /// </summary>
        /// <param name="data">源数据</param>
        /// <param name="count">数据量</param>
        /// <returns>1为升序，-1为降序,0为无序</returns>
        public static int CheckInOrder(dynamic data,int count)
        {
            int order=-1;//降序
            int i = 1;
            for (; i < count; i++)
            {
                if (data[i].CompareTo(data[i - 1]) > 0)
                {
                    order = 1;
                    break;
                }
                else if (data[i].CompareTo(data[i - 1]) < 0)
                    break;
            }
            i++;
            if (order==1)
            {
                for (; i < count; i++)
                {
                    if (data[i].CompareTo(data[i - 1]) < 0)
                        return 0;
                }
            }
            else
            {
                for (; i < count; i++)
                {
                    if (data[i].CompareTo(data[i - 1]) >0)
                        return 0;
                }
            }
            return order;
        }

        /// <summary>
        /// 反转数据
        /// </summary>
        /// <param name="data">源数据</param>
        /// <param name="count">数据量</param>
        /// <param name="indexes">伴随源数据反转的数据</param>
        public static void Reverse(dynamic data,int count,dynamic indexes)
        {
            for (int i = 0, j = count - 1; i < j; i++, j--)
            {
                Swap(data, i, j);
                Swap(indexes, i, j);
            }
        }
    }
}
