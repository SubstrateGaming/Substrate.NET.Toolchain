using Ajuna.DotNet.Extensions;
using Ajuna.DotNet.Service.Node;
using Ajuna.NetApi.Model.Meta;
using Ajuna.NetApi.Model.Types.Metadata.V14;
using Ajuna.NetApi.Model.Types.Primitive;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ajuna.DotNet.Service.Generators.Base
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

      protected Dictionary<uint, (string, List<string>)> GenerateTypes(Dictionary<uint, NodeType> nodeTypes, string basePath, bool write)
      {
         var typeDict = new Dictionary<uint, (string, List<string>)>();

         // TODO (svnscha): Why 10 iterations?
         int iterations = 10;

         for (int i = 0; i < iterations; i++)
         {
            for (uint id = 0; id < nodeTypes.Keys.Max(); id++)
            {
               if (!nodeTypes.TryGetValue(id, out NodeType nodeType) || typeDict.ContainsKey(id))
               {
                  continue;
               }

               switch (nodeType.TypeDef)
               {
                  case TypeDefEnum.Composite:
                     {
                        var type = nodeType as NodeTypeComposite;
                        (string, List<string>) fullItem = StructBuilder.Init(ProjectName, type.Id, type, typeDict)
                            .Create()
                            .Build(write: write, out bool success, basePath);
                        if (success)
                        {
                           typeDict.Add(type.Id, fullItem);
                        }

                        break;
                     }
                  case TypeDefEnum.Variant:
                     {
                        var type = nodeType as NodeTypeVariant;
                        string variantType = SolutionGeneratorBase.GetVariantType(NodeRuntime, string.Join('.', nodeType.Path));
                        CallVariant(variantType, type, ref typeDict, write, basePath);
                        break;
                     }
                  case TypeDefEnum.Sequence:
                     {
                        var type = nodeType as NodeTypeSequence;
                        if (typeDict.TryGetValue(type.TypeId, out (string, List<string>) fullItem))
                        {
                           string typeName = $"BaseVec<{fullItem.Item1}>";
                           typeDict.Add(type.Id, (typeName, fullItem.Item2));
                        }

                        break;
                     }
                  case TypeDefEnum.Array:
                     {
                        var type = nodeType as NodeTypeArray;
                        (string, List<string>) fullItem = ArrayBuilder.Create(ProjectName, type.Id, type, typeDict)
                            .Create()
                            .Build(write: write, out bool success, basePath);
                        if (success)
                        {
                           typeDict.Add(type.Id, fullItem);
                        }

                        break;
                     }
                  case TypeDefEnum.Tuple:
                     {
                        var type = nodeType as NodeTypeTuple;
                        SolutionGeneratorBase.CallTuple(type, ref typeDict);
                        break;
                     }
                  case TypeDefEnum.Primitive:
                     {
                        var type = nodeType as NodeTypePrimitive;
                        CallPrimitive(type, ref typeDict);
                        break;
                     }
                  case TypeDefEnum.Compact:
                     {
                        var type = nodeType as NodeTypeCompact;
                        if (typeDict.TryGetValue(type.TypeId, out (string, List<string>) fullItem))
                        {
                           string typeName = $"BaseCom<{fullItem.Item1}>";
                           typeDict.Add(type.Id, (typeName, fullItem.Item2));
                        }

                        break;
                     }
                  case TypeDefEnum.BitSequence:
                     {
                        var type = nodeType as NodeTypeBitSequence;
                        if (typeDict.TryGetValue(type.TypeIdStore, out (string, List<string>) fullItemStore)
                         && typeDict.TryGetValue(type.TypeIdOrder, out (string, List<string>) fullItemOrder))
                        {
                           string typeName = $"BaseBitSeq<{fullItemStore.Item1},{fullItemOrder.Item1}>";
                           var list = new List<string>();
                           list.AddRange(fullItemStore.Item2);
                           list.AddRange(fullItemOrder.Item2);
                           typeDict.Add(type.Id, (typeName, list));
                        }
                        break;
                     }
                  default:
                     throw new NotImplementedException($"Unimplemented enumeration of node type {nodeType.TypeDef}");
               }
            }
         }

         return typeDict;
      }

      private static string GetVariantType(string nodeRunetime, string path)
      {
         if (path == "Option")
         {
            return path;
         }
         else if (path == "Result")
         {
            return path;
         }
         else if ((path.Contains("pallet_") || path.Contains(".pallet.")) && path.Contains(".Call"))
         {
            return "Call";
         }
         else if ((path.Contains("pallet_") || path.Contains(".pallet.")) &&
                  (path.Contains(".Event") || path.Contains(".RawEvent")))
         {
            return "Event";
         }
         else if ((path.Contains("pallet_") || path.Contains(".pallet.")) && path.Contains(".Error"))
         {
            return "Error";
         }
         else if (path.Contains($"{nodeRunetime}.Event") || path.Contains($"{nodeRunetime}.Call"))
         {
            return "Runtime";
         }
         else if (path.Contains(".Void"))
         {
            return "Void";
         }
         else
         {
            return "Enum";
         }
      }

      private void CallVariant(string variantType, NodeTypeVariant nodeType, ref Dictionary<uint, (string, List<string>)> typeDict, bool write, string basePath = null)
      {
         switch (variantType)
         {
            case "Option":
               {
                  if (typeDict.TryGetValue(nodeType.Variants[1].TypeFields[0].TypeId, out (string, List<string>) fullItem))
                  {
                     typeDict.Add(nodeType.Id, ($"BaseOpt<{fullItem.Item1}>", fullItem.Item2));
                  }

                  break;
               }

            case "Result":
               {
                  var spaces = new List<string>() { $"Ajuna.NetApi.Model.Types.Base" };
                  typeDict.Add(nodeType.Id,
                      ($"BaseTuple<BaseTuple, {ProjectName}.Model.SpRuntime.EnumDispatchError>",
                          spaces));
                  break;
               }

            case "Call":
               {
                  (string, List<string>) fullItem = (ProjectName + $".{nodeType.Path[0].MakeMethod()}Call", new List<string>() { ProjectName });
                  typeDict.Add(nodeType.Id, fullItem);
                  break;
               }

            case "Event":
               {
                  (string, List<string>) fullItem = (ProjectName + $".{nodeType.Path[0].MakeMethod()}Event", new List<string>() { ProjectName });
                  typeDict.Add(nodeType.Id, fullItem);
                  break;
               }

            case "Error":
               {
                  (string, List<string>) fullItem = (ProjectName + $".{nodeType.Path[0].MakeMethod()}Error", new List<string>() { ProjectName });
                  typeDict.Add(nodeType.Id, fullItem);
                  break;
               }

            case "Runtime":
               {
                  (string, List<string>) fullItem = RunetimeBuilder.Init(ProjectName, nodeType.Id, nodeType, typeDict).Create().Build(write: write, out bool success, basePath);
                  if (success)
                  {
                     typeDict.Add(nodeType.Id, fullItem);
                  }

                  break;
               }

            case "Void":
               {
                  var spaces = new List<string>() { $"Ajuna.NetApi.Model.Types.Base" };
                  typeDict.Add(nodeType.Id, ($"Ajuna.NetApi.Model.Types.Base.BaseVoid", spaces));
                  break;
               }

            case "Enum":
               {
                  (string, List<string>) fullItem = EnumBuilder.Init(ProjectName, nodeType.Id, nodeType, typeDict).Create().Build(write: write, out bool success, basePath);
                  if (success)
                  {
                     typeDict.Add(nodeType.Id, fullItem);
                  }

                  break;
               }

            default:
               throw new NotImplementedException();
         }
      }

      private void CallPrimitive(NodeTypePrimitive nodeType, ref Dictionary<uint, (string, List<string>)> typeDict)
      {
         List<string> spaces = new() { $"Ajuna.NetApi.Model.Types.Primitive" };
         string path = $"Ajuna.NetApi.Model.Types.Primitive.";
         switch (nodeType.Primitive)
         {
            case TypeDefPrimitive.Bool:
               typeDict.Add(nodeType.Id, (path + nameof(Bool), spaces));
               break;
            case TypeDefPrimitive.Char:
               typeDict.Add(nodeType.Id, (path + nameof(PrimChar), spaces));
               break;
            case TypeDefPrimitive.Str:
               typeDict.Add(nodeType.Id, (path + nameof(Str), spaces));
               break;
            case TypeDefPrimitive.U8:
               typeDict.Add(nodeType.Id, (path + nameof(U8), spaces));
               break;
            case TypeDefPrimitive.U16:
               typeDict.Add(nodeType.Id, (path + nameof(U16), spaces));
               break;
            case TypeDefPrimitive.U32:
               typeDict.Add(nodeType.Id, (path + nameof(U32), spaces));
               break;
            case TypeDefPrimitive.U64:
               typeDict.Add(nodeType.Id, (path + nameof(U64), spaces));
               break;
            case TypeDefPrimitive.U128:
               typeDict.Add(nodeType.Id, (path + nameof(U128), spaces));
               break;
            case TypeDefPrimitive.U256:
               typeDict.Add(nodeType.Id, (path + nameof(U256), spaces));
               break;
            case TypeDefPrimitive.I8:
               typeDict.Add(nodeType.Id, (path + nameof(I8), spaces));
               break;
            case TypeDefPrimitive.I16:
               typeDict.Add(nodeType.Id, (path + nameof(I16), spaces));
               break;
            case TypeDefPrimitive.I32:
               typeDict.Add(nodeType.Id, (path + nameof(I32), spaces));
               break;
            case TypeDefPrimitive.I64:
               typeDict.Add(nodeType.Id, (path + nameof(I64), spaces));
               break;
            case TypeDefPrimitive.I128:
               typeDict.Add(nodeType.Id, (path + nameof(I128), spaces));
               break;
            case TypeDefPrimitive.I256:
               typeDict.Add(nodeType.Id, (path + nameof(I256), spaces));
               break;
            default:
               throw new NotImplementedException($"Please implement {nodeType.Primitive}, in Ajuna.NetApi.");
         }
      }

      private static void CallTuple(NodeTypeTuple nodeType, ref Dictionary<uint, (string, List<string>)> typeDict)
      {
         var typeIds = new List<string>();
         var imports = new List<string>();
         for (int j = 0; j < nodeType.TypeIds.Length; j++)
         {
            uint typeId = nodeType.TypeIds[j];
            if (!typeDict.TryGetValue(typeId, out (string, List<string>) fullItem))
            {
               typeIds = null;
               break;
            }

            imports.AddRange(fullItem.Item2);
            typeIds.Add(fullItem.Item1);
         }

         // all types found
         if (typeIds != null)
         {
            string typeName = $"BaseTuple{(typeIds.Count > 0 ? "<" + string.Join(',', typeIds.ToArray()) + ">" : "")}";
            typeDict.Add(nodeType.Id, (typeName, imports.Distinct().ToList()));
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