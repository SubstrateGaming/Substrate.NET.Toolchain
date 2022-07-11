using System;

namespace Ajuna.AspNetCore.Persistence
{
   /// <summary>
   /// Representation of a Node Storage Change 
   /// </summary>
   public class ChangeRecord
   {
      public ChangeRecord()
      {
      }
      
      public  ChangeRecord(string key, string identifier, ChangeAction action, string data )
      {
         Key = key;
         Identifier = identifier;
         Action = action;
         Data = data;
         UpdateDate = DateTimeOffset.Now;
      }
      
      public string Key { get;  set; }
      public string Identifier { get;  set; }
      public ChangeAction Action { get;  set; }
      public string Data { get;  set; }
      public DateTimeOffset? UpdateDate { get;  set; }
   }

   public enum ChangeAction
   {
      Create,
      Update,
      Delete
   }
   
}

