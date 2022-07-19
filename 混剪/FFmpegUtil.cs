using FFmpeg.AutoGen;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FFmpegUtil
{
    partial class FFmpegUtil
    {
        public 混剪.Form1 form;
        public FFmpegUtil(混剪.Form1 form)
        {
            this.form = form;
        }
        /// <summary>
        /// 显示图片委托
        /// </summary>
        /// <param name="bitmap"></param>
        public delegate void ShowBitmap(Bitmap bitmap);
        /// <summary>
        /// 执行控制变量
        /// </summary>
        bool CanRun;
        /// <summary>
        /// 对读取的264数据包进行解码和转换
        /// </summary>
        /// <param name="show">解码完成回调函数</param>
        /// <param name="url">播放地址，也可以是本地文件地址</param>
        public unsafe void Start()
        {
            form.debug("正在加载FFmpeg");
            CanRun = true;
            form.debug(@"Current directory: " + Environment.CurrentDirectory);
            form.debug(@"Runnung in {0}-bit mode."+ (Environment.Is64BitProcess ? @"64" : @"32"));
            #region ffmpeg 初始化
            // 初始化注册ffmpeg相关的编码器
            ffmpeg.avdevice_register_all();
            ffmpeg.avformat_network_init();

            form.debug($"FFmpeg version info: {ffmpeg.av_version_info()}");
            #endregion

            #region ffmpeg 日志
            // 设置记录ffmpeg日志级别
            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_VERBOSE);
            av_log_set_callback_callback logCallback = (p0, level, format, vl) =>
            {
                if (level > ffmpeg.av_log_get_level()) return;

                var lineSize = 1024;
                var lineBuffer = stackalloc byte[lineSize];
                var printPrefix = 1;
                ffmpeg.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);
                var line = Marshal.PtrToStringAnsi((IntPtr)lineBuffer);
                form.debug(line);
            };
            ffmpeg.av_log_set_callback(logCallback);
            #endregion

        }

        /// <summary>
        /// 获取ffmpeg错误信息
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        private static unsafe string GetErrorMessage(int error)
        {
            var bufferSize = 1024;
            var buffer = stackalloc byte[bufferSize];
            ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
            var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
            return message;
        }

        public void Stop()
        {
            CanRun = false;
        }
    }
}