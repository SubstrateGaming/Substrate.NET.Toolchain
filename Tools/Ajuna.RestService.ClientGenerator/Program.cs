using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;

namespace Ajuna.RestService.ClientGenerator
{
    internal class Program
    {
        /// <summary>
        /// Utility to load assembly and generate a Ajuna compatible REST Client.
        /// </summary>
        internal static void Main(string[] args)
        {
            // Initialize logging.
            Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Verbose()
             .WriteTo.Console()
             .CreateLogger();

            // Base path to load assemblies from and base path to configure output directory.
            var executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var filePath = Path.Combine(executingDirectory, "Ajuna.RestService.dll");

            if (!File.Exists(filePath))
            {
                Log.Error("Please provide an existing file path to Ajuna.RestService.dll. The file {file} does not exist.", filePath);
                return;
            }

            using (var loader = new AssemblyResolver(filePath))
            {
                // Initialize configuration.
                var configuration = new ClientGeneratorConfiguration()
                {
                    Assembly = loader.Assembly,
                    ControllerBaseType = typeof(ControllerBase),
                    OutputDirectory = Path.Combine(executingDirectory, "Generated", "Ajuna.RestClient"),
                    GeneratorOptions = new CodeGeneratorOptions()
                    {
                        BlankLinesBetweenMembers = false,
                        BracingStyle = "C",
                        IndentString = "   "
                    }
                };

#if DEBUG
                configuration.OutputDirectory = @"D:\Github\Ajuna\Ajuna.RestClient\Ajuna.RestClient";
#endif

                // Build and execute the client generator.
                var client = new ClientGenerator(configuration);
                client.Generate(Log.Logger);
            }
        }
    }
}
