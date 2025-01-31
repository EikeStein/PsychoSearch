﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Core;
using HtmlAgilityPack;
using static System.Diagnostics.Debug;

namespace TherapistEditor
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public TherapistLoader TherapistLoader { get; } = new TherapistLoader();
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            string path = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            Assert(path != null, "path != null");
            path = Path.Combine(path, "TherapistSites");
            Progress<double> progressReporter = new Progress<double>(d => LoadingProgressBar.Value = d);
            var therapists = await Task.Run(() => LoadTherapistsFromDirectory(path, progressReporter));
            SaveTherapists(therapists, @"G:\Dokumente\Visual Studio\Projects\PsychoSearch\therapists2.psycho");
        }

        private void SaveTherapists(Therapist[] therapists, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Therapist[]));
                serializer.Serialize(fs, therapists);
            }
        }

        private Therapist[] LoadTherapistsFromDirectory(string path, IProgress<double> progressReporter)
        {
            var therapists = new List<Therapist>();
            var files = Directory.GetFiles(path);

            {
                var special = "G:/Dokumente/Visual Studio/Projects/PsychoSearch/TherapistEditor/bin/Debug/TherapistSites/3880.psycho".Replace("/", @"\\\\");
                var htmlDocument = new HtmlDocument();
                htmlDocument.Load(special, Encoding.GetEncoding("ISO-8859-1"));
                TherapistLoader.LoadTherapists(htmlDocument);
            }

            for (int i = 0; i < files.Length; i++)
            {
                progressReporter.Report(1.0 * i / files.Length);
                var htmlDocument = new HtmlDocument();
                htmlDocument.Load(files[i], Encoding.GetEncoding("ISO-8859-1"));
                var therapist = TherapistLoader.LoadTherapists(htmlDocument);
                var name = new FileInfo(files[i]).Name.Split('.')[0].Replace("x", "");
                therapist.ID = Convert.ToInt64(name);
                therapist.KVNWebsite = @"http://www.arztauskunft-niedersachsen.de/arztsuche/detailAction.action?arztId=" + therapist.ID;
                therapists.Add(therapist);
            }
            return therapists.ToArray();
        }
    }
}
