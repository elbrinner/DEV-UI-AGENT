// Namespaces del Agent Framework y UI Dev
using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using DEV_UI_AGENT.Configuration;
using Microsoft.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// Configuración: appsettings + variables de entorno
builder.Configuration.AddEnvironmentVariables();

// Configurar Azure OpenAI Client
 AzureOpenAIConfig.Register(builder);

// Configurar Ollama Client
//OllamaConfig.Register(builder);

// Registrar agentes general del chat
DEV_UI_AGENT.Agents.General.ChatAgent.Register(builder);

// Registrar agente editor de archivos locales, es necesario configurar la carpeta "EditableFiles" con archivos de texto para editar
DEV_UI_AGENT.Agents.MCP.FileEditorAgent.Register(builder);

// Registrar agente de llamadas a funciones simples
DEV_UI_AGENT.Agents.CallFunction.SimpleFunctionsAgent.Register(builder);

// Registrar servicios requeridos por Dev UI
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
