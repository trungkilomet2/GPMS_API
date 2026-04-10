using Google.GenAI;
using Google.GenAI.Types;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using Microsoft.Extensions.Configuration;
using IOFile = System.IO.File;
using IOPath = System.IO.Path;

namespace GPMS.INFRASTRUCTURE.ChatAPI
{
    public class ChatService : IChatRepositories
    {
        private readonly IConfiguration _config;
        private static string? _cachedContext;
        private static DateTime _lastRead = DateTime.MinValue;

        
        private static readonly string[] FallbackModels =
        [
            "gemini-2.0-flash",
            "gemini-2.0-flash-lite",
            "gemini-2.5-pro-preview-03-25"
        ];

        public ChatService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<ChatResponseDTO> SendMessageAsync(ChatRequestDTO request)
        {
            var apiKey = _config["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Gemini API Key chưa được cấu hình.");

            
            var primaryModel = _config["Gemini:Model"] ?? FallbackModels[0];

            
            var modelsToTry = new List<string> { primaryModel };
            foreach (var m in FallbackModels)
            {
                if (!modelsToTry.Contains(m, StringComparer.OrdinalIgnoreCase))
                    modelsToTry.Add(m);
            }

            string docxContext = GetDocxContext();
            string systemPrompt = BuildUnifiedSystemPrompt(docxContext);
            string fullPrompt = $"[SYSTEM INSTRUCTION]\n{systemPrompt}\n\n[USER QUESTION]\n{request.Message}";

            var client = new Client(apiKey: apiKey);

            Exception? lastException = null;

            foreach (var model in modelsToTry)
            {
                try
                {
                    var response = await client.Models.GenerateContentAsync(
                        model: model,
                        contents: fullPrompt,
                        config: new GenerateContentConfig
                        {
                            Temperature = 0.7f,
                            MaxOutputTokens = 1200
                        }
                    );

                    var reply = response.Text
                        ?? "Xin lỗi, tôi không thể trả lời câu hỏi này lúc này.";

                    return new ChatResponseDTO
                    {
                        Reply = reply.Trim()
                    };
                }
                catch (Exception ex) when (IsQuotaOrTokenError(ex))
                {
                    
                    lastException = ex;
                    continue;
                }
            }
            throw new InvalidOperationException(
                "Tất cả các model Gemini đều đã vượt quá giới hạn quota. Vui lòng thử lại sau.",
                lastException);
        }

 
        private static bool IsQuotaOrTokenError(Exception ex)
        {
            var msg = ex.Message;
            if (ex is ClientError &&
                (msg.Contains("is not found", StringComparison.OrdinalIgnoreCase) ||
                 msg.Contains("not supported", StringComparison.OrdinalIgnoreCase)))
                return true;

            return msg.Contains("429")
                || msg.Contains("503")
                || msg.Contains("quota", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("rate limit", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("TOKEN_LIMIT_EXCEEDED", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("high demand", StringComparison.OrdinalIgnoreCase)
                || ex is ServerError;
        }

        private string GetDocxContext()
        {
            if (_cachedContext != null && (DateTime.Now - _lastRead).TotalMinutes < 5)
                return _cachedContext;

            string filePath = IOPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "ChatContext.docx");

            if (!IOFile.Exists(filePath))
                filePath = IOPath.Combine(Directory.GetCurrentDirectory(), "Data", "ChatContext.docx");

            if (IOFile.Exists(filePath))
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
