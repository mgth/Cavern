﻿using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Cavern.Format;
using Cavern.Remapping;

using VoidX.WPF;

namespace EnhancedAC3Merger {
    /// <summary>
    /// Single channel input mapper control. Loads a file and selects one of its channels as an output channel.
    /// </summary>
    public partial class InputChannel : UserControl {
        /// <summary>
        /// This input channel is assigned.
        /// </summary>
        public bool Active => SelectedFile != null;

        /// <summary>
        /// The output channel to set with the control.
        /// </summary>
        public ReferenceChannel TargetChannel {
            get => targetChannel;
            set {
                targetChannel = value;
                channelName.Text = EnumToTitleCase.GetTitleCase(value.ToString());
            }
        }
        ReferenceChannel targetChannel;

        /// <summary>
        /// The file selected for this channel.
        /// </summary>
        public string SelectedFile { get; private set; }

        /// <summary>
        /// Index of the selected channel in the <see cref="SelectedFile"/>.
        /// </summary>
        public int SelectedChannel => channelIndex.SelectedIndex;

        /// <summary>
        /// Path of the last opened file in the application.
        /// </summary>
        static string lastFile;

        /// <summary>
        /// Single channel input mapper control.
        /// </summary>
        public InputChannel() => InitializeComponent();

        /// <summary>
        /// Opens a file for selecting a channel from.
        /// </summary>
        void OpenFile(string path) {
            AudioReader reader;
            try {
                reader = AudioReader.Open(path);
                reader.ReadHeader();
            } catch (Exception ex) {
                MessageBox.Show("Importing the file failed for the following reason: " + ex.Message);
                return;
            }

            SelectedFile = path;
            ReferenceChannel[] channels = reader.GetRenderer().GetChannels();
            channelIndex.ItemsSource = channels;
            if (channels.Contains(targetChannel)) {
                channelIndex.SelectedItem = targetChannel;
            } else {
                channelIndex.SelectedIndex = 0;
            }
            reader.Dispose();
        }

        /// <summary>
        /// Opens a file for selecting a channel from.
        /// </summary>
        void OpenFile(object _, RoutedEventArgs e) {
            OpenFileDialog opener = new OpenFileDialog() {
                Filter = "Supported input files|" + AudioReader.filter
            };
            if (opener.ShowDialog().Value) {
                OpenFile(lastFile = opener.FileName);
            }
        }

        /// <summary>
        /// Uses the file opened the last time clicking another channel's &quot;Open file&quot; button.
        /// </summary>
        void LastFile(object _, RoutedEventArgs e) {
            if (lastFile == null) {
                MainWindow.Error("No file was opened before.");
            } else {
                OpenFile(lastFile);
            }
        }

        /// <summary>
        /// Display the referenced channel on string conversion.
        /// </summary>
        public override string ToString() => channelName.Text;
    }
}