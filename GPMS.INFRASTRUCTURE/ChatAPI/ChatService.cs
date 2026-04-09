using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;

namespace GPMS.INFRASTRUCTURE.ChatAPI
{
    public class ChatService : IChatRepositories
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private static string? _cachedContext;
        private static DateTime _lastRead = DateTime.MinValue;

        public ChatService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClient = httpClientFactory.CreateClient("Gemini");
        }

        public async Task<ChatResponseDTO> SendMessageAsync(ChatRequestDTO request)
        {
            var apiKey = _config["Gemini:ApiKey"];
            var model = _config["Gemini:Model"] ?? "gemini-1.5-flash";

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException(
                    "Gemini API Key chưa được cấu hình.");

            string docxContext = GetDocxContext();
            string systemPrompt = BuildUnifiedSystemPrompt(docxContext);

            var url = $"https://generativelanguage.googleapis.com/v1/models/{model}:generateContent?key={apiKey}";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new
                            {
                                text = $"[SYSTEM INSTRUCTION]\n{systemPrompt}\n\n[USER QUESTION]\n{request.Message}"
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 2000
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Gemini API trả về lỗi [{response.StatusCode}]: {errorBody}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            var reply = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "Xin lỗi, tôi không thể trả lời câu hỏi này lúc này.";

            return new ChatResponseDTO
            {
                Reply = reply.Trim()
            };
        }

        private string GetDocxContext()
        {            
            if (_cachedContext != null && (DateTime.Now - _lastRead).TotalMinutes < 5)
            {
                return _cachedContext;
            }
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "ChatContext.docx");
                
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "ChatContext.docx");
                }

                if (File.Exists(filePath))
                {
                    _cachedContext = DocxReader.ReadTextFromDocx(filePath);
                    _lastRead = DateTime.Now;
                    return _cachedContext;
                }

            return string.Empty;
        }

        private static string BuildUnifiedSystemPrompt(string additionalContext) => $"""
            Bạn là trợ lý ảo chuyên nghiệp của hệ thống GPMS (Garment Production Management System).
            
            NHIỆM VỤ:
            Hỗ trợ khách hàng giải đáp các thắc mắc về dịch vụ, đơn hàng và quy trình của xưởng may.

            KIẾN THỨC BỔ SUNG (Dựa trên tài liệu hệ thống):
            ---
            {additionalContext}
            ---

            QUY TẮC PHẢN HỒI:
            1. Sử dụng kiến thức bổ sung ở trên để trả lời chính xác nhất.
            2. Nếu không tìm thấy thông tin trong tài liệu, hãy trả lời dựa trên kiến thức chung về hệ thống GPMS nhưng phải giữ tính chuyên nghiệp.
            3. KHÔNG tiết lộ các thông tin nhạy cảm về nhân sự hoặc tài chính nội bộ trừ khi nó có trong tài liệu công khai.
            4. Luôn thân thiện, lịch sự và sử dụng tiếng Việt.
            5. Nếu câu hỏi hoàn toàn không liên quan đến xưởng may hoặc GPMS, hãy khéo léo từ chối.
            """;
    }
}
