using Substrate.DotNet.Extensions;
using Substrate.DotNet.Service.Node;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi.Model.Types.Metadata.V14;
using Substrate.NetApi.Model.Types.Primitive;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Substrate.DotNet.Service.Generators.Base
{

   /// <summary>
   /// Contains the common as well as the abstract methods to be implemented
   /// so that the solution and the respective classes are generated
   /// </summary>
   public abstract class SolutionGeneratorBase
   {
      protected string NodeRuntime { get; private set; }
      protected ILogger Logger { get; private set; }

      protected string ProjectName { get; private set; }

      protected SolutionGeneratorBase(ILogger logger, string nodeRuntime, string projectName)
      {
         NodeRuntime = nodeRuntime;
         ProjectName = projectName;
         Logger = logger;
      }

      /// <summary>
      /// Generates the classes as well as the solution structure and files
      /// </summary>
      /// <param name="metadata"></param>
      public void Generate(MetaData metadata)
      {
         GenerateClasses(metadata);
      }

      /// <summary>
      /// Generates the respective classes 
      /// </summary>
      /// <param name="metadata"></param>
      protected abstract void GenerateClasses(MetaData metadata);

      protected NodeTypeResolver GenerateTypes(Dictionary<uint, NodeType> nodeTypes, string basePath, bool write)
      {
         var resolver = new NodeTypeResolver(NodeRuntime, ProjectName, nodeTypes);

         foreach (KeyValuePair<uint, NodeTypeResolved> kvp in resolver.TypeNames)
         {
            NodeTypeResolved nodeTypeResolved = kvp.Value;
            NodeType nodeType = nodeTypeResolved.NodeType;

            switch (nodeType.TypeDef)
            {
               case TypeDefEnum.Composite:
                  {
                     var type = nodeType as NodeTypeComposite;
                     StructBuilder.Init(ProjectName, type.Id, type, resolver)
                         .Create()
                         .Build(write: write, out bool success, basePath);
                     
                     if (!success)
                     {
                        Logger.Error($"Could not build type {type.Id}!");
                     }

                     break;
                  }
               case TypeDefEnum.Variant:
                  {
                     var type = nodeType as NodeTypeVariant;
                     string variantType = NodeTypeResolver.GetVariantType(string.Join('.', nodeType.Path));
                     CallVariant(variantType, type, ref resolver, write, basePath);
                     break;
                  }
               case TypeDefEnum.Array:
                  {
                     var type = nodeType as NodeTypeArray;
                     ArrayBuilder.Create(ProjectName, type.Id, type, resolver)
                         .Create()
                         .Build(write: write, out bool success, basePath);

                     if (!success)
                     {
                        Logger.Error($"Could not build type {type.Id}!");
                     }

                     break;
                  }
               default:
                  break; // Handled by type resolver
            }
         }
         return resolver;
      }

      private void CallVariant(string variantType, NodeTypeVariant nodeType, ref NodeTypeResolver typeDict, bool write, string basePath = null)
      {
         switch (variantType)
         {
            case "Enum":
               {
                  EnumBuilder.Init(ProjectName, nodeType.Id, nodeType, typeDict).Create().Build(write: write, out bool success, basePath);
                  if (!success)
                  {
                     Logger.Error($"Could not build type {nodeType.Id}!");
                  }

                  break;
               }

            case "Option":
               // TODO (darkfriend77) ???
               break;

            case "Void":
               // TODO (darkfriend77) ???
               break;

            default:
               throw new NotSupportedException($"Unknown variant type {variantType}");
         }
      }
      
      private static Dictionary<string, int> GetRuntimeIndex(Dictionary<uint, NodeType> nodeTypes, string runtime, string runtimeType)
      {
         NodeType nodeType = nodeTypes.Select(p => p.Value).Where(p => p.Path != null && p.Path.Length == 2 && p.Path[0] == runtime && p.Path[1] == runtimeType).FirstOrDefault();
         if (nodeType is null or not NodeTypeVariant)
         {
            throw new Exception($"Node Index changed for {runtime}.{runtimeType} and {nodeType.GetType().Name}");
         }

         Dictionary<string, int> result = new();
         foreach (TypeVariant variant in (nodeType as NodeTypeVariant).Variants)
         {
            result.Add(variant.Name, variant.Index);
         }

         return result;
      }

      protected static void GetGenericStructs(Dictionary<uint, NodeType> nodeTypes)
      {
         Dictionary<string, int> _countPaths = new();
         for (uint id = 0; id < nodeTypes.Keys.Max(); id++)
         {
            if (!nodeTypes.TryGetValue(id, out NodeType nodeType))
            {
               continue;
            }

            if (nodeType.TypeDef == TypeDefEnum.Composite)
            {
               var type = nodeType as NodeTypeComposite;
               string key = string.Join('.', type.Path);
               if (_countPaths.ContainsKey(key))
               {
                  _countPaths[key]++;
               }
               else
               {
                  _countPaths[key] = 1;
               }
            }
         }

         var generics = _countPaths.Where(kv => kv.Value > 1).Select(kv => kv.Key).ToList();

         // TODO (svnscha) Should really fix that instead of logging?
         // generics.ForEach(p => Console.WriteLine($"Fixing generic: {p} - please do it properly, once you've more time!!!"));

         _countPaths.Clear();
         for (uint id = 0; id < nodeTypes.Keys.Max(); id++)
         {
            if (!nodeTypes.TryGetValue(id, out NodeType nodeType))
            {
               continue;
            }

            if (nodeType.TypeDef == TypeDefEnum.Composite)
            {
               var type = nodeType as NodeTypeComposite;
               string key = string.Join('.', type.Path);
               if (_countPaths.ContainsKey(key))
               {
                  _countPaths[key]++;
               }
               else
               {
                  _countPaths[key] = 1;
               }

               if (generics.Contains(key))
               {
                  type.Path[^1] = type.Path[^1] + "T" + (_countPaths.ContainsKey(key) ? _countPaths[key] : 1);
               }
            }
         }
      }
   }
}