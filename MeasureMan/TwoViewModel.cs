using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 双视模型
    /// </summary>
    public class TwoViewModel : IComparable<TwoViewModel>
    {
        /// <summary>
        /// 左图索引
        /// </summary>
        public int left;
        /// <summary>
        /// 右图索引
        /// </summary>
        public int right;
        /// <summary>
        /// 匹配文件路径
        /// </summary>
        public string path;
        /// <summary>
        /// 匹配数量
        /// </summary>
        public int match;
        /// <summary>
        /// 焦距是否相同
        /// </summary>
        public bool equalFocalLength;

        /// <summary>
        /// 双视模型初始化方法
        /// </summary>
        /// <param name="path">匹配文件路径</param>
        /// <param name="matchCount">匹配数量</param>
        public TwoViewModel(string path, int matchCount, bool equalFocalLength=false)
        {
            this.path = path;
            string[] nums = System.IO.Path.GetFileNameWithoutExtension(path).Split('-');
            left = int.Parse(nums[0]) - 1;
            right = int.Parse(nums[1]) - 1;
            this.match = matchCount;
            this.equalFocalLength = equalFocalLength;
        }

        public int CompareTo(TwoViewModel model2)
        {
            if (this.left > model2.left)
                return 1;
            else if (this.left == model2.left)
            {
                if (this.right > model2.right)
                    return 1;
                else if (this.right == model2.right)
                    return 0;
                else
                    return -1;
            }
            else
                return -1;
        }
    }
}
