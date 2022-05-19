namespace Ajuna.ServiceLayer.Storage
{
   public interface IStorageChangeDelegate
   {
      void OnUpdate(string identifier, string key, string data);
      void OnDelete(string identifier, string key, string data);
      void OnCreate(string identifier, string key, string data);
   }
}
