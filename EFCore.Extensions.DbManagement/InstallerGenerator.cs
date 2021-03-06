﻿using System.IO;
using System.Text;
using EFCore.Extensions.Scripting;

namespace EFCore.Extensions.DbManagement
{
    public class InstallerGenerator
    {
        public void Run(string rootPath, EFCore.Extensions.IDbContext context, IScriptGenerator scriptProvider)
        {
            var modelPath = Path.Combine(rootPath, "Models");
            var initializePath = Path.Combine(rootPath, "1_Initialize");
            var scriptPath = Path.Combine(rootPath, "2_Migrations");
            var createPath = Path.Combine(rootPath, "3_Create");
            var customScriptPath = Path.Combine(rootPath, "4_Programmability");
            var finializeScriptPath = Path.Combine(rootPath, "5_Finalize");
            if (!Directory.Exists(modelPath)) Directory.CreateDirectory(modelPath);
            if (!Directory.Exists(initializePath)) Directory.CreateDirectory(initializePath); 
            if (!Directory.Exists(scriptPath)) Directory.CreateDirectory(scriptPath);
            if (!Directory.Exists(createPath)) Directory.CreateDirectory(createPath);
            if (!Directory.Exists(customScriptPath)) Directory.CreateDirectory(customScriptPath);
            if (!Directory.Exists(finializeScriptPath)) Directory.CreateDirectory(finializeScriptPath);

            EnsureReadme(initializePath, true);
            EnsureReadme(scriptPath);
            EnsureReadme(createPath);
            EnsureReadme(customScriptPath, true);
            EnsureReadme(finializeScriptPath, true);

            //Create SQL generation object
            var sqlCreate = scriptProvider.GenerateCreateScript();
            File.WriteAllText(Path.Combine(createPath, "Create.sql"), sqlCreate);

            //Load version file
            var versionFile = Path.Combine(modelPath, "version.json");
            var oldVersion = new Versioning();
            if (File.Exists(versionFile))
            {
                oldVersion = ScriptingExtensions.FromJson<Versioning>(File.ReadAllText(versionFile));
            }
            var newVersion = new Versioning(oldVersion.ToString());
            newVersion.Increment();
            File.WriteAllText(versionFile, newVersion.ToJson());

            //Load last model (if one)
            DataModel oldModel = null;
            var oldVersionFile = Path.Combine(modelPath, oldVersion.GetDiffFileName()) + ".model";
            if (File.Exists(oldVersionFile))
            {
                oldModel = ScriptingExtensions.FromJson<DataModel>(File.ReadAllText(oldVersionFile));
            }

            //Diff Script
            if (oldModel != null)
            {
                var sqlDiff = scriptProvider.GenerateDiffScript(oldModel, newVersion);
                File.WriteAllText(Path.Combine(scriptPath, newVersion.GetDiffFileName() + ".sql"), sqlDiff);
            }

            //Write model to installer project
            var modelJson = scriptProvider.Model.ToJson();
            File.WriteAllText(Path.Combine(modelPath, newVersion.GetDiffFileName()) + ".model", modelJson);
        }

        private void EnsureReadme(string folder, bool allowCustom = false)
        {
            if (!Directory.Exists(folder))                 return;
            var path = Path.Combine(folder, "ReadMe.txt");
            if (!File.Exists(path))
            {
                var sb = new StringBuilder();
                sb.AppendLine("NOTE: Ensure that all '*.sql' files are embedded resources.");
                if (allowCustom)
                {
                    sb.AppendLine();
                    sb.AppendLine("Add any number of '*.sql' files as embedded resources and they will be run in alphabetical order.");
                }

                File.WriteAllText(path, sb.ToString());
            }
        }
    }
}
