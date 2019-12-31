using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace INI_ClassLibrary
{
    //定义接口
    [ComVisible(true)]
    [Guid("13E75B9B-D0AE-41C9-8246-12971BCF1019")]
    public interface IMyClass
    {
        //3.自定义函数
        //写入INI文件
        void Write_INI(string section, string key, string val, string filepath);
        //读取INI文件
        string ContentValue(string Section, string key, string strFilePath);
    }

    //实现接口
    [ComVisible(true)]
    [Guid("B01538DF-0AF2-4261-BF7A-3C62DEDAA7E8")]
    [ProgId("COM_INI.IMyClass")]
    public class INI: IMyClass
    {
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
        public string ContentValue(string Section, string key, string File_Path)
        {
            //创建可变字符串
            StringBuilder temp = new StringBuilder(1024);
            //获取返回值
            GetPrivateProfileString(Section, key, "", temp, 1024, File_Path);
            //返回内容
            return temp.ToString();
        }

        //写入INI配置文件
        public void Write_INI(string section, string key, string val, string filepath)
        {
            WritePrivateProfileString(section, key, val, filepath);
        }

    }
}
