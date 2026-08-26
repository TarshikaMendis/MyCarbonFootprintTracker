using System.Text;
using System.Text.Json;

namespace CarbonFootprintTracker.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        // ✅ USING THE WORKING MODEL
        private readonly string _baseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent";

        // ✅ Fallback responses when API is unavailable
        private readonly List<string> _fallbackResponses = new List<string>
        {
            "🌿 I'm currently experiencing high demand. Please try again in a few minutes. Meanwhile, here's a tip: Every small action counts! 💚",
            "🌍 I'm taking a short break! Try asking me again soon. Remember, reducing your carbon footprint starts with small steps! 🌱",
            "💚 I'll be right back! While you wait, think about this: Walking or cycling instead of driving saves about 0.21 kg CO₂ per km! 🚲",
            "🌿 Oops! I'm a bit busy right now. Please try again later. Did you know? Turning off lights saves electricity and reduces emissions! 💡",
            "🌍 I'm currently unavailable. Please try again in a few moments. Keep up the great work tracking your carbon footprint! 🌱"
        };

        private int _fallbackIndex = 0;

        public GeminiService(IConfiguration configuration)
        {
            _apiKey = configuration["Gemini:ApiKey"];
            _httpClient = new HttpClient();
        }

        public async Task<string> GetChatResponseAsync(string userMessage)
        {
            try
            {
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = userMessage }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 3000
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}?key={_apiKey}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(responseJson);
                    var root = document.RootElement;

                    var candidates = root.GetProperty("candidates");
                    if (candidates.GetArrayLength() > 0)
                    {
                        var firstCandidate = candidates[0];
                        var contentObj = firstCandidate.GetProperty("content");
                        var parts = contentObj.GetProperty("parts");
                        if (parts.GetArrayLength() > 0)
                        {
                            return parts[0].GetProperty("text").GetString();
                        }
                    }

                    return "I couldn't generate a response. Please try again.";
                }

                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                    response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    return GetFallbackResponse();
                }

                var errorJson = await response.Content.ReadAsStringAsync();
                return $"Error: {response.StatusCode} - {errorJson}";
            }
            catch (Exception ex)
            {
                return GetFallbackResponse();
            }
        }

        private string GetFallbackResponse()
        {
            var response = _fallbackResponses[_fallbackIndex];
            _fallbackIndex = (_fallbackIndex + 1) % _fallbackResponses.Count;
            return response;
        }
    }
}