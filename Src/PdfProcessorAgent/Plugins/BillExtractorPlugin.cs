using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;
using PdfProcessorAgent.Models;

namespace PdfProcessorAgent.Plugins
{
    /// <summary>
    /// Plugin for extracting bill data from documents using Semantic Kernel.
    /// </summary>
    public class BillExtractorPlugin
    {
        private readonly Kernel _kernel;

        public BillExtractorPlugin(Kernel kernel)
        {
            _kernel = kernel;
        }

        [KernelFunction]
        [Description("Extracts bill data from document content")]
        public async Task<BillData> ExtractBillData(string content)
        {
            try
            {
                var prompt = $$"""
                    Extract bill document data from the following document content and return it as JSON.
                    Extract: bill number, bill date, billed to (customer), billed by (vendor), total amount, due date, and line items (description, quantity, rate, amount).

                    If a field is not found, use empty string or 0 for that field.
                    Return ONLY valid JSON, no other text.

                    Document content:
                    {{content.Substring(0, Math.Min(content.Length, 3000))}}

                    Return JSON format:
                    {
                        "billNumber": "",
                        "billDate": "",
                        "billedTo": "",
                        "billedBy": "",
                        "totalAmount": 0,
                        "dueDate": "",
                        "lineItems": [
                            {"description": "", "quantity": 0, "rate": 0, "amount": 0}
                        ]
                    }
                    """;

                var result = await _kernel.InvokePromptAsync(prompt);
                var jsonContent = result.ToString().Trim();

                // Remove markdown code blocks if present
                if (jsonContent.StartsWith("```json"))
                    jsonContent = jsonContent[7..];
                if (jsonContent.StartsWith("```"))
                    jsonContent = jsonContent[3..];
                if (jsonContent.EndsWith("```"))
                    jsonContent = jsonContent[..^3];

                var jsonDoc = JsonDocument.Parse(jsonContent);
                var root = jsonDoc.RootElement;

                return new BillData
                {
                    Category = "Build",
                    BillNumber = root.GetProperty("billNumber").GetString() ?? string.Empty,
                    BillDate = DateTime.TryParse(root.GetProperty("billDate").GetString(), out var date) ? date : DateTime.MinValue,
                    BilledTo = root.GetProperty("billedTo").GetString() ?? string.Empty,
                    BilledBy = root.GetProperty("billedBy").GetString() ?? string.Empty,
                    TotalAmount = root.GetProperty("totalAmount").GetDecimal(),
                    DueDate = root.GetProperty("dueDate").GetString() ?? string.Empty,
                    LineItems = root.GetProperty("lineItems").EnumerateArray()
                        .Select(item => new BillLineItem
                        {
                            Description = item.GetProperty("description").GetString() ?? string.Empty,
                            Quantity = item.GetProperty("quantity").GetDecimal(),
                            Rate = item.GetProperty("rate").GetDecimal(),
                            Amount = item.GetProperty("amount").GetDecimal()
                        }).ToList()
                };
            }
            catch
            {
                return new BillData { Category = "Build" };
            }
        }
    }
}
