using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Nest;
using Swashbuckle.AspNetCore.Swagger;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Services.DefinitionStorage;
using WebApiWorkflow.Steps;
using WebApiWorkflow.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<HttpClientUtility>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });
});

// Workflow setup
builder.Services.AddWorkflow(cfg =>
{
    cfg.UseMongoDB(@"mongodb://localhost:27017", "workflow-web-api-app");
    // cfg.UseElasticsearch(new ConnectionSettings(new Uri("http://elastic:9200")), "workflows");
});

// Add workflow custom defined steps
builder.Services.AddTransient<CustomMessage>();
builder.Services.AddTransient<HttpRequestStep>();
builder.Services.AddTransient<SendSqsMessageStep>();

// Add workflow DSL service to load workflow definitions from JSON/YAML
builder.Services.AddWorkflowDSL();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sohaib Workflow Engine");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Start the workflow host
var workflowHost = app.Services.GetService<IWorkflowHost>();
if (workflowHost != null)
{

    // Load workflows manifestation (definitions)
    var loader = app.Services.GetRequiredService<IDefinitionLoader>();
    loader.LoadDefinition(File.ReadAllText("./Workflows/ChangeCustomerInformation.yaml"), Deserializers.Yaml);
    // loader.LoadDefinition(File.ReadAllText("./Workflows/RecurSample.json"), Deserializers.Json);


    workflowHost.Start();

    // Start a workflow

    var obj = new { customer_id = "66ddfdb97765ea0008d15e83" };

    string workflowId = await workflowHost.StartWorkflow("ChangeCustomerInformation", obj);
    bool resumed = await workflowHost.ResumeWorkflow(workflowId);

    Console.WriteLine(resumed);
}

app.Run();
