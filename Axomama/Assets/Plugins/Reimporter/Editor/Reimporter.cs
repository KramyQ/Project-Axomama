using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using Plugins.Reimporter.Editor;
using Unity.Plastic.Newtonsoft.Json;

namespace Plugins.Reimporter
{
    public class Reimporter : MonoBehaviour
    {
        private static readonly string JsonFilePath = "Assets/Plugins/Reimporter/ReimporterData.json";
        
        [MenuItem("Assets/Reimporter", true)]
        private static bool ValidateReimporter()
        {
            UnityEngine.Object selectedAsset = Selection.activeObject;
            return selectedAsset != null; // Only show the menu if an asset is selected
        }

        [MenuItem("Assets/Reimporter", false, 1)]
        public static void Init()
        {
            UnityEngine.Object selectedAsset = Selection.activeObject;
            ReimporterData matchingReimporterData = TryGetReimporterData(selectedAsset);

            if (matchingReimporterData != null)
            {
                // Use the matching data as needed
                //Debug.Log("Found matching data: " + matchingReimporterData.AssetName);

                // Reimport the asset
                if (string.IsNullOrEmpty(matchingReimporterData.OuterPath) ||
                    !File.Exists(matchingReimporterData.OuterPath))
                {
                    //Debug.Log("OuterPath not found for: " + matchingReimporterData.AssetName);
                    ShowFileDialog();
                }
                else
                {
                    //Debug.Log("OuterPath found for: " + matchingReimporterData.AssetName);
                    UpdateAsset(matchingReimporterData.OuterPath);
                }

                return;
            }
            else
            {
                //Debug.Log("Matching Data is null for: " + selectedAsset.name);
            }

            // If assetPath is not found, prompt file selection dialog
            Debug.Log("Failed to Reimported asset");
            ShowFileDialog();
        }

        [MenuItem("Assets/Remap", false, 1)]
        private static void ShowFileDialog()
        {
            // Retrieve the selected asset
            UnityEngine.Object selectedAsset = Selection.activeObject;
            string selectedAssetPath = AssetDatabase.GetAssetPath(selectedAsset);
            ReimporterData matchingReimporterData = TryGetReimporterData(selectedAsset);

            string outerFilePath = EditorUtility.OpenFilePanel(
                "Select Outer file reference",
                matchingReimporterData != null ? matchingReimporterData.OuterPath : selectedAssetPath,
                "");

            if (!string.IsNullOrEmpty(outerFilePath))
            {
                // Reimport the selected file
                UpdateAsset(outerFilePath);
            }
            else
            {
                Debug.Log("No file selected for import.");
            }
        }

        private static void UpdateAsset(string outerPath)
        {
            UnityEngine.Object selectedAsset = Selection.activeObject;
            string selectedAssetPath = AssetDatabase.GetAssetPath(selectedAsset);

            File.Copy(outerPath, selectedAssetPath, true);
            AssetDatabase.Refresh();

            ReimporterData reimporterData = new ReimporterData();
            reimporterData.AssetName = Path.GetFileNameWithoutExtension(selectedAssetPath);
            reimporterData.InnerPath = selectedAssetPath;
            reimporterData.OuterPath = outerPath;

            SaveReimporterData(reimporterData);

            //Debug.Log($"Copied {reimporterData.OuterPath} to {reimporterData.InnerPath}");
            Debug.Log($"{reimporterData.AssetName} successfully reimported");
        }

        public static void SaveReimporterData(ReimporterData newReimporterData)
        {
            List<ReimporterData> reimporterDatas = GetReimporterDatas();
            
            ReimporterData existingData = reimporterDatas.Find(data => data.AssetName == newReimporterData.AssetName);

            if (existingData != null)
            {
                existingData.InnerPath = newReimporterData.InnerPath;
                existingData.OuterPath = newReimporterData.OuterPath;
            }
            else
            {
                reimporterDatas.Add(newReimporterData);
            }

            // Create a JSON object and assign the names array to it
            string json = JsonConvert.SerializeObject(reimporterDatas, Formatting.Indented);

            // Write the JSON string to a file
            File.WriteAllText(JsonFilePath, json);
        }
        
        public static void OverwriteReimporterDatas(List<ReimporterData> reimporterDatas)
        {
            string emptyJson = "[]";
            File.WriteAllText(JsonFilePath, emptyJson);

            // Create a JSON object and assign the names array to it
            string json = JsonConvert.SerializeObject(reimporterDatas, Formatting.Indented);

            // Write the JSON string to a file
            File.WriteAllText(JsonFilePath, json);
        }

        public static List<ReimporterData> GetReimporterDatas()
        {
            if (File.Exists(JsonFilePath))
            {
                // Read the JSON file
                string jsonContent = File.ReadAllText(JsonFilePath);

                if (!string.IsNullOrEmpty(jsonContent))
                {
                    return JsonConvert.DeserializeObject<List<ReimporterData>>(jsonContent);
                }
            }

            return new List<ReimporterData>();
        }

        private static ReimporterData TryGetReimporterData(UnityEngine.Object selectedAsset)
        {
            // Retrieve the selected asset
            string selectedAssetPath = AssetDatabase.GetAssetPath(selectedAsset);
            string selectedAssetName = Path.GetFileNameWithoutExtension(selectedAssetPath);

            if (File.Exists(JsonFilePath))
            {
                // Read the JSON file
                string jsonContent = File.ReadAllText(JsonFilePath);

                if (!string.IsNullOrEmpty(jsonContent))
                {
                    List<ReimporterData> reimporterDatas =
                        JsonConvert.DeserializeObject<List<ReimporterData>>(jsonContent);

                    // Find the matching data in reimporterDatas
                    return reimporterDatas.Find(data => data.AssetName == selectedAssetName);
                }
            }

            return null;
        }
    }

}