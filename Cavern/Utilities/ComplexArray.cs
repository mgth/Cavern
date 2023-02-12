﻿namespace Cavern.Utilities {
    /// <summary>
    /// Operations on complex arrays.
    /// </summary>
    public static class ComplexArray {
        /// <summary>
        /// Convert all elements in the <paramref name="source"/> to their conjugates.
        /// </summary>
        public static void Conjugate(this Complex[] source) {
            for (int i = 0; i < source.Length; i++) {
                source[i].Imaginary = -source[i].Imaginary;
            }
        }

        /// <summary>
        /// Replace the <paramref name="source"/> with its convolution with an <paramref name="other"/> array.
        /// </summary>
        public static void Convolve(this Complex[] source, Complex[] other) {
            for (int i = 0; i < source.Length; ++i) {
                float oldReal = source[i].Real;
                source[i].Real = source[i].Real * other[i].Real - source[i].Imaginary * other[i].Imaginary;
                source[i].Imaginary = oldReal * other[i].Imaginary + source[i].Imaginary * other[i].Real;
            }
        }

        /// <summary>
        /// Replace the <paramref name="source"/> with its deconvolution with an <paramref name="other"/> array.
        /// </summary>
        public static void Deconvolve(this Complex[] source, Complex[] other) {
            for (int i = 0; i < source.Length; ++i) {
                float multiplier = 1 / (other[i].Real * other[i].Real + other[i].Imaginary * other[i].Imaginary),
                    oldReal = source[i].Real;
                source[i].Real = (source[i].Real * other[i].Real + source[i].Imaginary * other[i].Imaginary) * multiplier;
                source[i].Imaginary = (source[i].Imaginary * other[i].Real - oldReal * other[i].Imaginary) * multiplier;
            }
        }

        /// <summary>
        /// Convert a float array to complex a size that's ready for FFT.
        /// </summary>
        public static Complex[] ParseForFFT(this float[] source) {
            Complex[] result = new Complex[QMath.Base2Ceil(source.Length)];
            for (int i = 0; i < source.Length; ++i) {
                result[i].Real = source[i];
            }
            return result;
        }

        /// <summary>
        /// Move the waveform to a complex array before it's Fourier-transformed.
        /// </summary>
        /// <remarks>This function clears the imaginary part, allowing the use of reusable arrays.</remarks>
        public static void ParseForFFT(this float[] source, Complex[] target) {
            for (int i = 0; i < source.Length; ++i) {
                target[i] = new Complex(source[i]);
            }
        }
    }
}