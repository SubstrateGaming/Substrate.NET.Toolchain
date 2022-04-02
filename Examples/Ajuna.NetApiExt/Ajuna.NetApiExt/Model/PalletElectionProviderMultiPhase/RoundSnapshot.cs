//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ajuna.NetApi.Model.SpCore;
using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;


namespace Ajuna.NetApi.Model.PalletElectionProviderMultiPhase
{
    
    
    /// <summary>
    /// >> 343 - Composite[pallet_election_provider_multi_phase.RoundSnapshot]
    /// </summary>
    public sealed class RoundSnapshot : BaseType
    {
        
        /// <summary>
        /// >> voters
        /// </summary>
        private BaseVec<BaseTuple<Ajuna.NetApi.Model.SpCore.AccountId32,Ajuna.NetApi.Model.Types.Primitive.U64,BaseVec<Ajuna.NetApi.Model.SpCore.AccountId32>>> _voters;
        
        /// <summary>
        /// >> targets
        /// </summary>
        private BaseVec<Ajuna.NetApi.Model.SpCore.AccountId32> _targets;
        
        public BaseVec<BaseTuple<Ajuna.NetApi.Model.SpCore.AccountId32,Ajuna.NetApi.Model.Types.Primitive.U64,BaseVec<Ajuna.NetApi.Model.SpCore.AccountId32>>> Voters
        {
            get
            {
                return this._voters;
            }
            set
            {
                this._voters = value;
            }
        }
        
        public BaseVec<Ajuna.NetApi.Model.SpCore.AccountId32> Targets
        {
            get
            {
                return this._targets;
            }
            set
            {
                this._targets = value;
            }
        }
        
        public override string TypeName()
        {
            return "RoundSnapshot";
        }
        
        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Voters.Encode());
            result.AddRange(Targets.Encode());
            return result.ToArray();
        }
        
        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;
            Voters = new BaseVec<BaseTuple<Ajuna.NetApi.Model.SpCore.AccountId32,Ajuna.NetApi.Model.Types.Primitive.U64,BaseVec<Ajuna.NetApi.Model.SpCore.AccountId32>>>();
            Voters.Decode(byteArray, ref p);
            Targets = new BaseVec<Ajuna.NetApi.Model.SpCore.AccountId32>();
            Targets.Decode(byteArray, ref p);
            TypeSize = p - start;
        }
    }
}
