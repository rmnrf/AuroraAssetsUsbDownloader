using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AuroraAssetsUsbDownloader.Classes
{
    internal class XboxLiveCatalog
    {
        private static readonly HttpClient _client = new HttpClient() { Timeout = TimeSpan.FromSeconds(30) };

        public class CatalogAssets
        {
            public List<string> Icons = new();
            public List<string> Backgrounds = new();
            public List<string> Banners = new();
            public List<string> Screenshots = new();
        }

        public static async Task<CatalogAssets?> GetAssets(string titleId, string locale = "en-US")
        {
            var url = $"http://catalog-cdn.xboxlive.com/Catalog/Catalog.asmx/Query?methodName=FindGames&Names=Locale&Values={locale}&Names=LegalLocale&Values={locale}&Names=Store&Values=1&Names=PageSize&Values=100&Names=PageNum&Values=1&Names=DetailView&Values=5&Names=OfferFilterLevel&Values=1&Names=MediaIds&Values=66acd000-77fe-1000-9115-d802{titleId}&Names=UserTypes&Values=2&Names=MediaTypes&Values=1&Names=MediaTypes&Values=21&Names=MediaTypes&Values=23&Names=MediaTypes&Values=37&Names=MediaTypes&Values=46";
            
            try
            {
                var xmlString = await _client.GetStringAsync(url);
                var doc = XDocument.Parse(xmlString);
                XNamespace live = "http://www.live.com/marketplace";
                
                var result = new CatalogAssets();

                // Screenshots
                var slideshows = doc.Descendants(live + "slideShows");
                foreach (var slideshow in slideshows)
                {
                    var fileUrls = slideshow.Descendants(live + "fileUrl");
                    foreach (var fileUrl in fileUrls)
                    {
                        result.Screenshots.Add(fileUrl.Value);
                    }
                }

                // Images
                var images = doc.Descendants(live + "image");
                foreach (var img in images)
                {
                    var relType = img.Element(live + "relationshipType")?.Value;
                    var fileUrl = img.Element(live + "fileUrl")?.Value;
                    
                    if (!string.IsNullOrEmpty(relType) && !string.IsNullOrEmpty(fileUrl))
                    {
                        if (relType == "15" || relType == "23") result.Icons.Add(fileUrl);
                        else if (relType == "25") result.Backgrounds.Add(fileUrl);
                        else if (relType == "27") result.Banners.Add(fileUrl);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка XboxLiveCatalog {titleId}: {ex.Message}", ex);
            }
        }

        public static async Task<Image?> DownloadImage(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            try
            {
                var data = await _client.GetByteArrayAsync(url);
                return Image.FromStream(new MemoryStream(data));
            }
            catch { return null; }
        }
    }
}

