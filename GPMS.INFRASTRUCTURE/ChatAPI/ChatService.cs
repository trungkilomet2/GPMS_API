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


        public ChatService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClient = httpClientFactory.CreateClient("Gemini");
        }

        public async Task<ChatResponseDTO> SendMessageAsync(ChatRequestDTO request, string? userRole)
        {
            var apiKey = _config["Gemini:ApiKey"];
            var model = _config["Gemini:Model"] ?? "gemini-2.0-flash";

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException(
                    "Gemini API Key chưa được cấu hình. Vui lòng vào https://aistudio.google.com/ lấy key rồi điền vào appsettings.json.");

            // Xác định vai trò
            bool isStaff = !string.IsNullOrEmpty(userRole) && 
                           (userRole.Contains(Roles_Constants.PM)|| userRole.Contains(Roles_Constants.Worker) || userRole.Contains(Roles_Constants.Owner));
            string resolvedRole  = isStaff ? userRole! : Roles_Constants.Customer;
            string systemPrompt  = isStaff ? BuildStaffSystemPrompt() : BuildCustomerSystemPrompt();

            // ── Gemini API endpoint ──
            // POST https://generativelanguage.googleapis.com/v1/models/{model}:generateContent?key={apiKey}
            var url = $"https://generativelanguage.googleapis.com/v1/models/{model}:generateContent?key={apiKey}";

            // ── Payload theo chuẩn Gemini REST API ──
            var payload = new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = systemPrompt } }
                },
                contents = new[]
                {
                    new
                    {
                        role  = "user",
                        parts = new[] { new { text = request.Message } }
                    }
                },
                generationConfig = new
                {
                    temperature     = 0.7,
                    maxOutputTokens = 1000
                }
            };

            var json    = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Gemini API trả về lỗi [{response.StatusCode}]: {errorBody}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc    = JsonDocument.Parse(responseJson);

            // ── Parse response của Gemini ──
            // candidates[0].content.parts[0].text
            var reply = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "Xin lỗi, tôi không thể trả lời câu hỏi này lúc này.";

            return new ChatResponseDTO
            {
                Reply = reply.Trim(),
                Role  = resolvedRole
            };
        }

        // ── System Prompts ──────────────────────────────────────────────────

        private static string BuildCustomerSystemPrompt() => """
            Bạn là trợ lý ảo của hệ thống GPMS (Garment Production Management System) — hệ thống quản lý xưởng may.

            VAI TRÒ CỦA BẠN:
            Bạn đang hỗ trợ KHÁCH HÀNG của xưởng may. Khách hàng chỉ được phép hỏi về:
            - Thông tin chung về xưởng may và dịch vụ
            - Cách đặt đơn hàng, quy trình xử lý đơn hàng
            - Trạng thái đơn hàng (Chờ xét duyệt, Đã duyệt, Đang sản xuất, Hoàn thành, v.v.)
            - Chính sách giao hàng, thanh toán
            - Liên hệ hỗ trợ

            GIỚI HẠN NGHIÊM NGẶT:
            - KHÔNG tiết lộ thông tin nội bộ như: kế hoạch sản xuất chi tiết, dữ liệu nhân sự,
              số liệu tài chính, thông tin kỹ thuật sản xuất, lương thưởng công nhân, hay bất kỳ
              dữ liệu quản lý nội bộ nào.
            - Nếu khách hỏi ngoài phạm vi cho phép, hãy lịch sự từ chối và hướng dẫn họ liên hệ
              trực tiếp với xưởng.

            PHONG CÁCH:
            - Thân thiện, chuyên nghiệp, ngắn gọn.
            - Trả lời bằng tiếng Việt trừ khi khách hỏi bằng tiếng Anh.
            """;

        private static string BuildStaffSystemPrompt() => """
            Bạn là trợ lý ảo chuyên sâu của hệ thống GPMS (Garment Production Management System).

            VAI TRÒ CỦA BẠN:
            Bạn đang hỗ trợ NHÂN VIÊN NỘI BỘ của xưởng may. Bạn có thể tư vấn và giải thích về:
            - Quản lý đơn hàng: tạo, cập nhật, duyệt, từ chối đơn hàng
            - Kế hoạch sản xuất: tiến độ, phân công công đoạn (ProductionPart), phân chia ca
            - Quản lý nguyên vật liệu: vật liệu đơn hàng (OMaterial), số lượng, tồn kho
            - Quản lý nhân sự: công nhân (Worker), kỹ năng (WorkerSkill/WorkerRole), phân ca
            - Đơn xin nghỉ phép (LeaveRequest): xử lý, phê duyệt
            - Nhật ký sản xuất: WorkLog, IssueLog, CuttingNotebook
            - Báo cáo và thống kê nội bộ
            - Tính lương, thưởng (nếu được hỏi)
            - Hướng dẫn sử dụng các tính năng trong hệ thống GPMS

            NGUYÊN TẮC:
            - Cung cấp thông tin chính xác, chi tiết, có ích cho công việc.
            - Nếu không chắc chắn, hãy nói rõ và đề xuất người dùng xác nhận với quản lý.
            - Bảo mật: không chia sẻ thông tin ra bên ngoài hệ thống.

            PHONG CÁCH:
            - Chuyên nghiệp, súc tích, hướng dẫn từng bước nếu cần.
            - Trả lời bằng tiếng Việt trừ khi được hỏi bằng tiếng Anh.
            """;
    }
}
