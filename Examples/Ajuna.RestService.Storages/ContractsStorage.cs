//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ajuna.NetApi.Model.PalletContracts;
using Ajuna.NetApi.Model.PrimitiveTypes;
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
    
    
    public interface IContractsStorage : IStorage
    {
        
        /// <summary>
        /// >> PristineCode
        ///  A mapping from an original code hash to the original code, untouched by instrumentation.
        /// </summary>
        BaseVec<Ajuna.NetApi.Model.Types.Primitive.U8> GetPristineCode(string key);
        
        /// <summary>
        /// >> CodeStorage
        ///  A mapping between an original code hash and instrumented wasm code, ready for execution.
        /// </summary>
        Ajuna.NetApi.Model.PalletContracts.PrefabWasmModule GetCodeStorage(string key);
        
        /// <summary>
        /// >> AccountCounter
        ///  The subtrie counter.
        /// </summary>
        Ajuna.NetApi.Model.Types.Primitive.U64 GetAccountCounter();
        
        /// <summary>
        /// >> ContractInfoOf
        ///  The code associated with a given account.
        /// 
        ///  TWOX-NOTE: SAFE since `AccountId` is a secure hash.
        /// </summary>
        Ajuna.NetApi.Model.PalletContracts.RawContractInfo GetContractInfoOf(string key);
        
        /// <summary>
        /// >> DeletionQueue
        ///  Evicted contracts that await child trie deletion.
        /// 
        ///  Child trie deletion is a heavy operation depending on the amount of storage items
        ///  stored in said trie. Therefore this operation is performed lazily in `on_initialize`.
        /// </summary>
        BaseVec<Ajuna.NetApi.Model.PalletContracts.DeletedContract> GetDeletionQueue();
    }
    
    public sealed class ContractsStorage : IContractsStorage
    {
        
        private TypedMapStorage<BaseVec<Ajuna.NetApi.Model.Types.Primitive.U8>> _pristineCodeTypedStorage;
        
        private TypedMapStorage<Ajuna.NetApi.Model.PalletContracts.PrefabWasmModule> _codeStorageTypedStorage;
        
        private TypedStorage<Ajuna.NetApi.Model.Types.Primitive.U64> _accountCounterTypedStorage;
        
        private TypedMapStorage<Ajuna.NetApi.Model.PalletContracts.RawContractInfo> _contractInfoOfTypedStorage;
        
        private TypedStorage<BaseVec<Ajuna.NetApi.Model.PalletContracts.DeletedContract>> _deletionQueueTypedStorage;
        
        public ContractsStorage(IStorageChangeDelegate storageChangeDelegate)
        {
            this.PristineCodeTypedStorage = new TypedMapStorage<BaseVec<Ajuna.NetApi.Model.Types.Primitive.U8>>("Contracts.PristineCode", storageChangeDelegate);
            this.CodeStorageTypedStorage = new TypedMapStorage<Ajuna.NetApi.Model.PalletContracts.PrefabWasmModule>("Contracts.CodeStorage", storageChangeDelegate);
            this.AccountCounterTypedStorage = new TypedStorage<Ajuna.NetApi.Model.Types.Primitive.U64>("Contracts.AccountCounter", storageChangeDelegate);
            this.ContractInfoOfTypedStorage = new TypedMapStorage<Ajuna.NetApi.Model.PalletContracts.RawContractInfo>("Contracts.ContractInfoOf", storageChangeDelegate);
            this.DeletionQueueTypedStorage = new TypedStorage<BaseVec<Ajuna.NetApi.Model.PalletContracts.DeletedContract>>("Contracts.DeletionQueue", storageChangeDelegate);
        }
        
        public TypedMapStorage<BaseVec<Ajuna.NetApi.Model.Types.Primitive.U8>> PristineCodeTypedStorage
        {
            get
            {
                return _pristineCodeTypedStorage;
            }
            set
            {
                _pristineCodeTypedStorage = value;
            }
        }
        
        public TypedMapStorage<Ajuna.NetApi.Model.PalletContracts.PrefabWasmModule> CodeStorageTypedStorage
        {
            get
            {
                return _codeStorageTypedStorage;
            }
            set
            {
                _codeStorageTypedStorage = value;
            }
        }
        
        public TypedStorage<Ajuna.NetApi.Model.Types.Primitive.U64> AccountCounterTypedStorage
        {
            get
            {
                return _accountCounterTypedStorage;
            }
            set
            {
                _accountCounterTypedStorage = value;
            }
        }
        
        public TypedMapStorage<Ajuna.NetApi.Model.PalletContracts.RawContractInfo> ContractInfoOfTypedStorage
        {
            get
            {
                return _contractInfoOfTypedStorage;
            }
            set
            {
                _contractInfoOfTypedStorage = value;
            }
        }
        
        public TypedStorage<BaseVec<Ajuna.NetApi.Model.PalletContracts.DeletedContract>> DeletionQueueTypedStorage
        {
            get
            {
                return _deletionQueueTypedStorage;
            }
            set
            {
                _deletionQueueTypedStorage = value;
            }
        }
        
        public async Task InitializeAsync(Ajuna.NetApi.SubstrateClient client)
        {
            await PristineCodeTypedStorage.InitializeAsync(client, "Contracts", "PristineCode");
            await CodeStorageTypedStorage.InitializeAsync(client, "Contracts", "CodeStorage");
            await AccountCounterTypedStorage.InitializeAsync(client, "Contracts", "AccountCounter");
            await ContractInfoOfTypedStorage.InitializeAsync(client, "Contracts", "ContractInfoOf");
            await DeletionQueueTypedStorage.InitializeAsync(client, "Contracts", "DeletionQueue");
        }
        
        [StorageChange("Contracts", "PristineCode")]
        public void OnUpdatePristineCode(string key, string data)
        {
            PristineCodeTypedStorage.Update(key, data);
        }
        
        /// <summary>
        /// >> PristineCode
        ///  A mapping from an original code hash to the original code, untouched by instrumentation.
        /// </summary>
        public BaseVec<Ajuna.NetApi.Model.Types.Primitive.U8> GetPristineCode(string key)
        {
            if ((key == null))
            {
                return null;
            }
            if (PristineCodeTypedStorage.Dictionary.TryGetValue(key, out BaseVec<Ajuna.NetApi.Model.Types.Primitive.U8> result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        
        [StorageChange("Contracts", "CodeStorage")]
        public void OnUpdateCodeStorage(string key, string data)
        {
            CodeStorageTypedStorage.Update(key, data);
        }
        
        /// <summary>
        /// >> CodeStorage
        ///  A mapping between an original code hash and instrumented wasm code, ready for execution.
        /// </summary>
        public Ajuna.NetApi.Model.PalletContracts.PrefabWasmModule GetCodeStorage(string key)
        {
            if ((key == null))
            {
                return null;
            }
            if (CodeStorageTypedStorage.Dictionary.TryGetValue(key, out Ajuna.NetApi.Model.PalletContracts.PrefabWasmModule result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        
        [StorageChange("Contracts", "AccountCounter")]
        public void OnUpdateAccountCounter(string data)
        {
            AccountCounterTypedStorage.Update(data);
        }
        
        /// <summary>
        /// >> AccountCounter
        ///  The subtrie counter.
        /// </summary>
        public Ajuna.NetApi.Model.Types.Primitive.U64 GetAccountCounter()
        {
            return AccountCounterTypedStorage.Get();
        }
        
        [StorageChange("Contracts", "ContractInfoOf")]
        public void OnUpdateContractInfoOf(string key, string data)
        {
            ContractInfoOfTypedStorage.Update(key, data);
        }
        
        /// <summary>
        /// >> ContractInfoOf
        ///  The code associated with a given account.
        /// 
        ///  TWOX-NOTE: SAFE since `AccountId` is a secure hash.
        /// </summary>
        public Ajuna.NetApi.Model.PalletContracts.RawContractInfo GetContractInfoOf(string key)
        {
            if ((key == null))
            {
                return null;
            }
            if (ContractInfoOfTypedStorage.Dictionary.TryGetValue(key, out Ajuna.NetApi.Model.PalletContracts.RawContractInfo result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        
        [StorageChange("Contracts", "DeletionQueue")]
        public void OnUpdateDeletionQueue(string data)
        {
            DeletionQueueTypedStorage.Update(data);
        }
        
        /// <summary>
        /// >> DeletionQueue
        ///  Evicted contracts that await child trie deletion.
        /// 
        ///  Child trie deletion is a heavy operation depending on the amount of storage items
        ///  stored in said trie. Therefore this operation is performed lazily in `on_initialize`.
        /// </summary>
        public BaseVec<Ajuna.NetApi.Model.PalletContracts.DeletedContract> GetDeletionQueue()
        {
            return DeletionQueueTypedStorage.Get();
        }
    }
}
