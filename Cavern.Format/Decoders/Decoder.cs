﻿using Cavern.Format.Utilities;

namespace Cavern.Format.Decoders {
    /// <summary>
    /// Converts a bitstream to raw samples.
    /// </summary>
    public abstract class Decoder {
        /// <summary>
        /// Content channel count.
        /// </summary>
        public abstract int ChannelCount { get; }

        /// <summary>
        /// Content length in samples for a single channel.
        /// </summary>
        public abstract long Length { get; }

        /// <summary>
        /// Bitstream sample rate.
        /// </summary>
        public abstract int SampleRate { get; }

        /// <summary>
        /// Stream reader and block regrouping object.
        /// </summary>
        protected BlockBuffer<byte> reader;

        /// <summary>
        /// Converts a bitstream to raw samples.
        /// </summary>
        public Decoder(BlockBuffer<byte> reader) => this.reader = reader;

        /// <summary>
        /// Gives the possibility of setting <see cref="reader"/> after a derived constructor has read a header.
        /// </summary>
        /// <remarks>Not setting <see cref="reader"/> in all constructors can break a decoder.</remarks>
        protected Decoder() { }

        /// <summary>
        /// Read and decode a given number of samples.
        /// </summary>
        /// <param name="target">Array to decode data into</param>
        /// <param name="from">Start position in the input array (inclusive)</param>
        /// <param name="to">End position in the input array (exclusive)</param>
        /// <remarks>The next to - from samples will be read from the file.
        /// All samples are counted, not just a single channel.</remarks>
        public abstract void DecodeBlock(float[] target, long from, long to);

        /// <summary>
        /// Start the following reads from the selected sample.
        /// </summary>
        public abstract void Seek(long sample);
    }
}