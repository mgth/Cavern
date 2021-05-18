﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Cavern.Utilities;

namespace Cavern.QuickEQ.Utilities {
    /// <summary>Drastically faster versions of some functions written in C++.</summary>
    /// <remarks>Use alongside <see cref="CavernAmp"/>!</remarks>
    internal static class CavernQuickEQAmp {
        /// <summary>FFT cache constructor.</summary>
        [DllImport("CavernAmp.dll", EntryPoint = "FFTCache_Create")]
        internal static extern IntPtr FFTCache_Create(int size);

        /// <summary>Get the creation size of the FFT cache.</summary>
        [DllImport("CavernAmp.dll", EntryPoint = "FFTCache_Size")]
        internal static extern int FFTCache_Size(IntPtr cache);

        /// <summary>Dispose an FFT cache.</summary>
        [DllImport("CavernAmp.dll", EntryPoint = "FFTCache_Dispose")]
        internal static extern void FFTCache_Dispose(IntPtr cache);

        /// <summary>Actual FFT processing, somewhat in-place.</summary>
        [DllImport("CavernAmp.dll", EntryPoint = "ProcessFFT")]
        internal static extern void ProcessFFT(Complex[] samples, int sampleCount, IntPtr cache, int depth);

        /// <summary>Fourier-transform a signal in 1D. The result is the spectral power.</summary>
        [DllImport("CavernAmp.dll", EntryPoint = "ProcessFFT1D")]
        internal static extern void ProcessFFT(float[] samples, int sampleCount, IntPtr cache);

        /// <summary>Fast Fourier transform a 2D signal while keeping the source array allocation.</summary>
        [DllImport("CavernAmp.dll", EntryPoint = "InPlaceFFT")]
        static extern void InPlaceFFTCall(Complex[] samples, int sampleCount, IntPtr cache);

        /// <summary>Fast Fourier transform a 2D signal while keeping the source array allocation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InPlaceFFT(Complex[] samples, FFTCache cache = null) {
            if (cache == null)
                InPlaceFFTCall(samples, samples.Length, new IntPtr(0));
            else
                InPlaceFFTCall(samples, samples.Length, cache.Native);
        }

        /// <summary>Spectrum of a signal's FFT while keeping the source array allocation.</summary>
        [DllImport("CavernAmp.dll", EntryPoint = "InPlaceFFT1D")]
        static extern void InPlaceFFTCall(float[] samples, int sampleCount, IntPtr cache);

        /// <summary>Spectrum of a signal's FFT while keeping the source array allocation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InPlaceFFT(float[] samples, FFTCache cache = null) {
            if (cache == null)
                InPlaceFFTCall(samples, samples.Length, new IntPtr(0));
            else
                InPlaceFFTCall(samples, samples.Length, cache.Native);
        }

        /// <summary>Outputs IFFT(X) * N.</summary>
        [DllImport("CavernAmp.dll", EntryPoint = "ProcessIFFT")]
        internal static extern void ProcessIFFT(Complex[] samples, int sampleCount, IntPtr cache, int depth);

        /// <summary>Inverse Fast Fourier Transform of a transformed signal, while keeping the source array allocation.</summary>
        [DllImport("CavernAmp.dll", EntryPoint = "InPlaceIFFT")]
        static extern void InPlaceIFFTCall(Complex[] samples, int sampleCount, IntPtr cache);

        /// <summary>Inverse Fast Fourier Transform of a transformed signal, while keeping the source array allocation.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InPlaceIFFT(Complex[] samples, FFTCache cache = null) {
            if (cache == null)
                InPlaceIFFTCall(samples, samples.Length, new IntPtr(0));
            else
                InPlaceIFFTCall(samples, samples.Length, cache.Native);
        }
    }
}