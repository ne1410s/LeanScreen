using System;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace Av.Rendering.Ffmpeg
{
    internal static class FfmpegUtils
    {
        public static unsafe string av_strerror(int error)
        {
            const int bufferSize = 1024;
            var buffer = stackalloc byte[bufferSize];
            ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
            return Marshal.PtrToStringAnsi((IntPtr)buffer);
        }

        public static TimeSpan Clamp(this TimeSpan input, TimeSpan duration)
        {
            return input < TimeSpan.Zero ? TimeSpan.Zero
                : input > duration ? duration
                : input;
        }

        public static byte[] ToBytes(this IntPtr input, int ptrLen)
        {
            var retVal = new byte[ptrLen];
            Marshal.Copy(input, retVal, 0, ptrLen);
            return retVal;
        }

        public static TimeSpan ToTimeSpan(this long pts, AVRational timeBase)
        {
            var ptsd = (double)pts;
            if (double.IsNaN(ptsd) || Math.Abs(ptsd - ffmpeg.AV_NOPTS_VALUE) <= double.Epsilon)
            {
                return TimeSpan.MinValue;
            }

            return TimeSpan.FromTicks(timeBase.den == 0 ?
                Convert.ToInt64(TimeSpan.TicksPerSecond * ptsd / ffmpeg.AV_TIME_BASE) :
                Convert.ToInt64(TimeSpan.TicksPerSecond * ptsd * timeBase.num / timeBase.den));
        }

        public static TimeSpan ToTimeSpan(this long pts, double timeBase)
        {
            var ptsd = (double)pts;
            if (double.IsNaN(ptsd) || Math.Abs(ptsd - ffmpeg.AV_NOPTS_VALUE) <= double.Epsilon)
            {
                return TimeSpan.MinValue;
            }

            return TimeSpan.FromTicks(
                Convert.ToInt64(TimeSpan.TicksPerSecond * ptsd / timeBase));
        }

        public static long ToLong(this TimeSpan ts, AVRational timeBase)
        {
            return Convert.ToInt64(ts.TotalSeconds * timeBase.den / timeBase.num);
        }

        public static int ThrowExceptionIfError(this int error)
        {
            if (error < 0) throw new ApplicationException(av_strerror(error));
            return error;
        }

        public static unsafe void SetupBinaries()
        {
            ffmpeg.RootPath
                = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/lib/x86_64-linux-gnu"
                : RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg"
                : throw new InvalidOperationException("Windows and Linux only");
        }

        public static unsafe void SetupLogging()
        {
            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_VERBOSE);

            // do not convert to local function
            av_log_set_callback_callback logCallback = (p0, level, format, vl) =>
            {
                if (level > ffmpeg.av_log_get_level()) return;

                var lineSize = 1024;
                var lineBuffer = stackalloc byte[lineSize];
                var printPrefix = 1;
                ffmpeg.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);
                var line = Marshal.PtrToStringAnsi((IntPtr)lineBuffer);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(line);
                Console.ResetColor();
            };

            ffmpeg.av_log_set_callback(logCallback);
        }
    }
}
