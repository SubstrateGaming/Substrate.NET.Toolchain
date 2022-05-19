//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ajuna.NetApi.Model.PalletScheduler;
using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Primitive;
using Ajuna.ServiceLayer.Attributes;
using Ajuna.ServiceLayer.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Ajuna.RestService.Generated.Storage
{
    
    
    /// <summary>
    /// ISchedulerStorage interface definition.
    /// </summary>
    public interface ISchedulerStorage : IStorage
    {
        
        /// <summary>
        /// >> Agenda
        ///  Items to be executed, indexed by the block number that they should be executed on.
        /// </summary>
        BaseVec<BaseOpt<Ajuna.NetApi.Model.PalletScheduler.ScheduledV2>> GetAgenda(string key);
        
        /// <summary>
        /// >> Lookup
        ///  Lookup from identity to the block number and index of the task.
        /// </summary>
        BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32,Ajuna.NetApi.Model.Types.Primitive.U32> GetLookup(string key);
        
        /// <summary>
        /// >> StorageVersion
        ///  Storage version of the pallet.
        /// 
        ///  New networks start with last version.
        /// </summary>
        Ajuna.NetApi.Model.PalletScheduler.EnumReleases GetStorageVersion();
    }
    
    /// <summary>
    /// SchedulerStorage class definition.
    /// </summary>
    public sealed class SchedulerStorage : ISchedulerStorage
    {
        
        /// <summary>
        /// _agendaTypedStorage typed storage field
        /// </summary>
        private TypedMapStorage<BaseVec<BaseOpt<Ajuna.NetApi.Model.PalletScheduler.ScheduledV2>>> _agendaTypedStorage;
        
        /// <summary>
        /// _lookupTypedStorage typed storage field
        /// </summary>
        private TypedMapStorage<BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32,Ajuna.NetApi.Model.Types.Primitive.U32>> _lookupTypedStorage;
        
        /// <summary>
        /// _storageVersionTypedStorage typed storage field
        /// </summary>
        private TypedStorage<Ajuna.NetApi.Model.PalletScheduler.EnumReleases> _storageVersionTypedStorage;
        
        /// <summary>
        /// SchedulerStorage constructor.
        /// </summary>
        public SchedulerStorage(IStorageChangeDelegate storageChangeDelegate)
        {
            this.AgendaTypedStorage = new TypedMapStorage<BaseVec<BaseOpt<Ajuna.NetApi.Model.PalletScheduler.ScheduledV2>>>("Scheduler.Agenda", storageChangeDelegate);
            this.LookupTypedStorage = new TypedMapStorage<BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32,Ajuna.NetApi.Model.Types.Primitive.U32>>("Scheduler.Lookup", storageChangeDelegate);
            this.StorageVersionTypedStorage = new TypedStorage<Ajuna.NetApi.Model.PalletScheduler.EnumReleases>("Scheduler.StorageVersion", storageChangeDelegate);
        }
        
        /// <summary>
        /// _agendaTypedStorage property
        /// </summary>
        public TypedMapStorage<BaseVec<BaseOpt<Ajuna.NetApi.Model.PalletScheduler.ScheduledV2>>> AgendaTypedStorage
        {
            get
            {
                return _agendaTypedStorage;
            }
            set
            {
                _agendaTypedStorage = value;
            }
        }
        
        /// <summary>
        /// _lookupTypedStorage property
        /// </summary>
        public TypedMapStorage<BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32,Ajuna.NetApi.Model.Types.Primitive.U32>> LookupTypedStorage
        {
            get
            {
                return _lookupTypedStorage;
            }
            set
            {
                _lookupTypedStorage = value;
            }
        }
        
        /// <summary>
        /// _storageVersionTypedStorage property
        /// </summary>
        public TypedStorage<Ajuna.NetApi.Model.PalletScheduler.EnumReleases> StorageVersionTypedStorage
        {
            get
            {
                return _storageVersionTypedStorage;
            }
            set
            {
                _storageVersionTypedStorage = value;
            }
        }
        
        /// <summary>
        /// Connects to all storages and initializes the change subscription handling.
        /// </summary>
        public async Task InitializeAsync(Ajuna.NetApi.SubstrateClient client)
        {
            await AgendaTypedStorage.InitializeAsync(client, "Scheduler", "Agenda");
            await LookupTypedStorage.InitializeAsync(client, "Scheduler", "Lookup");
            await StorageVersionTypedStorage.InitializeAsync(client, "Scheduler", "StorageVersion");
        }
        
        /// <summary>
        /// Implements any storage change for Scheduler.Agenda
        /// </summary>
        [StorageChange("Scheduler", "Agenda")]
        public void OnUpdateAgenda(string key, string data)
        {
            AgendaTypedStorage.Update(key, data);
        }
        
        /// <summary>
        /// >> Agenda
        ///  Items to be executed, indexed by the block number that they should be executed on.
        /// </summary>
        public BaseVec<BaseOpt<Ajuna.NetApi.Model.PalletScheduler.ScheduledV2>> GetAgenda(string key)
        {
            if ((key == null))
            {
                return null;
            }
            if (AgendaTypedStorage.Dictionary.TryGetValue(key, out BaseVec<BaseOpt<Ajuna.NetApi.Model.PalletScheduler.ScheduledV2>> result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// Implements any storage change for Scheduler.Lookup
        /// </summary>
        [StorageChange("Scheduler", "Lookup")]
        public void OnUpdateLookup(string key, string data)
        {
            LookupTypedStorage.Update(key, data);
        }
        
        /// <summary>
        /// >> Lookup
        ///  Lookup from identity to the block number and index of the task.
        /// </summary>
        public BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32,Ajuna.NetApi.Model.Types.Primitive.U32> GetLookup(string key)
        {
            if ((key == null))
            {
                return null;
            }
            if (LookupTypedStorage.Dictionary.TryGetValue(key, out BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32,Ajuna.NetApi.Model.Types.Primitive.U32> result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// Implements any storage change for Scheduler.StorageVersion
        /// </summary>
        [StorageChange("Scheduler", "StorageVersion")]
        public void OnUpdateStorageVersion(string data)
        {
            StorageVersionTypedStorage.Update(data);
        }
        
        /// <summary>
        /// >> StorageVersion
        ///  Storage version of the pallet.
        /// 
        ///  New networks start with last version.
        /// </summary>
        public Ajuna.NetApi.Model.PalletScheduler.EnumReleases GetStorageVersion()
        {
            return StorageVersionTypedStorage.Get();
        }
    }
}