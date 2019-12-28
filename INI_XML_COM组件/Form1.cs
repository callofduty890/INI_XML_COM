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

        //声明引用 读取配置函数
        [DllImport("kernel32")]
        public static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string file_path);

        //对读取的配置函数进行二次封装，提高使用实用性
        /// <summary>
        /// 返回INI指定根节点下键对应的值
        /// </summary>
        /// <param name="Section">根节点</param>
        /// <param name="key">对应值</param>
        /// <param name="File_Path">文件路径</param>
        /// <returns></returns>
        public static string ContentValue(string Section,string key ,string File_Path)
        {
            //创建可变字符串
            StringBuilder temp = new StringBuilder(1024);
            //获取返回值
            GetPrivateProfileString(Section, key, "", temp, 1024, File_Path);
            //返回内容
            return temp.ToString();
        }

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
            WritePrivateProfileString("information1", "班级", this.textBox2.Text.Trim(), Save_File_Path);
            WritePrivateProfileString("information1", "学号", this.textBox3.Text.Trim(), Save_File_Path);
            WritePrivateProfileString("information1", "电话号码", this.textBox4.Text.Trim(), Save_File_Path);
        }

        //读取配置信息
        private void button2_Click(object sender, EventArgs e)
        {
            //设置读取路径
            string Save_File_Path = System.AppDomain.CurrentDomain.BaseDirectory + "Save_information.ini";
            //赋值
            this.textBox1.Text = ContentValue("information1", "姓名", Save_File_Path);
            this.textBox2.Text = ContentValue("information1", "班级", Save_File_Path);
            this.textBox3.Text = ContentValue("information1", "学号", Save_File_Path);
            this.textBox4.Text = ContentValue("information1", "电话号码", Save_File_Path);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //设置读取路径
            string Save_File_Path = System.AppDomain.CurrentDomain.BaseDirectory + "Save_information.ini";
            //赋值
            this.textBox1.Text = ContentValue("information1", "姓名", Save_File_Path);
            this.textBox2.Text = ContentValue("information1", "班级", Save_File_Path);
            this.textBox3.Text = ContentValue("information1", "学号", Save_File_Path);
            this.textBox4.Text = ContentValue("information1", "电话号码", Save_File_Path);
        }
    }
}
