using Ajuna.ServiceLayer.Storage;
using System;
using System.IO;

namespace Ajuna.AspNetCore.Persistence
{
   /// <summary>
   /// Responsible for storing all changes in a CSV file including the Identifier, Action type, Data and Update Date
   /// </summary>
   public class StoragePersistenceChangeDelegate : IStorageChangeDelegate
   {
      private readonly string _csvFilePath;

      /// <param name="fileDirectory"> File Path where the CSV file will be located</param>
      public StoragePersistenceChangeDelegate(string fileDirectory = null)
      {
         _csvFilePath = Path.Combine(fileDirectory ?? String.Empty, $"Storage_Changes_{DateTime.Now:yyyyMMdd_HH.mm.ss}.csv");
      }
      
      public void OnUpdate(string identifier, string key, string data)
      {
         var change = new ChangeRecord(key, identifier, ChangeAction.Update, data);
         StoreChange(change);
      }

      public void OnDelete(string identifier, string key, string data)
      {
         var change = new ChangeRecord(key, identifier, ChangeAction.Delete, data);
         StoreChange(change);
      }

      public void OnCreate(string identifier, string key, string data)
      {
         var change = new ChangeRecord(key, identifier, ChangeAction.Create, data);
         StoreChange(change);
      }

      private void StoreChange(ChangeRecord changeRecord)
      {
         if (!File.Exists(_csvFilePath))
         {
            // Create a file to write to.
            using StreamWriter sw = File.CreateText(_csvFilePath);
            sw.WriteLine("Key,Identifier,Action,Data,UpdateDate");
         }

         // This text is always added, making the file longer over time
         // if it is not deleted.
         using (StreamWriter sw = File.AppendText(_csvFilePath))
         {
            sw.WriteLine(String.Join(",",changeRecord.Key, changeRecord.Identifier, changeRecord.Action, changeRecord.Data, changeRecord.UpdateDate));
         }
      }
   }
}