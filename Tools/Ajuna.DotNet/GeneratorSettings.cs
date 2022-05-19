namespace Ajuna.DotNet
{
   internal class GeneratorSettings
   {
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
