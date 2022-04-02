//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ajuna.NetApi.Model.Extrinsics;
using Ajuna.NetApi.Model.Meta;
using Ajuna.NetApi.Model.PalletGilt;
using Ajuna.NetApi.Model.SpArithmetic;
using Ajuna.NetApi.Model.SpCore;
using Ajuna.NetApi.Model.Types;
using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Ajuna.NetApi.Model.PalletGilt
{
    
    
    public sealed class GiltStorage
    {
        
        // Substrate client for the storage calls.
        private SubstrateClientExt _client;
        
        public GiltStorage(SubstrateClientExt client)
        {
            this._client = client;
            _client.StorageKeyDict.Add(new System.Tuple<string, string>("Gilt", "QueueTotals"), new System.Tuple<Ajuna.NetApi.Model.Meta.Storage.Hasher[], System.Type, System.Type>(null, null, typeof(BaseVec<BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32,Ajuna.NetApi.Model.Types.Primitive.U128>>)));
            _client.StorageKeyDict.Add(new System.Tuple<string, string>("Gilt", "Queues"), new System.Tuple<Ajuna.NetApi.Model.Meta.Storage.Hasher[], System.Type, System.Type>(new Ajuna.NetApi.Model.Meta.Storage.Hasher[] {
                            Ajuna.NetApi.Model.Meta.Storage.Hasher.BlakeTwo128Concat}, typeof(Ajuna.NetApi.Model.Types.Primitive.U32), typeof(BaseVec<Ajuna.NetApi.Model.PalletGilt.GiltBid>)));
            _client.StorageKeyDict.Add(new System.Tuple<string, string>("Gilt", "ActiveTotal"), new System.Tuple<Ajuna.NetApi.Model.Meta.Storage.Hasher[], System.Type, System.Type>(null, null, typeof(Ajuna.NetApi.Model.PalletGilt.ActiveGiltsTotal)));
            _client.StorageKeyDict.Add(new System.Tuple<string, string>("Gilt", "Active"), new System.Tuple<Ajuna.NetApi.Model.Meta.Storage.Hasher[], System.Type, System.Type>(new Ajuna.NetApi.Model.Meta.Storage.Hasher[] {
                            Ajuna.NetApi.Model.Meta.Storage.Hasher.BlakeTwo128Concat}, typeof(Ajuna.NetApi.Model.Types.Primitive.U32), typeof(Ajuna.NetApi.Model.PalletGilt.ActiveGilt)));
        }
        
        /// <summary>
        /// >> QueueTotalsParams
        ///  The totals of items and balances within each queue. Saves a lot of storage reads in the
        ///  case of sparsely packed queues.
        /// 
        ///  The vector is indexed by duration in `Period`s, offset by one, so information on the queue
        ///  whose duration is one `Period` would be storage `0`.
        /// </summary>
        public static string QueueTotalsParams()
        {
            return RequestGenerator.GetStorage("Gilt", "QueueTotals", Ajuna.NetApi.Model.Meta.Storage.Type.Plain);
        }
        
        /// <summary>
        /// >> QueueTotals
        ///  The totals of items and balances within each queue. Saves a lot of storage reads in the
        ///  case of sparsely packed queues.
        /// 
        ///  The vector is indexed by duration in `Period`s, offset by one, so information on the queue
        ///  whose duration is one `Period` would be storage `0`.
        /// </summary>
        public async Task<BaseVec<BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32,Ajuna.NetApi.Model.Types.Primitive.U128>>> QueueTotals(CancellationToken token)
        {
            string parameters = GiltStorage.QueueTotalsParams();
            return await _client.GetStorageAsync<BaseVec<BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32,Ajuna.NetApi.Model.Types.Primitive.U128>>>(parameters, token);
        }
        
        /// <summary>
        /// >> QueuesParams
        ///  The queues of bids ready to become gilts. Indexed by duration (in `Period`s).
        /// </summary>
        public static string QueuesParams(Ajuna.NetApi.Model.Types.Primitive.U32 key)
        {
            return RequestGenerator.GetStorage("Gilt", "Queues", Ajuna.NetApi.Model.Meta.Storage.Type.Map, new Ajuna.NetApi.Model.Meta.Storage.Hasher[] {
                        Ajuna.NetApi.Model.Meta.Storage.Hasher.BlakeTwo128Concat}, new Ajuna.NetApi.Model.Types.IType[] {
                        key});
        }
        
        /// <summary>
        /// >> Queues
        ///  The queues of bids ready to become gilts. Indexed by duration (in `Period`s).
        /// </summary>
        public async Task<BaseVec<Ajuna.NetApi.Model.PalletGilt.GiltBid>> Queues(Ajuna.NetApi.Model.Types.Primitive.U32 key, CancellationToken token)
        {
            string parameters = GiltStorage.QueuesParams(key);
            return await _client.GetStorageAsync<BaseVec<Ajuna.NetApi.Model.PalletGilt.GiltBid>>(parameters, token);
        }
        
        /// <summary>
        /// >> ActiveTotalParams
        ///  Information relating to the gilts currently active.
        /// </summary>
        public static string ActiveTotalParams()
        {
            return RequestGenerator.GetStorage("Gilt", "ActiveTotal", Ajuna.NetApi.Model.Meta.Storage.Type.Plain);
        }
        
        /// <summary>
        /// >> ActiveTotal
        ///  Information relating to the gilts currently active.
        /// </summary>
        public async Task<Ajuna.NetApi.Model.PalletGilt.ActiveGiltsTotal> ActiveTotal(CancellationToken token)
        {
            string parameters = GiltStorage.ActiveTotalParams();
            return await _client.GetStorageAsync<Ajuna.NetApi.Model.PalletGilt.ActiveGiltsTotal>(parameters, token);
        }
        
        /// <summary>
        /// >> ActiveParams
        ///  The currently active gilts, indexed according to the order of creation.
        /// </summary>
        public static string ActiveParams(Ajuna.NetApi.Model.Types.Primitive.U32 key)
        {
            return RequestGenerator.GetStorage("Gilt", "Active", Ajuna.NetApi.Model.Meta.Storage.Type.Map, new Ajuna.NetApi.Model.Meta.Storage.Hasher[] {
                        Ajuna.NetApi.Model.Meta.Storage.Hasher.BlakeTwo128Concat}, new Ajuna.NetApi.Model.Types.IType[] {
                        key});
        }
        
        /// <summary>
        /// >> Active
        ///  The currently active gilts, indexed according to the order of creation.
        /// </summary>
        public async Task<Ajuna.NetApi.Model.PalletGilt.ActiveGilt> Active(Ajuna.NetApi.Model.Types.Primitive.U32 key, CancellationToken token)
        {
            string parameters = GiltStorage.ActiveParams(key);
            return await _client.GetStorageAsync<Ajuna.NetApi.Model.PalletGilt.ActiveGilt>(parameters, token);
        }
    }
    
    public sealed class GiltCalls
    {
        
        /// <summary>
        /// >> place_bid
        /// Contains one variant per dispatchable that can be called by an extrinsic.
        /// </summary>
        public static Method PlaceBid(BaseCom<Ajuna.NetApi.Model.Types.Primitive.U128> amount, Ajuna.NetApi.Model.Types.Primitive.U32 duration)
        {
            System.Collections.Generic.List<byte> byteArray = new List<byte>();
            byteArray.AddRange(amount.Encode());
            byteArray.AddRange(duration.Encode());
            return new Method(37, "Gilt", 0, "place_bid", byteArray.ToArray());
        }
        
        /// <summary>
        /// >> retract_bid
        /// Contains one variant per dispatchable that can be called by an extrinsic.
        /// </summary>
        public static Method RetractBid(BaseCom<Ajuna.NetApi.Model.Types.Primitive.U128> amount, Ajuna.NetApi.Model.Types.Primitive.U32 duration)
        {
            System.Collections.Generic.List<byte> byteArray = new List<byte>();
            byteArray.AddRange(amount.Encode());
            byteArray.AddRange(duration.Encode());
            return new Method(37, "Gilt", 1, "retract_bid", byteArray.ToArray());
        }
        
        /// <summary>
        /// >> set_target
        /// Contains one variant per dispatchable that can be called by an extrinsic.
        /// </summary>
        public static Method SetTarget(BaseCom<Ajuna.NetApi.Model.SpArithmetic.Perquintill> target)
        {
            System.Collections.Generic.List<byte> byteArray = new List<byte>();
            byteArray.AddRange(target.Encode());
            return new Method(37, "Gilt", 2, "set_target", byteArray.ToArray());
        }
        
        /// <summary>
        /// >> thaw
        /// Contains one variant per dispatchable that can be called by an extrinsic.
        /// </summary>
        public static Method Thaw(BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32> index)
        {
            System.Collections.Generic.List<byte> byteArray = new List<byte>();
            byteArray.AddRange(index.Encode());
            return new Method(37, "Gilt", 3, "thaw", byteArray.ToArray());
        }
    }
    
    /// <summary>
    /// >> BidPlaced
    /// A bid was successfully placed.
    /// \[ who, amount, duration \]
    /// </summary>
    public sealed class EventBidPlaced : BaseTuple<Ajuna.NetApi.Model.SpCore.AccountId32, Ajuna.NetApi.Model.Types.Primitive.U128, Ajuna.NetApi.Model.Types.Primitive.U32>
    {
    }
    
    /// <summary>
    /// >> BidRetracted
    /// A bid was successfully removed (before being accepted as a gilt).
    /// \[ who, amount, duration \]
    /// </summary>
    public sealed class EventBidRetracted : BaseTuple<Ajuna.NetApi.Model.SpCore.AccountId32, Ajuna.NetApi.Model.Types.Primitive.U128, Ajuna.NetApi.Model.Types.Primitive.U32>
    {
    }
    
    /// <summary>
    /// >> GiltIssued
    /// A bid was accepted as a gilt. The balance may not be released until expiry.
    /// \[ index, expiry, who, amount \]
    /// </summary>
    public sealed class EventGiltIssued : BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32, Ajuna.NetApi.Model.Types.Primitive.U32, Ajuna.NetApi.Model.SpCore.AccountId32, Ajuna.NetApi.Model.Types.Primitive.U128>
    {
    }
    
    /// <summary>
    /// >> GiltThawed
    /// An expired gilt has been thawed.
    /// \[ index, who, original_amount, additional_amount \]
    /// </summary>
    public sealed class EventGiltThawed : BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32, Ajuna.NetApi.Model.SpCore.AccountId32, Ajuna.NetApi.Model.Types.Primitive.U128, Ajuna.NetApi.Model.Types.Primitive.U128>
    {
    }
    
    public enum GiltErrors
    {
        
        /// <summary>
        /// >> DurationTooSmall
        /// The duration of the bid is less than one.
        /// </summary>
        DurationTooSmall,
        
        /// <summary>
        /// >> DurationTooBig
        /// The duration is the bid is greater than the number of queues.
        /// </summary>
        DurationTooBig,
        
        /// <summary>
        /// >> AmountTooSmall
        /// The amount of the bid is less than the minimum allowed.
        /// </summary>
        AmountTooSmall,
        
        /// <summary>
        /// >> BidTooLow
        /// The queue for the bid's duration is full and the amount bid is too low to get in
        /// through replacing an existing bid.
        /// </summary>
        BidTooLow,
        
        /// <summary>
        /// >> Unknown
        /// Gilt index is unknown.
        /// </summary>
        Unknown,
        
        /// <summary>
        /// >> NotOwner
        /// Not the owner of the gilt.
        /// </summary>
        NotOwner,
        
        /// <summary>
        /// >> NotExpired
        /// Gilt not yet at expiry date.
        /// </summary>
        NotExpired,
        
        /// <summary>
        /// >> NotFound
        /// The given bid for retraction is not found.
        /// </summary>
        NotFound,
    }
}
