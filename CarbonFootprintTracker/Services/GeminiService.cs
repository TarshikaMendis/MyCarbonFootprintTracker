using System.Text;
using System.Text.Json;

namespace CarbonFootprintTracker.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        // ✅ FIXED: Using correct model name
        private readonly string _baseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent";

        public GeminiService(IConfiguration configuration)
        {
            _apiKey = configuration["Gemini:ApiKey"];
            _httpClient = new HttpClient();
        }

        public async Task<string> GetChatResponseAsync(string userMessage)
        {
            try
            {
                // Build the request payload
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

                // Make the API call
                var response = await _httpClient.PostAsync($"{_baseUrl}?key={_apiKey}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(responseJson);
                    var root = document.RootElement;

                    // Extract the AI response
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

                var errorJson = await response.Content.ReadAsStringAsync();
                return $"Error: {response.StatusCode} - {errorJson}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}