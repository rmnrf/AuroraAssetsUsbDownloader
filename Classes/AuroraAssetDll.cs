using System;
using System.Runtime.InteropServices;

namespace AuroraAssetsUsbDownloader.Classes
{
    /// <summary>
    /// Wrapper class for interfacing with the AuroraAsset.dll
    /// Ported from AuroraAssetEditor by MaesterRowen (Phoenix)
    /// </summary>
    class AuroraAssetDll
    {
        [DllImport("AuroraAsset.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ConvertImageToAsset(IntPtr imageData, int imageDataLen, int imageWidth, int imageHeight, int useCompression,
                                                       IntPtr headerData, out int headerDataLen, IntPtr videoData, out int videoDataLen);

        [DllImport("AuroraAsset.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ConvertAssetToImage(IntPtr headerData, int headerDataLen, IntPtr videoData, int videoDataLen, IntPtr imageData, out int imageDataLen,
                                                       out int imageWidth, out int imageHeight);

        public static bool ProcessImageToAsset(ref byte[] pixelData, int imageWidth, int imageHeight, bool useCompression, ref byte[] headerData, ref byte[] videoData)
        {
            IntPtr hd = IntPtr.Zero, vd = IntPtr.Zero, pd = IntPtr.Zero;
            try
            {
                bool status = false;
                int pixelDataLen = pixelData.Length;
                if (pixelData == null || pixelDataLen == 0)
                    return false;

                pd = Marshal.AllocHGlobal(pixelDataLen);
                Marshal.Copy(pixelData, 0, pd, pixelDataLen);

                int headerDataLen;
                int videoDataLen;
                int result = ConvertImageToAsset(pd, pixelDataLen, imageWidth, imageHeight, useCompression ? 1 : 0, IntPtr.Zero, out headerDataLen, IntPtr.Zero, out videoDataLen);
                if (result == 1)
                {
                    hd = Marshal.AllocHGlobal(headerDataLen);
                    vd = Marshal.AllocHGlobal(videoDataLen);

                    result = ConvertImageToAsset(pd, pixelDataLen, imageWidth, imageHeight, useCompression ? 1 : 0, hd, out headerDataLen, vd, out videoDataLen);
                    if (result == 1)
                    {
                        headerData = new byte[headerDataLen];
                        Marshal.Copy(hd, headerData, 0, headerDataLen);
                        videoData = new byte[videoDataLen];
                        Marshal.Copy(vd, videoData, 0, videoDataLen);
                        status = true;
                    }
                }
                return status;
            }
            catch (Exception e)
            {
                Console.WriteLine($"  [ОШИБКА DLL] {e.Message}");
            }
            finally
            {
                if (pd != IntPtr.Zero) Marshal.FreeHGlobal(pd);
                if (hd != IntPtr.Zero) Marshal.FreeHGlobal(hd);
                if (vd != IntPtr.Zero) Marshal.FreeHGlobal(vd);
            }
            return false;
        }

        public static bool ProcessAssetToImage(ref byte[] headerData, ref byte[] videoData, ref byte[] pixelData, out int imageWidth, out int imageHeight)
        {
            IntPtr hd = IntPtr.Zero, vd = IntPtr.Zero, pd = IntPtr.Zero;
            try
            {
                bool status = false;
                int headerDataLen = headerData.Length;
                int videoDataLen = videoData.Length;
                if (headerDataLen == 0 || videoDataLen == 0)
                {
                    imageWidth = 0;
                    imageHeight = 0;
                    return false;
                }

                hd = Marshal.AllocHGlobal(headerDataLen);
                Marshal.Copy(headerData, 0, hd, headerDataLen);
                vd = Marshal.AllocHGlobal(videoDataLen);
                Marshal.Copy(videoData, 0, vd, videoDataLen);

                int imageDataLen;
                int result = ConvertAssetToImage(hd, headerDataLen, vd, videoDataLen, IntPtr.Zero, out imageDataLen, out imageWidth, out imageHeight);
                if (result == 1)
                {
                    pd = Marshal.AllocHGlobal(imageDataLen);
                    result = ConvertAssetToImage(hd, headerDataLen, vd, videoDataLen, pd, out imageDataLen, out imageWidth, out imageHeight);
                    if (result == 1)
                    {
                        pixelData = new byte[imageDataLen];
                        Marshal.Copy(pd, pixelData, 0, imageDataLen);
                        status = true;
                    }
                }
                return status;
            }
            catch (Exception e)
            {
                Console.WriteLine($"  [ОШИБКА DLL] {e.Message}");
            }
            finally
            {
                if (pd != IntPtr.Zero) Marshal.FreeHGlobal(pd);
                if (hd != IntPtr.Zero) Marshal.FreeHGlobal(hd);
                if (vd != IntPtr.Zero) Marshal.FreeHGlobal(vd);
            }
            imageWidth = 0;
            imageHeight = 0;
            return false;
        }
    }
}
