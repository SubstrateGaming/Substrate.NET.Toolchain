using Substrate.NetApi.Model.Meta;

namespace Substrate.DotNet.Service.Node.Base
{
   public abstract class TypeBuilderBaseCodeDom : BuilderBaseCodeDom
   {
      public NodeType TypeDef { get; }

      public TypeBuilderBaseCodeDom(string projectName, uint id, NodeType typeDef, NodeTypeResolver resolver)
          : base(projectName, id, resolver)
      {
         TypeDef = typeDef;
         NamespaceName = resolver.GetNamespace(id);
      }
   }
}