// Adapted from AuroraAssetEditor - InternetArchiveAssetDownloader.cs
// Downloads game covers from Internet Archive (xboxunity-covers-fulldump)

namespace AuroraAssetsUsbDownloader.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    internal class CoverDownloader
    {
        private const string BaseUrl = "https://archive.org/download/xboxunity-covers-fulldump_202311/xboxunity-covers-fulldump/";
        private static readonly HttpClient _client = new HttpClient() { Timeout = TimeSpan.FromSeconds(30) };

        /// <summary>
        /// Downloads the best available boxart PNG for a given TitleID
        /// </summary>
        public static async Task<Image?> DownloadBoxart(string titleId)
        {
            string titleFolder = titleId.ToUpperInvariant();
            string folderUrl = $"{BaseUrl}{titleFolder}/";

            try
            {
                // Get the directory listing to find sub-folders
                string htmlContent = await _client.GetStringAsync(folderUrl);
                var subDirs = ParseDirectoriesFromHtml(htmlContent);

                if (subDirs.Count == 0)
                {
                    Console.WriteLine($"    Нет обложек на archive.org для {titleId}");
                    return null;
                }

                // Try each subfolder until we find a boxart
                foreach (var subDir in subDirs)
                {
                    try
                    {
                        string imageUrl = $"{BaseUrl}{titleFolder}/{subDir}/boxart.png";
                        var imageData = await _client.GetByteArrayAsync(imageUrl);
                        var ms = new MemoryStream(imageData);
                        return Image.FromStream(ms);
                    }
                    catch
                    {
                        // Try front.png as fallback
                        try
                        {
                            string imageUrl = $"{BaseUrl}{titleFolder}/{subDir}/front.png";
                            var imageData = await _client.GetByteArrayAsync(imageUrl);
                            var ms = new MemoryStream(imageData);
                            return Image.FromStream(ms);
                        }
                        catch { continue; }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Title not found on archive.org
                // This is not an error we need to log to error.log, we can just return null.
                return null;
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception($"Таймаут при скачивании {titleId}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка скачивания {titleId}: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Downloads the icon/front image for a given TitleID
        /// </summary>
        public static async Task<Image?> DownloadIcon(string titleId)
        {
            string titleFolder = titleId.ToUpperInvariant();
            string folderUrl = $"{BaseUrl}{titleFolder}/";

            try
            {
                string htmlContent = await _client.GetStringAsync(folderUrl);
                var subDirs = ParseDirectoriesFromHtml(htmlContent);

                foreach (var subDir in subDirs)
                {
                    try
                    {
                        string imageUrl = $"{BaseUrl}{titleFolder}/{subDir}/front.png";
                        var imageData = await _client.GetByteArrayAsync(imageUrl);
                        var ms = new MemoryStream(imageData);
                        return Image.FromStream(ms);
                    }
                    catch { continue; }
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Parses directory names from archive.org HTML listing
        /// </summary>
        private static List<string> ParseDirectoriesFromHtml(string html)
        {
            var dirs = new List<string>();
            // archive.org directory listing format: <a href="DIRNAME/">DIRNAME/</a>
            var regex = new Regex(@"<a\s+href=""(\d+)/"">\d+/</a>", RegexOptions.IgnoreCase);
            var matches = regex.Matches(html);
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                    dirs.Add(match.Groups[1].Value);
            }
            return dirs;
        }
    }
}
