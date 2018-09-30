﻿using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Cavern {
    public partial class AudioSource3D : MonoBehaviour {
        /// <summary>Audio output writer delegate.</summary>
        /// <param name="Samples">Samples to write</param>
        /// <param name="Target">Channel array to write to</param>
        /// <param name="ChannelLength">Size of the source and destination arrays</param>
        /// <param name="Gain">Source gain</param>
        /// <param name="Channel">Channel ID</param>
        /// <param name="Channels">Total channels</param>
        internal delegate void OutputFunc(float[] Samples, float[] Target, int ChannelLength, float Gain, int Channel, int Channels);

        /// <summary>The audio output function to be used.</summary>
        internal static OutputFunc UsedOutputFunc;

        /// <summary>Output samples to a multichannel array.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteOutput(float[] Samples, float[] Target, int ChannelLength, float Gain, int Channel, int Channels) {
            for (int From = 0, To = Channel; From < ChannelLength; ++From, To += Channels)
                Target[To] += Samples[From] * Gain;
        }

        /// <summary>Output samples to a multichannel array with approximated constant power. The error margin is 15%.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteOutputApproxCP(float[] Samples, float[] Target, int ChannelLength, float Gain, int Channel, int Channels) {
            WriteOutput(Samples, Target, ChannelLength, Mathf.Sqrt(Gain), Channel, Channels);
        }

        /// <summary>Output samples to a multichannel array with constant power.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteOutputCP(float[] Samples, float[] Target, int ChannelLength, float Gain, int Channel, int Channels) {
            WriteOutput(Samples, Target, ChannelLength, Mathf.Sin(Mathf.PI / 2 * Gain), Channel, Channels);
        }

        /// <summary>Output samples to a multichannel array, while trying to fix standing waves.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteFixedOutput(float[] Samples, float[] Target, int ChannelLength, float Gain, int Channel, int Channels) {
            int FirstPassLength = ChannelLength;
            float OldMax = 0, NewMax = 0, AbsSample;
            for (int FromSample = 0, ToSample = Channel; FromSample < ChannelLength; ++FromSample, ToSample += Channels) {
                AbsSample = Math.Abs(Target[ToSample]);
                if (OldMax < AbsSample)
                    OldMax = AbsSample;
                Target[ToSample] += Samples[FromSample] * Gain;
                AbsSample = Math.Abs(Target[ToSample]);
                if (NewMax < AbsSample)
                    NewMax = AbsSample;
            }
            if (NewMax < OldMax)
                WriteOutput(Samples, Target, ChannelLength, Gain * -2, Channel, Channels);
        }

        /// <summary>Output samples to a multichannel array with approximated constant power, while trying to fix standing waves.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteFixedOutputApproxCP(float[] Samples, float[] Target, int ChannelLength, float Gain, int Channel, int Channels) {
            WriteFixedOutput(Samples, Target, ChannelLength, Mathf.Sqrt(Gain), Channel, Channels);
        }

        /// <summary>Output samples to a multichannel array with constant power, while trying to fix standing waves.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteFixedOutputCP(float[] Samples, float[] Target, int ChannelLength, float Gain, int Channel, int Channels) {
            WriteFixedOutput(Samples, Target, ChannelLength, Mathf.Sin(Mathf.PI / 2 * Gain), Channel, Channels);
        }
    }
}