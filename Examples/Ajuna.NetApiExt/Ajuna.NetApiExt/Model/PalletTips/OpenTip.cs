//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ajuna.NetApi.Model.PrimitiveTypes;
using Ajuna.NetApi.Model.SpCore;
using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;


namespace Ajuna.NetApi.Model.PalletTips
{
    
    
    /// <summary>
    /// >> 487 - Composite[pallet_tips.OpenTip]
    /// </summary>
    public sealed class OpenTip : BaseType
    {
        
        /// <summary>
        /// >> reason
        /// </summary>
        private Ajuna.NetApi.Model.PrimitiveTypes.H256 _reason;
        
        /// <summary>
        /// >> who
        /// </summary>
        private Ajuna.NetApi.Model.SpCore.AccountId32 _who;
        
        /// <summary>
        /// >> finder
        /// </summary>
        private Ajuna.NetApi.Model.SpCore.AccountId32 _finder;
        
        /// <summary>
        /// >> deposit
        /// </summary>
        private Ajuna.NetApi.Model.Types.Primitive.U128 _deposit;
        
        /// <summary>
        /// >> closes
        /// </summary>
        private BaseOpt<Ajuna.NetApi.Model.Types.Primitive.U32> _closes;
        
        /// <summary>
        /// >> tips
        /// </summary>
        private BaseVec<BaseTuple<Ajuna.NetApi.Model.SpCore.AccountId32,Ajuna.NetApi.Model.Types.Primitive.U128>> _tips;
        
        /// <summary>
        /// >> finders_fee
        /// </summary>
        private Ajuna.NetApi.Model.Types.Primitive.Bool _findersFee;
        
        public Ajuna.NetApi.Model.PrimitiveTypes.H256 Reason
        {
            get
            {
                return this._reason;
            }
            set
            {
                this._reason = value;
            }
        }
        
        public Ajuna.NetApi.Model.SpCore.AccountId32 Who
        {
            get
            {
                return this._who;
            }
            set
            {
                this._who = value;
            }
        }
        
        public Ajuna.NetApi.Model.SpCore.AccountId32 Finder
        {
            get
            {
                return this._finder;
            }
            set
            {
                this._finder = value;
            }
        }
        
        public Ajuna.NetApi.Model.Types.Primitive.U128 Deposit
        {
            get
            {
                return this._deposit;
            }
            set
            {
                this._deposit = value;
            }
        }
        
        public BaseOpt<Ajuna.NetApi.Model.Types.Primitive.U32> Closes
        {
            get
            {
                return this._closes;
            }
            set
            {
                this._closes = value;
            }
        }
        
        public BaseVec<BaseTuple<Ajuna.NetApi.Model.SpCore.AccountId32,Ajuna.NetApi.Model.Types.Primitive.U128>> Tips
        {
            get
            {
                return this._tips;
            }
            set
            {
                this._tips = value;
            }
        }
        
        public Ajuna.NetApi.Model.Types.Primitive.Bool FindersFee
        {
            get
            {
                return this._findersFee;
            }
            set
            {
                this._findersFee = value;
            }
        }
        
        public override string TypeName()
        {
            return "OpenTip";
        }
        
        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Reason.Encode());
            result.AddRange(Who.Encode());
            result.AddRange(Finder.Encode());
            result.AddRange(Deposit.Encode());
            result.AddRange(Closes.Encode());
            result.AddRange(Tips.Encode());
            result.AddRange(FindersFee.Encode());
            return result.ToArray();
        }
        
        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;
            Reason = new Ajuna.NetApi.Model.PrimitiveTypes.H256();
            Reason.Decode(byteArray, ref p);
            Who = new Ajuna.NetApi.Model.SpCore.AccountId32();
            Who.Decode(byteArray, ref p);
            Finder = new Ajuna.NetApi.Model.SpCore.AccountId32();
            Finder.Decode(byteArray, ref p);
            Deposit = new Ajuna.NetApi.Model.Types.Primitive.U128();
            Deposit.Decode(byteArray, ref p);
            Closes = new BaseOpt<Ajuna.NetApi.Model.Types.Primitive.U32>();
            Closes.Decode(byteArray, ref p);
            Tips = new BaseVec<BaseTuple<Ajuna.NetApi.Model.SpCore.AccountId32,Ajuna.NetApi.Model.Types.Primitive.U128>>();
            Tips.Decode(byteArray, ref p);
            FindersFee = new Ajuna.NetApi.Model.Types.Primitive.Bool();
            FindersFee.Decode(byteArray, ref p);
            TypeSize = p - start;
        }
    }
}
