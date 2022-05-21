using System;
using System.IO;

namespace Ajuna.DotNet.Service.Generators
{
   public class ProjectSettings
   {
      internal ProjectSettings(string projectName)
      {
         ProjectName = projectName;
         ProjectDirectory = Path.Join(Environment.CurrentDirectory, ProjectName);
      }

      internal string ProjectName { get; private set; }
      internal string ProjectDirectory { get; private set; }
   }
}
