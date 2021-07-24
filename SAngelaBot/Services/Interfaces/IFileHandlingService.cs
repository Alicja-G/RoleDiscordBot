using SAngelaBot.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SAngelaBot.Services.Interfaces
{
    public interface IFileHandlingService
    {
        Task<OperationResultModel> WriteToFileAsync(string filename, string[] content);
        IEnumerable<string> ReadFromFile(string filename);
        Task<OperationResultModel> RefreshFileAsync(string filename, string[] content);
        Task<OperationResultModel> RemoveLineFromFileAsync(string filename, string content);
        Task<OperationResultModel> RemoveLineWithContentInIndexPlaceAsync(string filename, string startingContent, int index);
        Task<OperationResultModel> ClearFileAsync(string fileName);
    }
}
