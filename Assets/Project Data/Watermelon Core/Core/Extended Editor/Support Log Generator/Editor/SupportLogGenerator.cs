using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public static class SupportLogGenerator
    {
        [MenuItem("Help/Generate Support Log", priority = 170)]
        public static void GenerateSupportLog()
        {
            string path = EditorUtility.SaveFilePanel("Save support log file", "", "WM Support Log.txt", "txt");

            if (path.Length != 0)
            {
                File.WriteAllText(path, GetLog());
            }
        }

        private static string GetLog()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Watermelon Support Log - ");
            stringBuilder.Append(TimeUtils.GetCurrentDateString(TimeUtils.FORMAT_FULL));

            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            stringBuilder.Append("Info");

            stringBuilder.AppendLine();
            stringBuilder.Append("OS - ");
            stringBuilder.Append(SystemInfo.operatingSystem);

            stringBuilder.AppendLine();
            stringBuilder.Append("Editor Version - ");
            stringBuilder.Append(Application.unityVersion);

            stringBuilder.AppendLine();
            stringBuilder.Append("Platform - ");
            stringBuilder.Append(EditorUserBuildSettings.activeBuildTarget);

            stringBuilder.AppendLine();
            stringBuilder.Append("Template Version - ");
            stringBuilder.Append(GetTemplateVersion());

            stringBuilder.AppendLine();
            stringBuilder.Append("Core Version - ");
            stringBuilder.Append(GetCoreVersion());
            stringBuilder.AppendLine();

            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            stringBuilder.Append("Defines");
            stringBuilder.AppendLine();

            string definesLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));

            if (!string.IsNullOrEmpty(definesLine))
            {
                string[] defines = definesLine.Split(';');
                for(int i = 0; i < defines.Length; i++)
                {
                    if(!string.IsNullOrEmpty(defines[i]))
                    {
                        stringBuilder.Append("- ");
                        stringBuilder.Append(defines[i]);
                        stringBuilder.AppendLine();
                    }
                }
            }
            else
            {
                stringBuilder.Append("---");
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            stringBuilder.Append("Packages");
            stringBuilder.AppendLine();

            string[] depencies = GetDepencies();
            if (!depencies.IsNullOrEmpty())
            {
                for (int i = 0; i < depencies.Length; i++)
                {
                    stringBuilder.Append(depencies[i]);
                    stringBuilder.AppendLine();
                }
            }
            else
            {
                stringBuilder.Append("---");
            }

            stringBuilder.AppendLine();

            stringBuilder.Append("Project Folders");
            stringBuilder.AppendLine();

            string[] folders = GetProjectFolders();
            if(!folders.IsNullOrEmpty())
            {
                for (int i = 0; i < folders.Length; i++)
                {
                    stringBuilder.Append(folders[i]);
                    stringBuilder.AppendLine();
                }
            }

            return stringBuilder.ToString();
        }

        private static string GetCoreVersion()
        {
            TextAsset coreChangelogText = EditorUtils.GetAssetByName<TextAsset>("Core Changelog");
            if(coreChangelogText != null)
            {
                string fileText = coreChangelogText.text;
                if(!string.IsNullOrEmpty(fileText))
                {
                    string firstLine = fileText.Substring(0, fileText.IndexOf(System.Environment.NewLine));
                    if (!string.IsNullOrEmpty(firstLine))
                    {
                        return firstLine;
                    }
                }
            }

            return "Unknown";
        }

        private static string GetTemplateVersion()
        {
            TextAsset templateChangelogText = EditorUtils.GetAssetByName<TextAsset>("Template Changelog");
            if (templateChangelogText != null)
            {
                string fileText = templateChangelogText.text;
                if (!string.IsNullOrEmpty(fileText))
                {
                    string firstLine = fileText.Substring(0, fileText.IndexOf(System.Environment.NewLine));
                    if (!string.IsNullOrEmpty(firstLine))
                    {
                        return firstLine;
                    }
                }
            }

            return "Unknown";
        }

        private static string[] GetDepencies()
        {
            string fullPath = EditorUtils.projectFolderPath + "Packages/manifest.json";
            if(File.Exists(fullPath))
            {
                string fileText = File.ReadAllText(fullPath);
                if (!string.IsNullOrEmpty(fileText))
                {
                    Regex regex = new Regex("(?<=\"dependencies\": {)[^}]*(?=})");

                    Match match = regex.Match(fileText);
                    if (match.Success)
                    {
                        List<string> tempResult = new List<string>();

                        string[] depencies = match.Value.Split(new[] { '\r', '\n' });
                        for(int i = 0; i < depencies.Length; i++)
                        {
                            if(!string.IsNullOrEmpty(depencies[i]))
                            {
                                tempResult.Add(depencies[i]);
                            }
                        }

                        return tempResult.ToArray();

                    }
                }
            }

            return null;
        }

        private static string[] GetProjectFolders()
        {
            if (Directory.Exists(Application.dataPath))
            {
                List<string> tempResult = new List<string>();

                string[] directories = Directory.GetDirectories(Application.dataPath);
                for (int i = 0; i < directories.Length; i++)
                {
                    tempResult.Add("- " + Path.GetFileName(Path.GetDirectoryName(directories[i] + "/")));

                    string[] subFolders = Directory.GetDirectories(directories[i]);
                    if (!subFolders.IsNullOrEmpty())
                    {
                        for (int k = 0; k < subFolders.Length; k++)
                        {
                            tempResult.Add("-- " + Path.GetFileName(Path.GetDirectoryName(subFolders[k] + "/")));
                        }
                    }
                }

                return tempResult.ToArray();
            }

            return null;
        }
    }
}
