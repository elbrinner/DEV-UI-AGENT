using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using System.ComponentModel;

namespace DEV_UI_AGENT.Agents.CallFunction
{
    public class SimpleFunctionsAgent
    {
        public static void Register(WebApplicationBuilder builder)
        {
            // Definimos varias funciones simples con atributos de descripción
            [Description("Devuelve la hora actual en formato HH:mm:ss.")]
            static string ObtenerHoraActual()
                => DateTime.Now.ToString("HH:mm:ss");

            [Description("Calcula la suma de dos números.")]
            static double Sumar(
                [Description("Primer número.")] double a,
                [Description("Segundo número.")] double b)
                => a + b;

            [Description("Convierte grados Celsius a Fahrenheit.")]
            static double CelsiusAFahrenheit(
                [Description("Temperatura en grados Celsius.")] double celsius)
                => (celsius * 9 / 5) + 32;

            [Description("Devuelve un saludo personalizado.")]
            static string Saludar(
                [Description("Nombre de la persona.")] string nombre)
                => $"¡Hola {nombre}! Encantado de ayudarte.";

            // Registramos el agente con estas funciones como herramientas
            builder.AddAIAgent("AgenteFuncionesSimples", "Eres un agente que puede ejecutar funciones básicas.")
                .WithAITools(
                    AIFunctionFactory.Create(ObtenerHoraActual, name: "obtener_hora"),
                    AIFunctionFactory.Create(Sumar, name: "sumar"),
                    AIFunctionFactory.Create(CelsiusAFahrenheit, name: "celsius_a_fahrenheit"),
                    AIFunctionFactory.Create(Saludar, name: "saludar")
                );
        }
    }
}
