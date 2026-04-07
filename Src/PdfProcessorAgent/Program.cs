using Microsoft.SemanticKernel;
using PdfProcessorAgent.Plugins;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// Configure Semantic Kernel with OpenAI
var openAiApiKey = builder.Configuration["OpenAi:ApiKey"];
var openAiModelId = builder.Configuration["OpenAi:ModelId"];

if (!string.IsNullOrEmpty(openAiApiKey) && !string.IsNullOrEmpty(openAiModelId))
{
    var kernelBuilder = Kernel.CreateBuilder()
        .AddOpenAIChatCompletion(openAiModelId, openAiApiKey);

    var kernel = kernelBuilder.Build();

    // Register plugins
    kernel.Plugins.AddFromObject(new InvoiceExtractorPlugin(kernel), nameof(InvoiceExtractorPlugin));
    kernel.Plugins.AddFromObject(new ResumeExtractorPlugin(kernel), nameof(ResumeExtractorPlugin));
    kernel.Plugins.AddFromObject(new BillExtractorPlugin(kernel), nameof(BillExtractorPlugin));

    builder.Services.AddSingleton(kernel);
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
