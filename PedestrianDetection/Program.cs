//----------------------------------------------------------------------------
//  Copyright (C) 2004-2013 by EMGU. All rights reserved.       
//----------------------------------------------------------------------------

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
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

            int nArgLength = args.Length;

            string strParam = "-o";

            if (nArgLength > 0)
            {
                strParam = args[0];
            }

            if (strParam.ToLower() == "-o")
            {
                OpenFileDialog openDlg = new OpenFileDialog();
                openDlg.Filter = "Image File|*.*";
                openDlg.RestoreDirectory = true;

                if (openDlg.ShowDialog() == DialogResult.OK)
                {
                    strParam = openDlg.FileName;
                    Detect(strParam);
                }
            }
            else
            {
                if (System.IO.File.Exists(strParam))
                {
                    Detect(strParam);
                }
            }
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
            Image<Bgr, Byte> src = new Image<Bgr, Byte>(bmpSrc);
            Image<Bgr, Byte> srcFit = new Image<Bgr, Byte>(bmpFit);
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

            string strPath = System.IO.Path.GetDirectoryName(strPathName);
            string strName = System.IO.Path.GetFileName(strPathName);
            string strExt = System.IO.Path.GetExtension(strPathName);

            string strMainPathName = strPath + "\\" + strName + "_pedestrian_regions";

            string strCSVTempPathName = strMainPathName + ".csv";
            string strImgTempPathName = strMainPathName + strExt;


            using (Image<Bgr, Byte> image = new Image<Bgr, byte>(bmpAdjust))
            {
                string strCSVPathName = strCSVTempPathName;
                StreamWriter swCSV = new StreamWriter(strCSVPathName);
                Rectangle[] results = FindPedestrian.Find(bmpAdjust, out processingTime, hitThreshold, scale, finalThreshold, useMeanshiftGrouping);

                int i = 0;

                swCSV.WriteLine(strPathName);
                swCSV.WriteLine("Rectangle,Left,Top,Width,Height");

                foreach (Rectangle rect in results)
                {
                    Rectangle rc;
                    rc = new Rectangle((int)(rect.Left / fRate),
                                       (int)(rect.Top / fRate),
                                       (int)(rect.Width / fRate),
                                       (int)(rect.Height / fRate));

                    i++;
                    string strLine = string.Format("{0},{1},{2},{3},{4}", i, rc.Left, rc.Top, rc.Width, rc.Height);
                    swCSV.WriteLine(strLine);

                    lstResults.Add(rc);

                    srcFit.Draw(rect, new Bgr(Color.Red), 1);
                    src.Draw(rc, new Bgr(Color.Red), 1);
                    src.Save(strImgTempPathName);
                }

                swCSV.Close();
                System.Diagnostics.Process.Start(strCSVPathName);

                ImageViewer.Show(srcFit);
            }

            Rectangle[] _results = lstResults.ToArray();
        }
    }
}
