using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;
using PdfProcessorAgent.Models;

namespace PdfProcessorAgent.Plugins
{
    /// <summary>
    /// Plugin for extracting invoice data from documents using Semantic Kernel.
    /// </summary>
    public class InvoiceExtractorPlugin
    {
        private readonly Kernel _kernel;

        public InvoiceExtractorPlugin(Kernel kernel)
        {
            _kernel = kernel;
        }

        [KernelFunction]
        [Description("Extracts invoice data from document content")]
        public async Task<InvoiceData> ExtractInvoiceData(string content)
        {
            try
            {
                var prompt = $$"""
                    Extract invoice data from the following document content and return it as JSON.
                    Extract: invoice number, invoice date, vendor name, customer name, total amount, due date, and line items (description, quantity, unit price, amount).

                    If a field is not found, use empty string or 0 for that field.
                    Return ONLY valid JSON, no other text.

                    Document content:
                    {{content.Substring(0, Math.Min(content.Length, 3000))}}

                    Return JSON format:
                    {
                        "invoiceNumber": "",
                        "invoiceDate": "",
                        "vendor": "",
                        "customer": "",
                        "totalAmount": 0,
                        "dueDate": "",
                        "lineItems": [
                            {"description": "", "quantity": 0, "unitPrice": 0, "amount": 0}
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

                return new InvoiceData
                {
                    Category = "Invoice",
                    InvoiceNumber = root.GetProperty("invoiceNumber").GetString() ?? string.Empty,
                    InvoiceDate = DateTime.TryParse(root.GetProperty("invoiceDate").GetString(), out var date) ? date : DateTime.MinValue,
                    Vendor = root.GetProperty("vendor").GetString() ?? string.Empty,
                    Customer = root.GetProperty("customer").GetString() ?? string.Empty,
                    TotalAmount = root.GetProperty("totalAmount").GetDecimal(),
                    DueDate = root.GetProperty("dueDate").GetString() ?? string.Empty,
                    LineItems = root.GetProperty("lineItems").EnumerateArray()
                        .Select(item => new InvoiceLineItem
                        {
                            Description = item.GetProperty("description").GetString() ?? string.Empty,
                            Quantity = item.GetProperty("quantity").GetDecimal(),
                            UnitPrice = item.GetProperty("unitPrice").GetDecimal(),
                            Amount = item.GetProperty("amount").GetDecimal()
                        }).ToList()
                };
            }
            catch
            {
                return new InvoiceData { Category = "Invoice" };
            }
        }
    }
}
