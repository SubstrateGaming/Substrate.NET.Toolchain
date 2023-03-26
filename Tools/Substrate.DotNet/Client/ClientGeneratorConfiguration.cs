using System;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Substrate.DotNet.Client
{
   /// <summary>
   /// The ClientGeneratorConfiguration holds all configurable settings
   /// for code generation and its output.
   /// </summary>
   public class ClientGeneratorConfiguration
   {
      /// <summary>
      /// The assembly to scan for ControllerBase instance so the generator can create code for the scanned classes.
      /// </summary>
      public Assembly Assembly { get; set; }

      /// <summary>
      /// Actual base class to scan for. Defaults to ControllerBase.
      /// </summary>
      public Type ControllerBaseType { get; set; }

      /// <summary>
      /// Client class file name.
      /// </summary>
      public string ClientClassname { get; set; } = "Client";

      /// <summary>
      /// Basic namespace to generate output for.
      /// </summary>
      public string BaseNamespace { get; set; } = "Substrate.RestClient";

      /// <summary>
      /// Code generation options.
      /// </summary>
      public CodeGeneratorOptions GeneratorOptions { get; set; } = new CodeGeneratorOptions()
      {
         BracingStyle = "C",
         BlankLinesBetweenMembers = false,
      };

      /// <summary>
      /// Where to save the generated code to?
      /// </summary>
      public string OutputDirectory { get; set; }
   }
}
