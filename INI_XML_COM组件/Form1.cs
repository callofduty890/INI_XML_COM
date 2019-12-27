using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//引用非委托动态链接库管理的类库(C++类型的动态链接库)
using System.Runtime.InteropServices;

namespace INI_XML_COM组件
{
    public partial class Form1 : Form
    {

        #region INI
        //非委托动态链接库的引用
        //声明引用库中的写入配置函数
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string value, string filepath);



        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        //保存配置信息
        private void button1_Click_1(object sender, EventArgs e)
        {
            //设置保存路径
            string Save_File_Path = System.AppDomain.CurrentDomain.BaseDirectory + "Save_information.ini";
            //保存信息
            WritePrivateProfileString("information1","姓名",this.textBox1.Text.Trim(),Save_File_Path);
        }
    }
}
