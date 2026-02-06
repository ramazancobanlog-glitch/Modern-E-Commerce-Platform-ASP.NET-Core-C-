using login.Models;
using System.Text.Json;

namespace login.Services
{
    /// <summary>
    /// Centralized SEO management service
    /// </summary>
    public class SeoService
    {
        private readonly string _baseUrl;
        private readonly string _siteName = "ZipApp";
        private readonly string _defaultDescription = "ZipApp - Türkiye'nin en güvenilir online alışveriş platformu. Binlerce ürün, uygun fiyatlar, hızlı teslimat ve %100 güvenli ödeme.";
        private readonly string _defaultKeywords = "online alışveriş, e-ticaret, indirimli ürünler, hızlı teslimat, güvenli alışveriş, ZipApp, elektronik, moda, ev dekorasyon";

        public SeoService(IConfiguration configuration)
        {
            _baseUrl = configuration["SiteSettings:BaseUrl"] ?? "https://zipapp.com";
        }

        /// <summary>
        /// Generate SEO meta for home page
        /// </summary>
        public SeoMeta GetHomePageSeo()
        {
            return new SeoMeta
            {
                Title = "ZipApp - Online Alışverişin Yeni Adresi | Hızlı Teslimat & Güvenli Ödeme",
                Description = _defaultDescription,
                Keywords = _defaultKeywords,
                CanonicalUrl = _baseUrl,
                OgTitle = "ZipApp - Online Alışverişin Yeni Adresi",
                OgDescription = _defaultDescription,
                OgUrl = _baseUrl,
                OgImage = $"{_baseUrl}/img/og-home.jpg",
                OgType = "website",
                TwitterTitle = "ZipApp - Online Alışverişin Yeni Adresi",
                TwitterDescription = _defaultDescription,
                TwitterImage = $"{_baseUrl}/img/og-home.jpg",
                JsonLd = GenerateOrganizationJsonLd()
            };
        }

        /// <summary>
        /// Generate SEO meta for product detail page
        /// </summary>
        public SeoMeta GetProductSeo(Product product, string? categoryName = null)
        {
            var productUrl = $"{_baseUrl}/Home/Detail/{product.Id}";
            var imageUrl = !string.IsNullOrEmpty(product.ImageUrl) 
                ? (product.ImageUrl.StartsWith("http") ? product.ImageUrl : $"{_baseUrl}{product.ImageUrl}")
                : $"{_baseUrl}/img/product-placeholder.jpg";

            var description = !string.IsNullOrEmpty(product.Description) 
                ? TruncateDescription(product.Description, 160)
                : $"{product.Name} - En uygun fiyat garantisi ile ZipApp'te. Hızlı teslimat ve güvenli ödeme seçenekleri.";

            return new SeoMeta
            {
                Title = $"{product.Name} | {categoryName ?? "Ürünler"} | ZipApp",
                Description = description,
                Keywords = $"{product.Name}, {categoryName}, online alışveriş, ZipApp, hızlı teslimat",
                CanonicalUrl = productUrl,
                OgTitle = product.Name,
                OgDescription = description,
                OgUrl = productUrl,
                OgImage = imageUrl,
                OgType = "product",
                TwitterTitle = product.Name,
                TwitterDescription = description,
                TwitterImage = imageUrl,
                ProductPrice = product.Price,
                ProductCurrency = "TRY",
                ProductInStock = true,
                ProductBrand = "ZipApp",
                ProductCategory = categoryName,
                JsonLd = GenerateProductJsonLd(product, productUrl, imageUrl, categoryName)
            };
        }

        /// <summary>
        /// Generate SEO meta for category page
        /// </summary>
        public SeoMeta GetCategorySeo(string categoryName, int productCount = 0)
        {
            var categoryUrl = $"{_baseUrl}/Category/{categoryName}";
            var description = $"{categoryName} kategorisinde {productCount}+ ürün. En uygun fiyatlar ve hızlı teslimat ile ZipApp'te.";

            return new SeoMeta
            {
                Title = $"{categoryName} | En İyi Fiyatlar | ZipApp",
                Description = description,
                Keywords = $"{categoryName}, {categoryName} ürünleri, online alışveriş, ZipApp",
                CanonicalUrl = categoryUrl,
                OgTitle = $"{categoryName} - ZipApp",
                OgDescription = description,
                OgUrl = categoryUrl,
                OgType = "website",
                TwitterTitle = $"{categoryName} - ZipApp",
                TwitterDescription = description,
                JsonLd = GenerateBreadcrumbJsonLd(categoryName, categoryUrl)
            };
        }

        /// <summary>
        /// Generate Organization JSON-LD structured data
        /// </summary>
        private string GenerateOrganizationJsonLd()
        {
            var jsonLd = new
            {
                @context = "https://schema.org",
                @type = "Organization",
                name = _siteName,
                url = _baseUrl,
                logo = $"{_baseUrl}/img/logo.png",
                description = _defaultDescription,
                contactPoint = new
                {
                    @type = "ContactPoint",
                    telephone = "+90-555-123-4567",
                    contactType = "customer service",
                    availableLanguage = "Turkish"
                },
                sameAs = new[]
                {
                    "https://facebook.com/zipapp",
                    "https://twitter.com/zipapp",
                    "https://instagram.com/zipapp",
                    "https://linkedin.com/company/zipapp"
                },
                address = new
                {
                    @type = "PostalAddress",
                    addressCountry = "TR",
                    addressLocality = "İstanbul"
                }
            };

            return JsonSerializer.Serialize(jsonLd, new JsonSerializerOptions { WriteIndented = false });
        }

        /// <summary>
        /// Generate Product JSON-LD structured data
        /// </summary>
        private string GenerateProductJsonLd(Product product, string productUrl, string imageUrl, string? categoryName)
        {
            var jsonLd = new
            {
                @context = "https://schema.org",
                @type = "Product",
                name = product.Name,
                description = product.Description ?? $"{product.Name} - ZipApp'te en uygun fiyatlarla",
                image = imageUrl,
                url = productUrl,
                brand = new
                {
                    @type = "Brand",
                    name = "ZipApp"
                },
                category = categoryName ?? "Genel",
                offers = new
                {
                    @type = "Offer",
                    price = product.Price,
                    priceCurrency = "TRY",
                    availability = "https://schema.org/InStock",
                    seller = new
                    {
                        @type = "Organization",
                        name = "ZipApp"
                    },
                    url = productUrl
                },
                aggregateRating = new
                {
                    @type = "AggregateRating",
                    ratingValue = "4.5",
                    reviewCount = "92",
                    bestRating = "5",
                    worstRating = "1"
                }
            };

            return JsonSerializer.Serialize(jsonLd, new JsonSerializerOptions { WriteIndented = false });
        }

        /// <summary>
        /// Generate Breadcrumb JSON-LD structured data
        /// </summary>
        private string GenerateBreadcrumbJsonLd(string categoryName, string categoryUrl)
        {
            var jsonLd = new
            {
                @context = "https://schema.org",
                @type = "BreadcrumbList",
                itemListElement = new[]
                {
                    new
                    {
                        @type = "ListItem",
                        position = 1,
                        name = "Ana Sayfa",
                        item = _baseUrl
                    },
                    new
                    {
                        @type = "ListItem",
                        position = 2,
                        name = categoryName,
                        item = categoryUrl
                    }
                }
            };

            return JsonSerializer.Serialize(jsonLd, new JsonSerializerOptions { WriteIndented = false });
        }

        /// <summary>
        /// Truncate description to specified length
        /// </summary>
        private string TruncateDescription(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - 3) + "...";
        }
    }
}
