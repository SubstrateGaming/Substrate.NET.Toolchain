﻿using System.Linq;

namespace Ajuna.DotNet.Extensions
{
   public static class StringExtensions
   {
      public static string ToLowerFirst(this string str)
      {
         if (str == null || str.Length == 0)
         {
            return str;
         }
         else if (str.Length == 1)
         {
            return char.ToLower(str[0]) + string.Empty;
         }
         else
         {
            return char.ToLower(str[0]) + str[1..];
         }
      }

      public static string ToUpperFirst(this string str)
      {
         if (str == null || str.Length == 0)
         {
            return str;
         }
         else if (str.Length == 1)
         {
            return char.ToUpper(str[0]) + string.Empty;
         }
         else
         {
            return char.ToUpper(str[0]) + str[1..];
         }
      }

      public static string MakeMethod(this string str)
      {
         if (str == null)
         {
            return str;
         }

         str = Filter(str);

         return string.Join(string.Empty, str.Split('_').Select(p => p.ToUpperFirst()).ToArray());
      }

      private static string Filter(string str)
      {
         return str.Replace("#", "H");
      }

      public static string MakePrivateField(this string str)
      {
         if (str == null)
         {
            return str;
         }

         str = Filter(str);

         return "_" + string.Join(string.Empty, str.Split('_').Select(p => p.ToUpperFirst()).ToArray()).ToLowerFirst();
      }

      public static string MakePublicField(this string str)
      {
         if (str == null)
         {
            return str;
         }

         str = Filter(str);

         return string.Join(string.Empty, str.Split('_').Select(p => p.ToUpperFirst()).ToArray()).ToLowerFirst();
      }
   }
}
