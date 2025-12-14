using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using System.ClientModel;

namespace DEV_UI_AGENT.Configuration
{
    public class AzureOpenAIConfig
    {
        public static void Register(WebApplicationBuilder builder)
        {
            var endpoint = builder.Configuration["AZURE_OPENAI_ENDPOINT"]!;
            var key = builder.Configuration["AZURE_OPENAI_API_KEY"]!;
            var deploymentName = builder.Configuration["AZURE_OPENAI_DEPLOYMENT_NAME"] ?? "gpt-4o-mini";

            var chatClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(key))
                .GetChatClient(deploymentName)
                .AsIChatClient();

            builder.Services.AddChatClient(chatClient);
        }
    }
}
