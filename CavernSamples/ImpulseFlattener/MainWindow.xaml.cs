﻿using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

using Cavern.Filters;
using Cavern.Format;
using Cavern.QuickEQ.Equalization;
using Cavern.Remapping;
using Cavern.Utilities;

using Window = System.Windows.Window;

namespace ImpulseFlattener {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        readonly OpenFileDialog browser = new OpenFileDialog {
            Filter = "RIFF WAVE files (*.wav)|*.wav"
        };

        readonly SaveFileDialog exporter = new SaveFileDialog {
            Filter = "RIFF WAVE files (*.wav)|*.wav"
        };

        public MainWindow() => InitializeComponent();

        Convolver GetFilter(Complex[] spectrum, float gain, int sampleRate) {
            Equalizer eq = EQGenerator.FlattenSpectrum(spectrum, sampleRate);
            float[] filterSamples = phasePerfect.IsChecked.Value
                ? eq.GetLinearConvolution(sampleRate, spectrum.Length, gain)
                : eq.GetConvolution(sampleRate, spectrum.Length, gain);
            return new Convolver(filterSamples, 0);
        }

        void ProcessPerChannel(RIFFWaveReader reader, ref float[] impulse) {
            int targetLen = QMath.Base2Ceil((int)reader.Length);
            Convolver[] filters = new Convolver[reader.ChannelCount];
            FFTCache cache = new FFTCache(targetLen);

            for (int ch = 0; ch < reader.ChannelCount; ++ch) {
                float[] channel = new float[targetLen];
                WaveformUtils.ExtractChannel(impulse, channel, ch, reader.ChannelCount);

                float gain = 1;
                if (normalizeToPeak.IsChecked.Value)
                    gain = WaveformUtils.GetPeak(channel);

                Complex[] spectrum = Measurements.FFT(channel, cache);
                filters[ch] = GetFilter(spectrum, gain, reader.SampleRate);
            }

            Array.Resize(ref impulse, impulse.Length << 1);
            for (int ch = 0; ch < reader.ChannelCount; ++ch)
                filters[ch].Process(impulse, ch, reader.ChannelCount);
        }

        void ProcessCommon(RIFFWaveReader reader, ref float[] impulse) {
            int targetLen = QMath.Base2Ceil((int)reader.Length);
            float gain = 1;
            Complex[] commonSpectrum = new Complex[targetLen];
            FFTCache cache = new FFTCache(targetLen);

            for (int ch = 0; ch < reader.ChannelCount; ++ch) {
                float[] channel = new float[targetLen];
                WaveformUtils.ExtractChannel(impulse, channel, ch, reader.ChannelCount);

                Complex[] spectrum = Measurements.FFT(channel, cache);
                for (int band = 0; band < spectrum.Length; ++band)
                    commonSpectrum[band] += spectrum[band];
            }

            float mul = 1f / reader.ChannelCount;
            for (int band = 0; band < commonSpectrum.Length; ++band)
                commonSpectrum[band] *= mul;
            if (normalizeToPeak.IsChecked.Value) {
                float[] channel = Measurements.GetRealPart(Measurements.IFFT(commonSpectrum, cache));
                gain = WaveformUtils.GetPeak(channel);
            }

            Array.Resize(ref impulse, impulse.Length << 1);
            for (int ch = 0; ch < reader.ChannelCount; ++ch) {
                Convolver filter = GetFilter(commonSpectrum, gain, reader.SampleRate);
                filter.Process(impulse, ch, reader.ChannelCount);
            }
        }

        void ProcessImpulse(object sender, RoutedEventArgs e) {
            if (browser.ShowDialog().Value) {
                BinaryReader stream = new BinaryReader(File.Open(browser.FileName, FileMode.Open));
                RIFFWaveReader reader = new RIFFWaveReader(stream);
                float[] impulse = reader.Read();

                if (commonEQ.IsChecked.Value)
                    ProcessCommon(reader, ref impulse);
                else
                    ProcessPerChannel(reader, ref impulse);

                BitDepth bits = reader.Bits;
                if (forceFloat.IsChecked.Value)
                    bits = BitDepth.Float32;

                int targetLen = QMath.Base2Ceil((int)reader.Length);
                if (separateExport.IsChecked.Value) {
                    ReferenceChannel[] channels = ChannelPrototype.StandardMatrix[reader.ChannelCount];
                    for (int ch = 0; ch < reader.ChannelCount; ++ch) {
                        string exportName = Path.GetFileName(browser.FileName);
                        int idx = exportName.LastIndexOf('.');
                        string channelName = ChannelPrototype.Mapping[(int)channels[ch]].Name;
                        exporter.FileName = $"{exportName.Substring(0, idx)} - {channelName}{exportName.Substring(idx)}";

                        if (exporter.ShowDialog().Value) {
                            float[] channel = new float[targetLen * 2];
                            WaveformUtils.ExtractChannel(impulse, channel, ch, reader.ChannelCount);
                            BinaryWriter outStream = new BinaryWriter(File.Open(exporter.FileName, FileMode.Create));
                            new RIFFWaveWriter(outStream, 1, targetLen, reader.SampleRate, bits).Write(channel);
                        }
                    }
                } else {
                    exporter.FileName = Path.GetFileName(browser.FileName);
                    if (exporter.ShowDialog().Value) {
                        BinaryWriter outStream = new BinaryWriter(File.Open(exporter.FileName, FileMode.Create));
                        new RIFFWaveWriter(outStream, reader.ChannelCount, targetLen, reader.SampleRate, bits).Write(impulse);
                    }
                }
            }
        }
    }
}