//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ajuna.NetApi.Model.FrameSupport;
using Ajuna.NetApi.Model.SpCore;
using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Primitive;
using Ajuna.ServiceLayer.Attributes;
using Ajuna.ServiceLayer.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Ajuna.Infrastructure.Storages
{
    
    
    public interface IImOnlineStorage : IStorage
    {
        
        /// <summary>
        /// >> HeartbeatAfter
        ///  The block number after which it&#39;s ok to send heartbeats in the current
        ///  session.
        /// 
        ///  At the beginning of each session we set this to a value that should fall
        ///  roughly in the middle of the session duration. The idea is to first wait for
        ///  the validators to produce a block in the current session, so that the
        ///  heartbeat later on will not be necessary.
        /// 
        ///  This value will only be used as a fallback if we fail to get a proper session
        ///  progress estimate from `NextSessionRotation`, as those estimates should be
        ///  more accurate then the value we calculate for `HeartbeatAfter`.
        /// </summary>
        Ajuna.NetApi.Model.Types.Primitive.U32 GetHeartbeatAfter();
        
        /// <summary>
        /// >> Keys
        ///  The current set of keys that may issue a heartbeat.
        /// </summary>
        Ajuna.NetApi.Model.FrameSupport.WeakBoundedVecT4 GetKeys();
        
        /// <summary>
        /// >> ReceivedHeartbeats
        ///  For each session index, we keep a mapping of &#39;SessionIndex` and `AuthIndex` to
        ///  `WrapperOpaque&lt;BoundedOpaqueNetworkState&gt;`.
        /// </summary>
        Ajuna.NetApi.Model.FrameSupport.WrapperOpaque GetReceivedHeartbeats(string key);
        
        /// <summary>
        /// >> AuthoredBlocks
        ///  For each session index, we keep a mapping of `ValidatorId&lt;T&gt;` to the
        ///  number of blocks authored by the given authority.
        /// </summary>
        Ajuna.NetApi.Model.Types.Primitive.U32 GetAuthoredBlocks(string key);
    }
    
    public sealed class ImOnlineStorage : IImOnlineStorage
    {
        
        private TypedStorage<Ajuna.NetApi.Model.Types.Primitive.U32> _heartbeatAfterTypedStorage;
        
        private TypedStorage<Ajuna.NetApi.Model.FrameSupport.WeakBoundedVecT4> _keysTypedStorage;
        
        private TypedMapStorage<Ajuna.NetApi.Model.FrameSupport.WrapperOpaque> _receivedHeartbeatsTypedStorage;
        
        private TypedMapStorage<Ajuna.NetApi.Model.Types.Primitive.U32> _authoredBlocksTypedStorage;
        
        public ImOnlineStorage(IStorageChangeDelegate storageChangeDelegate)
        {
            this.HeartbeatAfterTypedStorage = new TypedStorage<Ajuna.NetApi.Model.Types.Primitive.U32>("ImOnline.HeartbeatAfter", storageChangeDelegate);
            this.KeysTypedStorage = new TypedStorage<Ajuna.NetApi.Model.FrameSupport.WeakBoundedVecT4>("ImOnline.Keys", storageChangeDelegate);
            this.ReceivedHeartbeatsTypedStorage = new TypedMapStorage<Ajuna.NetApi.Model.FrameSupport.WrapperOpaque>("ImOnline.ReceivedHeartbeats", storageChangeDelegate);
            this.AuthoredBlocksTypedStorage = new TypedMapStorage<Ajuna.NetApi.Model.Types.Primitive.U32>("ImOnline.AuthoredBlocks", storageChangeDelegate);
        }
        
        public TypedStorage<Ajuna.NetApi.Model.Types.Primitive.U32> HeartbeatAfterTypedStorage
        {
            get
            {
                return _heartbeatAfterTypedStorage;
            }
            set
            {
                _heartbeatAfterTypedStorage = value;
            }
        }
        
        public TypedStorage<Ajuna.NetApi.Model.FrameSupport.WeakBoundedVecT4> KeysTypedStorage
        {
            get
            {
                return _keysTypedStorage;
            }
            set
            {
                _keysTypedStorage = value;
            }
        }
        
        public TypedMapStorage<Ajuna.NetApi.Model.FrameSupport.WrapperOpaque> ReceivedHeartbeatsTypedStorage
        {
            get
            {
                return _receivedHeartbeatsTypedStorage;
            }
            set
            {
                _receivedHeartbeatsTypedStorage = value;
            }
        }
        
        public TypedMapStorage<Ajuna.NetApi.Model.Types.Primitive.U32> AuthoredBlocksTypedStorage
        {
            get
            {
                return _authoredBlocksTypedStorage;
            }
            set
            {
                _authoredBlocksTypedStorage = value;
            }
        }
        
        public async Task InitializeAsync(Ajuna.NetApi.SubstrateClient client)
        {
            await HeartbeatAfterTypedStorage.InitializeAsync(client, "ImOnline", "HeartbeatAfter");
            await KeysTypedStorage.InitializeAsync(client, "ImOnline", "Keys");
            await ReceivedHeartbeatsTypedStorage.InitializeAsync(client, "ImOnline", "ReceivedHeartbeats");
            await AuthoredBlocksTypedStorage.InitializeAsync(client, "ImOnline", "AuthoredBlocks");
        }
        
        [StorageChange("ImOnline", "HeartbeatAfter")]
        public void OnUpdateHeartbeatAfter(string data)
        {
            HeartbeatAfterTypedStorage.Update(data);
        }
        
        /// <summary>
        /// >> HeartbeatAfter
        ///  The block number after which it&#39;s ok to send heartbeats in the current
        ///  session.
        /// 
        ///  At the beginning of each session we set this to a value that should fall
        ///  roughly in the middle of the session duration. The idea is to first wait for
        ///  the validators to produce a block in the current session, so that the
        ///  heartbeat later on will not be necessary.
        /// 
        ///  This value will only be used as a fallback if we fail to get a proper session
        ///  progress estimate from `NextSessionRotation`, as those estimates should be
        ///  more accurate then the value we calculate for `HeartbeatAfter`.
        /// </summary>
        public Ajuna.NetApi.Model.Types.Primitive.U32 GetHeartbeatAfter()
        {
            return HeartbeatAfterTypedStorage.Get();
        }
        
        [StorageChange("ImOnline", "Keys")]
        public void OnUpdateKeys(string data)
        {
            KeysTypedStorage.Update(data);
        }
        
        /// <summary>
        /// >> Keys
        ///  The current set of keys that may issue a heartbeat.
        /// </summary>
        public Ajuna.NetApi.Model.FrameSupport.WeakBoundedVecT4 GetKeys()
        {
            return KeysTypedStorage.Get();
        }
        
        [StorageChange("ImOnline", "ReceivedHeartbeats")]
        public void OnUpdateReceivedHeartbeats(string key, string data)
        {
            ReceivedHeartbeatsTypedStorage.Update(key, data);
        }
        
        /// <summary>
        /// >> ReceivedHeartbeats
        ///  For each session index, we keep a mapping of &#39;SessionIndex` and `AuthIndex` to
        ///  `WrapperOpaque&lt;BoundedOpaqueNetworkState&gt;`.
        /// </summary>
        public Ajuna.NetApi.Model.FrameSupport.WrapperOpaque GetReceivedHeartbeats(string key)
        {
            if ((key == null))
            {
                return null;
            }
            if (ReceivedHeartbeatsTypedStorage.Dictionary.TryGetValue(key, out Ajuna.NetApi.Model.FrameSupport.WrapperOpaque result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        
        [StorageChange("ImOnline", "AuthoredBlocks")]
        public void OnUpdateAuthoredBlocks(string key, string data)
        {
            AuthoredBlocksTypedStorage.Update(key, data);
        }
        
        /// <summary>
        /// >> AuthoredBlocks
        ///  For each session index, we keep a mapping of `ValidatorId&lt;T&gt;` to the
        ///  number of blocks authored by the given authority.
        /// </summary>
        public Ajuna.NetApi.Model.Types.Primitive.U32 GetAuthoredBlocks(string key)
        {
            if ((key == null))
            {
                return null;
            }
            if (AuthoredBlocksTypedStorage.Dictionary.TryGetValue(key, out Ajuna.NetApi.Model.Types.Primitive.U32 result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}
