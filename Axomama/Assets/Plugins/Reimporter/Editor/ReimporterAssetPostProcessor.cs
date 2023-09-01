using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Plugins.Reimporter.Editor
{
    public class ReimporterAssetPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            ReimporterSettings reimporterSettings = LoadReimporterSettingsAsset();
            if (reimporterSettings != null)
            {
                foreach (var importedAsset in importedAssets)
                {
                    UnityEngine.Object importedObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(importedAsset);
                    if (importedObject != null)
                    {
                        string importedAssetPath = AssetDatabase.GetAssetPath(importedObject);
                        string importedAssetPathExtension = Path.GetExtension(importedAssetPath);
                        if (reimporterSettings.allowedExtensions.Contains(importedAssetPathExtension))
                        {
                            //Debug.Log("Can Reimport");
                        }
                        else
                        {
                            // Todo prevent imported assets from continuing the script
                            //Debug.Log("Can't Reimport");
                            return;
                        }
                    }
                }
            }

            List<ReimporterData> reimporterDatas = Reimporter.GetReimporterDatas();

            // Remove deleted assets from the JSON
            foreach (string deletedAsset in deletedAssets)
            {
                reimporterDatas.RemoveAll(data => data.InnerPath == deletedAsset);
            }

            // Update innerPath of moved assets
            for (int i = 0; i < movedAssets.Length; i++)
            {
                string movedAsset = movedAssets[i];
                string movedFromAssetPath = movedFromAssetPaths[i];

                ReimporterData matchingData = reimporterDatas.Find(data => data.InnerPath == movedFromAssetPath);
                if (matchingData != null)
                {
                    matchingData.InnerPath = movedAsset;
                }
            }

            // Add imported assets to the JSON
            foreach (string importedAsset in importedAssets)
            {
                if (!reimporterDatas.Exists(data => data.InnerPath == importedAsset))
                {
                    ReimporterData newReimporterData = new ReimporterData
                    {
                        AssetName = Path.GetFileNameWithoutExtension(importedAsset),
                        InnerPath = importedAsset,
                        OuterPath = string.Empty // Set the outer path as needed
                    };
                    reimporterDatas.Add(newReimporterData);
                }
            }
            
            // Check if a file was renamed
            // foreach (ReimporterData reimporterData in reimporterDatas)
            // {
            //     string previousInnerPath = reimporterData.InnerPath;
            //     string currentInnerPath = AssetDatabase.GetAssetPath(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(previousInnerPath));
            //
            //     if (previousInnerPath != currentInnerPath)
            //     {
            //         // Asset has been renamed
            //         reimporterData.InnerPath = currentInnerPath;
            //         Debug.Log("Renamed asset");
            //     }
            // }

            // Save the updated data to the JSON file
            Reimporter.OverwriteReimporterDatas(reimporterDatas);
        }
        
        private static ReimporterSettings LoadReimporterSettingsAsset()
        {
            string settingsPath = "Assets/Plugins/Reimporter/ReimporterSettings.asset";
            ReimporterSettings reimporterSettings = AssetDatabase.LoadAssetAtPath<ReimporterSettings>(settingsPath);

            if (reimporterSettings == null)
            {
                Debug.LogWarning("Settings asset not found at path: " + settingsPath);
            }

            return reimporterSettings;
        }
    }
}