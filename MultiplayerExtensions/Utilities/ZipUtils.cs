using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Utilities
{
    public static class ZipUtils
    {
        public const int MaxFileSystemPathLength = 259;
        /// <summary>
        /// Extracts a zip file to the specified directory. If an exception is thrown during extraction, it is stored in ZipExtractResult.
        /// </summary>
        /// <param name="extractDirectory">Directory to extract to</param>
        /// <param name="overwriteTarget">If true, overwrites existing files with the zip's contents</param>
        /// <returns></returns>
        public static async Task<ZipExtractResult> ExtractZip(Stream zipStream, string extractDirectory, bool overwriteTarget = true, string? sourcePath = null)
        {
            if (zipStream == null)
                throw new ArgumentNullException(nameof(zipStream));
            if (string.IsNullOrEmpty(extractDirectory))
                throw new ArgumentNullException(nameof(extractDirectory));

            ZipExtractResult result = new ZipExtractResult
            {
                SourceZip = sourcePath ?? "Stream",
                ResultStatus = ZipExtractResultStatus.Unknown
            };

            string? createdDirectory = null;
            List<string>? createdFiles = new List<string>();
            try
            {
                //Plugin.Log?.Info($"ExtractDirectory is {extractDirectory}");
                using (ZipArchive? zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                {
                    //Plugin.Log?.Info("Zip opened");
                    //extractDirectory = GetValidPath(extractDirectory, zipArchive.Entries.Select(e => e.Name).ToArray(), shortDirName, overwriteTarget);
                    int longestEntryName = zipArchive.Entries.Select(e => e.Name).Max(n => n.Length);
                    try
                    {
                        extractDirectory = Path.GetFullPath(extractDirectory); // Could theoretically throw an exception: Argument/ArgumentNull/Security/NotSupported/PathTooLong
                        extractDirectory = GetValidPath(extractDirectory, longestEntryName, 3);
                        if (!overwriteTarget && Directory.Exists(extractDirectory))
                        {
                            int pathNum = 2;
                            string finalPath;
                            do
                            {
                                string? append = $" ({pathNum})";
                                finalPath = GetValidPath(extractDirectory, longestEntryName, append.Length) + append; // padding ensures we aren't continuously cutting off the append value
                                pathNum++;
                            } while (Directory.Exists(finalPath));
                            extractDirectory = finalPath;
                        }
                    }
                    catch (PathTooLongException ex)
                    {
                        result.Exception = ex;
                        result.ResultStatus = ZipExtractResultStatus.DestinationFailed;
                        return result;
                    }
                    result.OutputDirectory = extractDirectory;
                    bool extractDirectoryExists = Directory.Exists(extractDirectory);
                    string? toBeCreated = extractDirectoryExists ? null : extractDirectory; // For cleanup
                    try { Directory.CreateDirectory(extractDirectory); }
                    catch (Exception ex)
                    {
                        result.Exception = ex;
                        result.ResultStatus = ZipExtractResultStatus.DestinationFailed;
                        return result;
                    }

                    result.CreatedOutputDirectory = !extractDirectoryExists;
                    createdDirectory = string.IsNullOrEmpty(toBeCreated) ? null : extractDirectory;
                    // Ordering so largest files extracted first. If the extraction is interrupted, theoretically the song's hash won't match Beat Saver's.
                    foreach (ZipArchiveEntry entry in zipArchive.Entries.OrderByDescending(e => e.Length))
                    {
                        if (!entry.FullName.Equals(entry.Name)) // If false, the entry is a directory or file nested in one
                            continue;
                        string entryPath = Path.Combine(extractDirectory, entry.Name);
                        bool fileExists = File.Exists(entryPath);
                        if (overwriteTarget || !fileExists)
                        {
                            try
                            {
                                using (var fs = File.OpenWrite(entryPath))
                                {
                                    await entry.Open().CopyToAsync(fs);
                                }
                                createdFiles.Add(entryPath);
                            }
                            catch (InvalidDataException ex) // Entry is missing, corrupt, or compression method isn't supported
                            {
                                Plugin.Log?.Error($"Error extracting {extractDirectory}, archive appears to be damaged.");
                                Plugin.Log?.Error(ex);
                                result.Exception = ex;
                                result.ResultStatus = ZipExtractResultStatus.SourceFailed;
                                result.ExtractedFiles = createdFiles.ToArray();
                            }
                            catch (Exception ex)
                            {
                                Plugin.Log?.Error($"Error extracting {extractDirectory}");
                                Plugin.Log?.Error(ex);
                                result.Exception = ex;
                                result.ResultStatus = ZipExtractResultStatus.DestinationFailed;
                                result.ExtractedFiles = createdFiles.ToArray();

                            }
                            if (result.Exception != null)
                            {
                                foreach (string? file in createdFiles)
                                {
                                    TryDeleteAsync(file).Wait();
                                }
                                return result;
                            }
                        }
                    }
                    result.ExtractedFiles = createdFiles.ToArray();
                }
                result.ResultStatus = ZipExtractResultStatus.Success;
                return result;
#pragma warning disable CA1031 // Do not catch general exception types
            }
            catch (InvalidDataException ex) // FileStream is not in the zip archive format.
            {
                result.ResultStatus = ZipExtractResultStatus.SourceFailed;
                result.Exception = ex;
                return result;
            }
            catch (Exception ex) // If exception is thrown here, it probably happened when the FileStream was opened.
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Plugin.Log?.Error($"Error extracting zip from {sourcePath ?? "Stream"}");
                Plugin.Log?.Error(ex);
                try
                {
                    if (!string.IsNullOrEmpty(createdDirectory))
                    {
                        Directory.Delete(createdDirectory, true);
                    }
                    else // TODO: What is this doing here...
                    {
                        foreach (string? file in createdFiles)
                        {
                            File.Delete(file);
                        }
                    }
                }
                catch (Exception cleanUpException)
                {
                    // Failed at cleanup
                    Plugin.Log?.Debug($"Failed to clean up zip file: {cleanUpException.Message}");
                }

                result.Exception = ex;
                result.ResultStatus = ZipExtractResultStatus.SourceFailed;
                return result;
            }
        }

        private static char[] InvalidTrailingPathChars = new char[] { ' ', '.', '-' };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="extractDirectory"></param>
        /// <param name="longestEntryName"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        /// <exception cref="PathTooLongException">Thrown if shortening the path enough is impossible.</exception>
        public static string GetValidPath(string extractDirectory, int longestEntryName, int padding = 0)
        {
            int extLength = extractDirectory.Length;
            DirectoryInfo? dir = new DirectoryInfo(extractDirectory);
            int minLength = dir.Parent.FullName.Length + 2;
            string? dirName = dir.Name;
            int diff = MaxFileSystemPathLength - extLength - longestEntryName - padding;
            if (diff < 0)
            {

                if (dirName.Length + diff > 0)
                {
                    //Logger.log?.Warn($"{extractDirectory} is too long, attempting to shorten.");
                    extractDirectory = extractDirectory.Substring(0, minLength + dirName.Length + diff);
                }
                else
                {
                    //Logger.log?.Error($"{extractDirectory} is too long, couldn't shorten enough.");
                    throw new PathTooLongException(extractDirectory);
                }
            }
            return extractDirectory.TrimEnd(InvalidTrailingPathChars);
        }

        public static Task<bool> TryDeleteAsync(string filePath)
        {
            CancellationTokenSource? timeoutSource = new CancellationTokenSource(3000);
            CancellationToken timeoutToken = timeoutSource.Token;
            return WaitUntil(() =>
            {
                try
                {
                    File.Delete(filePath);
                    timeoutSource.Dispose();
                    return true;
                }
                catch (Exception)
                {
                    timeoutSource.Dispose();
                    throw;
                }
            }, 25, timeoutToken);
        }


        /// <summary>
        /// Waits until the provided condition function returns true or the cancellationToken is triggered.
        /// Poll rate is in milliseconds. Returns false if cancellationToken is triggered.
        /// WARNING: If this task doesn't complete or get cancelled it will run until the program ends.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="milliseconds"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> WaitUntil(Func<bool> condition, int milliseconds, CancellationToken cancellationToken)
        {
            while (!(condition?.Invoke() ?? true))
            {
                await Task.Yield();
                if (cancellationToken.CanBeCanceled && cancellationToken.IsCancellationRequested)
                    return false;
                await Task.Delay(milliseconds).ConfigureAwait(false);
            }
            return true;
        }



        public class ZipExtractResult
        {
            public string? SourceZip { get; set; }
            public string? OutputDirectory { get; set; }
            public bool CreatedOutputDirectory { get; set; }
            public string[]? ExtractedFiles { get; set; }
            public ZipExtractResultStatus ResultStatus { get; set; }
            public Exception? Exception { get; set; }
        }

        public enum ZipExtractResultStatus
        {
            /// <summary>
            /// Extraction hasn't been attempted.
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// Extraction was successful.
            /// </summary>
            Success = 1,
            /// <summary>
            /// Problem with the zip source.
            /// </summary>
            SourceFailed = 2,
            /// <summary>
            /// Problem with the destination target.
            /// </summary>
            DestinationFailed = 3
        }
    }
}
