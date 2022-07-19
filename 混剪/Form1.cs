using FFmpegUtil;
using System.Collections;

namespace 混剪
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;      //该值确定是否可以选择多个文件
            dialog.Title = "请选择您要的素材";     //弹窗的标题
            dialog.InitialDirectory = "C:\\";       //默认打开的文件夹的位置
            dialog.Filter = "视频文件(*.mp4)|*.mp4|所有文件(*.*)|*.*";       //筛选文件
            dialog.ShowHelp = false;     //是否显示“帮助”按钮

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string s in dialog.FileNames)
                {
                    listBox1.Items.Add(s);
                }
            }
            label3.Text = "素材总数："+listBox1.Items.Count.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string Path = "";
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.Description = "选择文件所在文件夹目录";  //提示的文字
            if (folder.ShowDialog() == DialogResult.OK)
            {
                Path = folder.SelectedPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //初始化
            FFmpegUtil.FFmpegUtil fFmpeg = new FFmpegUtil.FFmpegUtil(this);
            fFmpeg.Start();
            ffmpegHepler.form = this;
            //输出框获得焦点事件
            richTextBox1.GotFocus += new EventHandler((obj, ex) => {
                this.Focus();
            });
        }


        delegate void updatePrintln(string str);
        //输出框输出新的内容
        public void debug(string str)
        {
            string s = (str == null)?"null":str;
            if (s.IndexOf("错误") >= 0)
            {
                richTextBox1.SelectionFont = new Font(s, 9, FontStyle.Regular);
                richTextBox1.SelectionColor = Color.Red;
            }
            if(s.IndexOf("运行成功") >= 0|| s.IndexOf("处理完成") >= 0|| s.IndexOf("本次耗时") >= 0)
            {
                richTextBox1.SelectionFont = new Font(s, 9, FontStyle.Regular);
                richTextBox1.SelectionColor = Color.Green;
            }
            richTextBox1.AppendText(s + Environment.NewLine);
            this.richTextBox1.SelectionStart = this.richTextBox1.Text.Length;
            this.richTextBox1.SelectionLength = 0;
            this.richTextBox1.ScrollToCaret();
        }

        //textBox只能输入数字
        private void Enter_Number(object sender, KeyPressEventArgs e)
        {
            //IsNumber判断输入的是不是数字
            //e.KeyChar != (char)Keys.Back 判断输入的是不是退格

            if (!(Char.IsNumber(e.KeyChar)) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(this.textBox1.Text)|| string.IsNullOrEmpty(this.textBox2.Text)||string.IsNullOrEmpty(textBox5.Text))
            {
                debug(MessageText.Parameter_error);
                return;
            }
            if(int.Parse(textBox1.Text) > listBox1.Items.Count)
            {
                debug(MessageText.Parameter_error);
                return;
            }
            string[,] strArg = new string[int.Parse(textBox2.Text), int.Parse(textBox1.Text)];
            string[] sounds = new string[int.Parse(textBox2.Text)];
            progressBar1.Maximum = int.Parse(textBox2.Text);
            for (int i = 0; i < int.Parse(textBox2.Text); i++)
            {
                for(int x = 0; x < int.Parse(textBox1.Text); x++)
                {
                    Random rd;
                    string s;
                    do
                    {
                        rd = new Random();
                        s = listBox1.Items[rd.Next(0, listBox1.Items.Count)].ToString();
                    } while (isExist(strArg,i,s));
                    strArg[i, x] = s;
                }
                Random rds = new Random();
                if(listBox2.Items.Count > 0) sounds[i] = listBox2.Items[rds.Next(0, listBox2.Items.Count)].ToString();
            }
            debug(ffmpegHepler.Splice_Some_MP4(strArg,textBox5.Text,checkBox1.Checked,sounds));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string Path = "";
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.Description = "选择输出的文件夹目录";  //提示的文字
            if (folder.ShowDialog() == DialogResult.OK)
            {
                Path = folder.SelectedPath;
                textBox5.Text = Path;
            }
        }

        //判断数组里是否含有某元素
        private static bool isExist(string[,] str,int i,string s)
        {
            for(int x = 0; x < str.GetLength(1); x++)
            {
                if (str[i,x] == null) continue;
                if (str[i, x].Equals(s)) return true;
            }
            return false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;      //该值确定是否可以选择多个文件
            dialog.Title = "请选择您要的素材";     //弹窗的标题
            dialog.InitialDirectory = "C:\\";       //默认打开的文件夹的位置
            dialog.Filter = "音频文件(*.mp3)|*.mp3|所有文件(*.*)|*.*";       //筛选文件
            dialog.ShowHelp = false;     //是否显示“帮助”按钮

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string s in dialog.FileNames)
                {
                    listBox2.Items.Add(s);
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
        }

        //增加进度条
        public void Form_Schedule(int i)
        {
            progressBar1.Value += i;
        }

        //清空进度
        public void Clone_Schedule()
        {
            progressBar1.Value = 0;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ffmpegHepler.Stop();
        }
    }
}