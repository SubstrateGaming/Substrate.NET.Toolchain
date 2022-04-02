//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ajuna.NetApi.Model.PalletTransactionPayment;
using Ajuna.NetApi.Model.SpArithmetic;
using Ajuna.NetApi.Model.Types.Base;
using Ajuna.ServiceLayer.Attributes;
using Ajuna.ServiceLayer.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Ajuna.Infrastructure.Storages
{
    
    
    public interface ITransactionPaymentStorage : IStorage
    {
        
        /// <summary>
        /// >> NextFeeMultiplier
        /// </summary>
        Ajuna.NetApi.Model.SpArithmetic.FixedU128 GetNextFeeMultiplier();
        
        /// <summary>
        /// >> StorageVersion
        /// </summary>
        Ajuna.NetApi.Model.PalletTransactionPayment.EnumReleases GetStorageVersion();
    }
    
    public sealed class TransactionPaymentStorage : ITransactionPaymentStorage
    {
        
        private TypedStorage<Ajuna.NetApi.Model.SpArithmetic.FixedU128> _nextFeeMultiplierTypedStorage;
        
        private TypedStorage<Ajuna.NetApi.Model.PalletTransactionPayment.EnumReleases> _storageVersionTypedStorage;
        
        public TransactionPaymentStorage(IStorageChangeDelegate storageChangeDelegate)
        {
            this.NextFeeMultiplierTypedStorage = new TypedStorage<Ajuna.NetApi.Model.SpArithmetic.FixedU128>("TransactionPayment.NextFeeMultiplier", storageChangeDelegate);
            this.StorageVersionTypedStorage = new TypedStorage<Ajuna.NetApi.Model.PalletTransactionPayment.EnumReleases>("TransactionPayment.StorageVersion", storageChangeDelegate);
        }
        
        public TypedStorage<Ajuna.NetApi.Model.SpArithmetic.FixedU128> NextFeeMultiplierTypedStorage
        {
            get
            {
                return _nextFeeMultiplierTypedStorage;
            }
            set
            {
                _nextFeeMultiplierTypedStorage = value;
            }
        }
        
        public TypedStorage<Ajuna.NetApi.Model.PalletTransactionPayment.EnumReleases> StorageVersionTypedStorage
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
        
        public async Task InitializeAsync(Ajuna.NetApi.SubstrateClient client)
        {
            await NextFeeMultiplierTypedStorage.InitializeAsync(client, "TransactionPayment", "NextFeeMultiplier");
            await StorageVersionTypedStorage.InitializeAsync(client, "TransactionPayment", "StorageVersion");
        }
        
        [StorageChange("TransactionPayment", "NextFeeMultiplier")]
        public void OnUpdateNextFeeMultiplier(string data)
        {
            NextFeeMultiplierTypedStorage.Update(data);
        }
        
        /// <summary>
        /// >> NextFeeMultiplier
        /// </summary>
        public Ajuna.NetApi.Model.SpArithmetic.FixedU128 GetNextFeeMultiplier()
        {
            return NextFeeMultiplierTypedStorage.Get();
        }
        
        [StorageChange("TransactionPayment", "StorageVersion")]
        public void OnUpdateStorageVersion(string data)
        {
            StorageVersionTypedStorage.Update(data);
        }
        
        /// <summary>
        /// >> StorageVersion
        /// </summary>
        public Ajuna.NetApi.Model.PalletTransactionPayment.EnumReleases GetStorageVersion()
        {
            return StorageVersionTypedStorage.Get();
        }
    }
}
