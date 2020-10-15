using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using System;
using System.Diagnostics;
using System.IO;

// ReSharper disable once CheckNamespace
namespace MSBuildTasks
{
    public class AssemblyRename : Task
    {
        [Required]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ITaskItem[] Assemblies { get; set; }

        public override bool Execute()
        {
            foreach (ITaskItem assembly in Assemblies)
            {
                // ItemSpec holds the filename or path of an Item
                if (assembly.ItemSpec.Length > 0)
                {
                    if (!File.Exists(assembly.ItemSpec))
                    {
                        Log.LogMessage(MessageImportance.Normal, "No file at " + assembly.ItemSpec);
                        continue;
                    }

                    if (Path.GetExtension(assembly.ItemSpec) != ".dll")
                    {
                        Log.LogMessage(MessageImportance.Normal, assembly.ItemSpec + " not a DLL");
                        continue;
                    }

                    try
                    {
                        Log.LogMessage(MessageImportance.Normal, "Reading " + assembly.ItemSpec);
                        var module = ModuleDefinition.ReadModule(assembly.ItemSpec, new ReaderParameters
                        {
                            ReadWrite = false,
                            InMemory = true,
                            ReadingMode = ReadingMode.Deferred
                        });
                        var asmName = module.Assembly.Name;
                        var name = asmName.Name;
                        var version = asmName.Version;
                        var newFile = $"{name}.{version}.dll";
                        var newFilePath = Path.Combine(Path.GetDirectoryName(assembly.ItemSpec) ?? throw new InvalidOperationException(), newFile);

                        module.Dispose();

                        Log.LogMessage(MessageImportance.Normal, $"Old file: {assembly.ItemSpec}, new file: {newFilePath}");

                        if (File.Exists(newFilePath))
                            File.Delete(newFilePath);

                        File.Move(assembly.ItemSpec, newFilePath);

                        string pdbFile;
                        if (File.Exists(pdbFile = Path.ChangeExtension(assembly.ItemSpec, "pdb")))
                        {
                            Debug.Assert(pdbFile != null, nameof(pdbFile) + " != null");
                            File.Move(pdbFile, Path.ChangeExtension(newFilePath, "pdb"));
                        }

                    }
                    catch (Exception e)
                    {
                        Log.LogErrorFromException(e);
                    }
                }
            }

            return !Log.HasLoggedErrors;
        }
    }
}
