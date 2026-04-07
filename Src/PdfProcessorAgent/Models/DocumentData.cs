namespace PdfProcessorAgent.Models
{
    /// <summary>
    /// Base class for extracted document data.
    /// </summary>
    public abstract class DocumentData
    {
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Extracted data from an invoice document.
    /// </summary>
    public class InvoiceData : DocumentData
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string Vendor { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string DueDate { get; set; } = string.Empty;
        public List<InvoiceLineItem> LineItems { get; set; } = new();
    }

    /// <summary>
    /// Represents a line item in an invoice.
    /// </summary>
    public class InvoiceLineItem
    {
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Extracted data from a resume document.
    /// </summary>
    public class ResumeData : DocumentData
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<Experience> Experience { get; set; } = new();
        public List<Education> Education { get; set; } = new();
        public List<string> Skills { get; set; } = new();
    }

    /// <summary>
    /// Represents work experience in a resume.
    /// </summary>
    public class Experience
    {
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents education in a resume.
    /// </summary>
    public class Education
    {
        public string Degree { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public string GraduationYear { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
    }

    /// <summary>
    /// Extracted data from a bill document.
    /// </summary>
    public class BillData : DocumentData
    {
        public string BillNumber { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public string BilledTo { get; set; } = string.Empty;
        public string BilledBy { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string DueDate { get; set; } = string.Empty;
        public List<BillLineItem> LineItems { get; set; } = new();
    }

    /// <summary>
    /// Represents a line item in a bill.
    /// </summary>
    public class BillLineItem
    {
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }
}
