using photo_scraper;

using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("User-Agent", AppConfig.UserAgent);

var scraper = new GalleryScraper(httpClient);
await scraper.RunAsync();

Console.WriteLine("\n Completed successfully.");