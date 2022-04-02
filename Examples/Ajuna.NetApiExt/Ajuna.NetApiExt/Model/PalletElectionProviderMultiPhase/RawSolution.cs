//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ajuna.NetApi.Model.Base;
using Ajuna.NetApi.Model.NodeRuntime;
using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;


namespace Ajuna.NetApi.Model.PalletElectionProviderMultiPhase
{
    
    
    /// <summary>
    /// >> 144 - Composite[pallet_election_provider_multi_phase.RawSolution]
    /// </summary>
    public sealed class RawSolution : BaseType
    {
        
        /// <summary>
        /// >> solution
        /// </summary>
        private Ajuna.NetApi.Model.NodeRuntime.NposSolution16 _solution;
        
        /// <summary>
        /// >> score
        /// </summary>
        private Ajuna.NetApi.Model.Base.Arr3U128 _score;
        
        /// <summary>
        /// >> round
        /// </summary>
        private Ajuna.NetApi.Model.Types.Primitive.U32 _round;
        
        public Ajuna.NetApi.Model.NodeRuntime.NposSolution16 Solution
        {
            get
            {
                return this._solution;
            }
            set
            {
                this._solution = value;
            }
        }
        
        public Ajuna.NetApi.Model.Base.Arr3U128 Score
        {
            get
            {
                return this._score;
            }
            set
            {
                this._score = value;
            }
        }
        
        public Ajuna.NetApi.Model.Types.Primitive.U32 Round
        {
            get
            {
                return this._round;
            }
            set
            {
                this._round = value;
            }
        }
        
        public override string TypeName()
        {
            return "RawSolution";
        }
        
        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Solution.Encode());
            result.AddRange(Score.Encode());
            result.AddRange(Round.Encode());
            return result.ToArray();
        }
        
        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;
            Solution = new Ajuna.NetApi.Model.NodeRuntime.NposSolution16();
            Solution.Decode(byteArray, ref p);
            Score = new Ajuna.NetApi.Model.Base.Arr3U128();
            Score.Decode(byteArray, ref p);
            Round = new Ajuna.NetApi.Model.Types.Primitive.U32();
            Round.Decode(byteArray, ref p);
            TypeSize = p - start;
        }
    }
}
