using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeasureMan
{
    public partial class AboutAuthor : Form
    {
        public AboutAuthor(Language lang)
        {
            InitializeComponent();
            if (lang == Language.Chinese)
            {
                rtbAuthors.AppendText("该程序著作权归周坤、谷晨鹏、张东所有。\n");
                rtbAuthors.AppendText("机构：\n");
                rtbAuthors.AppendText("武汉理工大学，中国\n");
                rtbAuthors.AppendText("联系方式：\n");
            }
            else
            {
                this.Text = "About Author";
                rtbAuthors.Font=new Font(rtbAuthors.Font.FontFamily,9);
                rtbAuthors.AppendText("The copyright of this program is reserved by Kun Zhou, Chenpeng Gu and Dong Zhang.\n");
                rtbAuthors.AppendText("Organization:\n");
                rtbAuthors.AppendText("Wuhan University of Technology, China\n");
                rtbAuthors.AppendText("E-mails:\n");
            }
            rtbAuthors.AppendText("zhoukunzxz@qq.com\n");
            rtbAuthors.AppendText("1026950459@qq.com\n");
            rtbAuthors.AppendText("1273548614@qq.com\n");
        }
    }
}
