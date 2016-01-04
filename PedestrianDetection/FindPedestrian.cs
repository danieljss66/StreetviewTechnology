using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.Diagnostics;
#if !IOS
using Emgu.CV.GPU;
#endif

namespace PedestrianDetection
{
   public static class FindPedestrian
   {
       public static Rectangle[] Find(Image<Bgr, Byte> image,
                               out long processingTime,
                               double hitThreshold = 0,
                               double scale = 1.05,
                               int finalThreshold = 2,
                               bool useMeanshiftGrouping = false)
       {
           int nWidth = image.Width;
           int nHeight = image.Height;

           Rectangle[] regions = SubFind(image, out processingTime, hitThreshold, scale, finalThreshold, useMeanshiftGrouping);

           return regions;
       }

       public static Rectangle[] Find(Bitmap bmpSrc,
                        out long processingTime,
                        double hitThreshold = 0,
                        double scale = 1.05,
                        int finalThreshold = 2,
                        bool useMeanshiftGrouping = false)
       {
           List<Rectangle> lstRect = new List<Rectangle>();

           int nWidth = bmpSrc.Width;
           int nHeight = bmpSrc.Height;
           int nSplit = 3;
           int nSplitWidth = (int)(nWidth / (float)nSplit);
           int nSplitHeight = (int)(nHeight / (float)nSplit);
           processingTime = 0;

           Image<Bgr, Byte> image = new Image<Bgr, byte>(bmpSrc);
           Rectangle[] regions = SubFind(image, out processingTime, hitThreshold, scale, finalThreshold, useMeanshiftGrouping);

           for (int k = 0; k < regions.Length; k++)
           {
               Rectangle rc = regions[k];
               lstRect.Add(new Rectangle(rc.Location, rc.Size));
           }


           for (int i = 0; i < nSplit; i++)
           {
               for (int j = 0; j < nSplit; j++)
               {
                   Bitmap bmp = new Bitmap(nSplitWidth * nSplit, nSplitHeight * nSplit);
                   Graphics g = Graphics.FromImage(bmp);
                   g.DrawImage(bmpSrc,
                               new Rectangle(0, 0, nSplitWidth * nSplit, nSplitHeight * nSplit),
                               new Rectangle(i * nSplitWidth, j * nSplitHeight, nSplitWidth, nSplitHeight),
                               GraphicsUnit.Pixel); 

                   image = new Image<Bgr, byte>(bmp);
                   regions = SubFind(image, out processingTime, hitThreshold, scale, finalThreshold, useMeanshiftGrouping);

                   for (int k = 0; k < regions.Length; k++)
                   {
                       Rectangle rc = regions[k];
                       int nL = i * nSplitWidth + (int)(rc.Left / (float)nSplit);
                       int nT = j * nSplitHeight + (int)(rc.Top / (float)nSplit);
                       int nW = (int)(rc.Width / (float)nSplit);
                       int nH = (int)(rc.Height / (float)nSplit);

                       lstRect.Add(new Rectangle(nL, nT, nW, nH));
                   }

                   if (j < nSplit - 1)
                   {
                       bmp = new Bitmap(nSplitWidth * nSplit, nSplitHeight * nSplit);
                       g = Graphics.FromImage(bmp);
                       g.DrawImage(bmpSrc,
                                   new Rectangle(0, 0, nSplitWidth * nSplit, nSplitHeight * nSplit),
                                   new Rectangle(i * nSplitWidth, j * nSplitHeight + nSplitHeight / 2, nSplitWidth, nSplitHeight),
                                   GraphicsUnit.Pixel);

                       image = new Image<Bgr, byte>(bmp);
                       regions = SubFind(image, out processingTime, hitThreshold, scale, finalThreshold, useMeanshiftGrouping);

                       for (int k = 0; k < regions.Length; k++)
                       {
                           Rectangle rc = regions[k];
                           int nL = i * nSplitWidth + (int)(rc.Left / (float)nSplit);
                           int nT = j * nSplitHeight + nSplitHeight / 2 + (int)(rc.Top / (float)nSplit);
                           int nW = (int)(rc.Width / (float)nSplit);
                           int nH = (int)(rc.Height / (float)nSplit);

                           lstRect.Add(new Rectangle(nL, nT, nW, nH));
                       }
                   }

                   if (i < nSplit - 1)
                   {
                       bmp = new Bitmap(nSplitWidth * nSplit, nSplitHeight * nSplit);
                       g = Graphics.FromImage(bmp);
                       g.DrawImage(bmpSrc,
                                   new Rectangle(0, 0, nSplitWidth * nSplit, nSplitHeight * nSplit),
                                   new Rectangle(i * nSplitWidth + nSplitWidth / 2, j * nSplitHeight, nSplitWidth, nSplitHeight),
                                   GraphicsUnit.Pixel);

                       image = new Image<Bgr, byte>(bmp);
                       regions = SubFind(image, out processingTime, hitThreshold, scale, finalThreshold, useMeanshiftGrouping);

                       for (int k = 0; k < regions.Length; k++)
                       {
                           Rectangle rc = regions[k];
                           int nL = i * nSplitWidth + nSplitWidth / 2 + (int)(rc.Left / (float)nSplit);
                           int nT = j * nSplitHeight + (int)(rc.Top / (float)nSplit);
                           int nW = (int)(rc.Width / (float)nSplit);
                           int nH = (int)(rc.Height / (float)nSplit);

                           lstRect.Add(new Rectangle(nL, nT, nW, nH));
                       }
                   }

                   if ((i < nSplit - 1) && (j < nSplit - 1))
                   {
                       bmp = new Bitmap(nSplitWidth * nSplit, nSplitHeight * nSplit);
                       g = Graphics.FromImage(bmp);
                       g.DrawImage(bmpSrc,
                                   new Rectangle(0, 0, nSplitWidth * nSplit, nSplitHeight * nSplit),
                                   new Rectangle(i * nSplitWidth + nSplitWidth / 2, j * nSplitHeight + nSplitHeight / 2, nSplitWidth, nSplitHeight),
                                   GraphicsUnit.Pixel);

                       image = new Image<Bgr, byte>(bmp);
                       regions = SubFind(image, out processingTime, hitThreshold, scale, finalThreshold, useMeanshiftGrouping);

                       for (int k = 0; k < regions.Length; k++)
                       {
                           Rectangle rc = regions[k];
                           int nL = i * nSplitWidth + nSplitWidth / 2 + (int)(rc.Left / (float)nSplit);
                           int nT = j * nSplitHeight + nSplitHeight / 2 + (int)(rc.Top / (float)nSplit);
                           int nW = (int)(rc.Width / (float)nSplit);
                           int nH = (int)(rc.Height / (float)nSplit);

                           lstRect.Add(new Rectangle(nL, nT, nW, nH));
                       }
                   }
               }
           }

           for (int i = 0; i < lstRect.Count - 1; i++)
           {
               for (int j = i + 1; j < lstRect.Count; j++)
               {
                   Rectangle rc1 = lstRect[i];
                   Rectangle rc2 = lstRect[j];

                   if (rc1.IntersectsWith(rc2) || rc2.IntersectsWith(rc1))
                   {
                       if (rc2.Left >= rc1.Left &&
                           rc2.Top >= rc1.Top &&
                           rc2.Right <= rc1.Right &&
                           rc2.Bottom <= rc1.Bottom)
                       {
                           lstRect.RemoveAt(j);
                           j--;
                       }
                       else if (rc1.Left >= rc2.Left &&
                           rc1.Top >= rc2.Top &&
                           rc1.Right <= rc2.Right &&
                           rc1.Bottom <= rc2.Bottom)
                       {
                           lstRect.RemoveAt(i);
                           i--;
                           break;
                       }
                       else
                       {
                           long s1 = (long)(rc1.Width * rc1.Height * 0.8f);
                           long s2 = (long)(rc2.Width * rc2.Height * 0.8f);

                           int left = Math.Max(rc1.Left, rc2.Left);
                           int top = Math.Max(rc1.Top, rc2.Top);
                           int right = Math.Min(rc1.Right, rc2.Right);
                           int bottom = Math.Min(rc1.Bottom, rc2.Bottom);

                           long s = (right - left) * (bottom - top);

                           if (s > s2)
                           {
                               lstRect.RemoveAt(j);
                               j--;
                           }
                           else if (s > s1 && s < s2)
                           {
                               lstRect.RemoveAt(i);
                               i--;
                               break;
                           }
                       }
                   }
               }
           }

           return lstRect.ToArray();
       }

      /// <summary>
      /// Find the pedestrian in the image
      /// </summary>
      /// <param name="image">The image</param>
      /// <param name="processingTime">The pedestrian detection time in milliseconds</param>
      /// <returns>The region where pedestrians are detected</returns>

      public static Rectangle[] SubFind(Image<Bgr, Byte> image, 
                                     out long processingTime,
                                     double hitThreshold = 0, 
                                     double scale = 1.05, 
                                     int finalThreshold = 2,
                                     bool useMeanshiftGrouping = false)
      {
         Stopwatch watch;
         Rectangle[] regions;
         #if !IOS
         //check if there is a compatible GPU to run pedestrian detection
         if (GpuInvoke.HasCuda)
         {  //this is the GPU version
            using (GpuHOGDescriptor des = new GpuHOGDescriptor())
            {
               des.SetSVMDetector(GpuHOGDescriptor.GetDefaultPeopleDetector());

               watch = Stopwatch.StartNew();
               using (GpuImage<Bgr, Byte> gpuImg = new GpuImage<Bgr, byte>(image))
               using (GpuImage<Bgra, Byte> gpuBgra = gpuImg.Convert<Bgra, Byte>())
               {
                   regions = des.DetectMultiScale(gpuBgra);
               }
            }
         }
         else
         #endif
         {  //this is the CPU version
            using (HOGDescriptor des = new HOGDescriptor())
            {
               des.SetSVMDetector(HOGDescriptor.GetDefaultPeopleDetector());

               watch = Stopwatch.StartNew();
               regions = des.DetectMultiScale(image, hitThreshold, Size.Empty, Size.Empty, scale, finalThreshold, useMeanshiftGrouping);
               //regions = des.DetectMultiScale(image, 0, new Size(2, 2), new Size(2, 2), 1.05, 1, false);
            }
         }
         watch.Stop();

         processingTime = watch.ElapsedMilliseconds;

         return regions;
      }
   }
}
