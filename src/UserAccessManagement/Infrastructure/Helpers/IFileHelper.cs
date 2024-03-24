using System.Threading.Tasks;
using System.Threading;

namespace UserAccessManagement.Infrastructure.Helpers
{
    public interface IFileHelper
    {
        Task<string> DownloadFileAsync(
            string sourceFilePath,
            string destinationFilePath = null,
            CancellationToken cancellationToken = default);

        string AddSuffix(string filePath, string suffix);

        void DeleteFileIfExists(string filePath);
    }
}
