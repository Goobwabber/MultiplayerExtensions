using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CollectDependencies
{
    static class Program
    {
        static void Main(string[] args)
        {
            var depsFile = File.ReadAllText(args[0]);
            var directoryName = Path.GetDirectoryName(args[0]);
            string depsFullPath = Path.Combine(Environment.CurrentDirectory, args[0]);
            var resolveIn = new List<string>();
            var files = new List<(string file, int line, bool optional)>();
            { // Create files from stuff in depsfile
                var stack = new Stack<string>();

                void Push(string val)
                {
                    string pre = "";
                    if (stack.Count > 0)
                        pre = stack.Peek();
                    stack.Push(pre + val);
                }
                string Pop() => stack.Pop();
                string Replace(string val)
                {
                    var v2 = Pop();
                    Push(val);
                    return v2;
                }
                string Peek() => stack.Peek();

                var optBlock = false;
                var lineNo = 0;
                foreach (var line in depsFile.Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = line.Split('"');
                    var path = parts.Last();
                    var level = parts.Length - 1;
                    var addPathToResolve = false;

                    if (path.StartsWith("::"))
                    { // pseudo-command
                        parts = path.Split(' ');
                        var command = parts[0].Substring(2);
                        parts = parts.Skip(1).ToArray();
                        var arglist = string.Join(" ", parts);
                        if (command == "from")
                        { // an "import" type command
                            try
                            {
                                path = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), arglist));
                            }
                            catch (Exception e)
                            {
                                var errorStrength = optBlock ? "warning" : "error";
                                Console.WriteLine($"{depsFullPath}({lineNo}): {errorStrength}: Error resolving import {path}: {e}");
                                path = "\" Invalid Path: ";
                            }
                        }
                        else if (command == "prompt")
                        {
                            Console.Write(arglist);
                            path = Console.ReadLine();
                        }
                        else if (command == "startopt")
                        {
                            optBlock = true;
                            goto continueTarget;
                        }
                        else if (command == "endopt")
                        {
                            optBlock = false;
                            goto continueTarget;
                        }
                        else if (command == "resolveInHere")
                        {
                            path = arglist;
                            addPathToResolve = true;
                        }
                        else
                        {
                            path = "";
                            Console.WriteLine($"{depsFullPath}({lineNo}): ERROR: Invalid command {command}");
                        }
                    }

                    if (level > stack.Count - 1)
                        Push(path);
                    else if (level == stack.Count - 1)
                        files.Add((Replace(path), lineNo, optBlock));
                    else if (level < stack.Count - 1)
                    {
                        files.Add((Pop(), lineNo, optBlock));
                        while (level < stack.Count)
                            Pop();
                        Push(path);
                    }
                    if (addPathToResolve)
                        resolveIn.Add(Peek());

                    continueTarget:
                    lineNo++;
                }

                files.Add((Pop(), lineNo, optBlock));
            }

            foreach (var file in files)
            {
                var errorStrength = file.optional ? "WARNING" : "ERROR";
                string fname = null;
                try
                {
                    if (file.file[0] == '"') continue;

                    var fparts = file.file.Split('?');
                    fname = fparts[0];

                    if (fname == "") continue;
                    string fullPath = Path.Combine(directoryName, fname);
                    if (!File.Exists(fullPath))
                    {
                        Console.WriteLine($"{depsFullPath}({file.line}): {errorStrength}: Cannot find file at {fullPath}");
                        continue;
                    }
                    var outp = Path.Combine(directoryName ?? throw new InvalidOperationException(),
                        Path.GetFileName(fname) ?? throw new InvalidOperationException());

                    var aliasp = fparts.FirstOrDefault(s => s.StartsWith("alias="))?.Substring("alias=".Length);
                    if (aliasp != null)
                        outp = Path.Combine(directoryName, aliasp);

                    if (fparts.Contains("optional"))
                        errorStrength = "WARNING";

                    bool emptyDll = !fparts.Contains("noempty");

                    Console.WriteLine($"Copying \"{fname}\" to \"{outp}\"");
                    if (File.Exists(outp)) File.Delete(outp);

                    if (Path.GetExtension(fname)?.ToLower() == ".dll")
                    {
                        try
                        {
                            if (fparts.Contains("native"))
                                goto copy;
                            else
                            {
                                if (emptyDll)
                                {
                                    var resolver = new DefaultAssemblyResolver();
                                    resolver.AddSearchDirectory(Path.GetDirectoryName(fname));
                                    foreach (var path in resolveIn)
                                        resolver.AddSearchDirectory(path);
                                    var parameters = new ReaderParameters
                                    {
                                        AssemblyResolver = resolver,
                                        ReadWrite = false,
                                        ReadingMode = ReadingMode.Immediate,
                                        InMemory = true
                                    };

                                    using var modl = ModuleDefinition.ReadModule(fparts[0], parameters);
                                    var virtualize = fparts.Contains("virt");
                                    
                                    foreach (var t in modl.Types)
                                    {
                                        static void Clear(TypeDefinition type)
                                        {
                                            foreach (var m in type.Methods)
                                            {
                                                if (m.Body != null)
                                                {
                                                    m.Body.Instructions.Clear();
                                                    m.Body.InitLocals = false;
                                                    m.Body.Variables.Clear();
                                                }
                                            }
                                            foreach (var ty in type.NestedTypes)
                                            {
                                                Clear(ty);
                                            }
                                        }
                                        if (virtualize)
                                            VirtualizedModule.VirtualizeType(t);
                                        Clear(t);
                                    }

                                    modl.Write(outp);

                                    continue;
                                }
                                else if (fparts.Contains("virt"))
                                {
                                    using var module = VirtualizedModule.Load(fname);
                                    module.Virtualize(outp);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{depsFullPath}({file.line}): WARNING: {e}");
                        }
                    }

                    copy:
                    File.Copy(fname, outp);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine($"{depsFullPath}({file.line}): {errorStrength}: \"{file.file}\" {e}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{depsFullPath}({file.line}): {errorStrength}: {e}");
                }
            }

        }
    }
}
