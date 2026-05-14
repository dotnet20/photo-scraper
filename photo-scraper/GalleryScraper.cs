using HtmlAgilityPack;
using photo_scraper;

public class GalleryScraper
{
    private readonly HttpClient _httpClient;
    private readonly HtmlWeb _webLoader;

    public GalleryScraper(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _webLoader = new HtmlWeb { UserAgent = AppConfig.UserAgent };
    }

    public async Task RunAsync()
    {
        int currentPageIndex = 1;
        bool isDiscoveryActive = true;

        Console.WriteLine($"Starting crawler. Goal: Download galleries from {AppConfig.MinimumRequiredYear} onwards.");

        while (isDiscoveryActive)
        {
            string pageRequestUrl = currentPageIndex == 1 ? AppConfig.GalleryBaseUrl : $"{AppConfig.GalleryBaseUrl}page/{currentPageIndex}/";
            Console.WriteLine($"\n Scanning page {currentPageIndex}: {pageRequestUrl}");

            var document = await _webLoader.LoadFromWebAsync(pageRequestUrl);

            if (IsResponseInvalid(document))
            {
                Console.WriteLine("Reached the end of the archive or received an invalid response.");
                break;
            }

            var galleryLinkElements = document.DocumentNode.SelectNodes("//a[contains(@class, 'masonry-item')]");
            if (galleryLinkElements == null)
            {
                Console.WriteLine("No gallery items found on this page.");
                break;
            }

            int processedGalleriesOnPage = 0;

            foreach (var linkElement in galleryLinkElements)
            {
                string galleryUrl = linkElement.GetAttributeValue("href", string.Empty);
                string rawTitle = linkElement.SelectSingleNode(".//h2[@class='item-title']")?.InnerText ?? "Unknown_Gallery";
                string sanitizedTitle = StringUtilities.SanitizeFolderName(rawTitle);

                int galleryYear = StringUtilities.ExtractYearFromText(sanitizedTitle);

                if (galleryYear > 0 && galleryYear < AppConfig.MinimumRequiredYear)
                {
                    Console.WriteLine($"Found older content from {galleryYear} ({sanitizedTitle}). Stopping search.");
                    isDiscoveryActive = false;
                    break;
                }

                if (galleryYear >= AppConfig.MinimumRequiredYear && !string.IsNullOrEmpty(galleryUrl))
                {
                    Console.WriteLine($"Processing Gallery: {sanitizedTitle}");
                    await DownloadGalleryImagesAsync(galleryUrl, sanitizedTitle);
                    processedGalleriesOnPage++;
                }
            }

            if (processedGalleriesOnPage == 0 && isDiscoveryActive)
            {
                Console.WriteLine("No matching galleries found on this page. Finishing.");
                isDiscoveryActive = false;
            }

            currentPageIndex++;
        }
    }

    private async Task DownloadGalleryImagesAsync(string url, string folderName)
    {
        var galleryPage = await _webLoader.LoadFromWebAsync(url);
        var imageElements = galleryPage.DocumentNode.SelectNodes($"//div[@{AppConfig.ImageSourceAttribute}]");

        if (imageElements == null)
        {
            Console.WriteLine($"No images found in: {folderName}");
            return;
        }

        string targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConfig.DownloadFolderName, folderName);
        if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

        int savedFilesCount = 0;
        foreach (var element in imageElements)
        {
            string imageUrl = element.GetAttributeValue(AppConfig.ImageSourceAttribute, string.Empty);
            if (string.IsNullOrWhiteSpace(imageUrl)) continue;

            try
            {
                byte[] data = await _httpClient.GetByteArrayAsync(imageUrl);
                string fileName = Path.Combine(targetPath, $"img_{savedFilesCount + 1:D3}.jpg");
                await File.WriteAllBytesAsync(fileName, data);
                savedFilesCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download image: {ex.Message}");
            }
        }
        Console.WriteLine($"Successfully saved {savedFilesCount} images.");
    }

    private bool IsResponseInvalid(HtmlDocument doc)
    {
        if (doc?.DocumentNode == null) return true;
        bool is404 = doc.DocumentNode.InnerHtml.Contains("error404");
        bool isTooShort = doc.DocumentNode.InnerHtml.Length < 1000;
        return is404 || isTooShort;
    }
}