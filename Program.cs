// Namespaces del Agent Framework y UI Dev
using Microsoft.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n: appsettings + variables de entorno
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Registrar servicios requeridos por Dev UI y endpoints de OpenAI
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

var app = builder.Build();

// Pipeline HTTP m�nimo
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
