// Agents/Copilot/CopilotAgent.cs
using DEV_UI_AGENT.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace DEV_UI_AGENT.Agents.Copilot;

public static class CopilotAgent
{
    public static void Register(WebApplicationBuilder builder)
    {
        builder.AddAIAgent("AgenteCopilot", (sp, key) =>
        {
            var chatClient = sp.GetRequiredService<IChatClient>();

            // Prompt externo (pega tu contenido en Prompts/CopilotAgent.md)
            var prompt = PromptLoader.LoadPrompt("GeneralCopilotAgent.md");

            // A2A: recupera agentes auxiliares registrados
            var chatAgent = sp.GetRequiredKeyedService<AIAgent>("AgenteChat");
            var fileAgent = sp.GetRequiredKeyedService<AIAgent>("FileEditorAgent");

            static string Now() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            static double Add(double a, double b) => a + b;

            static string ObtenerHoraActual() => DateTime.Now.ToString("HH:mm:ss");
            static string ObtenerFechaActual() => DateTime.Now.ToString("dd/MM/yyyy");
            static double Multiplicar(double a, double b) => a * b;
            static double CelsiusAFahrenheit(double celsius) => (celsius * 9 / 5) + 32;
            static string Saludar(string nombre) => $"¡Hola {nombre}! Encantado de ayudarte.";
            static string ObtenerClima(string ciudad)
            {
                var random = new Random();
                var temperatura = random.Next(-10, 40);
                return $"El clima en {ciudad} es de {temperatura}°C.";
            }

            // A2A delegación a ChatAgent
            async Task<string> DelegarChatAsync(string mensaje)
            {
                var result = await chatAgent.RunAsync(new[] { new ChatMessage(ChatRole.User, mensaje) });
                return result.Text ?? string.Empty;
            }

            // A2A delegación a FileEditorAgent (mantiene sandbox del FileEditorAgent)
            async Task<string> DelegarFileAsync(string toolName, string argsJson)
            {
                // Como AIAgent no expone RunToolAsync en esta versión, usamos un protocolo textual simple.
                var mensaje = $"Usa la herramienta '{toolName}' con estos argumentos JSON:\n{argsJson}";
                var result = await fileAgent.RunAsync(new[] { new ChatMessage(ChatRole.User, mensaje) });
                return result.Text ?? "Sin respuesta";
            }

            static string ToJson(object value)
                => JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            // Wrappers MCP (FileEditorAgent) para exponer herramientas de filesystem desde CopilotAgent
            Task<string> LeerArchivoAsync(string path)
                => DelegarFileAsync("leer_archivo", ToJson(new { path }));

            Task<string> LeerLineasAsync(string path, int li, int lf)
                => DelegarFileAsync("leer_lineas", ToJson(new { path, li, lf }));

            Task<string> EscribirArchivoAsync(string path, string contenido)
                => DelegarFileAsync("escribir_archivo", ToJson(new { path, contenido }));

            Task<string> AppendArchivoAsync(string path, string contenido)
                => DelegarFileAsync("append_archivo", ToJson(new { path, contenido }));

            Task<string> ReemplazarLineaAsync(string path, int n, string contenido)
                => DelegarFileAsync("reemplazar_linea", ToJson(new { path, n, contenido }));

            Task<string> InsertarLineaAsync(string path, int n, string contenido)
                => DelegarFileAsync("insertar_linea", ToJson(new { path, n, contenido }));

            Task<string> EliminarLineaAsync(string path, int n)
                => DelegarFileAsync("eliminar_linea", ToJson(new { path, n }));

            Task<string> ListarArchivosAsync(string path)
                => DelegarFileAsync("listar_archivos", ToJson(new { path }));

            Task<string> BuscarArchivosAsync(string path, string patron)
                => DelegarFileAsync("buscar_archivos", ToJson(new { path, patron }));

            Task<string> DirectorioTrabajoAsync()
                => DelegarFileAsync("directorio_trabajo", ToJson(new { }));

            // Nota: en este SDK, las herramientas se adjuntan al crear el agente (no vía .WithAITools sobre AIAgentBuilder).
            var tools = new AITool[]
            {
                // Utilidades para orquestación
                AIFunctionFactory.Create(() => Now(), name: "now"),
                AIFunctionFactory.Create((double a, double b) => Add(a, b), name: "add"),

                // CallFunction (simples)
                AIFunctionFactory.Create(() => ObtenerHoraActual(), name: "obtener_hora"),
                AIFunctionFactory.Create(() => ObtenerFechaActual(), name: "obtener_fecha"),
                AIFunctionFactory.Create((double a, double b) => Add(a, b), name: "sumar"),
                AIFunctionFactory.Create((double a, double b) => Multiplicar(a, b), name: "multiplicar"),
                AIFunctionFactory.Create((double celsius) => CelsiusAFahrenheit(celsius), name: "celsius_a_fahrenheit"),
                AIFunctionFactory.Create((string ciudad) => ObtenerClima(ciudad), name: "obtener_clima"),
                AIFunctionFactory.Create((string nombre) => Saludar(nombre), name: "saludar"),

                // Puente A2A explícito
                AIFunctionFactory.Create((string mensaje) => DelegarChatAsync(mensaje), name: "delegar_chat"),

                // Puente MCP (a través de FileEditorAgent)
                AIFunctionFactory.Create((string path) => LeerArchivoAsync(path), name: "leer_archivo"),
                AIFunctionFactory.Create((string path, int li, int lf) => LeerLineasAsync(path, li, lf), name: "leer_lineas"),
                AIFunctionFactory.Create((string path, string contenido) => EscribirArchivoAsync(path, contenido), name: "escribir_archivo"),
                AIFunctionFactory.Create((string path, string contenido) => AppendArchivoAsync(path, contenido), name: "append_archivo"),
                AIFunctionFactory.Create((string path, int n, string contenido) => ReemplazarLineaAsync(path, n, contenido), name: "reemplazar_linea"),
                AIFunctionFactory.Create((string path, int n, string contenido) => InsertarLineaAsync(path, n, contenido), name: "insertar_linea"),
                AIFunctionFactory.Create((string path, int n) => EliminarLineaAsync(path, n), name: "eliminar_linea"),
                AIFunctionFactory.Create((string path) => ListarArchivosAsync(path), name: "listar_archivos"),
                AIFunctionFactory.Create((string path, string patron) => BuscarArchivosAsync(path, patron), name: "buscar_archivos"),
                AIFunctionFactory.Create(() => DirectorioTrabajoAsync(), name: "directorio_trabajo")
            };

            return chatClient.CreateAIAgent(
                    name: key,
                    instructions: prompt,
                    description: "Copilot orquestador de programación (A2A + MCP filesystem + CallFunction).",
                    tools: tools
                )
                .AsBuilder()
                .UseOpenTelemetry(configure: c => c.EnableSensitiveData = builder.Environment.IsDevelopment())
                .Build();
        });
    }
}
