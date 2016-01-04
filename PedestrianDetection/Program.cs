//----------------------------------------------------------------------------
//  Copyright (C) 2004-2013 by EMGU. All rights reserved.       
//----------------------------------------------------------------------------

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.GPU;

namespace PedestrianDetection
{
   static class Program
   {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new frmMain());
            string strParam = args[0];

            if (strParam.ToLower() == "-o")
            {
                OpenFileDialog openDlg = new OpenFileDialog();
                openDlg.Filter = "Image File|*.*";
                openDlg.RestoreDirectory = true;

                if (openDlg.ShowDialog() == DialogResult.OK)
                {
                    strParam = openDlg.FileName;
                }
                else
                {
                    Application.Exit();
                }
            }

            Detect(strParam);
        }

        static void Detect(string strPathName)
        {
            Bitmap bmpSrc = new Bitmap(strPathName);

            float fWidthSrc = bmpSrc.Width;
            float fHeightSrc = bmpSrc.Height;

            float fWidthDefault = 480;
            float fHeightDefault = 480;

            float fRateWidth = fWidthDefault / fWidthSrc;
            float fRateHeight = fHeightDefault / fHeightSrc;

            float fRate = Math.Min(fRateWidth, fRateHeight);

            float fWidthFit = fWidthSrc * fRate;
            float fHeightFit = fHeightSrc * fRate;

            Bitmap bmpFit = new Bitmap(bmpSrc, (int)fWidthFit, (int)fHeightFit);
            Image<Bgr, Byte> src = new Image<Bgr, Byte>(bmpFit);
            Image<Gray, Byte> gray = new Image<Gray, byte>(bmpFit);
            Bitmap bmpAdjust, bmpGray;
            bmpGray = gray.ToBitmap();
            bmpAdjust = bmpGray;

            long processingTime;
            double hitThreshold = 0.0;
            double scale = 1.05;
            int finalThreshold = 0;
            bool useMeanshiftGrouping = true;

            List<Rectangle> lstResults = new List<Rectangle>();

            using (Image<Bgr, Byte> image = new Image<Bgr, byte>(bmpAdjust))
            {
                Rectangle[] results = FindPedestrian.Find(bmpAdjust, out processingTime, hitThreshold, scale, finalThreshold, useMeanshiftGrouping);

                foreach (Rectangle rect in results)
                {
                    Rectangle rc;
                    rc = new Rectangle((int)(rect.Left / fRate),
                                       (int)(rect.Top / fRate),
                                       (int)(rect.Width / fRate),
                                       (int)(rect.Height / fRate));

                    Console.WriteLine("Rectangle Left={0}, Top={1}, Width={2}, Height={3}", rc.Left, rc.Top, rc.Width, rc.Height);
                    lstResults.Add(rc);

                    src.Draw(rect, new Bgr(Color.Red), 1);
                }
                ImageViewer.Show(src);
            }

            Rectangle[] _results = lstResults.ToArray();
        }
    }
}
