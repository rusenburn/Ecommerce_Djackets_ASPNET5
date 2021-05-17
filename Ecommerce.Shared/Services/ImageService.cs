
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

using System.Threading.Tasks;
using Ecommerce.Shared.Services.ServiceInterfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Shared.Services
{
    public class ImageService : IImageService
    {
        private readonly string _mediaPath;

        public ImageService(IWebHostEnvironment env)
        {
            _mediaPath = Path.Combine(env.WebRootPath, "MediaFiles");
        }

        public async Task<byte[]> GetImageAsync(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                throw new ArgumentNullException(nameof(imageName));
            }
            byte[] dataBytes = new byte[0];
            string totalPath = Path.Combine(_mediaPath, imageName);
            if (File.Exists(totalPath))
            {
                dataBytes = await File.ReadAllBytesAsync(totalPath);
            }

            return dataBytes;
        }

        public async Task<string> AddImageAsync(Image image, bool asThumbnail = false)
        {
            try
            {
                await EnsureDirectoryIsCreatedAsync();
                Rectangle destinationRectangle;
                int width,height;
                string fileName;
                Guid guid = Guid.NewGuid();
                if (asThumbnail)
                {
                    destinationRectangle = GetDestinationImageRectangle(image, 400, 300);
                    width = 400;
                    height = 300;
                    fileName = $"thumbnail_{DateTime.UtcNow:dd-MM-yyyy-HH-mm-ss}_{guid}.jpg";
                }
                else
                {
                    destinationRectangle = GetDestinationImageRectangle(image, 600, 450);
                    width = 600;
                    height = 450;
                    fileName = $"img_{DateTime.UtcNow:dd-MM-yyyy-HH-mm-ss}_{guid}.jpg";
                }
                using var bitmap = ResizeImage(image, width, height, destinationRectangle);
                string totalPath = Path.Combine(_mediaPath, fileName);
                VeryQualityLevelSaving(bitmap, totalPath);
                return await Task.FromResult(fileName);
            }
            catch
            {
                return "";
            }
        }

        public async Task<bool> DeleteImageAsync(string imagePath)
        {
            try
            {
                string path = Path.Combine(_mediaPath, imagePath);
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return await Task.FromResult(true);
                }
                return false;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public async Task<string> UpdateImageAsync(Image image, string oldImage , bool asThumbnail = false)
        {
            if (image is null) throw new ArgumentNullException(nameof(image));
            string newImage = await AddImageAsync(image,asThumbnail);
            if (newImage is null) return oldImage; // something went wrong we could not save the new image
            else if (!string.IsNullOrEmpty(oldImage)) // if there is an old image  delete it
            {
                await DeleteImageAsync(oldImage);
            }
            return newImage;
        }

        public bool IsValidImage(IFormFile image)
        {
            if (image is null ||
                image.Length > 2L * 1024 * 1024 ||
                image.ContentType is null ||
                image.ContentType.Split('/')[0].ToLower() != "image"
                )
                return false;
            else return true;
        }

        private async Task EnsureDirectoryIsCreatedAsync()
        {
            if (!Directory.Exists(_mediaPath))
            {
                await Task.FromResult(Directory.CreateDirectory(_mediaPath));
            }
        }


        // https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp

        private static Bitmap ResizeImage(Image image, int width, int height, Rectangle? destinationRectangle = null)
        {
            Rectangle destRect = destinationRectangle ?? new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using Graphics graphics = Graphics.FromImage(destImage);

            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            graphics.Clear(Color.Empty);
            using var wrapMode = new ImageAttributes();
            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            return destImage;
        }

        private Rectangle GetDestinationImageRectangle(Image image, int newWidth, int newHeight)
        {
            int sourceWidth = image.Width;
            int sourceHeight = image.Height;

            //Consider vertical pics
            if (sourceWidth < sourceHeight)
            {
                int buff = newWidth;

                newWidth = newHeight;
                newHeight = buff;
            }

            int destX = 0, destY = 0;
            float nPercent = 0, nPercentW = 0, nPercentH = 0;

            nPercentW = ((float)newWidth / (float)sourceWidth);
            nPercentH = ((float)newHeight / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((newWidth -
                          (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((newHeight -
                          (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Rectangle destRectangle = new Rectangle(destX, destY, destWidth, destHeight);
            return destRectangle;
        }

        // https://docs.microsoft.com/en-us/dotnet/desktop/winforms/advanced/how-to-set-jpeg-compression-level?view=netframeworkdesktop-4.8

        private void VeryQualityLevelSaving(Bitmap bmp1, string path)
        {
            // ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            Encoder myEncoder = Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            bmp1.Save(path, jpgEncoder, myEncoderParameters);
            // bmp1.Dispose();
        }
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}