using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UserAccessManagement.Infrastructure.Helpers
{
    public class FileHelper : IFileHelper
    {
        public async Task<string> DownloadFileAsync(
            string sourceFilePath,
            string destinationFilePath = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                throw new ArgumentException($"'{nameof(sourceFilePath)}' cannot be null or whitespace.", nameof(sourceFilePath));
            }

            var uri = new Uri(sourceFilePath);
            var fileName = uri.LocalPath;
            var filePath = destinationFilePath ?? $"{Path.GetTempPath()}{Path.DirectorySeparatorChar}{Path.GetFileName(fileName)}";

            DeleteFileIfExists(filePath);

            using var client = new HttpClient();
            using var stream = await client.GetStreamAsync(sourceFilePath, cancellationToken);
            using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate);

            await stream.CopyToAsync(fileStream, cancellationToken);

            return filePath;
        }

        public string AddSuffix(string filePath, string suffix)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException($"'{nameof(filePath)}' cannot be null or whitespace.", nameof(filePath));
            }

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var result = filePath.Replace(fileName, $"{fileName}{suffix}");

            return result;
        }

        public void DeleteFileIfExists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException($"'{nameof(filePath)}' cannot be null or whitespace.", nameof(filePath));
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
