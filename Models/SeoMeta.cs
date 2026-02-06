namespace login.Models
{
    /// <summary>
    /// SEO Meta data model for pages
    /// </summary>
    public class SeoMeta
    {
        // Basic Meta Tags
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Keywords { get; set; }
        public string? Author { get; set; } = "ZipApp";
        public string? Robots { get; set; } = "index, follow";
        public string? CanonicalUrl { get; set; }

        // Open Graph (Facebook, LinkedIn)
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        public string? OgImage { get; set; }
        public string? OgUrl { get; set; }
        public string? OgType { get; set; } = "website";
        public string? OgSiteName { get; set; } = "ZipApp";
        public string? OgLocale { get; set; } = "tr_TR";

        // Twitter Card
        public string? TwitterCard { get; set; } = "summary_large_image";
        public string? TwitterTitle { get; set; }
        public string? TwitterDescription { get; set; }
        public string? TwitterImage { get; set; }
        public string? TwitterSite { get; set; } = "@ZipApp";

        // JSON-LD Structured Data
        public string? JsonLd { get; set; }

        // Page specific
        public decimal? ProductPrice { get; set; }
        public string? ProductCurrency { get; set; } = "TRY";
        public bool? ProductInStock { get; set; }
        public string? ProductBrand { get; set; }
        public string? ProductCategory { get; set; }
    }
}
