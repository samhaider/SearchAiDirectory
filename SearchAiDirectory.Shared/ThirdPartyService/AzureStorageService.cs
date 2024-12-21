using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace SearchAiDirectory.Shared.ThirdPartyService;

public static class AzureStorageService
{
    private static readonly string connectionString = "DefaultEndpointsProtocol=https;AccountName=searchaidirectory;AccountKey=rxQWytK2f+4ErLBaLvK2AzjQ9aTBOWfwiwaXq7rJKv6hdo5jgYHxMzYNqIYMxwNoW9lObhp24VvU+AStWB9BaQ==;EndpointSuffix=core.windows.net";
    private static readonly BlobServiceClient storageClient = new(connectionString);

    public static async Task<Stream> GetBlobFileStream(BlobContainers container, string fileName)
    {
        string containerName = container == BlobContainers.tools ? nameof(BlobContainers.tools) :
            container == BlobContainers.blog ? nameof(BlobContainers.blog) :
            string.Empty;

        if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(fileName)) return null;

        var blobContainer = storageClient.GetBlobContainerClient(containerName);
        var blobClient = blobContainer.GetBlobClient(fileName);
        return await blobClient.OpenReadAsync();
    }

    public static async Task<string> UploadBlobFiles(BlobContainers container, string fileName, byte[] file)
    {
        string containerName = container == BlobContainers.tools ? nameof(BlobContainers.tools) :
            container == BlobContainers.blog ? nameof(BlobContainers.blog) :
            string.Empty;

        if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(fileName)) return null;

        var blobContainer = storageClient.GetBlobContainerClient(containerName);
        var blob = blobContainer.GetBlobClient(fileName);

        await blob.UploadAsync(
            new BinaryData(file),
            new BlobUploadOptions()
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = GetContentType(fileName) }
            });
        return blob.Uri.AbsoluteUri;
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".tiff" or ".tif" => "image/tiff",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".flac" => "audio/flac",
            ".aac" => "audio/aac",
            ".m4a" => "audio/mp4",
            ".wma" => "audio/x-ms-wma",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".wmv" => "video/x-ms-wmv",
            ".flv" => "video/x-flv",
            ".mkv" => "video/x-matroska",
            ".webm" => "video/webm",
            ".m4v" => "video/x-m4v",
            ".mpeg" or ".mpg" => "video/mpeg",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            ".rtf" => "application/rtf",
            ".odt" => "application/vnd.oasis.opendocument.text",
            ".ods" => "application/vnd.oasis.opendocument.spreadsheet",
            ".odp" => "application/vnd.oasis.opendocument.presentation",
            ".csv" => "text/csv",
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".yaml" or ".yml" => "text/yaml",
            ".xhtml" => "application/xhtml+xml",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".tar" => "application/x-tar",
            ".gz" => "application/gzip",
            ".7z" => "application/x-7z-compressed",
            ".exe" => "application/vnd.microsoft.portable-executable",
            ".msi" => "application/x-msdownload",
            ".apk" => "application/vnd.android.package-archive",
            ".bin" => "application/octet-stream",
            ".dmg" => "application/x-apple-diskimage",
            ".py" => "text/x-python",
            ".java" => "text/x-java-source",
            ".c" => "text/x-c",
            ".cpp" => "text/x-c++",
            ".cs" => "text/plain",
            ".sh" => "application/x-sh",
            ".php" => "application/x-httpd-php",
            ".rb" => "application/x-ruby",
            ".ttf" => "font/ttf",
            ".otf" => "font/otf",
            ".woff" => "font/woff",
            ".woff2" => "font/woff2",
            _ => "application/octet-stream",
        };
    }
}

public enum BlobContainers
{
    tools,
    blog
}