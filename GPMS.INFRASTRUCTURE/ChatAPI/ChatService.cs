using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GPMS.INFRASTRUCTURE.ChatAPI
{
    public class ChatService : IChatRepositories
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private static readonly HashSet<string> _staffRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "Admin", "PM", "Owner", "Worker"
        };

        public ChatService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClient = httpClientFactory.CreateClient("OpenAI");
        }

        public async Task<ChatResponseDTO> SendMessageAsync(ChatRequestDTO request, string? userRole)
        {
            var apiKey = _config["OpenAI:ApiKey"];
            var model = _config["OpenAI:Model"] ?? "gpt-4o-mini";

            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_OPENAI_API_KEY")
                throw new InvalidOperationException("OpenAI API Key chưa được cấu hình. Vui lòng thêm key vào appsettings.json.");

            // Xác định vai trò và chọn system prompt phù hợp
            bool isStaff = !string.IsNullOrEmpty(userRole) && _staffRoles.Contains(userRole);
            string resolvedRole = isStaff ? userRole! : "Customer";
            string systemPrompt = isStaff
                ? BuildStaffSystemPrompt()
                : BuildCustomerSystemPrompt();

            // Xây dựng payload theo OpenAI Chat Completions API
            var payload = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = request.Message }
                },
                temperature = 0.7,
                max_tokens = 1000
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"OpenAI API trả về lỗi [{response.StatusCode}]: {errorBody}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            var reply = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "Xin lỗi, tôi không thể trả lời câu hỏi này lúc này.";

            return new ChatResponseDTO
            {
                Reply = reply.Trim(),
                Role = resolvedRole
            };
        }
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
            - KHÔNG tiết lộ thông tin nội bộ như: kế hoạch sản xuất chi tiết, dữ liệu nhân sự, số liệu tài chính, 
              thông tin kỹ thuật sản xuất, lương thưởng công nhân, hay bất kỳ dữ liệu quản lý nội bộ nào.
            - Nếu khách hỏi ngoài phạm vi cho phép, hãy lịch sự từ chối và hướng dẫn họ liên hệ trực tiếp với xưởng.
            
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
