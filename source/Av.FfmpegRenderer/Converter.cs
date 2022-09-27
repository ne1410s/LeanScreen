using System;
using System.Runtime.InteropServices;
using Av.Abstractions.Rendering;
using Av.Abstractions.Shared;
using FFmpeg.AutoGen;

namespace Av.Rendering.Ffmpeg
{
    internal sealed unsafe class Converter : IDisposable
    {
        private const AVPixelFormat DestinationPixelFormat = AVPixelFormat.AV_PIX_FMT_RGB24;

        private readonly IntPtr _convertedFrameBufferPtr;
        private readonly Dimensions2D _destinationSize;
        private readonly byte_ptrArray4 _dstData;
        private readonly int_array4 _dstLinesize;
        private readonly SwsContext* _pConvertContext;
        private readonly int _destinationBufferLength;
        private readonly AVRational _srcTimeBase;

        public Converter(Dimensions2D sourceSize, AVPixelFormat sourcePixelFormat, AVRational sourceTimeBase, Dimensions2D destinationSize)
        {
            _destinationSize = destinationSize;
            _srcTimeBase = sourceTimeBase;

            _pConvertContext = ffmpeg.sws_getContext(sourceSize.Width,
                sourceSize.Height,
                sourcePixelFormat,
                destinationSize.Width,
                destinationSize.Height,
                DestinationPixelFormat,
                ffmpeg.SWS_FAST_BILINEAR,
                null,
                null,
                null);
            if (_pConvertContext == null)
                throw new ApplicationException("Could not initialize the conversion context.");

            _destinationBufferLength = ffmpeg.av_image_get_buffer_size(
                DestinationPixelFormat,
                destinationSize.Width,
                destinationSize.Height,
                1);

            _convertedFrameBufferPtr = Marshal.AllocHGlobal(_destinationBufferLength);
            _dstData = new byte_ptrArray4();
            _dstLinesize = new int_array4();

            ffmpeg.av_image_fill_arrays(ref _dstData,
                ref _dstLinesize,
                (byte*)_convertedFrameBufferPtr,
                DestinationPixelFormat,
                destinationSize.Width,
                destinationSize.Height,
                1);
        }

        public int DestinationBufferLength { get; }

        public void Dispose()
        {
            Marshal.FreeHGlobal(_convertedFrameBufferPtr);
            ffmpeg.sws_freeContext(_pConvertContext);
        }

        public RenderedFrame Render(AVFrame sourceFrame)
        {
            ffmpeg.sws_scale(_pConvertContext,
                sourceFrame.data,
                sourceFrame.linesize,
                0,
                sourceFrame.height,
                _dstData,
                _dstLinesize);

            var data = new byte_ptrArray8();
            data.UpdateFrom(_dstData);
            var linesize = new int_array8();
            linesize.UpdateFrom(_dstLinesize);

            var imageBytes = ((IntPtr)data[0]).ToBytes(_destinationBufferLength);

            return new RenderedFrame
            {
                Rgb24Bytes = imageBytes,
                Dimensions = _destinationSize,
                FrameNumber = sourceFrame.coded_picture_number,
                Position = sourceFrame.best_effort_timestamp.ToTimeSpan(_srcTimeBase),
            };
        }
    }
}
