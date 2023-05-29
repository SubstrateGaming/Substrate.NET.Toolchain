using Substrate.NetApi.Model.Meta;

namespace Substrate.DotNet.Service.Node.Base
{
   public abstract class TypeBuilderBase : BuilderBase
   {
      public NodeType TypeDef { get; }

      public TypeBuilderBase(string projectName, uint id, NodeType typeDef, NodeTypeResolver resolver)
          : base(projectName, id, resolver)
      {
         TypeDef = typeDef;
         NamespaceName = resolver.GetNamespace(id);
      }
   }
}