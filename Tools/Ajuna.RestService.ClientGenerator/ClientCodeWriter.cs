using Microsoft.CSharp;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ajuna.RestService.ClientGenerator
{
   /// <summary>
   /// Utility class to easily write a compilation unit to an actual code file.
   /// </summary>
   internal class ClientCodeWriter
   {
      internal static void Write(
         ClientGeneratorConfiguration configuration,
         CodeCompileUnit dom,
         CodeNamespace ns,
         string unit)
      {
         var provider = new CSharpCodeProvider();
         var path = Path.Combine(configuration.OutputDirectory, Path.Combine(ns.Name.Replace(configuration.BaseNamespace, string.Empty).Split('.').ToArray()));
         Directory.CreateDirectory(path);

         var filePath = Path.Combine(path, $"{unit}.cs");
         using (var memoryStream = new MemoryStream())
         using (var writer = new StreamWriter(memoryStream))
         {
            provider.GenerateCodeFromCompileUnit(dom, writer, configuration.GeneratorOptions);
            writer.Flush();
            var code = Encoding.UTF8.GetString(memoryStream.ToArray());
            code = PatchSystemThreadingTasksUsage(code);
            code = PatchNamespaceUsage(code, ns.Imports);
            code = PatchPublicVirtualWithAsyncPublic(code);
            code = PatchSendRequestFunctionCall(code);
            File.WriteAllText(filePath, code);
         }
      }
      /// <summary>
      /// This will simplify namespace usage of fully qualified types to imported types.
      /// </summary>
      private static string PatchNamespaceUsage(string code, CodeNamespaceImportCollection imports)
      {
         var ns = new List<string>();

         foreach (CodeNamespaceImport import in imports)
         {
            ns.Add(import.Namespace);
         }

         var index = code.IndexOf("public sealed class");
         if (index == -1)
            index = code.IndexOf("public interface");

         if (index == -1)
            return code;

         var lhs = code.Substring(0, index);
         var rhs = code.Remove(0, index);

         ns = ns.OrderByDescending(x => x.Length).ToList();
         foreach (var name in ns)
            rhs = rhs.Replace($"{name}.", string.Empty);

         return lhs + rhs;
      }

      /// <summary>
      /// Dirty workaround: This will patch "public override " to "public override async ".
      /// </summary>
      private static string PatchPublicVirtualWithAsyncPublic(string code)
      {
         return code.Replace("public virtual ", "public async ");
      }

      /// <summary>
      /// Dirty workaround: This will patch "System.Threading.Tasks.Task<" to "Task<".
      /// </summary>
      private static string PatchSystemThreadingTasksUsage(string code)
      {
         return code.Replace("System.Threading.Tasks.Task<", "Task<");
      }

      /// <summary>
      /// Dirty workaround: This will patch "return this.SendRequest" to "return await SendRequest"
      /// </summary>
      private static string PatchSendRequestFunctionCall(string code)
      {
         return code.Replace("return this.SendRequest", "return await SendRequest");
      }
   }
}
