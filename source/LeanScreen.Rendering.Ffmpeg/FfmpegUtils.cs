// <copyright file="FfmpegUtils.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg;

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
    /// Gets or sets the logger.
    /// </summary>
    public static Action<int, string>? Logger { get; set; }

    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    public static int LogLevel { get; set; } = ffmpeg.AV_LOG_WARNING;

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
        var boundedSeconds = Math.Max(0, Math.Min(duration.TotalSeconds, input.TotalSeconds));
        return TimeSpan.FromSeconds(boundedSeconds);
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
    public static TimeSpan ToTimeSpan(this double pts, AVRational timeBase)
    {
        if ((int)Math.Round(Math.Abs(pts - ffmpeg.AV_NOPTS_VALUE), 10) == 0)
        {
            return TimeSpan.Zero;
        }

        return TimeSpan.FromTicks(timeBase.den == 0 ?
            Convert.ToInt64(TimeSpan.TicksPerSecond * pts / ffmpeg.AV_TIME_BASE) :
            Convert.ToInt64(TimeSpan.TicksPerSecond * pts * timeBase.num / timeBase.den));
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
    [SuppressMessage("StyleCop.*", "SA1300:*", Justification = "Conform to mutation rules ignore glob")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Conform to mutation rules ignore glob")]
    public static int avThrowIfError(this int status) => status >= 0
        ? status
        : throw new InvalidOperationException(AvStrError(status));

    /// <summary>
    /// Sets binaries path.
    /// </summary>
    public static unsafe void SetBinariesPath()
    {
        var path = Environment.GetEnvironmentVariable("FFMPEG_BINARIES_PATH");
        ffmpeg.RootPath = path ?? "ffmpeg";
    }

    /// <summary>
    /// Sets up logging.
    /// </summary>
    public static unsafe void SetupLogging()
    {
        ffmpeg.av_log_set_level(ffmpeg.AV_LOG_INFO);
        av_log_set_callback_callback logCallback = (p0, level, format, vl) =>
        {
            if (Logger != null && level <= LogLevel)
            {
                const int lineSize = 1024;
                var lineBuffer = stackalloc byte[lineSize];
                var printPrefix = 1;
                ffmpeg.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);
                var line = Marshal.PtrToStringAnsi((IntPtr)lineBuffer);
                try
                {
                    Logger.Invoke(level, line);
                }
                catch
                {
                    // Well we can't exactly log this...
                }
            }
        };

        ffmpeg.av_log_set_callback(logCallback);
    }
}
