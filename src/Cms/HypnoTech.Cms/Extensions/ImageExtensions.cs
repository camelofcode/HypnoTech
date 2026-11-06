using System.IO;
using SkiaSharp;

namespace HypnoTech.Cms.Extensions
{
    public static class ImageExtensions
    {
        public static byte[] Scale(this byte[] fileBytes, int maxWidth, int maxHeight)
        {
            using (var codec = SKCodec.Create(new MemoryStream(fileBytes)))
            {
                if (codec == null)
                {
                    return fileBytes;
                }

                var info = codec.Info;
                if (info.Width <= maxWidth && info.Height <= maxHeight)
                {
                    return fileBytes;
                }

                int width = info.Width;
                int height = info.Height;

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

                using (var originalBitmap = SKBitmap.Decode(codec))
                {
                    if (originalBitmap == null)
                    {
                        return fileBytes;
                    }

                    var imageInfo = new SKImageInfo(newWidth, newHeight, originalBitmap.ColorType, originalBitmap.AlphaType);
                    using (var resizedBitmap = originalBitmap.Resize(imageInfo, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear)))
                    {
                        if (resizedBitmap == null)
                        {
                            return fileBytes;
                        }

                        using (var image = SKImage.FromBitmap(resizedBitmap))
                        using (var data = image.Encode(codec.EncodedFormat, 100))
                        {
                            return data.ToArray();
                        }
                    }
                }
            }
        }
    }
}
