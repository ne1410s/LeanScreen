// <copyright file="FfmpegUtils.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering.Ffmpeg
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using FFmpeg.AutoGen;

    /// <summary>
    /// Ffmpeg utilities.
    /// </summary>
    public static class FfmpegUtils
    {
        /// <summary>
        /// Gets the error message from a code.
        /// </summary>
        /// <param name="error">The error code.</param>
        /// <returns>The error message.</returns>
        public static unsafe string AvStrError(int error)
        {
            const int bufferSize = 1024;
            var buffer = stackalloc byte[bufferSize];
            ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
            return Marshal.PtrToStringAnsi((IntPtr)buffer);
        }

        /// <summary>
        /// Constricts a timespan to a duration.
        /// </summary>
        /// <param name="input">The position value.</param>
        /// <param name="duration">A value between zero and the total duration,
        /// inclusive.</param>
        /// <returns>The bounded result.</returns>
        public static TimeSpan Clamp(this TimeSpan input, TimeSpan duration)
        {
            return input < TimeSpan.Zero ? TimeSpan.Zero
                : input > duration ? duration
                : input;
        }

        /// <summary>
        /// Reads a byte array pointer to bytes.
        /// </summary>
        /// <param name="input">The byte array pointer.</param>
        /// <param name="ptrLen">The byte array length.</param>
        /// <returns>The array.</returns>
        public static byte[] ToBytes(this IntPtr input, int ptrLen)
        {
            var retVal = new byte[ptrLen];
            Marshal.Copy(input, retVal, 0, ptrLen);
            return retVal;
        }

        /// <summary>
        /// Maps a presentation to a timespan.
        /// </summary>
        /// <param name="pts">The presentation time.</param>
        /// <param name="timeBase">The timebase.</param>
        /// <returns>A timespan.</returns>
        public static TimeSpan ToTimeSpan(this long pts, AVRational timeBase)
        {
            var ptsd = (double)pts;
            if (Math.Abs(ptsd - ffmpeg.AV_NOPTS_VALUE) <= double.Epsilon)
            {
                return TimeSpan.MinValue;
            }

            return TimeSpan.FromTicks(timeBase.den == 0 ?
                Convert.ToInt64(TimeSpan.TicksPerSecond * ptsd / ffmpeg.AV_TIME_BASE) :
                Convert.ToInt64(TimeSpan.TicksPerSecond * ptsd * timeBase.num / timeBase.den));
        }

        /// <summary>
        /// Maps a presentation to a timespan.
        /// </summary>
        /// <param name="pts">The presentation time.</param>
        /// <param name="timeBase">The timebase.</param>
        /// <returns>A timespan.</returns>
        public static TimeSpan ToTimeSpan(this long pts, double timeBase)
        {
            var ptsd = (double)pts;
            if (Math.Abs(ptsd - ffmpeg.AV_NOPTS_VALUE) <= double.Epsilon)
            {
                return TimeSpan.MinValue;
            }

            return TimeSpan.FromTicks(
                Convert.ToInt64(TimeSpan.TicksPerSecond * ptsd / timeBase));
        }

        /// <summary>
        /// Maps a timespan to a presentation time.
        /// </summary>
        /// <param name="ts">The timespan.</param>
        /// <param name="timeBase">The timebase.</param>
        /// <returns>The presentation time.</returns>
        public static long ToLong(this TimeSpan ts, AVRational timeBase)
        {
            return Convert.ToInt64(ts.TotalSeconds * timeBase.den / timeBase.num);
        }

        /// <summary>
        /// Throws an error if an error status code is indeed found.
        /// </summary>
        /// <param name="status">The potential error code.</param>
        /// <returns>The code, if non-error.</returns>
        /// <exception cref="InvalidOperationException">Error.</exception>
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Mutation ignore glob")]
        public static int avThrowIfError(this int status) => status >= 0
            ? status
            : throw new InvalidOperationException(AvStrError(status));

        /// <summary>
        /// Sets up binaries, according to the operating system detected (or
        /// override value).
        /// </summary>
        /// <param name="isWindows">True if windows.</param>
        public static unsafe void SetupBinaries(bool? isWindows = null)
        {
            var isWin = isWindows ?? RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            ffmpeg.RootPath = isWin ? "ffmpeg" : "/lib/x86_64-linux-gnu";
        }

        /// <summary>
        /// Sets up logging.
        /// </summary>
        public static unsafe void SetupLogging()
        {
            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_VERBOSE);

            // do not convert to local function
            av_log_set_callback_callback logCallback = (p0, level, format, vl) =>
            {
                if (level > ffmpeg.av_log_get_level())
                {
                    return;
                }

                const int lineSize = 1024;
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
