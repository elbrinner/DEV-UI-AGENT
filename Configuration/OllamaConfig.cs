using Microsoft.Extensions.AI;

namespace DEV_UI_AGENT.Configuration
{
    public class OllamaConfig
    {
        public static void Register(WebApplicationBuilder builder)
        {
            var endpoint = builder.Configuration["OLLAMA_ENDPOINT"] ?? "http://localhost:11434";
            var modelId = builder.Configuration["OLLAMA_MODEL"] ?? "llama3.1:8b";

            var chatClient = new OllamaChatClient(new Uri(endpoint), modelId);

            builder.Services.AddChatClient(chatClient);
        }
    }
}
