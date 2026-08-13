using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarbonFootprintTracker.Services;

namespace CarbonFootprintTracker.Controllers
{
    [Authorize]  // Only logged-in users can access the chat
    public class ChatController : Controller
    {
        private readonly GeminiService _geminiService;

        public ChatController(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        // GET: Chat/Index
        public IActionResult Index()
        {
            return View();
        }

        // POST: Chat/SendMessage
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return Json(new { response = "Please enter a message." });
            }

            var aiResponse = await _geminiService.GetChatResponseAsync(request.Message);
            return Json(new { response = aiResponse });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}