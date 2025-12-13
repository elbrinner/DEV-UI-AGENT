# DEV-UI-AGENT

Proyecto mínimo en **ASP.NET Core** que muestra cómo levantar **DEV UI** (Microsoft Agents) sobre un host web y exponer los endpoints necesarios para interactuar con un modelo vía **Azure OpenAI** usando el **Agent Framework**.

La idea es tener un “backend de agente” listo para desarrollo local: arrancas el servidor, abres la Dev UI y pruebas conversaciones/respuestas contra tu deployment.

## Para qué sirve

- Entender la integración básica de **Microsoft.Agents.AI** + **Microsoft.Agents.AI.DevUI** en un proyecto web.
- Exponer endpoints compatibles con estilo OpenAI para que la Dev UI pueda llamar al backend.
- Probar prompts y flujos de conversación en local, con configuración por `appsettings`/variables de entorno.

## Qué hace el código (resumen)

En `Program.cs`:

- Registra los servicios:
  - `AddOpenAIResponses()`
  - `AddOpenAIConversations()`
- Mapea endpoints HTTP:
  - `MapOpenAIResponses()`
  - `MapOpenAIConversations()`
- En **Development** expone la **Dev UI**:
  - `MapDevUI("/devui")`

## Rutas principales

Con la configuración actual:

- **Dev UI (solo Development):** `http://localhost:5166/devui` (o `https://localhost:7171/devui`)
- **Responses API:** `/v1/responses`
- **Conversations API:** `/v1/conversations`

> Nota: Los endpoints `/v1/*` los aporta el paquete `Microsoft.Agents.AI` cuando llamas a `MapOpenAIResponses()` / `MapOpenAIConversations()`.

## Requisitos

- .NET SDK que soporte `net10.0`.
- Acceso a Azure OpenAI:
  - Endpoint del recurso
  - API Key
  - Nombre del deployment

## Configuración (Azure OpenAI)

El proyecto lee configuración desde `appsettings.json` y variables de entorno.

Claves usadas:

- `AZURE_OPENAI_ENDPOINT`
- `AZURE_OPENAI_API_KEY`
- `AZURE_OPENAI_DEPLOYMENT_NAME`

### Opción A: Variables de entorno (recomendado)

En PowerShell:

```pwsh
$env:AZURE_OPENAI_ENDPOINT = "https://tu-recurso.openai.azure.com"
$env:AZURE_OPENAI_API_KEY = "<tu-api-key>"
$env:AZURE_OPENAI_DEPLOYMENT_NAME = "<tu-deployment>"
```

### Opción B: appsettings (solo local)

Puedes rellenar `appsettings.Development.json` con tus valores locales (evita commitear secretos). En este repo `appsettings.json` está ignorado por `.gitignore`.

## Cómo ejecutar

Perfiles de lanzamiento en `Properties/launchSettings.json`:

- HTTP: `http://localhost:5166`
- HTTPS: `https://localhost:7171` (y también `http://localhost:5166`)

Comandos:

```pwsh
dotnet restore
dotnet run --launch-profile https
```

Luego abre:

- `https://localhost:7171/devui` (o `http://localhost:5166/devui`)

## Estructura del proyecto

- `Program.cs`: Host web + mapeo de endpoints + Dev UI.
- `appsettings*.json`: Configuración (endpoint/key/deployment).
- `DEV-UI-AGENT.http`: Archivo para pruebas manuales (puedes añadir requests a `/v1/*`).

## Troubleshooting rápido

- La Dev UI solo se mapea si `ASPNETCORE_ENVIRONMENT=Development`.
- Si HTTPS da problemas de certificado, prueba con el perfil `http`.
