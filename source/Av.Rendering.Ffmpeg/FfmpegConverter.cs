// <copyright file="FfmpegConverter.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering.Ffmpeg
{
    using System;
    using System.Runtime.InteropServices;
    using Av.Abstractions.Shared;
    using FFmpeg.AutoGen;

    /// <summary>
    /// Ffmpeg frame converter.
    /// </summary>
    public sealed unsafe class FfmpegConverter : IDisposable
    {
        private const AVPixelFormat DestinationPixelFormat = AVPixelFormat.AV_PIX_FMT_RGB24;

        private readonly IntPtr _convertedFrameBufferPtr;
        private readonly byte_ptrArray4 _dstData;
        private readonly int_array4 _dstLinesize;
        private readonly SwsContext* _pConvertContext;
        private readonly int _destinationBufferLength;

        /// <summary>
        /// Initialises a new <see cref="FfmpegConverter"/>.
        /// </summary>
        /// <param name="sourceSize">The source size.</param>
        /// <param name="sourcePixelFormat">The source pixel format.</param>
        /// <param name="destinationSize">The destination size.</param>
        /// <exception cref="ArgumentException"></exception>
        public FfmpegConverter(Dimensions2D sourceSize, AVPixelFormat sourcePixelFormat, Dimensions2D destinationSize)
        {
            AssertValid(sourceSize, nameof(sourceSize));
            AssertValid(destinationSize, nameof(destinationSize));

            this._pConvertContext = ffmpeg.sws_getContext(
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

            this._destinationBufferLength = ffmpeg.av_image_get_buffer_size(
                DestinationPixelFormat,
                destinationSize.Width,
                destinationSize.Height,
                1);

            this._convertedFrameBufferPtr = Marshal.AllocHGlobal(this._destinationBufferLength);
            this._dstData = new byte_ptrArray4();
            this._dstLinesize = new int_array4();

            ffmpeg.av_image_fill_arrays(ref this._dstData,
                ref this._dstLinesize,
                (byte*)this._convertedFrameBufferPtr,
                DestinationPixelFormat,
                destinationSize.Width,
                destinationSize.Height,
                1);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Marshal.FreeHGlobal(this._convertedFrameBufferPtr);
            ffmpeg.sws_freeContext(this._pConvertContext);
        }

        /// <summary>
        /// Throws an exception if the size is not valid.
        /// </summary>
        /// <param name="size">The size to check.</param>
        /// <param name="paramName">The original parameter name.</param>
        /// <exception cref="ArgumentException">Invalid data.</exception>
        private static void AssertValid(Dimensions2D size, string paramName)
        {
            if (size.Width <= 0 || size.Height <= 0)
            {
                throw new ArgumentException("The size is invalid.", paramName);
            }
        }

        /// <summary>
        /// Renders a raw frame.
        /// </summary>
        /// <param name="sourceFrame">The source frame.</param>
        /// <returns>Frame data.</returns>
        internal RawFrame RenderRawFrame(AVFrame sourceFrame)
        {
            ffmpeg.sws_scale(this._pConvertContext,
                sourceFrame.data,
                sourceFrame.linesize,
                0,
                sourceFrame.height,
                this._dstData,
                this._dstLinesize);

            var data = new byte_ptrArray8();
            data.UpdateFrom(this._dstData);
            var linesize = new int_array8();
            linesize.UpdateFrom(this._dstLinesize);

            var imageBytes = ((IntPtr)data[0]).ToBytes(this._destinationBufferLength);
            return new RawFrame
            {
                Rgb24Bytes = imageBytes,
                PresentationTime = sourceFrame.best_effort_timestamp,
            };
        }
    }
}
