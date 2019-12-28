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
using System.IO;
using System.Xml;

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

        #region XML
        public class XmlConfigUtil
        {
            #region 全局变量
            string _xmlPath;        //文件所在路径
            #endregion

            #region 构造函数
            /// <summary>
            /// 初始化一个配置
            /// </summary>
            /// <param name="xmlPath">配置所在路径</param>
            public XmlConfigUtil(string xmlPath)
            {
                _xmlPath = Path.GetFullPath(xmlPath);
            }
            #endregion

            #region 公有方法
            /// <summary>
            /// 写入配置
            /// </summary>
            /// <param name="value">写入的值</param>
            /// <param name="nodes">节点</param>
            public void Write(string value, params string[] nodes)
            {
                //初始化xml
                XmlDocument xmlDoc = new XmlDocument();
                if (File.Exists(_xmlPath))
                    xmlDoc.Load(_xmlPath);
                else
                    xmlDoc.LoadXml("<XmlConfig />");
                XmlNode xmlRoot = xmlDoc.ChildNodes[0];

                //新增、编辑 节点
                string xpath = string.Join("/", nodes);
                XmlNode node = xmlDoc.SelectSingleNode(xpath);
                if (node == null)    //新增节点
                {
                    node = makeXPath(xmlDoc, xmlRoot, xpath);
                }
                node.InnerText = value;

                //保存
                xmlDoc.Save(_xmlPath);
            }

            /// <summary>
            /// 读取配置
            /// </summary>
            /// <param name="nodes">节点</param>
            /// <returns></returns>
            public string Read(params string[] nodes)
            {
                XmlDocument xmlDoc = new XmlDocument();
                if (File.Exists(_xmlPath) == false)
                    return null;
                else
                    xmlDoc.Load(_xmlPath);

                string xpath = string.Join("/", nodes);
                XmlNode node = xmlDoc.SelectSingleNode("/XmlConfig/" + xpath);
                if (node == null)
                    return null;

                return node.InnerText;
            }
            #endregion

            #region 私有方法
            //递归根据 xpath 的方式进行创建节点
            static private XmlNode makeXPath(XmlDocument doc, XmlNode parent, string xpath)
            {

                // 在XPath抓住下一个节点的名称；父级如果是空的则返回
                string[] partsOfXPath = xpath.Trim('/').Split('/');
                string nextNodeInXPath = partsOfXPath.First();
                if (string.IsNullOrEmpty(nextNodeInXPath))
                    return parent;

                // 获取或从名称创建节点
                XmlNode node = parent.SelectSingleNode(nextNodeInXPath);
                if (node == null)
                    node = parent.AppendChild(doc.CreateElement(nextNodeInXPath));

                // 加入的阵列作为一个XPath表达式和递归余数
                string rest = String.Join("/", partsOfXPath.Skip(1).ToArray());
                return makeXPath(doc, node, rest);
            }
            #endregion
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

            //初始化并指定文件路径
            XmlConfigUtil util = new XmlConfigUtil("配置信息.xml");

            //对控件进行赋值
            this.textBox8.Text = util.Read("information1", "姓名");
            this.textBox7.Text = util.Read("information1", "班级");
            this.textBox6.Text = util.Read("information1", "学号");
            this.textBox5.Text = util.Read("information1", "电话号码");

        }

        //XML 保存配置信息
        private void button4_Click(object sender, EventArgs e)
        {
            //初始化并指定文件路径
            XmlConfigUtil util = new XmlConfigUtil("配置信息.xml");

            //写入要保存的值以及路径（System、Menu.....都是路径）  params string[] 的方式
            util.Write( this.textBox8.Text.Trim(),"information1", "姓名");
            util.Write(this.textBox7.Text.Trim(), "information1", "班级");
            util.Write(this.textBox6.Text.Trim(), "information1", "学号");
            util.Write(this.textBox5.Text.Trim(), "information1", "电话号码");
        }

        //读取XML配置文件
        private void button3_Click(object sender, EventArgs e)
        {
            //初始化并指定文件路径
            XmlConfigUtil util = new XmlConfigUtil("配置信息.xml");

            //对控件进行赋值
            this.textBox8.Text = util.Read("information1", "姓名");
            this.textBox7.Text = util.Read("information1", "班级");
            this.textBox6.Text = util.Read("information1", "学号");
            this.textBox5.Text = util.Read("information1", "电话号码");
        }
    }
}
