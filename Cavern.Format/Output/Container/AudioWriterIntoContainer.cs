﻿using System;

using Cavern.Format.Common;

namespace Cavern.Format.Container {
    /// <summary>
    /// Writes the audio data as a new <see cref="track"/> into a <see cref="container"/>, amongst other tracks.
    /// </summary>
    public class AudioWriterIntoContainer : AudioWriter {
        /// <summary>
        /// Handles the container itself.
        /// </summary>
        readonly ContainerWriter container;

        /// <summary>
        /// The new track to encode.
        /// </summary>
        readonly RenderTrack track;

        /// <summary>
        /// Writes the audio data as a new <see cref="track"/> into a <see cref="container"/>, amongst other tracks.
        /// </summary>
        /// <param name="path">Output file name</param>
        /// <param name="tracks">Tracks to bring from other containers, can be empty</param>
        /// <param name="newTrack">The codec of the new audio track</param>
        /// <param name="blockSize">Total number of samples for all channels that will be encoded in each frame</param>
        /// <param name="channelCount">Number of output channels</param>
        /// <param name="length">Content length in samples for a single channel</param>
        /// <param name="sampleRate">Sample rate of the new audio track</param>
        /// <param name="bits">Bit rate of the new audio track if applicable</param>
        /// <exception cref="UnsupportedFormatException">The container format is either unknown by file extension or
        /// there was no file extension</exception>
        public AudioWriterIntoContainer(string path, Track[] tracks, Codec newTrack, int blockSize,
            int channelCount, long length, int sampleRate, BitDepth bits) : base(path, channelCount, length, sampleRate, bits) {
            int index = path.LastIndexOf('.') + 1;
            if (index == 0) {
                throw new UnsupportedFormatException();
            }

            Array.Resize(ref tracks, tracks.Length + 1);
            tracks[^1] = track = new RenderTrack(newTrack, blockSize, channelCount, sampleRate, bits);

            switch (path[index..]) {
                case "mkv":
                case "mka":
                case "webm":
                case "weba":
                    container = new MatroskaWriter(writer, tracks, length / (double)sampleRate);
                    break;
                default:
                    throw new UnsupportedFormatException();
            }
        }

        /// <summary>
        /// Create the file header.
        /// </summary>
        public override void WriteHeader() => container.WriteHeader();

        /// <summary>
        /// Write a block of mono or interlaced samples.
        /// </summary>
        /// <param name="samples">Samples to write</param>
        /// <param name="from">Start position in the input array (inclusive)</param>
        /// <param name="to">End position in the input array (exclusive)</param>
        public override void WriteBlock(float[] samples, long from, long to) {
            track.EncodeNextBlock(samples);
            container.WriteBlock(track.timeStep);
        }

        /// <summary>
        /// Dispose the stream writer through the <see cref="container"/> as it might want to write a footer.
        /// </summary>
        public override void Dispose() => container.Dispose();
    }
}