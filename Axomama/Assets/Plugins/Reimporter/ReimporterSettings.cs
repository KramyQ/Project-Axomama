using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Plugins.Reimporter
{
    [CreateAssetMenu(fileName = "Reimporter", menuName = "Create Settings")]
    public class ReimporterSettings : ScriptableObject
    {
        [Label("All Art Assets")]
        public bool isReimportingAllArtAssets;
        [Label(".png Assets")]
        public bool isReimportingPngAssets;
        [Label(".jpg Assets")]
        public bool isReimportingJpgAssets;
        [Label(".jpeg Assets")]
        public bool isReimportingJpegAssets;
        [Label(".fbx Assets")]
        public bool isReimportingFbxAssets;
        [Label(".obj Assets")]
        public bool isReimportingObjAssets;
        [Label(".gtlf Assets")]
        public bool isReimportingGltfAssets;

        [HideInInspector]
        public List<string> allowedExtensions = new List<string>();

        private void OnValidate()
        {
            allowedExtensions.Clear();
            
            if (isReimportingAllArtAssets)
            {
                isReimportingPngAssets = true;
                isReimportingJpgAssets = true;
                isReimportingJpegAssets = true;
                isReimportingFbxAssets = true;
                isReimportingObjAssets = true;
                isReimportingGltfAssets = true;
            }

            if (isReimportingPngAssets)
                allowedExtensions.Add(".png");

            if (isReimportingJpgAssets)
                allowedExtensions.Add(".jpg");

            if (isReimportingJpegAssets)
                allowedExtensions.Add(".jpeg");

            if (isReimportingFbxAssets)
                allowedExtensions.Add(".fbx");

            if (isReimportingObjAssets)
                allowedExtensions.Add(".obj");

            if (isReimportingGltfAssets)
                allowedExtensions.Add(".gltf");
        }
    }
}