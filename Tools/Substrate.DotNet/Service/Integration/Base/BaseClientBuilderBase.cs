using System.CodeDom;
using System.Collections.Generic;

namespace Substrate.DotNet.Service.Node.Base
{
   public abstract class BaseClientBuilderBase : BuilderBase
   {
      public List<string> ModuleNames { get; }

      public BaseClientBuilderBase(string projectName, uint id, List<string> moduleNames, NodeTypeResolver typeDict)
          : base(projectName, id, typeDict)
      {
         ModuleNames = moduleNames;
         NamespaceName = $"{ProjectName}.Generated";
         ImportsNamespace.Imports.Add(new CodeNamespaceImport($"{ProjectName}.Generated.Storage"));
      }
   }
}