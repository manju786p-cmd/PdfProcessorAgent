using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;
using PdfProcessorAgent.Models;

namespace PdfProcessorAgent.Plugins
{
    /// <summary>
    /// Plugin for extracting resume data from documents using Semantic Kernel.
    /// </summary>
    public class ResumeExtractorPlugin
    {
        private readonly Kernel _kernel;

        public ResumeExtractorPlugin(Kernel kernel)
        {
            _kernel = kernel;
        }

        [KernelFunction]
        [Description("Extracts resume data from document content")]
        public async Task<ResumeData> ExtractResumeData(string content)
        {
            try
            {
                var prompt = $$"""
                    Extract resume data from the following document content and return it as JSON.
                    Extract: full name, email, phone, address, summary/objective, work experience (job title, company, start date, end date, description), education (degree, institution, graduation year, field), and skills.

                    If a field is not found, use empty string or empty array for that field.
                    Return ONLY valid JSON, no other text.

                    Document content:
                    {{content.Substring(0, Math.Min(content.Length, 3000))}}

                    Return JSON format:
                    {
                        "fullName": "",
                        "email": "",
                        "phone": "",
                        "address": "",
                        "summary": "",
                        "experience": [
                            {"jobTitle": "", "company": "", "startDate": "", "endDate": "", "description": ""}
                        ],
                        "education": [
                            {"degree": "", "institution": "", "graduationYear": "", "field": ""}
                        ],
                        "skills": []
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

                return new ResumeData
                {
                    Category = "Resume",
                    FullName = root.GetProperty("fullName").GetString() ?? string.Empty,
                    Email = root.GetProperty("email").GetString() ?? string.Empty,
                    Phone = root.GetProperty("phone").GetString() ?? string.Empty,
                    Address = root.GetProperty("address").GetString() ?? string.Empty,
                    Summary = root.GetProperty("summary").GetString() ?? string.Empty,
                    Experience = root.GetProperty("experience").EnumerateArray()
                        .Select(exp => new Experience
                        {
                            JobTitle = exp.GetProperty("jobTitle").GetString() ?? string.Empty,
                            Company = exp.GetProperty("company").GetString() ?? string.Empty,
                            StartDate = exp.GetProperty("startDate").GetString() ?? string.Empty,
                            EndDate = exp.GetProperty("endDate").GetString() ?? string.Empty,
                            Description = exp.GetProperty("description").GetString() ?? string.Empty
                        }).ToList(),
                    Education = root.GetProperty("education").EnumerateArray()
                        .Select(edu => new Education
                        {
                            Degree = edu.GetProperty("degree").GetString() ?? string.Empty,
                            Institution = edu.GetProperty("institution").GetString() ?? string.Empty,
                            GraduationYear = edu.GetProperty("graduationYear").GetString() ?? string.Empty,
                            Field = edu.GetProperty("field").GetString() ?? string.Empty
                        }).ToList(),
                    Skills = root.GetProperty("skills").EnumerateArray()
                        .Select(s => s.GetString() ?? string.Empty)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList()
                };
            }
            catch
            {
                return new ResumeData { Category = "Resume" };
            }
        }
    }
}
