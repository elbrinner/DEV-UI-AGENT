using DEV_UI_AGENT.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;

namespace DEV_UI_AGENT.Agents.General
{
    public class ChatAgent
    {
        public static void Register(WebApplicationBuilder builder)
        {
            builder.AddAIAgent("AgenteChat", (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>();
                var prompt = PromptLoader.LoadPrompt("GeneralAgenteChat.md");

                return chatClient.CreateAIAgent(
                    name: key,
                    instructions: prompt,
                    description: "Agente de propósito general que responde preguntas en castellano."
                )
                .AsBuilder()
                .UseOpenTelemetry(configure: c =>
                    c.EnableSensitiveData = builder.Environment.IsDevelopment())
                .Build();
            });
        }
    }
}
