using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace HypnoTech.Cms.Extensions
{
    public static class ImageExtensions
    {
        public static byte[] Scale(this byte[] fileBytes, int maxWidth, int maxHeight)
        {
            MemoryStream fullSizeStream = new MemoryStream(fileBytes);
            using (Image fullsizeImage = Image.Load<Rgba32>(fullSizeStream, out var format))
            {
                if ((fullsizeImage.Width <= maxWidth && fullsizeImage.Height <= maxHeight))
                {
                    return fileBytes;
                }

                int width = fullsizeImage.Width;
                int height = fullsizeImage.Height;

                int newWidth = 0;
                int newHeight = 0;
                double ratio;

                if (width > maxWidth)
                {
                    ratio = width / (double)maxWidth;
                    newWidth = maxWidth;

                    newHeight = (int)(height / ratio);
                }
                else if (height > maxHeight)
                {
                    ratio = height / (double)maxHeight;
                    newHeight = maxHeight;

                    newWidth = (int)(width / ratio);
                }

                if (newHeight < 1)
                {
                    newHeight = 1;
                }
                if (newWidth < 1)
                {
                    newWidth = 1;
                }

                if (newWidth > maxWidth)
                {
                    ratio = newWidth / (double)maxWidth;
                    newWidth = maxWidth;

                    newHeight = (int)(newHeight / ratio);
                }
                else if (newHeight > maxHeight)
                {
                    ratio = newHeight / (double)maxHeight;
                    newHeight = maxHeight;

                    newWidth = (int)(newWidth / ratio);
                }

                if (newHeight < 1)
                {
                    newHeight = 1;
                }
                if (newWidth < 1)
                {
                    newWidth = 1;
                }

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    fullsizeImage.Mutate(p => p.Resize(newWidth, newHeight));
                    fullsizeImage.Save(memoryStream, format);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
