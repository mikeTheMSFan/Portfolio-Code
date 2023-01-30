#region Imports

using System.Runtime.InteropServices;

#endregion

namespace Portfolio.Services.Interfaces;

public interface IMWSImageService
{
    public Task<byte[]> CreateThumbnailAsync(IFormFile image);
    string DecodeImage(byte[] image, string contentType);
    Task<byte[]> EncodeImageAsync(IFormFile image);
    Task<byte[]> EncodeImageURLAsync(string imageURL);
    public Task<string> GetBase64ExternalLoginImageAsync([Optional] string token);
    public Task<FormFile> GetFormFileAsync(string base64Image);
}