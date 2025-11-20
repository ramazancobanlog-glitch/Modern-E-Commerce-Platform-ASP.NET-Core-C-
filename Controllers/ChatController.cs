using Microsoft.AspNetCore.Mvc;
using login.Hubs;

public class ChatController : Controller
{
    public IActionResult Index()
    {
        return View("Chat");
    }
    [HttpPost]
    public IActionResult sendMessage([FromBody] ChatRequest req)
    {
        // Mesajı işleme ve SignalR hub'ına gönderme işlemleri burada yapılabilir
        if (!string.IsNullOrEmpty(req.text))
        {
            System.IO.File.AppendAllText("chatlog.txt", req.text + "\n");
        }
        return Ok(new { status = "Message received", message = req.text });
    }

    public class ChatRequest
    {
        public required string text { get; set; }
    }
}