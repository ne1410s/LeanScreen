// <copyright file="FfmpegConverter.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg;

using System;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using LeanScreen.Common;

/// <summary>
/// Ffmpeg frame converter.
/// </summary>
public sealed unsafe class FfmpegConverter : IDisposable
{
    private const AVPixelFormat DestinationPixelFormat = AVPixelFormat.AV_PIX_FMT_RGB24;

    private readonly byte_ptrArray4 dstData;
    private readonly int_array4 dstLinesize;
    private readonly int destinationBufferLength;
    private readonly SwsContext* pConvertContext;
    private IntPtr convertedFrameBufferPtr;

    /// <summary>
    /// Initializes a new instance of the <see cref="FfmpegConverter"/> class.
    /// </summary>
    /// <param name="sourceSize">The source size.</param>
    /// <param name="sourcePixelFormat">The source pixel format.</param>
    /// <param name="destinationSize">The destination size.</param>
    /// <exception cref="ArgumentException">Invalid data.</exception>
    public FfmpegConverter(Size2D sourceSize, AVPixelFormat sourcePixelFormat, Size2D destinationSize)
    {
        AssertValid(sourceSize, nameof(sourceSize));
        AssertValid(destinationSize, nameof(destinationSize));

        this.pConvertContext = ffmpeg.sws_getContext(
            sourceSize.Width,
            sourceSize.Height,
            sourcePixelFormat,
            destinationSize.Width,
            destinationSize.Height,
            DestinationPixelFormat,
            ffmpeg.SWS_FAST_BILINEAR,
            null,
            null,
            null);

        this.destinationBufferLength = ffmpeg.av_image_get_buffer_size(
            DestinationPixelFormat,
            destinationSize.Width,
            destinationSize.Height,
            1);

        this.convertedFrameBufferPtr = Marshal.AllocHGlobal(this.destinationBufferLength);
        this.dstData = default;
        this.dstLinesize = default;

        ffmpeg.av_image_fill_arrays(
            ref this.dstData,
            ref this.dstLinesize,
            (byte*)this.convertedFrameBufferPtr,
            DestinationPixelFormat,
            destinationSize.Width,
            destinationSize.Height,
            1);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Stryker disable all
        Marshal.FreeHGlobal(this.convertedFrameBufferPtr);
        ffmpeg.sws_freeContext(this.pConvertContext);
        this.convertedFrameBufferPtr = IntPtr.Zero;

        // Stryker restore all
    }

    /// <summary>
    /// Renders a raw frame.
    /// </summary>
    /// <param name="sourceFrame">The source frame.</param>
    /// <returns>Frame data.</returns>
    public RawFrame RenderRawFrame(AVFrame sourceFrame)
    {
        ffmpeg.sws_scale(
            this.pConvertContext,
            sourceFrame.data,
            sourceFrame.linesize,
            0,
            sourceFrame.height,
            this.dstData,
            this.dstLinesize);

        var data = default(byte_ptrArray8);
        data.UpdateFrom(this.dstData);

        // Removed: survives stryker mutation, despite resize hash-tests...?
        ////var linesize = default(int_array8);
        ////linesize.UpdateFrom(this.dstLinesize);

        var imageBytes = ((IntPtr)data[0]).ToBytes(this.destinationBufferLength);
        return new RawFrame
        {
            Rgb24Bytes = imageBytes,
            PresentationTime = sourceFrame.best_effort_timestamp,
        };
    }

    /// <summary>
    /// Throws an exception if the size is not valid.
    /// </summary>
    /// <param name="size">The size to check.</param>
    /// <param name="paramName">The original parameter name.</param>
    /// <exception cref="ArgumentException">Invalid data.</exception>
    private static void AssertValid(Size2D size, string paramName)
    {
        if (size.Width <= 0 || size.Height <= 0)
        {
            throw new ArgumentException("The size is invalid.", paramName);
        }
    }
}
