using Ajuna.DotNet.Extensions;
using Ajuna.NetApi.Model.Meta;
using System.Collections.Generic;

namespace Ajuna.DotNet.Node.Base
{
   public abstract class ModuleBuilderBase : BuilderBase
   {
      public Dictionary<uint, NodeType> NodeTypes { get; private set; }

      public PalletModule Module { get; private set; }

      public string PrefixName { get; private set; }

      public ModuleBuilderBase(string projectName, uint id, PalletModule module, Dictionary<uint, (string, List<string>)> typeDict,
          Dictionary<uint, NodeType> nodeTypes)
          : base(projectName, id, typeDict)
      {
         NodeTypes = nodeTypes;
         Module = module;
         PrefixName = module.Name == "System" ? "Frame" : "Pallet";
         NamespaceName = $"{ProjectName}.Model.{PrefixName + module.Name.MakeMethod()}";
      }
   }
}