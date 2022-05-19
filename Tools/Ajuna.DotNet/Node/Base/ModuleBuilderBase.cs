using Ajuna.DotNet.Extensions;
using Ajuna.NetApi.Model.Meta;
using System.Collections.Generic;

namespace Ajuna.DotNet.Node.Base
{
   public abstract class ModuleBuilderBase : BuilderBase
   {
      public Dictionary<uint, NodeType> NodeTypes { get; }

      public PalletModule Module { get; }

      public string PrefixName { get; }

      public ModuleBuilderBase(uint id, PalletModule module, Dictionary<uint, (string, List<string>)> typeDict,
          Dictionary<uint, NodeType> nodeTypes)
          : base(id, typeDict)
      {
         NodeTypes = nodeTypes;
         Module = module;
         PrefixName = module.Name == "System" ? "Frame" : "Pallet";
         NameSpace = $"{BASE_NAMESPACE}.Model.{PrefixName + module.Name.MakeMethod()}";
      }
   }
}