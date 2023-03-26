#nullable enable
using Substrate.DotNet.Extensions;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi.Model.Types.Metadata.V14;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Substrate.DotNet.Service.Node
{
   public class NodeTypeResolved
   {
      public NodeTypeResolved(NodeType nodeType, NodeTypeName name)
      {
         NodeType = nodeType;
         Name = name;
      }

      public NodeType NodeType { get; private set; }
      public NodeTypeName Name { get; private set; }

      public override string ToString() => Name.ToString();

      public string ClassName => Name.ClassName;
      public string Namespace => Name.Namespace;
   }

   public enum NodeTypeNamespaceSource
   {
      Generated,
      Base,
      Primitive
   }

   public class NodeTypeName
   {
      public NodeTypeResolver Resolver { get; private set; }
      public NodeTypeNamespaceSource NamespaceSource { get; private set; }
      private string BaseName { get; set; }
      public string ClassName => $"{ClassNamePrefix}{BaseName.Split('.').Last()}";
      public string ClassNamePrefix { get; private set; }
      public NodeTypeName[]? Arguments { get; private set; }

      public string ClassNameWithModule
      {
         get
         {
            string className = ClassName;
            int idx = BaseName.LastIndexOf('.');
            if (idx >= 0)
            {
               string moduleName = BaseName.Substring(0, idx);
               return $"{moduleName}.{className}";
            }
            return className;
         }
      }

      public string Namespace
      {
         get
         {
            string[]? paths = BaseName.Split('.').ToArray();
            string[]? reduced = paths.Take(paths.Length - 1).ToArray();
            string? result = string.Join('.', reduced);
            
            if (string.IsNullOrEmpty(result))
            {
               switch (NamespaceSource)
               {
                  case NodeTypeNamespaceSource.Primitive:
                     return "Substrate.NetApi.Model.Types.Primitive";
                  case NodeTypeNamespaceSource.Generated:
                     return $"{Resolver.NetApiProjectName}.Generated.Types.Base";
                  default:
                     break;
               }

               return "Substrate.NetApi.Model.Types.Base";
            }

            // TODO (svnscha) use configurable project name
            return $"{Resolver.NetApiProjectName}.Generated.Model.{result}";
         }
      }

      public static NodeTypeName Primitive(NodeTypeResolver resolver, string baseName) => new NodeTypeName(resolver, NodeTypeNamespaceSource.Primitive, baseName, null);
      public static NodeTypeName Base(NodeTypeResolver resolver, string baseName) => new NodeTypeName(resolver, NodeTypeNamespaceSource.Base, baseName, null);
      public static NodeTypeName Base(NodeTypeResolver resolver, string baseName, NodeTypeName[]? arguments) => new NodeTypeName(resolver, NodeTypeNamespaceSource.Base, baseName, arguments);
      public static NodeTypeName Generated(NodeTypeResolver resolver, string baseName) => new NodeTypeName(resolver, NodeTypeNamespaceSource.Generated, baseName, null);
      public static NodeTypeName Generated(NodeTypeResolver resolver, string baseName, NodeTypeName[]? arguments) => new NodeTypeName(resolver, NodeTypeNamespaceSource.Generated, baseName, arguments);

      internal static NodeTypeName Array(NodeTypeResolver nodeTypeResolver, NodeTypeName nodeTypeName, uint length)
      {
         var result = new NodeTypeName(nodeTypeResolver, NodeTypeNamespaceSource.Generated, nodeTypeName.ClassNameWithModule, null)
         {
            ClassNamePrefix = $"Arr{length}"
         };

         return result;
      }

      private NodeTypeName(NodeTypeResolver resolver, NodeTypeNamespaceSource namespaceType, string baseName, NodeTypeName[]? arguments)
      {
         Resolver = resolver;
         NamespaceSource = namespaceType;
         BaseName = baseName;
         Arguments = arguments;
         ClassNamePrefix = string.Empty;
      }

      public override string ToString()
      {
         string baseQualified;

         if (string.IsNullOrEmpty(ClassNamePrefix))
         {
            baseQualified = $"{Namespace}.{ClassName}";
         }
         else
         {
            baseQualified = $"{Namespace}.{ClassName}";
         }

         if (Arguments == null)
         {
            return baseQualified;
         }

         return $"{baseQualified}<{string.Join(", ", Arguments.Select(x => x.ToString()).ToArray())}>";
      }
   }

   public class NodeTypeResolver
   {
      protected string NodeRuntime { get; private set; }

      public Dictionary<uint, NodeTypeResolved> TypeNames { get; private set; }
      public string NetApiProjectName { get; private set; }

      public NodeTypeResolver(string nodeRuntime, string netApiProjectName, Dictionary<uint, NodeType> types)
      {
         NodeRuntime = nodeRuntime;
         NetApiProjectName = netApiProjectName;
         TypeNames = Resolve(types);
      }

      private Dictionary<uint, NodeTypeResolved> Resolve(Dictionary<uint, NodeType> types)
      {
         var result = new Dictionary<uint, NodeTypeResolved>();

         foreach (uint typeId in types.Keys)
         {
            NodeTypeName name = ResolveTypeName(typeId, types);
            result.Add(typeId, new NodeTypeResolved(types[typeId], name));
         }

         return result;
      }

      private NodeTypeName ResolveTypeName(uint typeId, Dictionary<uint, NodeType> types)
      {
         NodeType nodeType = types[typeId];
         switch (nodeType.TypeDef)
         {
            case TypeDefEnum.Composite:
               {
                  var nodeTypeComposite = (NodeTypeComposite)nodeType;
                  EnsurePathIsNotNull(nodeTypeComposite.Path);
                  return NodeTypeName.Generated(this, ResolvePath(nodeTypeComposite.Path, string.Empty));
               }
            case TypeDefEnum.Variant:
               {
                  var nodeTypeVariant = (NodeTypeVariant)nodeType;
                  EnsurePathIsNotNull(nodeTypeVariant.Path);
                  return ResolveVariantType(nodeTypeVariant, types);
               }
            case TypeDefEnum.Sequence:
               {
                  var nodeTypeSequence = (NodeTypeSequence)nodeType;
                  EnsurePathIsNull(nodeTypeSequence.Path);
                  return NodeTypeName.Base(this, "BaseVec", new NodeTypeName[] { ResolveTypeName(nodeTypeSequence.TypeId, types) });
               }
            case TypeDefEnum.Array:
               {
                  var nodeTypeArray = (NodeTypeArray)nodeType;
                  EnsurePathIsNull(nodeTypeArray.Path);
                  return NodeTypeName.Array(this, ResolveTypeName(nodeTypeArray.TypeId, types), nodeTypeArray.Length);
               }
            case TypeDefEnum.Tuple:
               {
                  var nodeTypeTuple = (NodeTypeTuple)nodeType;
                  EnsurePathIsNull(nodeTypeTuple.Path);
                  if (nodeTypeTuple.TypeIds == null || nodeTypeTuple.TypeIds.Length == 0)
                  {
                     return NodeTypeName.Base(this, "BaseTuple");
                  }

                  EnsureTypeIdsIsNotNull(nodeTypeTuple.TypeIds);
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                  NodeTypeName[]? arguments = nodeTypeTuple.TypeIds.Select(x => ResolveTypeName(x, types)).ToArray();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
                  return NodeTypeName.Base(this, "BaseTuple", arguments);
               }
            case TypeDefEnum.Primitive:
               {
                  var nodeTypePrimitive = (NodeTypePrimitive)nodeType;
                  EnsurePathIsNull(nodeTypePrimitive.Path);
                  return NodeTypeName.Primitive(this, nodeTypePrimitive.Primitive.ToString());
               }
            case TypeDefEnum.Compact:
               {
                  var nodeTypeCompact = (NodeTypeCompact)nodeType;
                  EnsurePathIsNull(nodeTypeCompact.Path);
                  return NodeTypeName.Base(this, "BaseCom", new NodeTypeName[] { ResolveTypeName(nodeTypeCompact.TypeId, types) });
               }
            case TypeDefEnum.BitSequence:
               {
                  var nodeTypeBitSequence = (NodeTypeBitSequence)nodeType;
                  EnsurePathIsNull(nodeTypeBitSequence.Path);
                  NodeTypeName type1 = ResolveTypeName(nodeTypeBitSequence.TypeIdStore, types);
                  NodeTypeName type2 = ResolveTypeName(nodeTypeBitSequence.TypeIdOrder, types);
                  return NodeTypeName.Base(this, "BaseBitSeq", new NodeTypeName[] { type1, type2 });
               }
            default:
               break;
         }

         throw new NotImplementedException("This is not implemented yet.");
      }

      private NodeTypeName ResolveVariantType(NodeTypeVariant nodeTypeVariant, Dictionary<uint, NodeType> types)
      {
         string variantType = GetVariantType(string.Join('.', nodeTypeVariant.Path));
         switch (variantType)
         {
            case "Option":
               return NodeTypeName.Base(this, "BaseOpt", new NodeTypeName[] { ResolveTypeName(nodeTypeVariant.Variants[1].TypeFields[0].TypeId, types) });
            case "Void":
               return NodeTypeName.Base(this, "BaseVoid");
            default:
               break;
         }

         return NodeTypeName.Generated(this, ResolvePath(nodeTypeVariant.Path, variantType));
      }

      public static string GetVariantType(string path)
      {
         if (path == "Option")
         {
            return path;
         }
         else if (path.Contains(".Void"))
         {
            return "Void";
         }

         return "Enum";
      }

      // private string ResolvePath(string[] path, string prefix) => ResolvePathInternal(path.Select(x => x.MakeMethod().ToUpperFirst()).ToArray(), prefix);
      private string ResolvePath(string[] path, string prefix) => ResolvePathInternal(path, prefix);

      private string ResolvePathInternal(string[] path, string prefix)
      {
         if (path.Length == 1)
         {
            return $"{prefix}{string.Join(".", path)}";
         }

         string? lastElement = path[path.Length - 1];
         string[]? previousElements = path.Take(path.Length - 1).ToArray();
         return $"{ResolvePathInternal(previousElements, string.Empty)}.{prefix}{lastElement}";
      }


      private static void EnsureTypeIdsIsNotNull(uint[] typeIds)
      {
         if (typeIds == null || typeIds.Length == 0)
         {
            throw new Exception("Expected that TypeIds are not null and not empty!");
         }
      }

      private static void EnsurePathIsNotNull(string[] path)
      {
         if (path == null || path.Length == 0)
         {
            throw new Exception("Expected that path is not null and not empty!");
         }
      }

      private static void EnsurePathIsNull(string[] path)
      {
         if (path != null)
         {
            throw new Exception("Expected that path is null!");
         }
      }

      internal string GetNamespace(uint id)
      {
         NodeTypeResolved nodeTypeResolved = TypeNames[id];
         return nodeTypeResolved.Namespace;
      }

      private string TypeNameSpace(string[] path)
      {
         if (path == null || path.Length < 2)
         {
            return "Base";
         }

         // heck if we have a versioned name space
         IEnumerable<string> vWhere = path.Where(p => Regex.IsMatch(p, @"v[0-9]+"));
         if (vWhere.Any())
         {
            return path[0].MakeMethod() + "." + vWhere.First().MakeMethod();
         }

         return path[0].MakeMethod();
      }
   }
}
