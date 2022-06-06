using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Primitive;
using System;
using System.Net.Http;

namespace Ajuna.RestClient.Test
{
   public class ClientTestBase
   {
      protected HttpClient CreateHttpClient()
      {
         var httpClient = new HttpClient()
         {
            BaseAddress = new Uri(Environment.GetEnvironmentVariable("AJUNA_SERVICE_ENDPOINT") ?? "http://localhost:61752")
         };

         httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/ajuna");
         return httpClient;
      }

      protected BaseVoid GetTestValueBaseVoid()
      {
         var result = new BaseVoid();
         return result;
      }

      protected T GetTestValue<T>() where T : class, new()
      {
         return new T();
      }

      protected Bool GetTestValueBool()
      {
         var result = new Bool();
         result.Create(true);
         return result;
      }

      protected I8 GetTestValueI8()
      {
         var result = new I8();
         result.Create(sbyte.MaxValue);
         return result;
      }

      protected I16 GetTestValueI16()
      {
         var result = new I16();
         result.Create(Int16.MaxValue);
         return result;
      }

      protected I32 GetTestValueI32()
      {
         var result = new I32();
         result.Create(Int32.MaxValue);
         return result;
      }

      protected I64 GetTestValueI64()
      {
         var result = new I64();
         result.Create(Int64.MaxValue);
         return result;
      }

      protected I128 GetTestValueI128()
      {
         var result = new I128();
         result.Create(GetTestValueBytes(result.TypeSize));
         return result;
      }

      protected I256 GetTestValueI256()
      {
         var result = new I256();
         result.Create(GetTestValueBytes(result.TypeSize));
         return result;
      }

      protected byte[] GetTestValueBytes(int typeSize)
      {
         byte[] data = new byte[typeSize];
         for (int i = 0; i < data.Length; i++)
         {
            data[i] = byte.MaxValue;
         }
         return data;
      }

      protected U8 GetTestValueU8()
      {
         var result = new U8();
         result.Create(byte.MaxValue);
         return result;
      }

      protected U16 GetTestValueU16()
      {
         var result = new U16();
         result.Create(UInt16.MaxValue);
         return result;
      }

      protected U32 GetTestValueU32()
      {
         var result = new U32();
         result.Create(UInt32.MaxValue);
         return result;
      }

      protected U64 GetTestValueU64()
      {
         var result = new U64();
         result.Create(UInt64.MaxValue);
         return result;
      }

      protected U128 GetTestValueU128()
      {
         var result = new U128();
         var bytes = GetTestValueBytes(result.TypeSize);
         bytes[15] = 0x00;
         bytes[14] = 0x00;
         bytes[13] = 0x00;
         result.Create(bytes);
         return result;
      }

      protected U256 GetTestValueU256()
      {
         var result = new U256();
         result.Create(GetTestValueBytes(result.TypeSize));
         return result;
      }

      protected PrimChar GetTestValuePrimChar()
      {
         var result = new PrimChar();
         result.Create('a');
         return result;
      }

      protected Str GetTestValueStr()
      {
         var result = new Str();
         result.Create("ajuna");
         return result;
      }

      protected TEnum GetTestValueEnum<TEnum>() where TEnum : struct, Enum
      {
         return Enum.GetValues<TEnum>()[0];
      }
   }
}
