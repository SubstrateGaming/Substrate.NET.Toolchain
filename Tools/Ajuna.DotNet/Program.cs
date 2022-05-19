using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ajuna.DotNet
{
   partial class Program
   {
      /// <summary>
      /// Command line utility to easily maintain and scaffold Ajuna SDK related projects.
      /// This tool should be installed as dotnet tool so it can be easily invoked on a developer machine
      /// to get started with Ajuna SDK.
      /// 
      /// Usage
      /// dotnet ajuna new TYPE
      /// 
      ///   TYPE
      ///      --service WEBSOCKET_CONNECTION_STRING | METADATA_FILE_PATH
      ///      --client  ??
      ///      
      /// dotnet ajuna update
      /// 
      /// </summary>
      /// <returns></returns>
      static async Task Main(string[] args)
      {
         // Initialize logging.
         Log.Logger = new LoggerConfiguration()
          .MinimumLevel.Verbose()
          .WriteTo.Console()
          .CreateLogger();

         try
         {
            if (args.Length >= 1)
            {
               switch (args[0])
               {
                  case "new":
                     await NewCommandAsync(args);
                     break;
                  case "update":
                     await UpdateCommandAsync(args);
                     break;
                  default:
                     break;
               }
            }
         }
         catch (InvalidOperationException ex)
         {
            Log.Error(ex, "Could not complete operation!");
         }
         catch (Exception ex)
         {
            Log.Error(ex, "Unhandled exception!");
         }

      }

      /// <summary>
      /// Invokable with dotnet ajuna update
      /// </summary>
      private static async Task UpdateCommandAsync(string[] args)
      {
         await Task.Delay(0);
         throw new NotImplementedException();
      }

      /// <summary>
      /// Implements any dotnet ajuna new command handling and extracts the parameters and forwards them to the actual command handlers.
      /// </summary>
      private static async Task NewCommandAsync(string[] args)
      {
         bool wantService = false;
         string serviceArgument = null;

         bool wantClient = false;
         string clientArgument = null;

         for (int i = 0; i < args.Length; i++)
         {
            switch (args[i])
            {
               case "--service":
                  {
                     if (i + 1 < args.Length)
                     {
                        wantService = true;
                        serviceArgument = args[i + 1];
                        i += 1;
                     }
                  }
                  break;

               case "--client":
                  {
                     if (i + 1 < args.Length)
                     {
                        wantClient = true;
                        clientArgument = args[i + 1];
                        i += 1;
                     }
                  }
                  break;

               default:
                  break;
            }
         }

         var gen = new Generator(Log.Logger, new GeneratorSettings()
         {
            WantService = wantService,
            ServiceArgument = serviceArgument,

            WantClient = wantClient,
            ClientArgument = clientArgument
         });

         await gen.GenerateAsync(CancellationToken.None);
      }
   }
}
