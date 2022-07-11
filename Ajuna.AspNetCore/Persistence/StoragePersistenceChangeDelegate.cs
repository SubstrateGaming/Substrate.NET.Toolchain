using Ajuna.ServiceLayer.Storage;
using CsvHelper;
using System;
using System.Globalization;
using System.IO;

namespace Ajuna.AspNetCore
{
   /// <summary>
   /// Responsible for storing all changes in a CSV file including the Identifier, Action type, Data and Update Date
   /// </summary>
   public class StoragePersistenceChangeDelegate : IStorageChangeDelegate
   {
      private readonly string _csvFileName;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="csvFilePath"> File Path of the CSV file</param>
      public StoragePersistenceChangeDelegate(string csvFileName = null)
      {
         _csvFileName = csvFileName ?? $"TempChangesStorage_{DateTimeOffset.Now}";
      }
      
      public void OnUpdate(string identifier, string key, string data)
      {
         var change = new ChangeRecord(key, identifier, ChangeAction.Update, data);
         StoreChange(change);
         System.Console.WriteLine("StoragePersistenceChangeDelegate: OnUpdate triggered");
      }

      public void OnDelete(string identifier, string key, string data)
      {
         var change = new ChangeRecord(key, identifier, ChangeAction.Delete, data);
         StoreChange(change);
         System.Console.WriteLine("StoragePersistenceChangeDelegate: OnDelete triggered");
      }

      public void OnCreate(string identifier, string key, string data)
      {
         var change = new ChangeRecord(key, identifier, ChangeAction.Create, data);
         StoreChange(change);
         System.Console.WriteLine("StoragePersistenceChangeDelegate: OnCreate triggered");
      }

      private void StoreChange(ChangeRecord changeRecord)
      {
         if (!File.Exists(_csvFileName))
         {
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(_csvFileName))
            {
               sw.WriteLine("Key,Identifier,Action,Data,UpdateDate");
            }	
         }

         // This text is always added, making the file longer over time
         // if it is not deleted.
         using (StreamWriter sw = File.AppendText(_csvFileName))
         {
            sw.WriteLine(String.Join(",",changeRecord.Key, changeRecord.Identifier, changeRecord.Action, changeRecord.Data, changeRecord.UpdateDate));
         }	
      }
   }
}