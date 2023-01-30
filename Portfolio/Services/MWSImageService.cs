#region Imports

using Portfolio.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

#endregion

namespace Portfolio.Services;

public class MWSImageService : IMWSImageService
{
    private readonly IHttpClientFactory _httpClient;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public MWSImageService(IHttpClientFactory httpClient,
        IWebHostEnvironment webHostEnvironment)
    {
        _httpClient = httpClient;
        _webHostEnvironment = webHostEnvironment;
    }

    #region Decode image

    public string DecodeImage(byte[] image, string contentType)
    {
        if (image == default!) return null!;

        var ms = new MemoryStream(image);
        var sharpImage = Image.Load(ms, out var format);
        var result = sharpImage.ToBase64String(format);

        return result;
    }

    #endregion

    #region Encode image

    public async Task<byte[]> EncodeImageAsync(IFormFile image)
    {
        if (image == default!) return null!;
        var imageStream = new MemoryStream();
        var sharpImage = Image.Load(image.OpenReadStream(), out var format);
        await sharpImage.SaveAsPngAsync(imageStream);

        return imageStream.ToArray();
    }

    #endregion

    #region Encode image URL

    public async Task<byte[]> EncodeImageURLAsync(string imageURL)
    {
        var client = _httpClient.CreateClient();
        var response = await client.GetAsync(imageURL);
        using var stream = await response.Content.ReadAsStreamAsync();

        var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }

    #endregion

    #region Get Base64 External login Image

    public async Task<string> GetBase64ExternalLoginImageAsync(string profileImageUrl)
    {
        var result = string.Empty;
        if (string.IsNullOrEmpty(profileImageUrl) == false)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(profileImageUrl);
            var stream = await response.Content.ReadAsStreamAsync();
            var image = Image.Load(stream, out var format);
            result = image.ToBase64String(format);
        }
        else
        {
            var webRootPath = _webHostEnvironment.WebRootPath;
            var imagePath = "imgs\\defaultProfilePicture.png";
            var defaultProfilePicturePath = Path.Combine(webRootPath, imagePath);
            var image = Image.Load(defaultProfilePicturePath, out var format);
            result = image.ToBase64String(format);
        }

        return result;
    }

    #endregion

    #region Convert Base64 Image to byte array

    public async Task<FormFile> GetFormFileAsync(string base64Image)
    {
        var imageBytes = Convert.FromBase64String(base64Image);
        var ms = new MemoryStream(imageBytes);

        Image.Load(ms, out var format);
        await using var stream = File.OpenWrite($"profileImage.{format.FileExtensions.FirstOrDefault()}");

        var result = new FormFile(ms, 0, ms.Length, null!, Path.GetFileName(stream.Name))
        {
            Headers = new HeaderDictionary(),
            ContentType = format.DefaultMimeType
        };

        return result;
    }

    public async Task<byte[]> CreateThumbnailAsync(IFormFile image)
    {
        var originalImage = Image.Load(image.OpenReadStream(), out var format);
        originalImage.Mutate(x => x.Resize(100, 100));
        var ms = new MemoryStream();

        await originalImage.SaveAsync(ms, format);
        var result = ms.ToArray();

        return result;
    }

    #endregion
}