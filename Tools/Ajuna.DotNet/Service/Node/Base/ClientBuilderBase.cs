using System.Collections.Generic;

namespace Ajuna.DotNet.Service.Node.Base
{
   public abstract class ClientBuilderBase : BuilderBase
   {
      public List<(string, List<string>)> ModuleNames { get; }

      public ClientBuilderBase(string projectName, uint id, List<(string, List<string>)> moduleNames, Dictionary<uint, (string, List<string>)> typeDict)
          : base(projectName, id, typeDict)
      {
         ModuleNames = moduleNames;
         NamespaceName = $"{ProjectName}.Generated";
      }
   }
}