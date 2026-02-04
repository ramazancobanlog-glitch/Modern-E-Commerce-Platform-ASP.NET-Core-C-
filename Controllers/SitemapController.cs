using Microsoft.AspNetCore.Mvc;
using login.Data;
using System.Text;
using System.Xml.Linq;

namespace login.Controllers
{
    /// <summary>
    /// Controller for generating XML sitemap for search engines
    /// </summary>
    public class SitemapController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _baseUrl;

        public SitemapController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _baseUrl = configuration["SiteSettings:BaseUrl"] ?? "https://zipapp.com";
        }

        /// <summary>
        /// Generate XML sitemap
        /// </summary>
        [HttpGet]
        [Route("sitemap.xml")]
        [ResponseCache(Duration = 3600)] // Cache for 1 hour
        public IActionResult Index()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"");
            sb.AppendLine("        xmlns:image=\"http://www.google.com/schemas/sitemap-image/1.1\">");

            // Home page
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{_baseUrl}/</loc>");
            sb.AppendLine("    <changefreq>daily</changefreq>");
            sb.AppendLine("    <priority>1.0</priority>");
            sb.AppendLine($"    <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
            sb.AppendLine("  </url>");

            // Categories
            var categories = _context.Categories.ToList();
            foreach (var category in categories)
            {
                sb.AppendLine("  <url>");
                sb.AppendLine($"    <loc>{_baseUrl}/Category/Index/{category.Id}</loc>");
                sb.AppendLine("    <changefreq>weekly</changefreq>");
                sb.AppendLine("    <priority>0.8</priority>");
                sb.AppendLine("  </url>");
            }

            // Products
            var products = _context.Products.Where(p => p.Name != null).ToList();
            foreach (var product in products)
            {
                sb.AppendLine("  <url>");
                sb.AppendLine($"    <loc>{_baseUrl}/Home/Detail/{product.Id}</loc>");
                sb.AppendLine("    <changefreq>weekly</changefreq>");
                sb.AppendLine("    <priority>0.7</priority>");
                
                // Add image if exists
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var imageUrl = product.ImageUrl.StartsWith("http") 
                        ? product.ImageUrl 
                        : $"{_baseUrl}{product.ImageUrl}";
                    sb.AppendLine("    <image:image>");
                    sb.AppendLine($"      <image:loc>{imageUrl}</image:loc>");
                    sb.AppendLine($"      <image:title>{System.Security.SecurityElement.Escape(product.Name)}</image:title>");
                    sb.AppendLine("    </image:image>");
                }
                
                sb.AppendLine("  </url>");
            }

            // Static pages
            var staticPages = new[] 
            { 
                ("about", "Hakkımızda", "monthly", "0.5"),
                ("contact", "İletişim", "monthly", "0.5"),
                ("privacy", "Gizlilik Politikası", "yearly", "0.3"),
                ("terms", "Kullanım Koşulları", "yearly", "0.3")
            };

            foreach (var (slug, _, changefreq, priority) in staticPages)
            {
                sb.AppendLine("  <url>");
                sb.AppendLine($"    <loc>{_baseUrl}/{slug}</loc>");
                sb.AppendLine($"    <changefreq>{changefreq}</changefreq>");
                sb.AppendLine($"    <priority>{priority}</priority>");
                sb.AppendLine("  </url>");
            }

            sb.AppendLine("</urlset>");

            return Content(sb.ToString(), "application/xml", Encoding.UTF8);
        }

        /// <summary>
        /// Generate sitemap index for large sites
        /// </summary>
        [HttpGet]
        [Route("sitemap-index.xml")]
        public IActionResult SitemapIndex()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<sitemapindex xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
            
            sb.AppendLine("  <sitemap>");
            sb.AppendLine($"    <loc>{_baseUrl}/sitemap.xml</loc>");
            sb.AppendLine($"    <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
            sb.AppendLine("  </sitemap>");
            
            sb.AppendLine("</sitemapindex>");

            return Content(sb.ToString(), "application/xml", Encoding.UTF8);
        }
    }
}
