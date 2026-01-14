namespace Merge.Application.Interfaces;

public interface IFileUploadService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string folder = "products");
    Task<bool> DeleteImageAsync(string filePath);
    Task<string> GetImageUrlAsync(string filePath);
    Task<List<string>> UploadMultipleImagesAsync(List<(Stream stream, string fileName)> files, string folder = "products");
}

