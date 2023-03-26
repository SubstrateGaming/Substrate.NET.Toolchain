using Substrate.DotNet.Client.Interfaces;
using Substrate.DotNet.Client.Services;
using Substrate.DotNet.Extensions;
using Serilog;
using System.CodeDom;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Substrate.DotNet.Client
{
   /// <summary>
   /// The ClientGenerator class handles the actual code generation to build REST API clients for a given Substrate RestService assembly.
   /// </summary>
   public class ClientGenerator
   {
      private readonly ClientGeneratorConfiguration _configuration;
      private string GetNamespace(string ns) => $"{_configuration.BaseNamespace}.Generated.{ns}";
      private string InterfaceNamespace => GetNamespace("Interfaces");
      private string ClientsNamespace => GetNamespace("Clients");

      /// <summary>
      /// Entrypoint.
      /// </summary>
      /// <param name="configuration">Configuration to adjust the generator settings.</param>
      public ClientGenerator(ClientGeneratorConfiguration configuration)
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
            logger.Information("Generate controller {controller} client.", controller);
            BuildControllerClientInterface(controller);
            BuildControllerClientImplementation(controller);
         }

         // Build general purpose client.
         BuildClient(controllers);
      }

      /// <summary>
      /// This method builds a general purpose REST client that has access to all specialized clients.
      /// </summary>
      /// <param name="controllers"></param>
      private void BuildClient(IEnumerable<IReflectedController> controllers)
      {
         string className = _configuration.ClientClassname;
         var targetNamespace = new CodeNamespace(_configuration.BaseNamespace);
         AddDefaultControllerImports(targetNamespace);

         var dom = new CodeCompileUnit();
         dom.Namespaces.Add(targetNamespace);

         // Generate a general use client that accesses the other interfaces.
         var target = new CodeTypeDeclaration(className)
         {
            TypeAttributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed
         };

         // Generate constructor.
         var ctor = new CodeConstructor()
         {
            Attributes = MemberAttributes.Public
         };
         ctor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(HttpClient), "httpClient"));
         ctor.Parameters.Add(new CodeParameterDeclarationExpression("BaseSubscriptionClient", "subscriptionClient"));

         ctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("_httpClient"), new CodeVariableReferenceExpression("httpClient")));
         ctor.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("_subscriptionClient"), new CodeVariableReferenceExpression("subscriptionClient")));

         // Generate HttpClient private variable.
         target.Members.AddHttpClientPrivateMember(targetNamespace);
         target.Members.AddPrivateFieldAssignableFromConstructor("BaseSubscriptionClient", "_subscriptionClient", targetNamespace);

         // Generate constructor assignments for all controller clients.
         foreach (IReflectedController controller in controllers)
         {
            string controllerMemberName = controller.GetClientClassName();

            target.Members.Add(new CodeMemberField()
            {
               Attributes = MemberAttributes.Public,
               Name = controllerMemberName,
               Type = new CodeTypeReference(controller.GetInterfaceName())
            });

            ctor.Statements.Add(new CodeAssignStatement(
               new CodeVariableReferenceExpression(controllerMemberName),
               new CodeSnippetExpression($"new {controller.GetClientClassName()}(_httpClient, _subscriptionClient)")));
         }

         // Add constructor to member list.
         target.Members.Add(ctor);

         // Add the default imports.
         targetNamespace.Imports.Add(new CodeNamespaceImport(InterfaceNamespace));
         targetNamespace.Imports.Add(new CodeNamespaceImport(ClientsNamespace));
         targetNamespace.Types.Add(target);

         ClientCodeWriter.Write(_configuration, dom, targetNamespace, className);
      }

      /// <summary>
      /// Builds the controller client implementation class.
      /// </summary>
      /// <param name="controller">The controller to generate implementation for.</param>
      private void BuildControllerClientImplementation(IReflectedController controller)
      {
         var clientNamespace = new CodeNamespace(ClientsNamespace);
         AddDefaultControllerImports(clientNamespace);

         var dom = new CodeCompileUnit();
         dom.Namespaces.Add(clientNamespace);

         CodeTypeDeclaration controllerClient = controller.ToClient(clientNamespace);
         clientNamespace.Types.Add(controllerClient);

         // Generate methods.
         foreach (IReflectedEndpoint endpoint in controller.GetEndpoints())
         {
            controllerClient.Members.Add(endpoint.ToClientMethod(controller, clientNamespace));
            controllerClient.Members.Add(endpoint.ToSubscriptionClientMethod(controller, clientNamespace));
         }

         clientNamespace.Imports.Add(new CodeNamespaceImport(InterfaceNamespace));
         ClientCodeWriter.Write(_configuration, dom, clientNamespace, controller.GetClientClassName());

      }

      /// <summary>
      /// Builds the controller client interface.
      /// </summary>
      /// <param name="controller">The controller to generate the interface for.</param>
      private void BuildControllerClientInterface(IReflectedController controller)
      {
         var interfaceNamespace = new CodeNamespace(InterfaceNamespace);
         AddDefaultControllerImports(interfaceNamespace);

         var dom = new CodeCompileUnit();
         dom.Namespaces.Add(interfaceNamespace);

         CodeTypeDeclaration controllerInterface = controller.ToInterface();
         interfaceNamespace.Types.Add(controllerInterface);
         foreach (IReflectedEndpoint endpoint in controller.GetEndpoints())
         {
            controllerInterface.Members.Add(endpoint.ToInterfaceMethod(interfaceNamespace));
            controllerInterface.Members.Add(endpoint.ToSubscriptionInterfaceMethod());
         }

         ClientCodeWriter.Write(_configuration, dom, interfaceNamespace, controller.GetInterfaceName());
      }

      /// <summary>
      /// Utility function to add some common namespaces to existing CodeNamespace instance.
      /// </summary>
      private static void AddDefaultControllerImports(CodeNamespace ns)
      {
         ns.Imports.Add(new CodeNamespaceImport("System"));
         ns.Imports.Add(new CodeNamespaceImport(typeof(Task).Namespace));
      }
   }
}
