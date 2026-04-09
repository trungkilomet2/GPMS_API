using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace GPMS.INFRASTRUCTURE.ChatAPI
{
    public static class DocxReader
    {
        public static string ReadTextFromDocx(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }

            try
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
                {
                    var body = wordDoc.MainDocumentPart?.Document.Body;
                    if (body == null) return string.Empty;

                    StringBuilder sb = new StringBuilder();
                    foreach (var paragraph in body.Elements<Paragraph>())
                    {
                        sb.AppendLine(paragraph.InnerText);
                    }
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"[Lỗi khi đọc file: {ex.Message}]";
            }
        }
    }
}
