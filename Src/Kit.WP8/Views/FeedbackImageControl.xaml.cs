﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Input;
using System.Windows.Ink;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.HockeyApp.ViewModels;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.HockeyApp.Tools;

namespace Microsoft.HockeyApp.Views
{
    public partial class FeedbackImageControl : UserControl
    {
        FeedbackPage parentControl;
        internal FeedbackImageVM VM
        {
            get { return (this.DataContext as FeedbackImageVM); }
            set {
                this.DataContext = value;
                ImageArea.Strokes.Clear();
                UpdateBgImage();
            }
        }

        public FeedbackImageControl(FeedbackPage parent)
        {
            InitializeComponent();
            parentControl = parent;
            this.DataContext = new FeedbackImageVM(parent.VM);
        }

        private Size ScreenResolution
        {
            get
            {
                var content = Application.Current.Host.Content;
                double scale = (double)content.ScaleFactor / 100;
                int h = (int)Math.Ceiling(content.ActualHeight * scale);
                int w = (int)Math.Ceiling(content.ActualWidth * scale);
                return new Size(w, h);
            }
        }

        private void UpdateBgImage()
        {
            if (this.VM != null && this.VM.FeedbackImage != null && this.VM.FeedbackImage.DataBytes != null)
            {
                BitmapImage image = new BitmapImage();
                var imgStream = new MemoryStream(this.VM.FeedbackImage.DataBytes);
                image.SetSource(imgStream);
                double imgwidth, imgheight, width, height = 0;
                width = imgwidth = image.PixelWidth;
                height = imgheight = image.PixelHeight;

                if (Math.Max(imgwidth, imgheight) > ScreenResolution.Height)
                {
                    if (imgwidth > imgheight)
                    {
                        width = ScreenResolution.Height;
                        height = ScreenResolution.Height * (imgheight / imgwidth);
                    }
                    else
                    {
                        height = ScreenResolution.Height;
                        width = ScreenResolution.Height * (imgwidth / imgheight);
                    }
                }
                
                ImageArea.Width = width;
                ImageArea.Height = height;
                ImageBrush.ImageSource = image;
            }
        }

        Stroke _newStroke;
        private void ImageArea_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            ImageArea.CaptureMouse();
            var MyStylusPointCollection = new StylusPointCollection();
            MyStylusPointCollection.Add(e.StylusDevice.GetStylusPoints(ImageArea));
            _newStroke = new Stroke(MyStylusPointCollection);
            _newStroke.DrawingAttributes.Color = (LocalizedStrings.LocalizedResources.FeedbackDrawingColor as string).ConvertStringToColor(Colors.Red);
                
            ImageArea.Strokes.Add(_newStroke);
        }
       
        private void ImageArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (_newStroke != null)
                _newStroke.StylusPoints.Add(e.StylusDevice.GetStylusPoints(ImageArea));
        }

        private void ImageArea_LostMouseCapture(object sender, MouseEventArgs e)
        {
            _newStroke = null;
        }

        internal void ResetButtonClicked()
        {
            ImageArea.Strokes.Clear();
        }

        internal void OkButtonClicked()
        {
            this.VM.SaveChangesToImage(ImageArea);
            this.parentControl.NavigateBack();
        }

        internal void DeleteButtonClicked()
        {
            this.parentControl.formControl.VM.RemoveAttachment(this.VM);
            this.parentControl.NavigateBack();
        }

        //http://stackoverflow.com/questions/16527990/windows-phone-serialize-save-inkpresenter-control
        /*
        public void SaveStrokes(string filename, StrokeCollection strokeCollection)
        {
            var isf = IsolatedStorageFile.GetUserStoreForApplication();
            var stream = isf.CreateFile(filename);

            using (var writer = new StreamWriter(stream))
            {
                foreach (var stroke in strokeCollection)
                {
                    writer.WriteLine(String.Format("{0}|{1}",
                        stroke.DrawingAttributes.Color.ToString(),
                        String.Join("$", stroke.StylusPoints.Select(p => String.Format("{0},{1}", p.X, p.Y)))));
                }
            }
        }

        public void LoadStrokes(string filename, InkPresenter inkPresenter)
        {
            var isf = IsolatedStorageFile.GetUserStoreForApplication();
            var stream = isf.OpenFile(filename, FileMode.Open);

            using (var reader = new StreamReader(stream))
            {
                var strokes = reader.ReadToEnd().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var stroke in strokes)
                {
                    var strokeParams = stroke.Split('|');

                    var myStroke = new Stroke();
                    myStroke.DrawingAttributes.Color = HexToColor(strokeParams[0].ToString());

                    var pointList = strokeParams[1].Split('$');
                    foreach (var pointPair in pointList)
                    {
                        var pointPairList = pointPair.Split(',');
                        var x = Convert.ToDouble(pointPairList[0]);
                        var y = Convert.ToDouble(pointPairList[1]);

                        myStroke.StylusPoints.Add(new StylusPoint(x, y));
                    }

                    inkPresenter.Strokes.Add(myStroke);
                }
            }
        }

        public System.Windows.Media.Color HexToColor(string hexString)
        {
            string cleanString = hexString.Replace("-", String.Empty).Substring(1);

            var bytes = Enumerable.Range(0, cleanString.Length)
                           .Where(x => x % 2 == 0)
                           .Select(x => Convert.ToByte(cleanString.Substring(x, 2), 16))
                           .ToArray();

            return System.Windows.Media.Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]);
        }
        */


    }
}
