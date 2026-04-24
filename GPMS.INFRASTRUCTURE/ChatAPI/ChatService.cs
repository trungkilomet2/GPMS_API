using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using IOFile = System.IO.File;
using IOPath = System.IO.Path;

namespace GPMS.INFRASTRUCTURE.ChatAPI
{
    public class ChatService : IChatRepositories
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private static string? _cachedContext;
        private static DateTime _lastRead = DateTime.MinValue;

        public ChatService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ChatResponseDTO> SendMessageAsync(ChatRequestDTO request)
        {
            var client = CreateHttpClient();
            var payload = BuildPayload(request.Message);

            var json = JsonSerializer.Serialize(payload);

            var response = await client.PostAsync(_config["OpenRouter:BaseUrl"] + Chat_Constants.Uri,
                new StringContent(json, Encoding.UTF8, Chat_Constants.MediaType));

            var responseBody = await response.Content.ReadAsStringAsync();
            var completion = JsonSerializer.Deserialize<OpenRouterResponse>(
                responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var reply = completion?.Choices?.FirstOrDefault()?.Message?.Content?.Trim()
                        ?? "Xin lỗi, tôi không thể trả lời lúc này.";

            return new ChatResponseDTO { Reply = reply.Replace("\n", "") };
        }

        private string GetDocxContext()
        {
            if (_cachedContext != null && (DateTime.Now - _lastRead).TotalMinutes < 5)
                return _cachedContext;

            string filePath = IOPath.Combine(AppDomain.CurrentDomain.BaseDirectory, ReadFile_Constants.DataFolder, ReadFile_Constants.FileName);

            if (!IOFile.Exists(filePath))
                filePath = IOPath.Combine(Directory.GetCurrentDirectory(), ReadFile_Constants.DataFolder, ReadFile_Constants.FileName);

            if (IOFile.Exists(filePath))
            {
                _cachedContext = DocxReader.ReadTextFromDocx(filePath);
                _lastRead = DateTime.Now;
                return _cachedContext;
            }

            return string.Empty;
        }

        private HttpClient CreateHttpClient()
        {
            var apiKey = _config["OpenRouter:ApiKey"]
                ?? throw new InvalidOperationException("API Key chưa cấu hình.");

            var baseUrl = (_config["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1/").TrimEnd('/');

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUrl);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Chat_Constants.MediaType));
            client.DefaultRequestHeaders.TryAddWithoutValidation("HTTP-Referer", _config["OpenRouter:SiteUrl"]);

            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Title", _config["OpenRouter:SiteName"]);
            return client;
        }

        private object BuildPayload(string userMessage)
        {
            return new
            {
                model = _config["OpenRouter:Model"] ?? Chat_Constants.ModelName,
                temperature = 0.7,
                max_tokens = 500,

                messages = new[]
                {
            new { role = "system", content = BuildUnifiedSystemPrompt(GetDocxContext()) },
            new { role = "user", content = userMessage }
        }
            };
        }

        private static string BuildUnifiedSystemPrompt(string additionalContext) => $"""
            Bạn là trợ lý ảo chuyên nghiệp của hệ thống GPMS (Garment Production Management System).
            
            ### NHIỆM VỤ:
            Hỗ trợ khách hàng giải đáp các thắc mắc về dịch vụ, đơn hàng và quy trình của xưởng may.

            ### KIẾN THỨC BỔ SUNG (Dựa trên tài liệu hệ thống):
            ---
            {additionalContext}
            ---

            ### QUY TẮC PHẢN HỒI:
            1. Sử dụng kiến thức bổ sung ở trên để trả lời chính xác nhất.
            2. Nếu không tìm thấy thông tin trong tài liệu, hãy trả lời dựa trên kiến thức chung về hệ thống GPMS nhưng phải giữ tính chuyên nghiệp.
            3. KHÔNG tiết lộ các thông tin nhạy cảm về nhân sự hoặc tài chính nội bộ trừ khi nó có trong tài liệu công khai.
            4. Luôn thân thiện, lịch sự và sử dụng tiếng Việt.
            5. Nếu câu hỏi hoàn toàn không liên quan đến xưởng may hoặc GPMS, hãy khéo léo từ chối.
            6. Phải trả lời ngắn gọn hết sức có thể, không cần đi quá chi tiết về các khía cạnh khác của hệ thống GPMS.
            7. Khi trả lời thì hãy trả về dạng html sử dụng <br> để xuống dòng, không trả về một đoạn văn bản dài, hãy chia nhỏ câu trả lời thành nhiều đoạn nếu cần thiết để dễ đọc hơn.
            8.Câu hỏi: Quy trình tạo đơn hàng
            ### VÍ DỤ TRẢ LỜI ĐÚNG:
            Bước 1: Tiếp nhận yêu cầu từ khách hàng<br>
            Bước 2: Tạo đơn hàng trên hệ thống<br>
            Bước 3: Phân bổ sản xuất<br>
            Bước 4: Theo dõi tiến độ<br>
            ### CHÚ Ý THÊM:
            - Đặc biệt chú ý quy tắc thứ 6, luôn trả lời ngắn gọn nhất có thể, không cần đi quá chi tiết về các khía cạnh khác của hệ thống GPMS, 
            chỉ cần trả lời đúng trọng tâm câu hỏi là được, không cần phải giải thích thêm nhiều về các quy trình khác của hệ thống GPMS nếu nó không liên quan đến câu hỏi.
            """;

        private sealed class OpenRouterRequest
        {
            public string Model { get; set; } = string.Empty;
            public List<OpenRouterMessage> Messages { get; set; } = [];
            public decimal Temperature { get; set; }
            [JsonPropertyName("max_tokens")]
            public int MaxTokens { get; set; }
        }

        private sealed class OpenRouterMessage
        {
            public string Role { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }

        private sealed class OpenRouterResponse
        {
            public List<OpenRouterChoice>? Choices { get; set; }
        }

        private sealed class OpenRouterChoice
        {
            public OpenRouterMessage? Message { get; set; }
        }
    }
}
