using Ajuna.NetApi.Model.Meta;
using System.Collections.Generic;

namespace Ajuna.DotNet.Node.Base
{
   public abstract class ModulesBuilderBase : BuilderBase
   {
      public Dictionary<uint, NodeType> NodeTypes { get; private set; }

      public PalletModule[] Modules { get; private set; }

      public ModulesBuilderBase(string projectName, uint id, PalletModule[] modules, Dictionary<uint, (string, List<string>)> typeDict,
          Dictionary<uint, NodeType> nodeTypes)
          : base(projectName, id, typeDict)
      {
         NodeTypes = nodeTypes;
         Modules = modules;
      }
   }
}