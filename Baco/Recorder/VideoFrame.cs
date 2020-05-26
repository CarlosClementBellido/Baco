using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using static Baco.Utils.ImageUtils;

namespace Baco.Recorder
{
    public static class VideoFrame
    {

        private const int MAX_PARTS_SPLIT = 20;
        private const int FRAME_CHANGE_PROBABILITY_INTERVAL = 8;

        /// <summary>
        /// Block of image
        /// </summary>
        [Serializable]
        public class FrameMapping
        {
            public Rectangle Portion { get; set; }
            public byte[] Frame { get; set; }

            public FrameMapping(Rectangle portion, byte[] compressedFrame)
            {
                Portion = portion;
                Frame = compressedFrame;
            }
        }

        public static Bitmap[][] frameParts;
        private static int xParts;
        private static int yParts;
        private static readonly Dictionary<Rectangle?, int> frameChageProbability = new Dictionary<Rectangle?, int>();
        private static readonly Random rnd = new Random();
        /// <summary>
        /// Check the changed blocks
        /// </summary>
        /// <param name="NewFrame">New frame</param>
        /// <param name="imageCodecInfo">Codec info for the image</param>
        /// <param name="encoderParameters">Encoder parameters for the image</param>
        /// <returns>List of changed blocks</returns>
        internal static List<FrameMapping> UpdateFrameParts(Bitmap NewFrame, ImageCodecInfo imageCodecInfo, EncoderParameters encoderParameters)
        {
            Bitmap[][] NewFrameParts = SplitBitmap(NewFrame, out Rectangle[][] portions);

            List<FrameMapping> changedFrames = new List<FrameMapping>();
            bool haveChanges = false;
            if (frameParts != null)
            {
                for (int i = 0; i < xParts; i++)
                    for (int j = 0; j < yParts; j++)
                    {
                        KeyValuePair<Rectangle?, int>? probability = frameChageProbability.SingleOrDefault(f => f.Key == portions[i][j]);

                        if (probability.Value.Key == null)
                            // If there is no probability registered sets the default probability to FRAME_CHANGE_PROBABILITY_INTERVAL / 2
                            frameChageProbability.Add(portions[i][j], FRAME_CHANGE_PROBABILITY_INTERVAL / 2);

                        if (probability.Value.Value == FRAME_CHANGE_PROBABILITY_INTERVAL || rnd.Next(0, 10 + 1) >= probability.Value.Value)
                        {
                            if (!CompareBitmaps(frameParts[i][j], NewFrameParts[i][j]))
                            {
                                haveChanges = true;
                                // There is changes, so adds 1 to probability of change in the next frame
                                frameChageProbability[portions[i][j]] += FRAME_CHANGE_PROBABILITY_INTERVAL == frameChageProbability[portions[i][j]] ? 0 : 1;

                                changedFrames.Add(new FrameMapping(portions[i][j], ImageToByteArray(NewFrameParts[i][j], imageCodecInfo, encoderParameters)));
                                frameParts[i][j] = NewFrameParts[i][j];

                                // If half of the blocks have changed we send all image
                                if (changedFrames.Count >= Math.Pow(MAX_PARTS_SPLIT, 2) / 2)
                                {
                                    changedFrames.Clear();
                                    changedFrames.Add(new FrameMapping(new Rectangle(0, 0, NewFrame.Width, NewFrame.Height), ImageToByteArray(NewFrame, imageCodecInfo, encoderParameters)));
                                    return changedFrames;
                                }
                            }
                            else
                                // If there's no changes whe substract 1 to the probability of changing next frame
                                frameChageProbability[portions[i][j]] -= frameChageProbability[portions[i][j]] == 0 ? 0 : 1;
                        }
                    }
            }
            else    // In case no previous blocks-register
            {       
                haveChanges = true;
                frameParts = new Bitmap[xParts][];
                for (int i = 0; i < xParts; i++)
                {
                    frameParts[i] = new Bitmap[yParts];
                    for (int j = 0; j < yParts; j++)
                    {
                        changedFrames.Add(new FrameMapping(portions[i][j], ImageToByteArray(NewFrameParts[i][j], imageCodecInfo, encoderParameters)));
                        frameParts[i][j] = NewFrameParts[i][j];
                    }
                }
            }

            // If no changes we return null
            return haveChanges ? changedFrames : null;

        }

        private static bool CompareBitmaps(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1 == null || bmp2 == null)
                return false;
            if (Equals(bmp1, bmp2))
                return true;
            if (!bmp1.Size.Equals(bmp2.Size) || !bmp1.PixelFormat.Equals(bmp2.PixelFormat))
                return false;

            int bytes = bmp1.Width * bmp1.Height * (Image.GetPixelFormatSize(bmp1.PixelFormat) / 8);

            bool result = true;
            byte[] b1bytes = new byte[bytes];
            byte[] b2bytes = new byte[bytes];

            BitmapData bitmapData1 = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width, bmp1.Height), ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData bitmapData2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), ImageLockMode.ReadOnly, bmp2.PixelFormat);

            Marshal.Copy(bitmapData1.Scan0, b1bytes, 0, bytes);
            Marshal.Copy(bitmapData2.Scan0, b2bytes, 0, bytes);

            for (int n = 0; n <= bytes - 1; n++)
            {
                if (b1bytes[n] != b2bytes[n])
                {
                    result = false;
                    break;
                }
            }

            bmp1.UnlockBits(bitmapData1);
            bmp2.UnlockBits(bitmapData2);

            return result;
        }

        private static Bitmap[][] SplitBitmap(Bitmap bitmap, out Rectangle[][] portions)
        {
            xParts = GetGreatestDivisor(bitmap.Width);
            yParts = GetGreatestDivisor(bitmap.Height);
            int xPartDimmension = bitmap.Width / xParts;
            int yPartDimmension = bitmap.Height / yParts;

            Bitmap[][] splitBitmap = new Bitmap[xParts][];
            portions = new Rectangle[xParts][];

            for (int i = 0; i < xParts; i++)
            {
                splitBitmap[i] = new Bitmap[yParts];
                portions[i] = new Rectangle[yParts];
                for (int j = 0; j < yParts; j++)
                {
                    Rectangle cloneRect = new Rectangle(xPartDimmension * i, yPartDimmension * j, xPartDimmension, yPartDimmension);
                    BitmapData bmpData = bitmap.LockBits(cloneRect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                    bitmap.UnlockBits(bmpData);

                    splitBitmap[i][j] = bitmap.Clone(cloneRect, bmpData.PixelFormat);
                    portions[i][j] = cloneRect;
                }
            }

            return splitBitmap;
        }

        private static int GetGreatestDivisor(int n, int limit = MAX_PARTS_SPLIT)
        {
            int lastDivisor = 0;
            for (int i = 1; i <= n; i++)
            {
                if (n % i == 0)
                {
                    if (i >= limit)
                        break;
                    lastDivisor = i;
                }
            }

            return lastDivisor;
        }

    }
}
