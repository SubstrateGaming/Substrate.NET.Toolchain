using System.CodeDom;
using System.Collections.Generic;

namespace Substrate.DotNet.Service.Node.Base
{
   public abstract class IntegrationBuilderBase : BuilderBase
   {
      public List<string> ModuleNames { get; }
      public string NetApiExtProject { get; private set; }

      public IntegrationBuilderBase(string projectName, uint id, List<string> moduleNames, NodeTypeResolver typeDict)
          : base(projectName, id, typeDict)
      {
         ModuleNames = moduleNames;
         NetApiExtProject = ProjectName.Replace("Integration", "NetApiExt");
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"{NetApiExtProject}.Generated"));
      }

   }
}