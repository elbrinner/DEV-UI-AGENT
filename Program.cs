// Namespaces del Agent Framework y UI Dev
using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Microsoft.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// Configuración: appsettings + variables de entorno
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Configurar Azure OpenAI Client
var endpoint = builder.Configuration["AZURE_OPENAI_ENDPOINT"];
var apikey = builder.Configuration["AZURE_OPENAI_API_KEY"];
var deployment = builder.Configuration["AZURE_OPENAI_DEPLOYMENT_NAME"] ?? "gpt-4.1-mini";

var chatClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apikey))
    .GetChatClient(deployment)
    .AsIChatClient();

builder.Services.AddChatClient(chatClient);

builder.AddAIAgent("ChatAgent", (sp, key) =>
{
    // get logger
    var logger = sp.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Configuring AI Agent with key '{Key}' for model '{Model}'", key, "gpt-5-mini");



    // create agent
    var chatClient = sp.GetRequiredService<IChatClient>();
    var aiAgent = chatClient.CreateAIAgent(
        name: key,
        instructions: "You are an useful agent that helps users with short and funny answers.",
        description: "An AI agent that helps users with short and funny answers.",
        tools: []
        )
    .AsBuilder()
    .UseOpenTelemetry(configure: c =>
        c.EnableSensitiveData = builder.Environment.IsDevelopment())
    .Build();
    return aiAgent;
});

// Registrar servicios requeridos por Dev UI y endpoints de OpenAI
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

var app = builder.Build();

// Pipeline HTTP mínimo
app.UseHttpsRedirection();

// Mapear endpoints necesarios para respuestas y conversaciones (usados por Dev UI)
// Nota: por defecto exponen rutas tipo OpenAI: /v1/responses y /v1/conversations
app.MapOpenAIResponses();
app.MapOpenAIConversations();

// Exponer Dev UI solo en desarrollo
if (builder.Environment.IsDevelopment())
{
    // Dev UI solo para desarrollo
    app.MapDevUI();
}

app.Run();
