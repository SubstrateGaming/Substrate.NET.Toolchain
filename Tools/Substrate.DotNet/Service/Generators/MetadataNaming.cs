using Substrate.DotNet.Extensions;
using Substrate.NetApi.Model.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.DotNet.Service.Generators
{
   public class MetadataNaming
   {
      public const string DefaultTypeSeparator = "";
      private readonly Dictionary<uint, NodeType> _nodeTypes;

      public bool DisplayPropertyName { get; init; } = true;
      public bool AggregatePropertyType { get; init; } = false;

      public MetadataNaming(Dictionary<uint, NodeType> nodeTypes)
      {
         _nodeTypes = nodeTypes;
      }

      public string WriteType(uint typeId)
         => WriteType(GetPalletType(typeId));

      public string WriteType(NodeType detailType)
      {

         if (detailType is NodeTypeVariant detailVariant)
         {
            return WriteNodeVariant(detailVariant);
         }
         else if (detailType is NodeTypeCompact detailCompact)
         {
            return WriteNodeCompact(detailCompact);
         }
         else if (detailType is NodeTypePrimitive detailPrimitive)
         {
            return WriteNodePrimitive(detailPrimitive);
         }
         else if (detailType is NodeTypeComposite detailComposite)
         {
            return WriteNodeComposite(detailComposite);
         }
         else if (detailType is NodeTypeSequence detailSequence)
         {
            return WriteNodeSequence(detailSequence);
         }
         else if (detailType is NodeTypeTuple detailTuple)
         {
            return WriteNodeTuple(detailTuple);
         }
         else if (detailType is NodeTypeArray detailArray)
         {
            return WriteNodeArray(detailArray);
         }
         else
         {
            throw new NotSupportedException("Type not supported yet..."); // BitSequence ??
         }
      }

      public string WriteNodeVariant(NodeTypeVariant nodeType)
      {
         return nodeType.Path[^1];
      }

      public string WriteNodeCompact(NodeTypeCompact nodeType)
      {
         return $"{nodeType.TypeDef}{WriteType(nodeType.TypeId)}";
      }

      public string WriteNodePrimitive(NodeTypePrimitive nodeType)
      {
         return nodeType.Primitive.ToString().ToUpperFirst();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="nodeType"></param>
      /// <param name="expandDetails">Define if we have to display class details</param>
      /// <returns></returns>
      public string WriteNodeComposite(NodeTypeComposite nodeType, bool expandDetails = false)
      {
         string display = nodeType.Path[^1];
         if (expandDetails && nodeType.TypeParams != null && nodeType.TypeParams.Length > 0)
         {
            string fullName = string.Join(
               DefaultTypeSeparator,
               nodeType.TypeFields.Select(x => $"{(DisplayPropertyName ? x.Name?.ToUpperFirst() : string.Empty)}{WriteType(x.TypeId)}"));

            display += $"{DefaultTypeSeparator}{fullName}";
         }
         return display;
      }

      public string WriteNodeSequence(NodeTypeSequence nodeType)
      {
         return $"Vec{WriteType(nodeType.TypeId)}";
      }

      public string WriteNodeTuple(NodeTypeTuple nodeType)
      {
         return $"Tuple{string.Join(DefaultTypeSeparator, nodeType.TypeIds.Select(WriteType))}";
      }

      public string WriteNodeArray(NodeTypeArray nodeType)
      {
         return $"{nodeType.Length}{WriteType(nodeType.TypeId)}";
      }

      public NodeType GetPalletType(uint typeId)
      {
         NodeType nodeType = default;
         _nodeTypes.TryGetValue(typeId, out nodeType);

         if (nodeType == null)
         {
            throw new KeyNotFoundException($"{nameof(nodeType)} is not found in current metadata type");
         }

         return nodeType;
      }

      public string WriteClassName(NodeTypeComposite nodeType)
      {
         string display = nodeType.Path[^1];
         if (nodeType.TypeParams != null && nodeType.TypeParams.Length > 0)
         {
            var propType = nodeType.TypeFields.Select(x => WriteType(x.TypeId)).ToList();

            string suffix = propType.Count switch
            {
               1 => $"{propType[0]}",
               2 when propType[0] == propType[1] => $"{propType[0]}",
               > 2 => ((Func<string>)(() => {
                  if (propType.All(x => propType[0] == x)) {
                     return $"{propType[0]}s";
                  }

                  IEnumerable<IGrouping<string, string>> groupedName = propType.GroupBy(x => x);
                  if (groupedName.Count() < propType.Count)
                  {
                     return string.Join("_", groupedName.Where(x => x.Count() > 1).Select(x => x.Key));
                  }

                  // No good solution found
                  return string.Empty;
               }))()
            };

            if (!string.IsNullOrEmpty(suffix))
            {
               return $"{display}_{suffix}";
            }
         }
         return display;
      }
   }
}
