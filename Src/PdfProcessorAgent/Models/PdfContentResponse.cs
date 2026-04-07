namespace PdfProcessorAgent.Models
{
    /// <summary>
    /// Response model containing the extracted content from a processed PDF file.
    /// </summary>
    public class PdfContentResponse
    {
        /// <summary>
        /// The name of the uploaded PDF file.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// The size of the PDF file in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// The total number of pages in the PDF.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// AI-determined category: Resume, Build, or Invoice.
        /// </summary>
        public string Category { get; set; } = "Unknown";

        /// <summary>
        /// Extracted document data based on category.
        /// </summary>
        public DocumentData? ExtractedData { get; set; }

        /// <summary>
        /// List of page contents extracted from the PDF.
        /// </summary>
        public List<PageContent> Pages { get; set; } = new();
    }

    /// <summary>
    /// Model representing the content of a single page from a PDF.
    /// </summary>
    public class PageContent
    {
        /// <summary>
        /// The page number (1-indexed).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// The extracted text content from the page.
        /// </summary>
        public string Text { get; set; } = string.Empty;
    }
}
