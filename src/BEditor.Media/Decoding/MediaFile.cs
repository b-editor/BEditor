﻿using System;
using System.IO;
using System.Linq;

namespace BEditor.Media.Decoding
{
    /// <summary>
    /// Represents a multimedia file.
    /// </summary>
    public sealed class MediaFile : IDisposable
    {
        private readonly IInputContainer _container;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaFile"/> class.
        /// </summary>
        /// <param name="container"></param>
        public MediaFile(IInputContainer container)
        {
            _container = container;

            VideoStreams = container.Streams.OfType<IVideoStream>().ToArray();
            AudioStreams = container.Streams.OfType<IAudioStream>().ToArray();

            Info = container.Info;
        }

        /// <summary>
        /// Gets all the video streams in the media file.
        /// </summary>
        public IVideoStream[] VideoStreams { get; }

        /// <summary>
        /// Gets the first video stream in the media file.
        /// </summary>
        public IVideoStream? Video => VideoStreams.FirstOrDefault();

        /// <summary>
        /// Gets a value indicating whether the file contains video streams.
        /// </summary>
        public bool HasVideo => VideoStreams.Length > 0;

        /// <summary>
        /// Gets all the audio streams in the media file.
        /// </summary>
        public IAudioStream[] AudioStreams { get; }

        /// <summary>
        /// Gets the first audio stream in the media file.
        /// </summary>
        public IAudioStream? Audio => AudioStreams.FirstOrDefault();

        /// <summary>
        /// Gets a value indicating whether the file contains video streams.
        /// </summary>
        public bool HasAudio => AudioStreams.Length > 0;

        /// <summary>
        /// Gets informations about the media container.
        /// </summary>
        public MediaInfo Info { get; }

        /// <summary>
        /// Opens a media file from the specified path with default settings.
        /// </summary>
        /// <param name="path">A path to the media file.</param>
        /// <returns>The opened <see cref="MediaFile"/>.</returns>
        public static MediaFile Open(string path)
        {
            return Open(path, new MediaOptions());
        }

        /// <summary>
        /// Opens a media file from the specified path.
        /// </summary>
        /// <param name="path">A path to the media file.</param>
        /// <param name="options">The decoder settings.</param>
        /// <returns>The opened <see cref="MediaFile"/>.</returns>
        public static MediaFile Open(string path, MediaOptions options)
        {
            try
            {
                return DecoderFactory.Open(path, options) ?? throw new NotSupportedException("Not supported format.");
            }
            catch (DirectoryNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to open the media file", ex);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            var video = VideoStreams.Cast<IMediaStream>();
            var audio = AudioStreams.Cast<IMediaStream>();

            foreach (var stream in video.Concat(audio))
            {
                stream.Dispose();
            }

            _container.Dispose();
            GC.SuppressFinalize(this);
            _isDisposed = true;
        }
    }
}