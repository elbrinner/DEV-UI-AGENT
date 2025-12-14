namespace DEV_UI_AGENT.Services
{
    public class PromptLoader
    {
        public static string LoadPrompt(string fileName)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Prompts", fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException($"No se encontró el archivo de prompt: {path}");

            return File.ReadAllText(path);
        }
    }
}
