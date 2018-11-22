using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BilibiliSubtitle2SRT
{
    public partial class Form1 : Form
    {
        string file_path = "";
        BiliSubStyle myBiliSubStyle = new BiliSubStyle();
        List<BiliSub> myBiliSub = new List<BiliSub>();
        public Form1()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);            
        }
        
        public class BiliSubStyle
        {
            public string font_size { get; set; }
            public string font_color { get; set; }
            public string background_alpha { get; set; }
            public string background_color { get; set; }
            public string Stroke { get; set; }
            //public BiliSub[] body { get; set; }
        }
        
        public class BiliSub
        {
            public string from { get; set; }
            public string to { get; set; }
            public string location { get; set; }
            public string content { get; set; }
        }

        /*b站字幕格式
         * 根元素是样式
         * fontsize
         * font_color
         * background_alpha
         * background_color
         * Stroke
         * body
         * 
         * body下面是字幕本体
         * from
         * to
         * location
         * content
         * 
         * 取出字幕本体后，每行srt字幕占据三行
         * 1，加入行号
         * 2，将from和to的秒转换为HH:mm:ss,fff，格式为“HH:mm:ss,fff --> HH:mm:ss,fff”
         * 3，加入字幕文本内容
         * 4，如果并非最后一行，加入一个换行符
         */

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Count() != 0)
            {
                file_path = files[0];
            }
            this.Text = file_path;
            string contents = File.ReadAllText(file_path);            
            string contents_temp = contents.Replace("},{", "\r\n").Replace(":[{", "\r\n");            
            textBox1.Text = contents_temp;
            processBiliSub();
            BiliSubToSrt();
            //this.Text = "finished";
            myBiliSub.Clear();
        }

        private string SecondToTime(string s)
        {
            TimeSpan t = TimeSpan.FromSeconds(Convert.ToDouble(s));
            string result = string.Format("{0:D2}:{1:D2}:{2:D2},{3:D3}",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds);
            return result;
        }

        private void processBiliSub()
        {
            string patternSytle = "(\"font_size\":)(?<font_size>\\d+\\.?\\d*)(,\"font_color\":\")(?<=\")(?<font_color>.*?)(?=\")(\",\"background_alpha\":)(?<background_alpha>\\d+\\.?\\d*)(,\"background_color\":\")(?<=\")(?<background_color>.*?)(?=\")(\",\"Stroke\":\")(?<=\")(?<Stroke>.*?)(?=\")";
            //@"(\"from\":)(\d+\.?\d*)(,\"to\":)(\d+\.?\d*)(,"location":)(\d+)(,"content":")(?<= ")(.*?)(?=")";
            string patternSub = "(\"from\":)(?<from>\\d+\\.?\\d*)(,\"to\":)(?<to>\\d+\\.?\\d*)(,\"location\":)(?<location>\\d+)(,\"content\":\")(?<=\")(?<content>.*?)(?=\")";          //when it's a timed line

            var text = textBox1.Lines[0];
            if (Regex.IsMatch(text, patternSytle))
            {
                myBiliSubStyle = new BiliSubStyle
                {
                    font_size = Regex.Match(text, patternSytle).Groups["font_size"].Value,
                    font_color = Regex.Match(text, patternSytle).Groups["font_color"].Value,
                    background_alpha = Regex.Match(text, patternSytle).Groups["background_alpha"].Value,
                    background_color = Regex.Match(text, patternSytle).Groups["background_color"].Value,
                    Stroke = Regex.Match(text, patternSytle).Groups["Stroke"].Value,
                };
            }

            foreach (var line in textBox1.Lines)
            {
                if (Regex.IsMatch(line, patternSub))
                {
                    foreach (Match m in Regex.Matches(line, patternSub))
                    {
                        myBiliSub.Add(new BiliSub
                        {
                            from = m.Groups["from"].Value,
                            to = m.Groups["to"].Value,
                            location = m.Groups["location"].Value,
                            content = m.Groups["content"].Value,
                        });
                    }
                }
            }
        }

        private void BiliSubToSrt()
        {
            int flag = 1;
            string temp = "";
            foreach(var sub in myBiliSub)
            {
                temp += flag.ToString() + "\r\n";
                temp += SecondToTime(sub.from) + " --> " + SecondToTime(sub.to) + "\r\n";
                temp += sub.content + "\r\n\r\n";
                flag++;
            }
            textBox2.Text = temp;
        }
    }
}
