using Microsoft.AspNetCore.Mvc;
using UglyToad.PdfPig;
using Microsoft.SemanticKernel;
using PdfProcessorAgent.Models;
using PdfProcessorAgent.Plugins;

namespace PdfProcessorAgent.Controllers
{
    /// <summary>
    /// Controller for processing PDF files and extracting text content.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class PdfProcessorController : ControllerBase
    {
        private readonly Kernel? _kernel;
        private readonly IConfiguration _configuration;

        public PdfProcessorController(IConfiguration configuration, Kernel? kernel = null)
        {
            _configuration = configuration;
            _kernel = kernel;
        }

        /// <summary>
        /// Uploads and processes a PDF file to extract text content and categorize it.
        /// </summary>
        /// <param name="file">The PDF file to upload and process.</param>
        /// <returns>A response containing the file information, extracted text, and AI categorization.</returns>
        /// <response code="200">PDF processed successfully with extracted content and categorization.</response>
        /// <response code="400">No file was uploaded or file is not a PDF.</response>
        /// <response code="500">An error occurred while processing the PDF.</response>
        [HttpPost("upload", Name = "UploadPdf")]
        [ProducesResponseType(typeof(PdfContentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PdfContentResponse>> UploadPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("File must be a PDF.");
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    using (var document = PdfDocument.Open(stream))
                    {
                        var pages = new List<PageContent>();
                        var fullText = string.Empty;

                        foreach (var page in document.GetPages())
                        {
                            var text = page.Text ?? string.Empty;
                            fullText += text + "\n";
                            pages.Add(new PageContent
                            {
                                PageNumber = page.Number,
                                Text = text
                            });
                        }

                        var category = "Unknown";
                        DocumentData? extractedData = null;

                        if (_kernel != null && !string.IsNullOrWhiteSpace(fullText))
                        {
                            category = await CategorizeDocumentAsync(fullText);
                            extractedData = await ExtractDocumentDataAsync(category, fullText);
                        }

                        return Ok(new PdfContentResponse
                        {
                            FileName = file.FileName,
                            FileSize = file.Length,
                            TotalPages = document.NumberOfPages,
                            Category = category,
                            ExtractedData = extractedData,
                            Pages = pages
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error processing PDF", error = ex.Message });
            }
        }

        private async Task<string> CategorizeDocumentAsync(string content)
        {
            try
            {
                var prompt = $"""
                    Categorize the following document as one of: Resume, Bill, or Invoice.
                    Return ONLY the category name, nothing else.

                    Document content:
                    {content.Substring(0, Math.Min(content.Length, 2000))}
                    """;

                var result = await _kernel!.InvokePromptAsync(prompt, new KernelArguments 
                { 
                    { "input", prompt } 
                });

                return result.ToString().Trim();
            }
            catch
            {
                return "Unknown";
            }
        }

        private async Task<DocumentData?> ExtractDocumentDataAsync(string category, string content)
        {
            try
            {
                // Use AI to intelligently route to the appropriate plugin
                var pluginDecision = await DeterminePluginAsync(category, content);

                return pluginDecision.ToLower() switch
                {
                    "invoice" => await InvokeInvoiceExtractor(content),
                    "resume" => await InvokeResumeExtractor(content),
                    "bill" => await InvokeBillExtractor(content),
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task<string> DeterminePluginAsync(string category, string content)
        {
            try
            {
                var prompt = $"""
                    Based on the document category "{category}" and the following content preview, determine which specialized extractor plugin to use.
                    Return ONLY one of: Invoice, Resume, or Bill

                    - Invoice plugin: For invoices, purchase orders, receipts, and payment documents
                    - Resume plugin: For resumes, CVs, and professional profiles
                    - Bill plugin: For bills, statements, and charges

                    Document preview:
                    {content.Substring(0, Math.Min(content.Length, 1500))}
                    """;

                var result = await _kernel!.InvokePromptAsync(prompt);
                return result.ToString().Trim();
            }
            catch
            {
                return category;
            }
        }

        private async Task<InvoiceData> InvokeInvoiceExtractor(string content)
        {
            var plugin = new InvoiceExtractorPlugin(_kernel!);
            return await plugin.ExtractInvoiceData(content);
        }

        private async Task<ResumeData> InvokeResumeExtractor(string content)
        {
            var plugin = new ResumeExtractorPlugin(_kernel!);
            return await plugin.ExtractResumeData(content);
        }

        private async Task<BillData> InvokeBillExtractor(string content)
        {
            var plugin = new BillExtractorPlugin(_kernel!);
            return await plugin.ExtractBillData(content);
        }
    }
}
