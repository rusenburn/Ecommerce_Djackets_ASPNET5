using System.Drawing;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Shared.Services.ServiceInterfaces
{
    public interface IImageService
    {
         Task<byte[]> GetImageAsync(string imageName);
         Task<string> AddImageAsync(Image image,bool asThumbnail=false);
         Task<bool> DeleteImageAsync(string imagePath);
         Task<string> UpdateImageAsync(Image image, string oldImage,bool asThumbnail=false);

         bool IsValidImage(IFormFile image);
    }
}