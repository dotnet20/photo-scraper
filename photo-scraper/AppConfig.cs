namespace photo_scraper
{
    public static class AppConfig
    {
        public const string GalleryBaseUrl = "https://galerie.malutkiemisie.pl/";
        public const int MinimumRequiredYear = 2021;
        public const string ImageSourceAttribute = "data-image-moon-max-size";
        public const string DownloadFolderName = "Downloads";
        public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0";
    }
}
