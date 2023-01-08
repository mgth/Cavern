﻿using Cavern;
using Cavern.Remapping;

namespace CavernizeGUI.Elements {
    /// <summary>
    /// Standard rendering channel layouts.
    /// </summary>
    class RenderTarget {
        /// <summary>
        /// Layout name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// List of used channels.
        /// </summary>
        public ReferenceChannel[] Channels { get; }

        /// <summary>
        /// The <see cref="Channels"/> are used for rendering, but it could be rematrixed.
        /// This is the number of channels actually written to the file.
        /// </summary>
        public int OutputChannels { get; protected set; }

        /// <summary>
        /// Standard rendering channel layouts.
        /// </summary>
        public RenderTarget(string name, ReferenceChannel[] channels) {
            Name = name;
            Channels = channels;
            OutputChannels = channels.Length;
        }

        /// <summary>
        /// Apply this render target on the system's output.
        /// </summary>
        public virtual void Apply() {
            Channel[] systemChannels = new Channel[Channels.Length];
            for (int ch = 0; ch < Channels.Length; ch++) {
                bool lfe = Channels[ch] == ReferenceChannel.ScreenLFE;
                systemChannels[ch] = new Channel(ChannelPrototype.AlternativePositions[(int)Channels[ch]], lfe);
            }
            Listener.HeadphoneVirtualizer = false;
            Listener.ReplaceChannels(systemChannels);
        }

        /// <summary>
        /// Top rear channels are used as &quot;side&quot; channels as no true rears are available in standard mappings.
        /// These have to be mapped back to sides in some cases, for example, for the wiring popup.
        /// </summary>
        public ReferenceChannel[] GetNameMappedChannels() {
            ReferenceChannel[] result = (ReferenceChannel[])Channels.Clone();
            for (int i = 0; i < result.Length; i++) {
                if (result[i] == ReferenceChannel.TopRearLeft) {
                    result[i] = ReferenceChannel.TopSideLeft;
                }
                if (result[i] == ReferenceChannel.TopRearRight) {
                    result[i] = ReferenceChannel.TopSideRight;
                }
            }
            return result;
        }

        /// <summary>
        /// Return the <see cref="Name"/> on string conversion.
        /// </summary>
        override public string ToString() => Name;

        // These have to come before the Targets, otherwise they would be null.
        /// <summary>
        /// Channels of the 5.1.4 layout used for both 5.1.4 and rendering before downmixing to 5.1.2 front.
        /// </summary>
        static readonly ReferenceChannel[] layout514 = {
            ReferenceChannel.FrontLeft, ReferenceChannel.FrontRight, ReferenceChannel.FrontCenter, ReferenceChannel.ScreenLFE,
            ReferenceChannel.SideLeft, ReferenceChannel.SideRight, ReferenceChannel.TopFrontLeft, ReferenceChannel.TopFrontRight,
            ReferenceChannel.TopRearLeft, ReferenceChannel.TopRearRight
        };

        /// <summary>
        /// Channels of the 9.1.4 layout used for both 9.1.4 and rendering before downmixing to 9.1.2 front.
        /// </summary>
        static readonly ReferenceChannel[] layout914 = {
            ReferenceChannel.FrontLeft, ReferenceChannel.FrontRight, ReferenceChannel.FrontCenter, ReferenceChannel.ScreenLFE,
            ReferenceChannel.RearLeft, ReferenceChannel.RearRight, ReferenceChannel.SideLeft, ReferenceChannel.SideRight,
            ReferenceChannel.WideLeft, ReferenceChannel.WideRight, ReferenceChannel.TopFrontLeft, ReferenceChannel.TopFrontRight,
            ReferenceChannel.TopRearLeft, ReferenceChannel.TopRearRight
        };

        /// <summary>
        /// Default render targets.
        /// </summary>
        /// <remarks>Top rears are used instead of sides for smooth height transitions and WAVEFORMATEXTENSIBLE support.</remarks>
        public static readonly RenderTarget[] Targets = {
            new RenderTarget("5.1 side", ChannelPrototype.GetStandardMatrix(6)),
            new RenderTarget("5.1 rear", new[] {
                ReferenceChannel.FrontLeft, ReferenceChannel.FrontRight, ReferenceChannel.FrontCenter, ReferenceChannel.ScreenLFE,
                ReferenceChannel.RearLeft, ReferenceChannel.RearRight
            }),
            new DownmixedRenderTarget("5.1.2 front", layout514, (8, 4), (9, 5)),
            new RenderTarget("5.1.2 side", new[] {
                ReferenceChannel.FrontLeft, ReferenceChannel.FrontRight, ReferenceChannel.FrontCenter,  ReferenceChannel.ScreenLFE,
                ReferenceChannel.SideLeft, ReferenceChannel.SideRight, ReferenceChannel.TopRearLeft, ReferenceChannel.TopRearRight
            }),
            new RenderTarget("5.1.4", layout514),
            new RenderTarget("5.1.6", new[] {
                ReferenceChannel.FrontLeft, ReferenceChannel.FrontRight, ReferenceChannel.FrontCenter, ReferenceChannel.ScreenLFE,
                ReferenceChannel.SideLeft, ReferenceChannel.SideRight,
                ReferenceChannel.TopFrontLeft, ReferenceChannel.TopFrontCenter, ReferenceChannel.TopFrontRight,
                ReferenceChannel.TopRearLeft, ReferenceChannel.TopRearCenter, ReferenceChannel.TopRearRight
            }),
            new RenderTarget("7.1", ChannelPrototype.GetStandardMatrix(8)),
            new DownmixedRenderTarget("7.1.2 front", ChannelPrototype.GetStandardMatrix(12), (10, 4), (11, 5)),
            new RenderTarget("7.1.2 side", new[] {
                ReferenceChannel.FrontLeft, ReferenceChannel.FrontRight, ReferenceChannel.FrontCenter, ReferenceChannel.ScreenLFE,
                ReferenceChannel.RearLeft, ReferenceChannel.RearRight, ReferenceChannel.SideLeft, ReferenceChannel.SideRight,
                ReferenceChannel.TopRearLeft, ReferenceChannel.TopRearRight
            }),
            new RenderTarget("7.1.4", ChannelPrototype.GetStandardMatrix(12)),
            new RenderTarget("7.1.6", ChannelPrototype.GetStandardMatrix(14)),
            new RenderTarget("9.1", new[] {
                ReferenceChannel.FrontLeft, ReferenceChannel.FrontRight, ReferenceChannel.FrontCenter, ReferenceChannel.ScreenLFE,
                ReferenceChannel.RearLeft, ReferenceChannel.RearRight, ReferenceChannel.SideLeft, ReferenceChannel.SideRight,
                ReferenceChannel.WideLeft, ReferenceChannel.WideRight
            }),
            new DownmixedRenderTarget("9.1.2 front", layout914, (12, 4), (13, 5)),
            new RenderTarget("9.1.2 side", new[] {
                ReferenceChannel.FrontLeft, ReferenceChannel.FrontRight, ReferenceChannel.FrontCenter, ReferenceChannel.ScreenLFE,
                ReferenceChannel.RearLeft, ReferenceChannel.RearRight, ReferenceChannel.SideLeft, ReferenceChannel.SideRight,
                ReferenceChannel.WideLeft, ReferenceChannel.WideRight, ReferenceChannel.TopRearLeft, ReferenceChannel.TopRearRight
            }),
            new RenderTarget("9.1.4", layout914),
            new RenderTarget("9.1.6", ChannelPrototype.GetStandardMatrix(16)),
            new DriverRenderTarget(),
            new VirtualizerRenderTarget()
        };
    }
}