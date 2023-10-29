using Substrate.DotNet.Extensions;
using Substrate.DotNet.Service.Node;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi.Model.Types.Metadata.V14;
using Substrate.NetApi.Model.Types.Primitive;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Reflection.Metadata.Ecma335;

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
            case "Runtime":
               {
                  RunetimeBuilder.Init(ProjectName, nodeType.Id, nodeType, typeDict).Create().Build(write: write, out bool success, basePath);
                  if (!success)
                  {
                     Logger.Error($"Could not build type {nodeType.Id}!");
                  }
                  break;
               }
            case "Enum":
               {
                  EnumBuilder.Init(ProjectName, nodeType.Id, nodeType, typeDict).Create().Build(write: write, out bool success, basePath);
                  if (!success)
                  {
                     Logger.Error($"Could not build type {nodeType.Id}!");
                  }

                  break;
               }
            default:
               break;
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
         var metadataNaming = new MetadataNaming(nodeTypes);
         List<string> rewritedName = new();

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
                  string suggestedClassName = metadataNaming.WriteClassName(type);
                  if(suggestedClassName != type.Path[^1] && !rewritedName.Any(x => x == suggestedClassName))
                  {
                     type.Path[^1] = suggestedClassName;
                  } else
                  {
                     type.Path[^1] = type.Path[^1] + "T" + (_countPaths.ContainsKey(key) ? _countPaths[key] : 1);
                  }
                  rewritedName.Add(type.Path[^1]);
               }
            }
         }
      }

      /// <summary>
      /// Refine current metadata by removing unecessary classes that encapsulate mostly Rust lists
      /// C# is more permissive so we don't need to wrap BaseVec<> into a class
      /// </summary>
      public static void RefinedUnnecessaryWrapper(MetaData metadata)
      {
         Dictionary<uint, NodeType> nodeTypes = metadata.NodeMetadata.Types;
         var metadataNaming = new MetadataNaming(nodeTypes);

         bool hasSomethingChanged = false;
         do
         {
            IDictionary<uint, uint> wrapperNodes = ExtractWrappers(nodeTypes);

            // Loop over all node types to switch sourceId to new destinationId
            hasSomethingChanged = MapSourceToDestination(nodeTypes, wrapperNodes);

            if(hasSomethingChanged)
            {
               RemoveSourceIds(nodeTypes, metadataNaming, wrapperNodes);
               RefineModules(metadata, wrapperNodes);
            }
         } while (hasSomethingChanged);
      }

      /// <summary>
      /// Remove all unecessary id
      /// </summary>
      /// <param name="nodeTypes"></param>
      /// <param name="metadataNaming"></param>
      /// <param name="wrapperNodes"></param>
      private static void RemoveSourceIds(Dictionary<uint, NodeType> nodeTypes, MetadataNaming metadataNaming, IDictionary<uint, uint> wrapperNodes)
      {
         foreach (KeyValuePair<uint, uint> node in wrapperNodes)
         {
            Log.Verbose("\t Replace {sourceType} (id = {sourceKey}) by {destinationType} (id = {destinationKey})", metadataNaming.WriteType(node.Key), node.Key, metadataNaming.WriteType(node.Value), node.Value);
            nodeTypes.Remove(node.Key);
         }
      }

      /// <summary>
      /// Loop over all modules and replace old occurences
      /// </summary>
      /// <param name="metadata"></param>
      /// <param name="wrapperNodes"></param>
      private static void RefineModules(MetaData metadata, IDictionary<uint, uint> wrapperNodes)
      {
         Dictionary<uint, PalletModule> modules = metadata.NodeMetadata.Modules;
         foreach (KeyValuePair<uint, PalletModule> module in modules)
         {
            PalletStorage storage = module.Value.Storage;

            if (storage == null || storage.Entries == null)
            {
               continue;
            }

            foreach (Entry entry in storage.Entries)
            {
               if (wrapperNodes.ContainsKey(entry.TypeMap.Item1))
               {
                  entry.TypeMap = new(wrapperNodes[entry.TypeMap.Item1], entry.TypeMap.Item2);
               }
               if (entry.TypeMap.Item2 != null && wrapperNodes.ContainsKey(entry.TypeMap.Item2.Key))
               {
                  entry.TypeMap.Item2.Key = wrapperNodes[entry.TypeMap.Item2.Key];
               }

               if (entry.TypeMap.Item2 != null && wrapperNodes.ContainsKey(entry.TypeMap.Item2.Value))
               {
                  entry.TypeMap.Item2.Value = wrapperNodes[entry.TypeMap.Item2.Value];
               }

            }
         }
      }

      /// <summary>
      /// Check every TypeDef composite which have only on TypeDef Sequence as property field.
      /// Target multi generic references (BoundedVec, WeakBoundedVec etc)
      /// Return a dictionnary of sourceId, destinationId
      /// </summary>
      /// <param name="nodeTypes"></param>
      /// <returns></returns>
      private static IDictionary<uint, uint> ExtractWrappers(Dictionary<uint, NodeType> nodeTypes)
      {
         var wrappers =
                     nodeTypes
                     .Where(x => x.Value.TypeDef == TypeDefEnum.Composite)
                     .Select(x => (NodeTypeComposite)x.Value)
                     .Where(x => x.Path != null)
                     .GroupBy(x => string.Join('.', x.Path))
                     .Where(x => x.Count() > 1)
                     .SelectMany(x => x)
                     .Where(x => x.TypeFields != null && x.TypeFields.Length == 1)
                     .Where(x => nodeTypes[x.TypeFields[0].TypeId].TypeDef == TypeDefEnum.Sequence)
                     .Select(x => new
                     {
                        sourceId = x.Id,
                        sourceName = x.Path != null ? string.Join(".", x.Path) : string.Empty,
                        destinationId = nodeTypes[x.TypeFields[0].TypeId].Id
                     });

         IDictionary<uint, uint> wrapperNodes = wrappers.ToDictionary(x => x.sourceId, x => x.destinationId);
         return wrapperNodes;
      }

      /// <summary>
      /// Change all occurences of old Id to Destination Id
      /// </summary>
      /// <param name="nodeTypes"></param>
      /// <param name="wrapperNodes"></param>
      /// <returns>True if a node has been changed</returns>
      private static bool MapSourceToDestination(Dictionary<uint, NodeType> nodeTypes, IDictionary<uint, uint> wrapperNodes)
      {
         bool anyUpdate = false;
         foreach (KeyValuePair<uint, NodeType> node in nodeTypes)
         {
            switch (node.Value)
            {
               case NodeTypeVariant detailVariant when detailVariant.Variants is not null:
                  foreach (NodeTypeField nodeTypeField in detailVariant.Variants
                     .Where(x => x.TypeFields != null)
                     .SelectMany(x => x.TypeFields))
                  {
                     if (wrapperNodes.ContainsKey(nodeTypeField.TypeId))
                     {
                        nodeTypeField.TypeId = wrapperNodes[nodeTypeField.TypeId];
                        anyUpdate = true;
                     }
                  }
                  break;

               case NodeTypeCompact detailCompact when wrapperNodes.ContainsKey(detailCompact.TypeId):
                  detailCompact.TypeId = wrapperNodes[detailCompact.TypeId];
                  anyUpdate = true;
                  break;

               case NodeTypeComposite detailComposite when detailComposite.TypeFields is not null:
                  foreach (NodeTypeField typeField in detailComposite.TypeFields)
                  {
                     if (wrapperNodes.ContainsKey(typeField.TypeId))
                     {
                        typeField.TypeId = wrapperNodes[typeField.TypeId];
                        anyUpdate = true;
                     }
                  }
                  break;

               case NodeTypeSequence detailSequence when wrapperNodes.ContainsKey(detailSequence.TypeId):
                  detailSequence.TypeId = wrapperNodes[detailSequence.TypeId];
                  anyUpdate = true;
                  break;

               case NodeTypeTuple detailTuple when detailTuple.TypeIds != null:
                  for (int i = 0; i < detailTuple.TypeIds.Length; i++)
                  {
                     if (wrapperNodes.ContainsKey(detailTuple.TypeIds[i]))
                     {
                        detailTuple.TypeIds[i] = wrapperNodes[detailTuple.TypeIds[i]];
                        anyUpdate = true;
                     }
                  }
                  break;

               case NodeTypeArray detailArray when wrapperNodes.ContainsKey(detailArray.TypeId):
                  detailArray.TypeId = wrapperNodes[detailArray.TypeId];
                  anyUpdate = true;
                  break;

               case NodeTypeBitSequence detailBitSequence:
                  if(wrapperNodes.ContainsKey(detailBitSequence.TypeIdStore))
                  {
                     detailBitSequence.TypeIdStore = wrapperNodes[detailBitSequence.TypeIdStore];
                  }

                  if (wrapperNodes.ContainsKey(detailBitSequence.TypeIdOrder))
                  {
                     detailBitSequence.TypeIdOrder = wrapperNodes[detailBitSequence.TypeIdOrder];
                  }

                  string x = "1";
                  break;
            }
         }

         return anyUpdate;
      }
   }
}