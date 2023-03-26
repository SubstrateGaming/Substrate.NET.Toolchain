using Substrate.DotNet.Client.Interfaces;
using Substrate.DotNet.Client.Services;
using Substrate.DotNet.Extensions;
using Serilog;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Substrate.DotNet.Client
{
   /// <summary>
   /// The ClientGenerator class handles the actual code generation to build REST API clients for a given Ajuna RestService assembly.
   /// </summary>
   public class UnitTestGenerator
   {
      private readonly ClientGeneratorConfiguration _configuration;
      private string GetNamespace() => $"{_configuration.BaseNamespace}.Generated";

      /// <summary>
      /// Entrypoint.
      /// </summary>
      /// <param name="configuration">Configuration to adjust the generator settings.</param>
      public UnitTestGenerator(ClientGeneratorConfiguration configuration)
      {
         _configuration = configuration;
      }

      /// <summary>
      /// Main entry point to generate controller clients and the general purpose client.
      /// </summary>
      public void Generate(ILogger logger)
      {
         using var reflector = new ReflectorService();
         Assembly assembly = _configuration.Assembly;
         System.Type type = _configuration.ControllerBaseType;
         IEnumerable<IReflectedController> controllers = reflector.GetControllers(assembly, type);

         // Build controller clients.
         foreach (IReflectedController controller in controllers)
         {
            logger.Information("Generate unit test for controller {controller} client.", controller);
            BuildControllerTest(controller);
         }
      }

      /// <summary>
      /// Builds the controller client implementation class.
      /// </summary>
      /// <param name="controller">The controller to generate implementation for.</param>
      private void BuildControllerTest(IReflectedController controller)
      {
         var clientNamespace = new CodeNamespace(GetNamespace());
         AddDefaultControllerImports(clientNamespace);

         var dom = new CodeCompileUnit();
         dom.Namespaces.Add(clientNamespace);

         CodeTypeDeclaration controllerClient = controller.ToUnitTest(clientNamespace);
         clientNamespace.Types.Add(controllerClient);

         // Generate methods.
         foreach (IReflectedEndpoint endpoint in controller.GetEndpoints())
         {
            controllerClient.Members.Add(endpoint.ToUnitTestMethod(controllerClient.Members, controller, clientNamespace));
         }

         ClientCodeWriter.Write(_configuration, dom, clientNamespace, controller.GetUnitTestClassName());

      }

      /// <summary>
      /// Utility function to add some common namespaces to existing CodeNamespace instance.
      /// </summary>
      private static void AddDefaultControllerImports(CodeNamespace ns)
      {
         ns.Imports.Add(new CodeNamespaceImport("System"));
         ns.Imports.Add(new CodeNamespaceImport("NUnit.Framework"));
         ns.Imports.Add(new CodeNamespaceImport(typeof(Task).Namespace));
      }
   }
}
