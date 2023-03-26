using System.Threading.Tasks;

namespace Substrate.ServiceLayer.Storage
{
   public interface IStorage
   {
      Task InitializeAsync(IStorageDataProvider dataProvider);
   }
}
