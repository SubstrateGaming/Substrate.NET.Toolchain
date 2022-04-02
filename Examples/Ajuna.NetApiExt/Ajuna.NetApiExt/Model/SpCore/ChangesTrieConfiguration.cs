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


namespace Ajuna.NetApi.Model.SpCore
{
    
    
    /// <summary>
    /// >> 17 - Composite[sp_core.changes_trie.ChangesTrieConfiguration]
    /// </summary>
    public sealed class ChangesTrieConfiguration : BaseType
    {
        
        /// <summary>
        /// >> digest_interval
        /// </summary>
        private Ajuna.NetApi.Model.Types.Primitive.U32 _digestInterval;
        
        /// <summary>
        /// >> digest_levels
        /// </summary>
        private Ajuna.NetApi.Model.Types.Primitive.U32 _digestLevels;
        
        public Ajuna.NetApi.Model.Types.Primitive.U32 DigestInterval
        {
            get
            {
                return this._digestInterval;
            }
            set
            {
                this._digestInterval = value;
            }
        }
        
        public Ajuna.NetApi.Model.Types.Primitive.U32 DigestLevels
        {
            get
            {
                return this._digestLevels;
            }
            set
            {
                this._digestLevels = value;
            }
        }
        
        public override string TypeName()
        {
            return "ChangesTrieConfiguration";
        }
        
        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(DigestInterval.Encode());
            result.AddRange(DigestLevels.Encode());
            return result.ToArray();
        }
        
        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;
            DigestInterval = new Ajuna.NetApi.Model.Types.Primitive.U32();
            DigestInterval.Decode(byteArray, ref p);
            DigestLevels = new Ajuna.NetApi.Model.Types.Primitive.U32();
            DigestLevels.Decode(byteArray, ref p);
            TypeSize = p - start;
        }
    }
}
