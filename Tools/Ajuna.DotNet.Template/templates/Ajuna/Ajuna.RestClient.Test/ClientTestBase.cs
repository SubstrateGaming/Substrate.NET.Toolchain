using Ajuna.NetApi.Model.Types.Primitive;
using System;
using System.Net.Http;
using System.Numerics;

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

      protected T GetTestValue<T>() where T : class, new()
      {
         return new T();
      }

      protected Bool GetTestValueBool()
      {
         return new Bool
         {
            Value = true
         };
      }

      protected I8 GetTestValueI8()
      {
         return new I8
         {
            Value = sbyte.MaxValue
         };
      }

      protected I16 GetTestValueI16()
      {
         return new I16
         {
            Value = Int16.MaxValue
         };
      }

      protected I32 GetTestValueI32()
      {
         return new I32
         {
            Value = Int32.MaxValue
         };
      }

      protected I64 GetTestValueI64()
      {
         return new I64
         {
            Value = Int64.MaxValue
         };
      }

      protected I128 GetTestValueI128()
      {
         return new I128
         {
            Value = BigInteger.Pow(2, 64)
         };
      }

      protected I256 GetTestValueI256()
      {
         return new I256
         {
            Value = BigInteger.Pow(2, 128)
         };
      }

      protected U8 GetTestValueU8()
      {
         return new U8
         {
            Value = byte.MaxValue
         };
      }

      protected U16 GetTestValueU16()
      {
         return new U16
         {
            Value = UInt16.MaxValue
         };
      }

      protected U32 GetTestValueU32()
      {
         return new U32
         {
            Value = UInt32.MaxValue
         };
      }

      protected U64 GetTestValueU64()
      {
         return new U64
         {
            Value = UInt64.MaxValue
         };
      }

      protected U128 GetTestValueU128()
      {
         return new U128
         {
            Value = BigInteger.Pow(2, 64)
         };
      }

      protected U256 GetTestValueU256()
      {
         return new U256
         {
            Value = BigInteger.Pow(2, 128)
         };
      }

      protected PrimChar GetTestValuePrimChar()
      {
         return new PrimChar
         {
            Value = 'a'
         };
      }

      protected Str GetTestValueStr()
      {
         var x = new Str();
         x.Create("ajuna");
         return x;
      }
   }
}
