using System.CodeDom;
using System.Net.Http;
using System.Reflection;
using Ajuna.RestService.ClientGenerator.Interfaces;

namespace Ajuna.RestService.ClientGenerator.Extensions
{
   /// <summary>
   /// Simplifies access to ReflectedController interface.
   /// </summary>
   internal static class ReflectedControllerExtensions
   {
      /// <summary>
      /// Converts a reflected controller to an interface code element.
      /// The generated interface is public.
      /// </summary>
      /// <param name="controller">The controller to convert.</param>
      internal static CodeTypeDeclaration ToInterface(this IReflectedController controller)
      {
         return new CodeTypeDeclaration(controller.GetInterfaceName())
         {
            TypeAttributes = TypeAttributes.Public | TypeAttributes.Interface,
         };
      }

      /// <summary>
      /// Converts a reflected controller to a client class that implements the previously generated interface.
      /// The generated client class is public but sealed.
      /// </summary>
      /// <param name="controller">The controller to convert.</param>
      /// <param name="currentNamespace">The current namespace where the generated class will be attached to.</param>
      internal static CodeTypeDeclaration ToClient(this IReflectedController controller, CodeNamespace currentNamespace)
      {
         // Client class
         var target = new CodeTypeDeclaration(controller.GetClientClassName())
         {
            TypeAttributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed,
         };

         // Client class inheritance (IClient)
         // TODO: Do we really want to have this hard-coded?
         target.BaseTypes.Add(new CodeTypeReference("BaseClient"));
         target.BaseTypes.Add(GetInterfaceName(controller));

         // Generate private member variabels.
         target.Members.AddHttpClientPrivateMember(currentNamespace);

         // Generate constructor.
         var ctor = new CodeConstructor()
         {
            Attributes = MemberAttributes.Public | MemberAttributes.Final
         };

         // TODO: Do we really want to have this hard-coded?
         ctor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(HttpClient), "httpClient"));
         ctor.Statements.Add(new CodeAssignStatement(
            new CodeVariableReferenceExpression("_httpClient"),
            new CodeVariableReferenceExpression("httpClient")));

         target.Members.Add(ctor);
         return target;
      }

      /// <summary>
      /// Returns an interface name for the given controller.
      /// The generated name uses the following pattern: I[Controller]Client.
      /// </summary>
      /// <param name="controller">The controller to query the interface name for.</param>
      internal static string GetInterfaceName(this IReflectedController controller) => $"I{controller.Name}Client";

      /// <summary>
      /// Returns a client class name for the given controller.
      /// The generated name uses the following pattern: [Controller]Client.
      /// </summary>
      /// <param name="controller">The controller to query the client class name for.</param>
      internal static string GetClientClassName(this IReflectedController controller) => $"{controller.Name}Client";

      /// <summary>
      /// Returns an URL string for the given controller.
      /// The URL string is the base path for any endpoint request.
      /// </summary>
      /// <param name="controller">The controller to query the URL string for.</param>
      internal static string GetEndpointUrl(this IReflectedController controller) => controller.Name.Replace("Controller", string.Empty).ToLower();
   }
}
