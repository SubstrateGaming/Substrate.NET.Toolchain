using Ajuna.NetApi.Model.Meta;
using System.Collections.Generic;

namespace Ajuna.DotNet.Node.Base
{
   public abstract class ModulesBuilderBase : BuilderBase
   {
      public Dictionary<uint, NodeType> NodeTypes { get; }

      public PalletModule[] Modules { get; }

      public string PrefixName { get; }

      public ModulesBuilderBase(uint id, PalletModule[] modules, Dictionary<uint, (string, List<string>)> typeDict,
          Dictionary<uint, NodeType> nodeTypes)
          : base(id, typeDict)
      {
         NodeTypes = nodeTypes;
         Modules = modules;
      }
   }
}