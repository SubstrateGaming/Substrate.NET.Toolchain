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


namespace Ajuna.NetApi.Model.PalletSociety
{
    
    
    public enum BidKind
    {
        
        Deposit,
        
        Vouch,
    }
    
    /// <summary>
    /// >> 446 - Variant[pallet_society.BidKind]
    /// </summary>
    public sealed class EnumBidKind : BaseEnumExt<BidKind, Ajuna.NetApi.Model.Types.Primitive.U128, BaseTuple<Ajuna.NetApi.Model.SpCore.AccountId32, Ajuna.NetApi.Model.Types.Primitive.U128>>
    {
    }
}
