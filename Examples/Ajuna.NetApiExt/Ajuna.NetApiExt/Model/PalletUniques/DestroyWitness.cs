//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;


namespace Ajuna.NetApi.Model.PalletUniques
{
    
    
    /// <summary>
    /// >> 305 - Composite[pallet_uniques.types.DestroyWitness]
    /// </summary>
    public sealed class DestroyWitness : BaseType
    {
        
        /// <summary>
        /// >> instances
        /// </summary>
        private BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32> _instances;
        
        /// <summary>
        /// >> instance_metadatas
        /// </summary>
        private BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32> _instanceMetadatas;
        
        /// <summary>
        /// >> attributes
        /// </summary>
        private BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32> _attributes;
        
        public BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32> Instances
        {
            get
            {
                return this._instances;
            }
            set
            {
                this._instances = value;
            }
        }
        
        public BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32> InstanceMetadatas
        {
            get
            {
                return this._instanceMetadatas;
            }
            set
            {
                this._instanceMetadatas = value;
            }
        }
        
        public BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32> Attributes
        {
            get
            {
                return this._attributes;
            }
            set
            {
                this._attributes = value;
            }
        }
        
        public override string TypeName()
        {
            return "DestroyWitness";
        }
        
        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Instances.Encode());
            result.AddRange(InstanceMetadatas.Encode());
            result.AddRange(Attributes.Encode());
            return result.ToArray();
        }
        
        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;
            Instances = new BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32>();
            Instances.Decode(byteArray, ref p);
            InstanceMetadatas = new BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32>();
            InstanceMetadatas.Decode(byteArray, ref p);
            Attributes = new BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32>();
            Attributes.Decode(byteArray, ref p);
            TypeSize = p - start;
        }
    }
}
