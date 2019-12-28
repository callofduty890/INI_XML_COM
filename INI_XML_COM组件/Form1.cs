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
using MSWord = Microsoft.Office.Interop.Word;
using System.Reflection;

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

        #region CSV
        public class CSV
        {
            private static char quotechar = ',';

            public static void WriteCSV(string filePathName, List<string[]> rows, bool append)
            {
                StreamWriter streamWriter = new StreamWriter(filePathName, append, Encoding.Default);
                foreach (string[] row in rows)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = 0; i < row.Length; i++)
                    {
                        string text = row[i].Replace("\"", "").Trim();
                        if (text == null)
                        {
                            text = "";
                        }
                        if (text.IndexOf(",") > -1)
                        {
                            text = "\"" + text + "\"";
                        }
                        stringBuilder.Append(text);
                        if (i != row.Length - 1)
                        {
                            stringBuilder.Append(quotechar);
                        }
                    }
                    streamWriter.WriteLine(stringBuilder.ToString());
                }
                streamWriter.Flush();
                streamWriter.Close();
            }

            public static List<string[]> ReadCSV(string filePathName)
            {
                StreamReader streamReader = new StreamReader(filePathName, Encoding.Default);
                string text = streamReader.ReadLine();
                List<string[]> list = new List<string[]>();
                while (text != null)
                {
                    List<string> strCellVal = getStrCellVal(text);
                    string[] array = new string[strCellVal.Count];
                    for (int i = 0; i < strCellVal.Count; i++)
                    {
                        array[i] = strCellVal[i];
                    }
                    list.Add(array);
                    text = streamReader.ReadLine();
                }
                streamReader.Close();
                return list;
            }

            private static List<string> getStrCellVal(string rowStr)
            {
                List<string> list = new List<string>();
                while (rowStr != null && rowStr.Length > 0)
                {
                    string text = "";
                    if (rowStr.StartsWith("\""))
                    {
                        rowStr = rowStr.Substring(1);
                        int num = rowStr.IndexOf("\",");
                        int num2 = rowStr.IndexOf("\" ,");
                        int num3 = rowStr.IndexOf("\"");
                        if (num < 0)
                        {
                            num = num2;
                        }
                        if (num < 0)
                        {
                            num = num3;
                        }
                        if (num > -1)
                        {
                            text = rowStr.Substring(0, num);
                            rowStr = ((num + 2 >= rowStr.Length) ? "" : rowStr.Substring(num + 2).Trim());
                        }
                        else
                        {
                            text = rowStr;
                            rowStr = "";
                        }
                    }
                    else
                    {
                        int num = rowStr.IndexOf(",");
                        if (num > -1)
                        {
                            text = rowStr.Substring(0, num);
                            rowStr = ((num + 1 >= rowStr.Length) ? "" : rowStr.Substring(num + 1).Trim());
                        }
                        else
                        {
                            text = rowStr;
                            rowStr = "";
                        }
                    }
                    if (text == "")
                    {
                        text = " ";
                    }
                    list.Add(text);
                }
                return list;
            }

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

        //world 保存
        private void button7_Click(object sender, EventArgs e)
        {
            object path;                              //文件路径变量
            string strContent;                        //文本内容变量
            MSWord.Application wordApp;                   //Word应用程序变量 
            MSWord.Document wordDoc;                  //Word文档变量

            path = Environment.CurrentDirectory + "\\MyWord_Print.doc";
            wordApp = new MSWord.Application(); //初始化

            wordApp.Visible = false;//使文档可见

            //如果已存在，则删除
            if (File.Exists((string)path))
            {
                File.Delete((string)path);
            }

            //由于使用的是COM库，因此有许多变量需要用Missing.Value代替
            Object Nothing = Missing.Value;
            wordDoc = wordApp.Documents.Add(ref Nothing, ref Nothing, ref Nothing, ref Nothing);

            #region 页面设置、页眉图片和文字设置，最后跳出页眉设置

            //页面设置
            wordDoc.PageSetup.PaperSize = MSWord.WdPaperSize.wdPaperA4;//设置纸张样式为A4纸
            wordDoc.PageSetup.Orientation = MSWord.WdOrientation.wdOrientPortrait;//排列方式为垂直方向
            wordDoc.PageSetup.TopMargin = 57.0f;
            wordDoc.PageSetup.BottomMargin = 57.0f;
            wordDoc.PageSetup.LeftMargin = 57.0f;
            wordDoc.PageSetup.RightMargin = 57.0f;
            wordDoc.PageSetup.HeaderDistance = 30.0f;//页眉位置

            //设置页眉
            wordApp.ActiveWindow.View.Type = MSWord.WdViewType.wdNormalView;//普通视图（即页面视图）样式
            wordApp.ActiveWindow.View.SeekView = MSWord.WdSeekView.wdSeekPrimaryHeader;//进入页眉设置，其中页眉边距在页面设置中已完成
            wordApp.Selection.ParagraphFormat.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphRight;//页眉中的文字右对齐


            //插入页眉图片(测试结果图片未插入成功)
            wordApp.Selection.ParagraphFormat.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphCenter;
            string headerfile = @"C:\Users\xiahui\Desktop\OficeProgram\3.jpg";
            MSWord.InlineShape shape1 = wordApp.ActiveWindow.ActivePane.Selection.InlineShapes.AddPicture(headerfile, ref Nothing, ref Nothing, ref Nothing);
            shape1.Height = 5;//强行设置貌似无效，图片没有按设置的缩放——图片的比例并没有改变。
            shape1.Width = 20;
            wordApp.ActiveWindow.ActivePane.Selection.InsertAfter("  文档页眉");//在页眉的图片后面追加几个字

            //去掉页眉的横线
            wordApp.ActiveWindow.ActivePane.Selection.ParagraphFormat.Borders[MSWord.WdBorderType.wdBorderBottom].LineStyle = MSWord.WdLineStyle.wdLineStyleNone;
            wordApp.ActiveWindow.ActivePane.Selection.Borders[MSWord.WdBorderType.wdBorderBottom].Visible = false;
            wordApp.ActiveWindow.ActivePane.View.SeekView = MSWord.WdSeekView.wdSeekMainDocument;//退出页眉设置
            #endregion

            #region 页码设置并添加页码

            //为当前页添加页码
            MSWord.PageNumbers pns = wordApp.Selection.Sections[1].Headers[MSWord.WdHeaderFooterIndex.wdHeaderFooterEvenPages].PageNumbers;//获取当前页的号码
            pns.NumberStyle = MSWord.WdPageNumberStyle.wdPageNumberStyleNumberInDash;//设置页码的风格，是Dash形还是圆形的
            pns.HeadingLevelForChapter = 0;
            pns.IncludeChapterNumber = false;
            pns.RestartNumberingAtSection = false;
            pns.StartingNumber = 0; //开始页页码？
            object pagenmbetal = MSWord.WdPageNumberAlignment.wdAlignPageNumberCenter;//将号码设置在中间
            object first = true;
            wordApp.Selection.Sections[1].Footers[MSWord.WdHeaderFooterIndex.wdHeaderFooterEvenPages].PageNumbers.Add(ref pagenmbetal, ref first);

            #endregion

            #region 行间距与缩进、文本字体、字号、加粗、斜体、颜色、下划线、下划线颜色设置

            wordApp.Selection.ParagraphFormat.LineSpacing = 16f;//设置文档的行间距
            wordApp.Selection.ParagraphFormat.FirstLineIndent = 30;//首行缩进的长度
            //写入普通文本
            strContent = "我是普通文本\n";
            wordDoc.Paragraphs.Last.Range.Text = strContent;

            wordDoc.Paragraphs.Last.Range.Text = "我再加一行试试，这里不加'\\n'";
            //直接添加段，不是覆盖( += )
            wordDoc.Paragraphs.Last.Range.Text += "不会覆盖的,";

            //添加在此段的文字后面，不是新段落
            wordDoc.Paragraphs.Last.Range.InsertAfter("这是后面的内容\n");

            //将文档的前4个字替换成"哥是替换文字"，并将其颜色设为红色
            object start = 0;
            object end = 4;
            MSWord.Range rang = wordDoc.Range(ref start, ref end);
            rang.Font.Color = MSWord.WdColor.wdColorRed;
            rang.Text = "哥是替换文字";
            wordDoc.Range(ref start, ref end);

            //写入黑体文本
            object unite = MSWord.WdUnits.wdStory;
            wordApp.Selection.EndKey(ref unite, ref Nothing);//将光标移到文本末尾
            wordApp.Selection.ParagraphFormat.FirstLineIndent = 0;//取消首行缩进的长度
            strContent = "这是黑体文本\n";
            wordDoc.Paragraphs.Last.Range.Font.Name = "黑体";
            wordDoc.Paragraphs.Last.Range.Text = strContent;

            //写入加粗文本
            strContent = "这是粗体文本\n"; //
            wordApp.Selection.EndKey(ref unite, ref Nothing);//这一句不加，有时候好像也不出问题，不过还是加了安全
            wordDoc.Paragraphs.Last.Range.Font.Bold = 1;
            wordDoc.Paragraphs.Last.Range.Text = strContent;

            //写入15号字体文本
            strContent = "我这个文本的字号是15号，而且是宋体\n";
            wordApp.Selection.EndKey(ref unite, ref Nothing);
            wordDoc.Paragraphs.Last.Range.Font.Size = 15;
            wordDoc.Paragraphs.Last.Range.Font.Name = "宋体";
            wordDoc.Paragraphs.Last.Range.Text = strContent;

            //写入斜体文本
            strContent = "我是斜体字文本\n";
            wordApp.Selection.EndKey(ref unite, ref Nothing);
            wordDoc.Paragraphs.Last.Range.Font.Italic = 1;
            wordDoc.Paragraphs.Last.Range.Text = strContent;

            //写入蓝色文本
            strContent = "我是蓝色的文本\n";
            wordApp.Selection.EndKey(ref unite, ref Nothing);
            wordDoc.Paragraphs.Last.Range.Font.Color = MSWord.WdColor.wdColorBlue;
            wordDoc.Paragraphs.Last.Range.Text = strContent;

            //写入下划线文本
            strContent = "我是下划线文本\n";
            wordApp.Selection.EndKey(ref unite, ref Nothing);
            wordDoc.Paragraphs.Last.Range.Font.Underline = MSWord.WdUnderline.wdUnderlineThick;
            wordDoc.Paragraphs.Last.Range.Text = strContent;

            //写入红色下画线文本
            strContent = "我是点线下划线，并且下划线是红色的\n";
            wordApp.Selection.EndKey(ref unite, ref Nothing);
            wordDoc.Paragraphs.Last.Range.Font.Underline = MSWord.WdUnderline.wdUnderlineDottedHeavy;
            wordDoc.Paragraphs.Last.Range.Font.UnderlineColor = MSWord.WdColor.wdColorRed;
            wordDoc.Paragraphs.Last.Range.Text = strContent;

            //取消下划线，并且将字号调整为12号
            strContent = "我他妈不要下划线了，并且设置字号为12号，黑色不要斜体\n";
            wordApp.Selection.EndKey(ref unite, ref Nothing);
            wordDoc.Paragraphs.Last.Range.Font.Size = 12;
            wordDoc.Paragraphs.Last.Range.Font.Underline = MSWord.WdUnderline.wdUnderlineNone;
            wordDoc.Paragraphs.Last.Range.Font.Color = MSWord.WdColor.wdColorBlack;
            wordDoc.Paragraphs.Last.Range.Font.Italic = 0;
            wordDoc.Paragraphs.Last.Range.Text = strContent;


            #endregion


            #region 插入图片、居中显示，设置图片的绝对尺寸和缩放尺寸，并给图片添加标题

            wordApp.Selection.EndKey(ref unite, ref Nothing); //将光标移动到文档末尾
            //图片文件的路径
            string filename = Environment.CurrentDirectory + "\\6.jpg";
            //要向Word文档中插入图片的位置
            Object range = wordDoc.Paragraphs.Last.Range;
            //定义该插入的图片是否为外部链接
            Object linkToFile = false;               //默认,这里貌似设置为bool类型更清晰一些
            //定义要插入的图片是否随Word文档一起保存
            Object saveWithDocument = true;              //默认
            //使用InlineShapes.AddPicture方法(【即“嵌入型”】)插入图片
            wordDoc.InlineShapes.AddPicture(filename, ref linkToFile, ref saveWithDocument, ref range);
            wordApp.Selection.ParagraphFormat.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphCenter;//居中显示图片

            //设置图片宽高的绝对大小

            //wordDoc.InlineShapes[1].Width = 200;
            //wordDoc.InlineShapes[1].Height = 150;
            //按比例缩放大小

            wordDoc.InlineShapes[1].ScaleWidth = 20;//缩小到20% ？
            wordDoc.InlineShapes[1].ScaleHeight = 20;

            //在图下方居中添加图片标题

            wordDoc.Content.InsertAfter("\n");//这一句与下一句的顺序不能颠倒，原因还没搞透
            wordApp.Selection.EndKey(ref unite, ref Nothing);
            wordApp.Selection.ParagraphFormat.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphCenter;
            wordApp.Selection.Font.Size = 10;//字体大小
            wordApp.Selection.TypeText("图1 测试图片\n");

            #endregion

            #region 添加表格、填充数据、设置表格行列宽高、合并单元格、添加表头斜线、给单元格添加图片
            wordDoc.Content.InsertAfter("\n");//这一句与下一句的顺序不能颠倒，原因还没搞透
            wordApp.Selection.EndKey(ref unite, ref Nothing); //将光标移动到文档末尾
            wordApp.Selection.ParagraphFormat.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphLeft;
            //object WdLine2 = MSWord.WdUnits.wdLine;//换一行;  
            //wordApp.Selection.MoveDown(ref WdLine2, 6, ref Nothing);//向下跨15行输入表格，这样表格就在文字下方了，不过这是非主流的方法

            //设置表格的行数和列数
            int tableRow = 6;
            int tableColumn = 6;

            //定义一个Word中的表格对象
            MSWord.Table table = wordDoc.Tables.Add(wordApp.Selection.Range,
            tableRow, tableColumn, ref Nothing, ref Nothing);

            //默认创建的表格没有边框，这里修改其属性，使得创建的表格带有边框 
            table.Borders.Enable = 1;//这个值可以设置得很大，例如5、13等等

            //表格的索引是从1开始的。
            wordDoc.Tables[1].Cell(1, 1).Range.Text = "列\n行";
            for (int i = 1; i < tableRow; i++)
            {
                for (int j = 1; j < tableColumn; j++)
                {
                    if (i == 1)
                    {
                        table.Cell(i, j + 1).Range.Text = "Column " + j;//填充每列的标题
                    }
                    if (j == 1)
                    {
                        table.Cell(i + 1, j).Range.Text = "Row " + i; //填充每行的标题
                    }
                    table.Cell(i + 1, j + 1).Range.Text = i + "行 " + j + "列";  //填充表格的各个小格子
                }
            }


            //添加行
            table.Rows.Add(ref Nothing);
            table.Rows[tableRow + 1].Height = 35;//设置新增加的这行表格的高度
            //向新添加的行的单元格中添加图片
            string FileName = Environment.CurrentDirectory + "\\6.jpg";//图片所在路径
            object LinkToFile = false;
            object SaveWithDocument = true;
            object Anchor = table.Cell(tableRow + 1, tableColumn).Range;//选中要添加图片的单元格
            wordDoc.Application.ActiveDocument.InlineShapes.AddPicture(FileName, ref LinkToFile, ref SaveWithDocument, ref Anchor);

            //由于是本文档的第2张图，所以这里是InlineShapes[2]
            wordDoc.Application.ActiveDocument.InlineShapes[2].Width = 50;//图片宽度
            wordDoc.Application.ActiveDocument.InlineShapes[2].Height = 35;//图片高度

            // 将图片设置为四周环绕型
            MSWord.Shape s = wordDoc.Application.ActiveDocument.InlineShapes[2].ConvertToShape();
            s.WrapFormat.Type = MSWord.WdWrapType.wdWrapSquare;


            //设置table样式
            table.Rows.HeightRule = MSWord.WdRowHeightRule.wdRowHeightAtLeast;//高度规则是：行高有最低值下限？
            table.Rows.Height = wordApp.CentimetersToPoints(float.Parse("0.8"));// 

            table.Range.Font.Size = 10.5F;
            table.Range.Font.Bold = 0;

            table.Range.ParagraphFormat.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphCenter;//表格文本居中
            table.Range.Cells.VerticalAlignment = MSWord.WdCellVerticalAlignment.wdCellAlignVerticalBottom;//文本垂直贴到底部
            //设置table边框样式
            table.Borders.OutsideLineStyle = MSWord.WdLineStyle.wdLineStyleDouble;//表格外框是双线
            table.Borders.InsideLineStyle = MSWord.WdLineStyle.wdLineStyleSingle;//表格内框是单线

            table.Rows[1].Range.Font.Bold = 1;//加粗
            table.Rows[1].Range.Font.Size = 12F;
            table.Cell(1, 1).Range.Font.Size = 10.5F;
            wordApp.Selection.Cells.Height = 30;//所有单元格的高度

            //除第一行外，其他行的行高都设置为20
            for (int i = 2; i <= tableRow; i++)
            {
                table.Rows[i].Height = 20;
            }

            //将表格左上角的单元格里的文字（“行” 和 “列”）居右
            table.Cell(1, 1).Range.ParagraphFormat.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphRight;
            //将表格左上角的单元格里面下面的“列”字移到左边，相比上一行就是将ParagraphFormat改成了Paragraphs[2].Format
            table.Cell(1, 1).Range.Paragraphs[2].Format.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphLeft;

            table.Columns[1].Width = 50;//将第 1列宽度设置为50

            //将其他列的宽度都设置为75
            for (int i = 2; i <= tableColumn; i++)
            {
                table.Columns[i].Width = 75;
            }


            //添加表头斜线,并设置表头的样式
            table.Cell(1, 1).Borders[MSWord.WdBorderType.wdBorderDiagonalDown].Visible = true;
            table.Cell(1, 1).Borders[MSWord.WdBorderType.wdBorderDiagonalDown].Color = MSWord.WdColor.wdColorRed;
            table.Cell(1, 1).Borders[MSWord.WdBorderType.wdBorderDiagonalDown].LineWidth = MSWord.WdLineWidth.wdLineWidth150pt;

            //合并单元格
            table.Cell(4, 4).Merge(table.Cell(4, 5));//横向合并

            table.Cell(2, 3).Merge(table.Cell(4, 3));//纵向合并，合并(2,3),(3,3),(4,3)

            #endregion

            wordApp.Selection.EndKey(ref unite, ref Nothing); //将光标移动到文档末尾

            wordDoc.Content.InsertAfter("\n");
            wordDoc.Content.InsertAfter("就写这么多，算了吧！2016.09.27");



            //WdSaveFormat为Word 2003文档的保存格式
            object format = MSWord.WdSaveFormat.wdFormatDocument;// office 2007就是wdFormatDocumentDefault
            //将wordDoc文档对象的内容保存为DOCX文档
            wordDoc.SaveAs(ref path, ref format, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing);
            //关闭wordDoc文档对象

            //看是不是要打印
            //wordDoc.PrintOut();



            wordDoc.Close(ref Nothing, ref Nothing, ref Nothing);
            //关闭wordApp组件对象
            wordApp.Quit(ref Nothing, ref Nothing, ref Nothing);
            Console.WriteLine(path + " 创建完毕！");
            Console.ReadKey();


            //我还要打开这个文档玩玩
            MSWord.Application app = new MSWord.Application();
            MSWord.Document doc = null;
            try
            {

                object unknow = Type.Missing;
                app.Visible = true;
                string str = Environment.CurrentDirectory + "\\MyWord_Print.doc";
                object file = str;
                doc = app.Documents.Open(ref file,
                    ref unknow, ref unknow, ref unknow, ref unknow,
                    ref unknow, ref unknow, ref unknow, ref unknow,
                    ref unknow, ref unknow, ref unknow, ref unknow,
                    ref unknow, ref unknow, ref unknow);
                string temp = doc.Paragraphs[1].Range.Text.Trim();
                Console.WriteLine("你他妈输出temp干嘛？");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            wordDoc = doc;
            wordDoc.Paragraphs.Last.Range.Text += "我真的不打算再写了,就写这么多吧";

            Console.ReadKey();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //保存路径
            string Path = "学生信息.csv";
            //第一行内容- 标题头信息
            string[] rowCells1 = new string[4];
            rowCells1[0] = "姓名";
            rowCells1[1] = "班级";
            rowCells1[2] = "学号";
            rowCells1[3] = "电话号码";

            //第二行内容
            string[] rowCells2 = new string[4];
            rowCells2[0] = "助教";
            rowCells2[1] = "视觉班";
            rowCells2[2] = "0002";
            rowCells2[3] = "123456789";

            string[] rowCells3 = new string[4];
            rowCells3[0] = "助教A";
            rowCells3[1] = "视觉班";
            rowCells3[2] = "0003";
            rowCells3[3] = "123456789";

            //保存信息-准备写入全部内容
            List<string[]> rowList_write = new List<string[]>();

            //添加全部行内容
            rowList_write.Add(rowCells1);
            rowList_write.Add(rowCells2);
            rowList_write.Add(rowCells3);

            //写入CSV文件当中
            CSV.WriteCSV(Path, rowList_write, true);

        }

        //显示内容
        private void button10_Click(object sender, EventArgs e)
        {
            //读取CSV文件
            List<string[]> ROW_LIST = CSV.ReadCSV("学生信息.csv");

            MessageBox.Show(ROW_LIST[0][0].ToString());
        }
    }
}
