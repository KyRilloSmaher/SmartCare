using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.ExternalServiceInterfaces
{
    public interface IImageUploaderService
    {
        Task<string> UploadImageAsync(Stream fileStream, string fileName);
        Task<bool> DeleteImageAsync(string publicId);
        Task<ImageUploadResult> UploadImageAsync(IFormFile imageFile, ImageFolder imageType);
        Task<bool> DeleteImageByUrlAsync(string imageUrl);
        Task<IEnumerable<ImageUploadResult>> UploadMultipleImagesAsync(IEnumerable<IFormFile> imageFiles, ImageFolder imageType);
        Task<string> GetImageUrlAsync(string imageName);
    }
}
