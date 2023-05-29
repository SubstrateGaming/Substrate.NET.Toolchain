using Substrate.DotNet.Extensions;
using Substrate.NetApi.Model.Meta;
using System.Collections.Generic;

namespace Substrate.DotNet.Service.Node.Base
{
   public abstract class ModuleBuilderBaseRoslyn : BuilderBaseRoslyn
   {
      public Dictionary<uint, NodeType> NodeTypes { get; private set; }

      public PalletModule Module { get; private set; }

      public string PrefixName { get; private set; }

      protected ModuleBuilderBaseRoslyn(string projectName, uint id, PalletModule module, NodeTypeResolver typeDict,
          Dictionary<uint, NodeType> nodeTypes)
          : base(projectName, id, typeDict)
      {
         NodeTypes = nodeTypes;
         Module = module;
         PrefixName = module.Name == "System" ? "Frame" : "Pallet";
         NamespaceName = $"{ProjectName}.Generated.Model.{PrefixName + module.Name.MakeMethod()}";
      }
   }
}