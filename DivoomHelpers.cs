using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Text;
using System.Text.Json;

internal static class DivoomHelpers
{

    public static StringContent CreateResponseContent(string error)
    {
        return new StringContent(
            error, // The error message content
            Encoding.UTF8, // Encoding for the message
            "application/json" // Media type for the response (can be application/json, text/plain, etc.)
        );
    }

    public static int GetFrameTime(Bitmap gif, int frame)
    {
        if (gif is null) return 0;

        FrameDimension dimension = new(gif.FrameDimensionsList[0]);
        gif.SelectActiveFrame(dimension, frame);
        PropertyItem? frameProperty = gif.GetPropertyItem(20736);
        if (frameProperty is null) return 0;
        byte[]? delayPropertyBytes = frameProperty.Value;
        if (delayPropertyBytes is null) return 0;

        int frameDelay = BitConverter.ToInt32(delayPropertyBytes, frame * 4) * 10;
        return frameDelay;
    }

    public static byte[][] GetRgbBytes(Bitmap gif)
    {
        FrameDimension dimension = new FrameDimension(gif.FrameDimensionsList[0]);
        // Number of frames
        int frameCount = gif.GetFrameCount(dimension);
        int sizeOfRgbData = gif.Width * gif.Height * 3;
        byte[][] rgbData = new byte[frameCount][];

        for (int curFrame = 0; curFrame < frameCount; ++curFrame)
        {
            // Operate 1 frame at a time, initially each frame as we go
            gif.SelectActiveFrame(dimension, curFrame);
            rgbData[curFrame] = new byte[sizeOfRgbData];

            // Lock the bitmap data for direct access
            BitmapData bitmapData = gif.LockBits(
                new Rectangle(0, 0, gif.Width, gif.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            // Calculate the size of the RGB data
            int stride = Math.Abs(bitmapData.Stride);
            int bgraDataSize = stride * gif.Height;

            // Copy the RGB data into a byte array
            byte[] bgraData = new byte[bgraDataSize];
            System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, bgraData, 0, bgraDataSize);

            // Unlock the bitmap data
            gif.UnlockBits(bitmapData);

            // Rearrange BGR to RGB
            int rgbPixel = 0;
            for (int i = 0; i < bgraData.Length; i += 4)
            {
                rgbData[curFrame][rgbPixel] = bgraData[i + 2];
                rgbData[curFrame][rgbPixel + 1] = bgraData[i + 1];
                rgbData[curFrame][rgbPixel + 2] = bgraData[i];
                rgbPixel += 3;
            }
        }
        return rgbData;
    }
    public static string[] ConvertImageToBase64(Bitmap gif)
    {
        try
        {
            // Get raw RGB bytes
            byte[][] rgbData = GetRgbBytes(gif);

            string[] strings = new string[rgbData.Length];
            // Convert to Base64
            for (int i = 0; i < rgbData.Length; i++)
            {
                strings[i] = Convert.ToBase64String(rgbData[i]);
            }
            return strings;
        }
        catch (Exception ex)
        {
            return ["Exception: " + ex.Message];
        }
    }

    public static bool IsValidImageSize(Bitmap gif)
    {
        return (gif.Width == 64 && gif.Height == 64) ||
                (gif.Width == 32 && gif.Height == 32) ||
                 (gif.Width == 16 && gif.Height == 16);
    }

    public static StringContent MakePayload(Object content)
    {
        StringContent payload = new(
            JsonSerializer.Serialize(content),
            Encoding.UTF8,
            "application/json");
        return payload;
    }
}