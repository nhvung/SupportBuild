using System.Text;
using Newtonsoft.Json;
using VSSystem.Collections.Generic.Extensions;
using VSSystem.IO;
using VSSystem.IO.SourceCode;
using VSSystem.Extensions;

class MainClass
{
    static FileInfo _sourceCodeFile = null;
    static Dictionary<string, ProjectFileExtInfo> _projectObjs = null;
    static Task Main(string[] args)
    {
        try
        {
            if (_projectObjs == null)
            {
                _projectObjs = new Dictionary<string, ProjectFileExtInfo>(StringComparer.InvariantCultureIgnoreCase);
            }

            if (_sourceCodeFile == null)
            {
                DirectoryInfo workingFolder = new DirectoryInfo(Directory.GetCurrentDirectory());
                _sourceCodeFile = new FileInfo(workingFolder.FullName + "/sources.json");
                if (_sourceCodeFile.Exists)
                {
                    try
                    {
                        string pjJson = File.ReadAllText(_sourceCodeFile.FullName, Encoding.UTF8);
                        var tPJObjs = JsonConvert.DeserializeObject<Dictionary<string, ProjectFileExtInfo>>(pjJson);
                        if (tPJObjs.Count > 0)
                        {
                            foreach (var pjObj in tPJObjs)
                            {
                                _projectObjs[pjObj.Key] = pjObj.Value;
                            }
                        }
                    }
                    catch { }

                }
            }
            Dictionary<string, string> mArgs = CommandLine.GetCommandLineArgs(new string[]
            {
                "-pf", "--projectFile",
                "-n", "--name",
                "-v", "--version",
                "-o", "--output",
                "-r", "--runtime",
                "-des", "--description"
            });
            if ((mArgs?.ContainsKey("-a") ?? false) || (mArgs?.ContainsKey("add") ?? false))
            {
                _Add(mArgs);
            }
            else if ((mArgs?.ContainsKey("-p") ?? false) || (mArgs?.ContainsKey("publish") ?? false))
            {
                _Publish(mArgs);
            }
            else
            {
                Console.WriteLine("please type input.");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace);
        }
        return Task.CompletedTask;
    }

    static void _Add(Dictionary<string, string> mArgs)
    {
        try
        {
            string projectFilePath = string.Empty;
            if ((mArgs?.ContainsKey("-pf") ?? false))
            {
                projectFilePath = mArgs["-pf"];
            }
            else if ((mArgs?.ContainsKey("--projectFile") ?? false))
            {
                projectFilePath = mArgs["--projectFile"];
            }
            if (!string.IsNullOrWhiteSpace(projectFilePath))
            {

                FileInfo pjFile = new FileInfo(projectFilePath);
                if (pjFile.Exists)
                {
                    ProjectFileExtInfo pjObj = new ProjectFileExtInfo(pjFile.FullName);
                    if (pjObj != null)
                    {
                        bool hasChange = false;
                        string outputPath = string.Empty;
                        if ((mArgs?.ContainsKey("-o") ?? false))
                        {
                            outputPath = mArgs["-o"];
                        }
                        else if ((mArgs?.ContainsKey("--output") ?? false))
                        {
                            outputPath = mArgs["--output"];
                        }
                        pjObj.OutputPath = outputPath;

                        if (!_projectObjs?.ContainsKey(pjObj.ProjectFilePath) ?? false)
                        {
                            _projectObjs[pjObj.ProjectFilePath] = pjObj;
                            hasChange = true;
                        }
                        if (hasChange)
                        {
                            _UpdateSources();
                        }

                        Console.WriteLine("Add project done.");
                    }

                }

            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace);
        }
    }

    static void _UpdateSources()
    {
        try
        {
            string pjJson = JsonConvert.SerializeObject(_projectObjs);
            File.WriteAllText(_sourceCodeFile.FullName, pjJson, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace);
        }
    }

    static void _Publish(Dictionary<string, string> mArgs)
    {
        try
        {
            string projectName = string.Empty;
            if ((mArgs?.ContainsKey("-n") ?? false))
            {
                projectName = mArgs["-n"];
            }
            else if ((mArgs?.ContainsKey("--name") ?? false))
            {
                projectName = mArgs["--name"];
            }

            var pjObj = _projectObjs?.Values?.FirstOrDefault(ite => ite.Name?.Equals(projectName, StringComparison.InvariantCultureIgnoreCase) ?? false);
            if (pjObj != null)
            {
                if (pjObj.OutputType?.Equals("exe", StringComparison.InvariantCultureIgnoreCase) ?? false)
                {
                    string version = pjObj.FileVersion;
                    if ((mArgs?.ContainsKey("-v") ?? false))
                    {
                        version = mArgs["-v"];
                    }
                    else if ((mArgs?.ContainsKey("--version") ?? false))
                    {
                        version = mArgs["--version"];
                    }

                    bool hasChange = false;
                    if (version.Equals("next", StringComparison.InvariantCultureIgnoreCase))
                    {
                        version = _GetNextVersion(pjObj.FileVersion);

                        if (!version.Equals(pjObj.FileVersion, StringComparison.InvariantCultureIgnoreCase))
                        {
                            try
                            {
                                pjObj.Version = version;
                                pjObj.FileVersion = version;
                                hasChange = true;
                            }
                            catch { }

                        }
                    }

                    string description = pjObj.Description;
                    if ((mArgs?.ContainsKey("-des") ?? false))
                    {
                        description = mArgs["-des"];
                    }
                    else if ((mArgs?.ContainsKey("--description") ?? false))
                    {
                        description = mArgs["--description"];
                    }

                    if (!description.Equals(pjObj.Description, StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            pjObj.Description = description;
                            hasChange = true;
                        }
                        catch { }

                    }
                    if (hasChange)
                    {
                        pjObj.Save();
                        _UpdateSources();
                    }

                    string targetRuntime = TargetRuntime.Windows_x64;
                    if ((mArgs?.ContainsKey("-r") ?? false))
                    {
                        targetRuntime = mArgs["-r"];
                    }
                    else if ((mArgs?.ContainsKey("--runtime") ?? false))
                    {
                        targetRuntime = mArgs["--runtime"];
                    }

                    string outputPath = pjObj.OutputPath;
                    if ((mArgs?.ContainsKey("-o") ?? false))
                    {
                        outputPath = mArgs["-o"];
                    }
                    else if ((mArgs?.ContainsKey("--output") ?? false))
                    {
                        outputPath = mArgs["--output"];
                    }

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        if (!string.IsNullOrWhiteSpace(outputPath))
                        {
                            DirectoryInfo outputFolder = new DirectoryInfo(outputPath + "/" + targetRuntime + "/" + pjObj.Name + "." + version);
                            if (!outputFolder.Exists)
                            {
                                outputFolder.Create();
                            }
                            try
                            {
                                DateTime utcNow = DateTime.UtcNow, now = DateTime.Now;

                                pjObj.Publish(outputFolder.FullName, targetRuntime, pjObj.ExcludeCondition);

                                object publishInfoObj = new
                                {
                                    Name = pjObj.Name,
                                    Version = version,
                                    Description = description,
                                    BuiltTimeUtcTicks = utcNow.Ticks,
                                    BuiltTimeUtc = utcNow.ToString("MM/dd/yyyy HH:mm"),
                                    BuiltTimeTicks = now.Ticks,
                                    BuiltTime = now.ToString("MM/dd/yyyy HH:mm"),
                                };

                                FileInfo publishInfoFile = new FileInfo(outputFolder.FullName + "/version.json");

                                File.WriteAllText(publishInfoFile.FullName, JsonConvert.SerializeObject(publishInfoObj));
                                Console.WriteLine($"Publish {pjObj.Name} with runtime {targetRuntime} done.");
                            }
                            catch { }
                        }
                        else
                        {
                            Console.WriteLine($"Please check output path again.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Please input description.");
                    }

                }
                else
                {
                    Console.WriteLine($"Output type: {pjObj.OutputType} invalid. Please check again.");
                }
            }


        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace);
        }
    }

    static string _GetNextVersion(string currentVersion)
    {
        string result = currentVersion;
        try
        {
            string[] temp = currentVersion.Split('.');
            int lastNumber;
            if (int.TryParse(temp[temp.Length - 1], out lastNumber))
            {
                temp[temp.Length - 1] = (lastNumber + 1).ToString();
                result = string.Join(".", temp);
            }
        }
        catch { }
        return result;

    }
}
