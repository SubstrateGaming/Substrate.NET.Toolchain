using Ajuna.DotNet.Extensions;
using Ajuna.NetApi.Model.Meta;
using System.Collections.Generic;

namespace Ajuna.DotNet.Service.Node.Base
{
   public abstract class TypeBuilderBase : BuilderBase
   {
      public NodeType TypeDef { get; }

      public TypeBuilderBase(string projectName, uint id, NodeType typeDef, Dictionary<uint, (string, List<string>)> typeDict)
          : base(projectName, id, typeDict)
      {
         TypeDef = typeDef;
         NamespaceName = typeDef.Path != null && typeDef.Path.Length > 1
             ? $"{ProjectName}.Generated.Model.{typeDef.Path[0].MakeMethod()}"
             : $"{ProjectName}.Generated.Model.Base";
      }
   }
}