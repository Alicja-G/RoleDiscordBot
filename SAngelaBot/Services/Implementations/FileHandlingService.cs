using Polly;
using SAngelaBot.Models;
using SAngelaBot.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SAngelaBot.Services.Implementations
{
    public class FileHandlingService : IFileHandlingService
    {
        public IEnumerable<string> ReadFromFile(string fullPath)
        {
            if (File.Exists(fullPath))
                return File.ReadAllLines(fullPath);
            return new string[] { };
        }

        public async Task<OperationResultModel> RefreshFileAsync(string filename, string[] content)
        {
            var result = new OperationResultModel { IsSuccessful = true };

            var policy = Policy
                .Handle<IOException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            var outcome = await policy.ExecuteAndCaptureAsync(async () => await ResetFileAndWrite(filename, content));
            var exception = outcome.FinalException;
            if (exception != null)
            {
                result.IsSuccessful = false;
                result.Message = exception.Message;
            }

            return result;
        }

        public async Task<OperationResultModel> RemoveLineFromFileAsync(string filename, string content)
        {
            var result = new OperationResultModel { IsSuccessful = true };

            var policy = Policy
                .Handle<IOException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            var outcome = await policy.ExecuteAndCaptureAsync(async () => await TruncateFile(filename, content));
            var exception = outcome.FinalException;
            if (exception != null)
            {
                result.IsSuccessful = false;
                result.Message = exception.Message;
            }

            return result;
        }


        public async Task<OperationResultModel> RemoveLineWithContentInIndexPlaceAsync(string filename, string content, int index)
        {
            var result = new OperationResultModel { IsSuccessful = true };

            var policy = Policy
                .Handle<IOException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            var outcome = await policy.ExecuteAndCaptureAsync(async () => await TruncateFileWithStartingContentOnPostion(filename, content, index));
            var exception = outcome.FinalException;
            if (exception != null)
            {
                result.IsSuccessful = false;
                result.Message = exception.Message;
            }

            return result;
        }

        public async Task<OperationResultModel> WriteToFileAsync(string filename, string[] content)
        {
            var result = new OperationResultModel { IsSuccessful = true };
            var policy = Policy
                .Handle<IOException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            var outcome = await policy.ExecuteAndCaptureAsync(async () => await AppendFile(filename, content));
            var exception = outcome.FinalException;
            if (exception != null)
            {
                result.IsSuccessful = false;
                result.Message = exception.Message;
            }

            return result;
        }

        public async Task<OperationResultModel> ClearFileAsync(string fileName)
        {
            var result = new OperationResultModel { IsSuccessful = true };
            var policy = Policy
                .Handle<IOException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            var outcome = await policy.ExecuteAndCaptureAsync(async () => await DeleteFile(fileName));
            var exception = outcome.FinalException;
            if (exception != null)
            {
                result.IsSuccessful = false;
                result.Message = exception.Message;
            }

            return result;
        }

        private async Task DeleteFile(string fullPath)
        {
            try
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            catch (IOException e)
            {
                throw e;
            }
        }

        private async Task AppendFile(string fullPath, string[] arg)
        {
            try
            {
                File.AppendAllLines(fullPath, arg);
            }
            catch (IOException e)
            {
                throw e;
            }
        }



        private async Task ResetFileAndWrite(string fullPath, string[] arg)
        {
            try
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
                File.AppendAllLines(fullPath, arg);
            }
            catch (IOException e)
            {
                throw e;
            }
        }
        private async Task TruncateFileWithStartingContentOnPostion(string filename, string content, int index)
        {
            try
            {
                var lines = File.ReadAllLines(filename);
                var newLines = new List<string>();
                foreach (var line in lines)
                {
                    var splitted = line.Split("|");
                    var searchedInPart = splitted[index];

                    if (searchedInPart != content)
                        newLines.Add(line);
                }
                await ResetFileAndWrite(filename, newLines.ToArray());
            }
            catch (IOException e)
            {
                throw e;
            }
        }

        private async Task TruncateFile(string fullPath, string arg)
        {
            try
            {
                var lines = File.ReadAllLines(fullPath);
                var newLines = new List<string>();
                foreach (var line in lines)
                {
                    if (line.Trim() != arg.Trim())
                        newLines.Add(line);
                }
                await ResetFileAndWrite(fullPath, newLines.ToArray());
            }
            catch (IOException e)
            {
                throw e;
            }
        }


    }
}
