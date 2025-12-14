using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using System.ComponentModel;

namespace DEV_UI_AGENT.Agents.CallFunction
{
    public class SimpleFunctionsAgent
    {
        public static void Register(WebApplicationBuilder builder)
        {
            // Definimos varias funciones simples con atributos de descripción mejorados
            [Description("Obtiene la hora actual del sistema en formato de 24 horas (HH:mm:ss). Útil para responder preguntas sobre qué hora es.")]
            static string ObtenerHoraActual()
                => DateTime.Now.ToString("HH:mm:ss");

            [Description("Obtiene la fecha actual del sistema en formato dd/MM/yyyy. Útil para saber qué día es hoy.")]
            static string ObtenerFechaActual()
                => DateTime.Now.ToString("dd/MM/yyyy");

            [Description("Calcula la suma aritmética de dos números decimales.")]
            static double Sumar(
                [Description("El primer número a sumar.")] double a,
                [Description("El segundo número a sumar.")] double b)
                => a + b;

            [Description("Calcula el producto de multiplicar dos números decimales.")]
            static double Multiplicar(
                [Description("El primer factor.")] double a,
                [Description("El segundo factor.")] double b)
                => a * b;

            [Description("Convierte una temperatura dada en grados Celsius a grados Fahrenheit.")]
            static double CelsiusAFahrenheit(
                [Description("La temperatura en grados Celsius que se desea convertir.")] double celsius)
                => (celsius * 9 / 5) + 32;

            [Description("Obtiene el clima actual simulado para una ciudad específica.")]
            static string ObtenerClima(
                [Description("El nombre de la ciudad para la cual se desea conocer el clima.")] string ciudad)
            {
                var random = new Random();
                var temperatura = random.Next(-10, 40);
                return $"El clima en {ciudad} es de {temperatura}°C.";
            }

            [Description("Genera un mensaje de saludo personalizado para un usuario específico.")]
            static string Saludar(
                [Description("El nombre de la persona a la que se quiere saludar.")] string nombre)
                => $"¡Hola {nombre}! Encantado de ayudarte.";

            // Registramos el agente con estas funciones como herramientas
            builder.AddAIAgent("AgenteFuncionesSimples", "Eres un agente útil que puede ejecutar funciones matemáticas, de tiempo y conversión.")
                .WithAITools(
                    AIFunctionFactory.Create(ObtenerHoraActual, name: "obtener_hora"),
                    AIFunctionFactory.Create(ObtenerFechaActual, name: "obtener_fecha"),
                    AIFunctionFactory.Create(Sumar, name: "sumar"),
                    AIFunctionFactory.Create(Multiplicar, name: "multiplicar"),
                    AIFunctionFactory.Create(CelsiusAFahrenheit, name: "celsius_a_fahrenheit"),
                    AIFunctionFactory.Create(ObtenerClima, name: "obtener_clima"),
                    AIFunctionFactory.Create(Saludar, name: "saludar")
                );
        }
    }
}
