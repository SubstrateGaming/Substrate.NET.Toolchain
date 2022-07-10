using Ajuna.ServiceLayer.Storage;

namespace Ajuna.AspNetCore
{
   //TODO: To implement the actual persistence logic
   public class StoragePersistenceChangeDelegate : IStorageChangeDelegate
   {
      public void OnUpdate(string identifier, string key, string data)
      {
         System.Console.WriteLine("StoragePersistenceChangeDelegate: OnUpdate triggered");
      }

      public void OnDelete(string identifier, string key, string data)
      {
         System.Console.WriteLine("StoragePersistenceChangeDelegate: OnDelete triggered");
      }

      public void OnCreate(string identifier, string key, string data)
      {
         System.Console.WriteLine("StoragePersistenceChangeDelegate: OnCreate triggered");
      }
   }
}