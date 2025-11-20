using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace login.Services
{
    public class WhatsAppService
    {
        private readonly ILogger<WhatsAppService> _logger;
        private readonly HttpClient _httpClient;
        
        // WhatsApp Business API configuration
        private readonly string _accessToken;
        private readonly string _phoneNumberId;
        private readonly string _apiVersion = "v18.0";

        public WhatsAppService(ILogger<WhatsAppService> logger, HttpClient httpClient, IConfiguration config)
        {
            _logger = logger;
            _httpClient = httpClient;
            _accessToken = config["WhatsApp:AccessToken"] ?? "";
            _phoneNumberId = config["WhatsApp:PhoneNumberId"] ?? "";
        }

        public async Task<bool> SendOrderConfirmationAsync(string phoneNumber, string orderDetails, string customerName)
        {
            try
            {
                // Telefon numarasını format et (Türkiye: +90 ile başlamalı)
                if (!phoneNumber.StartsWith("+"))
                {
                    // 05xx formatını +905xx'e dönüştür
                    if (phoneNumber.StartsWith("0"))
                    {
                        phoneNumber = "+90" + phoneNumber.Substring(1);
                    }
                    else
                    {
                        phoneNumber = "+90" + phoneNumber;
                    }
                }

                string message = $"Merhaba {customerName},\n\n" +
                                $"Siparişiniz onaylanmıştır! ✅\n\n" +
                                $"Sipariş Detayları:\n" +
                                $"{orderDetails}\n\n" +
                                $"En kısa sürede kargo ile gönderilecektir.\n" +
                                $"Sorularınız için bizimle iletişime geçebilirsiniz.\n\n" +
                                $"ZipApp - Profesyonel Alışveriş Platformu";

                var payload = new
                {
                    messaging_product = "whatsapp",
                    recipient_type = "individual",
                    to = phoneNumber,
                    type = "text",
                    text = new { body = message }
                };

                string jsonPayload = JsonSerializer.Serialize(payload);
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                string url = $"https://graph.instagram.com/{_apiVersion}/{_phoneNumberId}/messages";

                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    request.Content = content;
                    request.Headers.Add("Authorization", $"Bearer {_accessToken}");

                    var response = await _httpClient.SendAsync(request);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"WhatsApp mesajı gönderildi: {phoneNumber}");
                        return true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"WhatsApp gönderme hatası: {response.StatusCode} - {errorContent}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"WhatsApp servisi hatası: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendCustomerNotificationAsync(string phoneNumber, string message, string title)
        {
            try
            {
                // Telefon numarasını format et
                if (!phoneNumber.StartsWith("+"))
                {
                    if (phoneNumber.StartsWith("0"))
                    {
                        phoneNumber = "+90" + phoneNumber.Substring(1);
                    }
                    else
                    {
                        phoneNumber = "+90" + phoneNumber;
                    }
                }

                var payload = new
                {
                    messaging_product = "whatsapp",
                    recipient_type = "individual",
                    to = phoneNumber,
                    type = "text",
                    text = new { body = message }
                };

                string jsonPayload = JsonSerializer.Serialize(payload);
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                string url = $"https://graph.instagram.com/{_apiVersion}/{_phoneNumberId}/messages";

                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    request.Content = content;
                    request.Headers.Add("Authorization", $"Bearer {_accessToken}");

                    var response = await _httpClient.SendAsync(request);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"WhatsApp bildirim hatası: {ex.Message}");
                return false;
            }
        }
    }
}
