using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using System.ComponentModel;

namespace DEV_UI_AGENT.Agents.MCP
{
    public static class FileEditorAgent
    {
        // Directorio raíz cargado una vez desde configuración
        private static string _rootPath = string.Empty;

        private static void EnsureRootPath(WebApplicationBuilder builder)
        {
            if (!string.IsNullOrEmpty(_rootPath))
            {
                return;
            }

            var configured = builder.Configuration["FileEditorRoot"];
            _rootPath = !string.IsNullOrWhiteSpace(configured)
                ? Path.GetFullPath(configured)
                : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Data"));

            Directory.CreateDirectory(_rootPath);
        }

        private static string RootPath()
            => _rootPath;

        // Resuelve rutas relativas asegurando que no se salga del root
        private static string ResolvePath(string relativePath)
        {
            var root = RootPath();
            var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));

            if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Acceso fuera del directorio permitido.");

            return fullPath;
        }

        // ------------------- Funciones de archivos -------------------

        [Description("Lee el contenido completo de un archivo dentro del directorio raíz.")]
        public static string LeerArchivo(string relativePath)
        {
            var path = ResolvePath(relativePath);
            return File.Exists(path) ? File.ReadAllText(path) : $"No existe {relativePath}";
        }

        [Description("Lee un rango de líneas de un archivo (inclusivo).")]
        public static string LeerLineas(string relativePath, int lineaInicio, int lineaFin)
        {
            if (lineaInicio <= 0 || lineaFin < lineaInicio)
            {
                return "Rango de líneas inválido.";
            }

            var path = ResolvePath(relativePath);
            if (!File.Exists(path)) return $"No existe {relativePath}";

            var todas = File.ReadAllLines(path);
            lineaInicio = Math.Max(1, lineaInicio);
            lineaFin = Math.Min(todas.Length, lineaFin);

            var seleccion = todas
                .Skip(lineaInicio - 1)
                .Take(lineaFin - lineaInicio + 1);

            return string.Join(Environment.NewLine, seleccion);
        }

        [Description("Escribe contenido en un archivo (sobrescribe).")]
        public static string EscribirArchivo(string relativePath, string contenido)
        {
            var path = ResolvePath(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, contenido);
            return $"Archivo {relativePath} sobrescrito.";
        }

        [Description("Agrega contenido al final de un archivo.")]
        public static string AppendArchivo(string relativePath, string contenido)
        {
            var path = ResolvePath(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.AppendAllText(path, contenido);
            return $"Contenido agregado a {relativePath}";
        }

        [Description("Reemplaza una línea concreta (1-based) en un archivo.")]
        public static string ReemplazarLinea(string relativePath, int numeroLinea, string nuevoContenido)
        {
            if (numeroLinea <= 0)
            {
                return "El número de línea debe ser mayor que cero.";
            }

            var path = ResolvePath(relativePath);
            if (!File.Exists(path)) return $"No existe {relativePath}";

            var lineas = File.ReadAllLines(path).ToList();
            if (numeroLinea > lineas.Count)
            {
                return $"El archivo solo tiene {lineas.Count} líneas.";
            }

            lineas[numeroLinea - 1] = nuevoContenido;
            File.WriteAllLines(path, lineas);
            return $"Línea {numeroLinea} actualizada en {relativePath}.";
        }

        [Description("Inserta una línea antes de la posición indicada (1-based).")]
        public static string InsertarLinea(string relativePath, int numeroLinea, string contenido)
        {
            if (numeroLinea <= 0)
            {
                return "El número de línea debe ser mayor que cero.";
            }

            var path = ResolvePath(relativePath);
            var lineas = File.Exists(path) ? File.ReadAllLines(path).ToList() : new List<string>();

            // Si la línea es mayor que el total, se agrega al final
            numeroLinea = Math.Min(numeroLinea, lineas.Count + 1);

            lineas.Insert(numeroLinea - 1, contenido);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllLines(path, lineas);
            return $"Línea insertada en posición {numeroLinea} de {relativePath}.";
        }

        [Description("Elimina una línea concreta (1-based) de un archivo.")]
        public static string EliminarLinea(string relativePath, int numeroLinea)
        {
            if (numeroLinea <= 0)
            {
                return "El número de línea debe ser mayor que cero.";
            }

            var path = ResolvePath(relativePath);
            if (!File.Exists(path)) return $"No existe {relativePath}";

            var lineas = File.ReadAllLines(path).ToList();
            if (numeroLinea > lineas.Count)
            {
                return $"El archivo solo tiene {lineas.Count} líneas.";
            }

            lineas.RemoveAt(numeroLinea - 1);
            File.WriteAllLines(path, lineas);
            return $"Línea {numeroLinea} eliminada de {relativePath}.";
        }

        [Description("Elimina un archivo.")]
        public static string EliminarArchivo(string relativePath)
        {
            var path = ResolvePath(relativePath);
            if (!File.Exists(path)) return $"No existe {relativePath}";
            File.Delete(path);
            return $"Archivo {relativePath} eliminado.";
        }

        [Description("Renombra un archivo.")]
        public static string RenombrarArchivo(string relativePath, string nuevoNombre)
        {
            var path = ResolvePath(relativePath);
            if (!File.Exists(path)) return $"No existe {relativePath}";
            var dir = Path.GetDirectoryName(path)!;
            var nuevoPath = Path.Combine(dir, nuevoNombre);
            File.Move(path, nuevoPath, overwrite: true);
            return $"Archivo renombrado a {nuevoNombre}";
        }

        [Description("Copia un archivo a otra ubicación relativa.")]
        public static string CopiarArchivo(string origen, string destino)
        {
            var src = ResolvePath(origen);
            var dst = ResolvePath(destino);
            Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
            File.Copy(src, dst, overwrite: true);
            return $"Archivo copiado a {destino}";
        }

        [Description("Mueve un archivo a otra ubicación relativa.")]
        public static string MoverArchivo(string origen, string destino)
        {
            var src = ResolvePath(origen);
            var dst = ResolvePath(destino);
            Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
            File.Move(src, dst, overwrite: true);
            return $"Archivo movido a {destino}";
        }

        [Description("Obtiene información de un archivo.")]
        public static string InfoArchivo(string relativePath)
        {
            var path = ResolvePath(relativePath);
            if (!File.Exists(path)) return $"No existe {relativePath}";
            var info = new FileInfo(path);
            return $"Nombre: {info.Name}, Tamaño: {info.Length} bytes, Creado: {info.CreationTime}";
        }

        [Description("Verifica si un archivo existe.")]
        public static string ExisteArchivo(string relativePath)
        {
            var path = ResolvePath(relativePath);
            return File.Exists(path) ? "Sí existe" : "No existe";
        }

        // ------------------- Funciones de directorios -------------------

        [Description("Lista los archivos de un directorio.")]
        public static string ListarArchivos(string relativePath)
        {
            var path = ResolvePath(relativePath);
            if (!Directory.Exists(path)) return $"No existe {relativePath}";
            return string.Join("\n", Directory.GetFiles(path));
        }

        [Description("Lista los subdirectorios de un directorio.")]
        public static string ListarDirectorios(string relativePath)
        {
            var path = ResolvePath(relativePath);
            if (!Directory.Exists(path)) return $"No existe {relativePath}";
            return string.Join("\n", Directory.GetDirectories(path));
        }

        [Description("Crea un directorio.")]
        public static string CrearDirectorio(string relativePath)
        {
            var path = ResolvePath(relativePath);
            Directory.CreateDirectory(path);
            return $"Directorio creado en {relativePath}";
        }

        [Description("Elimina un directorio.")]
        public static string EliminarDirectorio(string relativePath)
        {
            var path = ResolvePath(relativePath);
            if (!Directory.Exists(path)) return $"No existe {relativePath}";
            Directory.Delete(path, recursive: true);
            return $"Directorio {relativePath} eliminado.";
        }

        [Description("Obtiene información de un directorio.")]
        public static string InfoDirectorio(string relativePath)
        {
            var path = ResolvePath(relativePath);
            if (!Directory.Exists(path)) return $"No existe {relativePath}";
            var info = new DirectoryInfo(path);
            return $"Nombre: {info.Name}, Creado: {info.CreationTime}";
        }

        [Description("Verifica si un directorio existe.")]
        public static string ExisteDirectorio(string relativePath)
        {
            var path = ResolvePath(relativePath);
            return Directory.Exists(path) ? "Sí existe" : "No existe";
        }

        // ------------------- Utilidades -------------------

        [Description("Busca archivos por patrón en un directorio.")]
        public static string BuscarArchivos(string relativePath, string patron)
        {
            var path = ResolvePath(relativePath);
            if (!Directory.Exists(path)) return $"No existe {relativePath}";
            return string.Join("\n", Directory.GetFiles(path, patron));
        }

        [Description("Busca directorios por patrón en un directorio.")]
        public static string BuscarDirectorios(string relativePath, string patron)
        {
            var path = ResolvePath(relativePath);
            if (!Directory.Exists(path)) return $"No existe {relativePath}";
            return string.Join("\n", Directory.GetDirectories(path, patron));
        }

        [Description("Normaliza una ruta relativa a absoluta dentro del root.")]
        public static string ResolverRuta(string relativePath)
        {
            return ResolvePath(relativePath);
        }

        [Description("Devuelve el directorio raíz configurado.")]
        public static string DirectorioTrabajo()
        {
            return RootPath();
        }

        // ------------------- Registro del agente -------------------

        public static void Register(WebApplicationBuilder builder)
        {
            EnsureRootPath(builder);

            builder.AddAIAgent("FileEditorAgent", "Agente para manipular el sistema de archivos dentro de un directorio raíz seguro.")
                .WithAITools(
                    AIFunctionFactory.Create((string path) => LeerArchivo(path), name: "leer_archivo"),
                    AIFunctionFactory.Create((string path, int li, int lf) => LeerLineas(path, li, lf), name: "leer_lineas"),
                    AIFunctionFactory.Create((string path, string contenido) => EscribirArchivo(path, contenido), name: "escribir_archivo"),
                    AIFunctionFactory.Create((string path, string contenido) => AppendArchivo(path, contenido), name: "append_archivo"),
                    AIFunctionFactory.Create((string path, int n, string contenido) => ReemplazarLinea(path, n, contenido), name: "reemplazar_linea"),
                    AIFunctionFactory.Create((string path, int n, string contenido) => InsertarLinea(path, n, contenido), name: "insertar_linea"),
                    AIFunctionFactory.Create((string path, int n) => EliminarLinea(path, n), name: "eliminar_linea"),
                    AIFunctionFactory.Create((string path) => EliminarArchivo(path), name: "eliminar_archivo"),
                    AIFunctionFactory.Create((string path, string nuevoNombre) => RenombrarArchivo(path, nuevoNombre), name: "renombrar_archivo"),
                    AIFunctionFactory.Create((string origen, string destino) => CopiarArchivo(origen, destino), name: "copiar_archivo"),
                    AIFunctionFactory.Create((string origen, string destino) => MoverArchivo(origen, destino), name: "mover_archivo"),
                    AIFunctionFactory.Create((string path) => InfoArchivo(path), name: "info_archivo"),
                    AIFunctionFactory.Create((string path) => ExisteArchivo(path), name: "existe_archivo"),
                    AIFunctionFactory.Create((string path) => ListarArchivos(path), name: "listar_archivos"),
                    AIFunctionFactory.Create((string path) => ListarDirectorios(path), name: "listar_directorios"),
                    AIFunctionFactory.Create((string path) => CrearDirectorio(path), name: "crear_directorio"),
                    AIFunctionFactory.Create((string path) => EliminarDirectorio(path), name: "eliminar_directorio"),
                    AIFunctionFactory.Create((string path) => InfoDirectorio(path), name: "info_directorio"),
                    AIFunctionFactory.Create((string path) => ExisteDirectorio(path), name: "existe_directorio"),
                    AIFunctionFactory.Create((string path, string patron) => BuscarArchivos(path, patron), name: "buscar_archivos"),
                    AIFunctionFactory.Create((string path, string patron) => BuscarDirectorios(path, patron), name: "buscar_directorios"),
                    AIFunctionFactory.Create((string path) => ResolverRuta(path), name: "resolver_ruta"),
                    AIFunctionFactory.Create(() => DirectorioTrabajo(), name: "directorio_trabajo")
                );
        }
    }
}
