﻿using Cavern.Filters.Utilities;

namespace Cavern.Format.ConfigurationFile {
    /// <summary>
    /// Full parsed setup of a freely configurable system-wide equalizer or audio processor software.
    /// </summary>
    public abstract class ConfigurationFile {
        /// <summary>
        /// Root nodes of each channel, start attaching their filters as a children chain.
        /// </summary>
        public (string name, FilterGraphNode root)[] InputChannels { get; }

        /// <summary>
        /// Create an empty configuration file with the passed input channel names/labels.
        /// </summary>
        public ConfigurationFile(string[] inputs) {
            InputChannels = new (string name, FilterGraphNode root)[inputs.Length];
            for (int i = 0; i < inputs.Length; i++) {
                InputChannels[i] = (inputs[i], new FilterGraphNode(null));
            }
        }
    }
}