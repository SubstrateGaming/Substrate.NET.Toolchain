#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Ajuna.DotNet
{
   public class ProjectSettings
   {
      [AllowNull]
      internal string ProjectName { get; set; }

      [AllowNull]
      internal string ProjectNamespace { get; set; }
      
      [AllowNull]
      internal string ProjectSolutionName { get; set; }
      
      [AllowNull]
      internal string ProjectDirectory { get; set; }
   }

   internal class GeneratorSettings
   {
      internal ProjectSettings? RestService { get; set; }

      internal bool WantService { get; set; }

      internal string ServiceArgument { get; set; }

      internal bool WantClient { get; set; }

      internal string ClientArgument { get; set; }

      internal string RestServiceProjectName { get; set; } = "Ajuna.RestService";
      internal string RestClientProjectName { get; set; } = "Ajuna.RestClient";
      internal string NetApiProjectName { get; set; } = "Ajuna.NetApiExt";

      internal string NodeRuntime { get; set; } = "ajuna_solo_runtime";

   }
}
