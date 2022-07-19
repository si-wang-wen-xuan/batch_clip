using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using System.Text.RegularExpressions;
using System.Diagnostics;

public class ffmpegHepler
{
    //ffmpeg执行文件的路径
    private static string ffmpeg = System.AppDomain.CurrentDomain.BaseDirectory + "ffmpeg\\ffmpeg.exe";
    private static int? width;    //视频宽度
    private static int? height;   //视频高度
    public static 混剪.Form1 form;
    private static bool Can_Run;

    #region 属性访问
    /// <summary>
    /// 获取宽度
    /// </summary>
    /// <returns></returns>
    public static int? GetWidth()
    {
        return width;
    }
    /// <summary>
    /// 获取高度
    /// </summary>
    /// <returns></returns>
    public static int? GetHeight()
    {
        return height;
    }
    #endregion

    #region 从视频画面中截取一帧画面为图片
    /// <summary>
    /// 从视频画面中截取一帧画面为图片
    /// </summary>
    /// <param name="VideoName">视频文件，绝对路径</param>
    /// <param name="Width">图片的宽:620</param>
    /// <param name="Height">图片的长:360</param>
    /// <param name="CutTimeFrame">开始截取的时间如:"1"【单位秒】</param>
    /// <param name="PicPath">截图文件的保存路径【含文件名及后缀名】</param>
    /// <param name="SleepTime">线程挂起等待时间，单位毫秒【默认值是7000】</param>
    /// <returns>截图成功返回截图路径，失败返回空</returns>
    public static string GetPicFromVideo(string VideoName, int Width, int Height, string CutTimeFrame, string PicPath, int SleepTime)
    {
        //获取视频长宽尺寸
        GetMovWidthAndHeight(VideoName);
        if (!string.IsNullOrWhiteSpace(width.ToString()))   //说明获取到了视频的长宽参数
        {
            int resultWidht;
            int resultHeight;
            DealWidthAndHeight(int.Parse(width.ToString()), int.Parse(height.ToString()), Width, Height, out resultWidht, out resultHeight);

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(ffmpeg);
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.Arguments = " -i " + VideoName
                                + " -y -f image2 -ss " + CutTimeFrame
                                + " -t 0.001 -s " + resultWidht.ToString() + "*" + resultHeight.ToString()
                                + " " + PicPath;  //設定程式執行參數
            try
            {
                System.Diagnostics.Process.Start(startInfo);
                Thread.Sleep(SleepTime);//线程挂起，等待ffmpeg截图完毕
            }
            catch (Exception e)
            {
                return e.Message;
            }

            //返回视频图片完整路径
            if (System.IO.File.Exists(PicPath))
                return PicPath;
            return "";
        }
        else
            return "";
    }
    #endregion

    #region 获取视频的帧宽度和帧高度
    /// <summary>
    /// 获取视频的帧宽度和帧高度
    /// </summary>
    /// <param name="videoFilePath">mov文件的路径</param>
    /// <returns>null表示获取宽度或高度失败</returns>
    public static void GetMovWidthAndHeight(string videoFilePath)
    {
        width = null;
        height = null;
        try
        {
            //判断文件是否存在
            if (File.Exists(videoFilePath))
            {
                string output;
                string error;
                //执行命令
                ExecuteCommand("\"" + ffmpeg + "\"" + " -i " + "\"" + videoFilePath + "\"", out output, out error);

                if (!string.IsNullOrEmpty(error))
                {
                    width = null;
                    height = null;

                    //通过正则表达式获取信息里面的宽度信息
                    Regex regex = new Regex("(\\d{2,4})x(\\d{2,4})", RegexOptions.Compiled);
                    Match m = regex.Match(error);
                    if (m.Success)
                    {
                        width = int.Parse(m.Groups[1].Value);
                        height = int.Parse(m.Groups[2].Value);
                    }
                }
            }
        }
        catch (Exception)
        { }
    }
    #endregion

    #region  处理图片宽高比例截图问题
    /// <summary>
    /// 处理图片宽高比例截图问题
    /// </summary>
    /// <param name="videoWidht">304</param>
    /// <param name="videoHeight">640</param>
    /// <param name="imgWidth">640</param>
    /// <param name="imgHeight">360</param>
    /// <param name="width">最终处理的图片宽</param>
    /// <param name="height">最终处理的图片高</param>
    private static void DealWidthAndHeight(int videoWidht, int videoHeight, int imgWidth, int imgHeight, out int width, out int height)
    {
        if (videoWidht < videoHeight)  //说明是竖屏拍摄
        {
            if (imgWidth > videoWidht)
                width = videoWidht;
            else
                width = imgWidth;
            height = videoHeight;
        }
        else                           //说明是横屏拍摄
        {
            if (imgHeight > videoHeight)
                height = videoHeight;
            else
                height = imgHeight;
            width = videoWidht;
        }
    }
    #endregion

    #region 视频旋转
    /// <summary>
    /// 视频旋转
    /// </summary>
    /// <param name="videoFilePath">视频绝对路径</param>
    /// <param name="dealVideFilePath">视频旋转后保存路径</param>
    /// <param name="flag">1=顺时针旋转90度  2=逆时针旋转90度</param>
    /// <returns>true  成功  false  失败</returns>
    public static bool VideoRotate(string videoFilePath, string dealVideFilePath, string flag)
    {
        //ffmpeg -i success.mp4 -metadata:s:v rotate="90" -codec copy output_success.mp4
        string output;
        string error;
        //执行命令
        ExecuteCommand("\"" + ffmpeg + "\"" + " -y -i " + "\"" + videoFilePath + "\"" + " -vf transpose=" + flag + " -acodec copy " + "\"" + dealVideFilePath + "\"", out output, out error);
        if (File.Exists(dealVideFilePath))
            return true;
        else
            return false;
    }
    #endregion

    #region 给视频添加水印
    /// <summary>
    /// 给视频添加水印
    /// </summary>
    /// <param name="videoFilePath">原视频位置</param>
    /// <param name="dealVideFilePath">处理后的视频位置</param>
    /// <param name="waterPicPath">水印图片</param>
    /// <param name="location">水印距离视频的左上角坐标比如： 10:10</param>
    /// <returns></returns>
    public static bool VideoWaterMark(string videoFilePath, string dealVideFilePath, string waterPicPath, string location)
    {
        //ffmpeg -i success.mp4 -metadata:s:v rotate="90" -codec copy output_success.mp4
        string output;
        string error;
        //执行命令
        ExecuteCommand("\"" + ffmpeg + "\"" + " -i " + "\"" + videoFilePath + "\"" + " -i " + "\"" + waterPicPath + "\"" + " -filter_complex overlay=" + location + " \"" + dealVideFilePath + "\"", out output, out error);
        if (File.Exists(dealVideFilePath))
            return true;
        else
            return false;
    }
    #endregion

    #region 让ffmpeg执行一条命令
    /// <summary>
    /// 让ffmpeg执行一条command命令
    /// </summary>
    /// <param name="command">需要执行的Command</param>
    /// <param name="output">输出</param>
    /// <param name="error">错误</param>
    private static void ExecuteCommand(string command, out string output, out string error)
    {
        try
        {
            //创建一个进程
            Process pc = new Process();
            pc.StartInfo.FileName = command;
            pc.StartInfo.UseShellExecute = false;
            pc.StartInfo.RedirectStandardOutput = true;
            pc.StartInfo.RedirectStandardError = true;
            pc.StartInfo.CreateNoWindow = true;

            //启动进程
            pc.Start();

            //准备读出输出流和错误流
            string outputData = string.Empty;
            string errorData = string.Empty;
            pc.BeginOutputReadLine();
            pc.BeginErrorReadLine();

            pc.OutputDataReceived += (ss, ee) =>
            {
                outputData += ee.Data;
            };

            pc.ErrorDataReceived += (ss, ee) =>
            {
                errorData += ee.Data;
            };

            //等待退出
            pc.WaitForExit();

            //关闭进程
            pc.Close();

            //返回流结果
            output = outputData;
            error = errorData;
        }
        catch (Exception ex)
        {
            output = null;
            error = null;
        }
        form.debug(error);
    }
    #endregion

    #region 拼接两条视频
    /// <summary>
    /// 拼接两条视频
    /// </summary>
    /// <param name="StrMP4A">视频A</param>
    /// <param name="StrMP4B">视频B</param>
    /// <param name="StrOutMp4Path">视频输出路径</param>
    public static string CombineMp4WithoutTxt(string StrMP4A, string StrMP4B, string StrOutMp4Path)
    {
        if (Can_Run) return MessageText.Can_Run;
        Can_Run = true;
        string output;
        string error;
        string StrArg = "\"" + ffmpeg + "\"" + " -i concat:" + "\"" + StrMP4A + "|" + StrMP4B + "\"" + " -vcodec copy -acodec copy " + StrOutMp4Path + " -y";
        if (!File.Exists(StrMP4A) || !File.Exists(StrMP4B))
        {
            form.debug(MessageText.File_Not);
            return MessageText.Run_End;
        }

        new Thread(() =>
        {
            // 判断文件是否存在，不存在则创建，否则读取值显示到窗体
            string temp = System.AppDomain.CurrentDomain.BaseDirectory + "combine_temp.txt";
            if (!File.Exists(temp))
            {
                FileStream fs1 = new FileStream(temp, FileMode.Create, FileAccess.Write);//创建写入文件 
                StreamWriter sw = new StreamWriter(fs1);
                sw.WriteLine("file " + "'" + StrMP4A + "'");
                sw.WriteLine("file " + "'" + StrMP4B + "'");
                sw.Close();
                fs1.Close();
            }
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(ffmpeg);
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.Arguments = " -f concat -safe 0 -i " + temp
                                   + "-c copy " + StrOutMp4Path;
            startInfo.ErrorDialog = true;

            try
            {
                System.Diagnostics.Process.Start(startInfo);
                Thread.Sleep(7000);//线程挂起
            }
            catch (Exception e)
            {
                form.Invoke(new Action(() =>
                {
                    form.debug(e.Message);
                }));
                Can_Run = false;
                return;
            }
            form.Invoke(new Action(() =>
            {
                form.debug(MessageText.Run_Success);
                File.Delete(temp);
            }));
            Can_Run = false;
        }).Start();
        return MessageText.Runing;
    }
    #endregion

    #region 软件主函数
    /// <summary>
    /// 软件主要函数
    /// </summary>
    /// <param name="StrArg">多个视频路径</param>
    /// <param name="StrOutMp4Path">输出路径</param>
    /// <param name="isSound">是否去除音乐</param>
    /// <param name="sound">背景音乐</param>
    /// <returns></returns>
    public static string Splice_Some_MP4(string[,] StrArg, string StrOutMp4Path,bool isSound,string[] sound)
    {
        if (Can_Run) return MessageText.Can_Run;
        Can_Run = true;
        DateTime time = System.DateTime.Now;
        form.Invoke(new Action(() =>
        {
            form.Clone_Schedule();
        }));
        new Thread(() =>
        {
            for(int i = 0; i < StrArg.GetLength(0); i++)
            {
                if (!Can_Run)
                {
                    Form_Debug("停止成功");
                    Form_Debug("本次耗时：" + ((System.DateTime.Now - time).Hours + "h" + (System.DateTime.Now - time).Minutes + "m" + (System.DateTime.Now - time).Seconds + "s"));
                    return;
                }
                Form_Debug("正在处理第"+(i + 1)+"个视频");
                Form_Debug("正在创建临时文件");
                string temp = System.AppDomain.CurrentDomain.BaseDirectory + "combine_temp"+(i + 1)+".txt";
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }
                if (!File.Exists(temp))
                {
                    Form_Debug(temp);
                    FileStream fs1 = new FileStream(temp, FileMode.Create, FileAccess.Write);//创建写入文件 
                    StreamWriter sw = new StreamWriter(fs1);
                    Form_Debug("正在写入文件");
                    for(int x = 0; x < StrArg.GetLength(1); x++)
                    {
                        sw.WriteLine("file " + "'" + StrArg[i,x] + "'");
                    }
                    sw.Close();
                    fs1.Close();
                    Form_Debug("写入完成");
                }
                Form_Debug("开始拼接视频");
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(ffmpeg);
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.Arguments = " -f concat -safe 0 -i " + temp
                                       +(isSound?" -an":"")+ " -y -c copy -q:v 1 " + StrOutMp4Path + @"\成品" + (i + 1) + ".mp4";
                try
                {
                    Process process = new Process();
                    process.StartInfo = startInfo;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.Start();
                    process.WaitForExit();//等待进程关闭
                }
                catch(Exception e)
                {
                    Can_Run = false;
                    Form_Debug(e.Message);
                    Form_Debug(MessageText.Run_End);
                    return;
                }
                Form_Debug("视频拼接完成");
                //添加背景音乐
                if (sound.Length > 0)
                {
                    Form_Debug("开始添加音乐");
                    System.Diagnostics.ProcessStartInfo startInfo1 = new System.Diagnostics.ProcessStartInfo(ffmpeg);
                    startInfo1.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    Random rds = new Random();
                    startInfo1.Arguments = " -i " + StrOutMp4Path + @"\成品" + (i + 1) + ".mp4"
                                           + " -i " + sound[rds.Next(0, sound.Length)]
                                           + " -filter_complex [1:0]apad  -y -shortest " + StrOutMp4Path + @"\成品" + (i + 1) + "(音频).mp4";
                    try
                    {
                        Process process1 = new Process();
                        process1.StartInfo = startInfo1;
                        process1.Start();
                        process1.WaitForExit();//等待进程关闭
                    }
                    catch (Exception e)
                    {
                        Can_Run = false;
                        Form_Debug(e.Message);
                        Form_Debug(MessageText.Run_End);
                        return;
                    }
                    Form_Debug("音频添加成功");
                }
                Form_Debug("第"+(i + 1)+"个视频处理完成："+ StrOutMp4Path + @"\成品" + (i + 1) + ".mp4");
                temp.Clone();
                File.Delete(temp);
                Form_Schedule(1);
            }
            Form_Debug(MessageText.Run_Success);
            Form_Debug("本次耗时：" + ((System.DateTime.Now - time).Hours + "h" + (System.DateTime.Now - time).Minutes + "m" + (System.DateTime.Now - time).Seconds + "s"));
            form.Invoke(new Action(() =>
            {
                form.Clone_Schedule();
            }));
            Can_Run = false;
        }).Start();
        return MessageText.Runing;
    }
    #endregion
    private static void Form_Debug(string str)
    {
        form.Invoke(new Action(() =>
        {
            form.debug(str);
        }));
    }

    //增加进度条
    private static void Form_Schedule(int i)
    {
        form.Invoke(new Action(() =>
        {
            form.Form_Schedule(i);
        }));
    }

    public static void Stop()
    {
        Can_Run = (Can_Run) ? false : Can_Run;
    }
}