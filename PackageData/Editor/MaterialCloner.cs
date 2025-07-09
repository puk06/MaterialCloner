using MaterialCloner.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MaterialCloner
{
    public class MaterialCloner : EditorWindow
    {
        private const bool BETA_VERSION_BOOL = false;
        private const string CURRENT_VERSION = "v1.0.0";
        private const string AUTHOR = "ぷこるふ";

        private GameObject targetObject;

        private static readonly GUIContent TargetObjectLabel = GuiUtils.GenerateCustomLabel("ルートオブジェクト", "このオブジェクト内のマテリアル全てがクローンされたオブジェクトが新規作成されます。");
        private const string ClonedMaterialSuffix = "_Clone";
        private const string ClonedMaterialPath = "Assets/ClonedMaterials/";

        [MenuItem("Tools/ぷこのつーる/MaterialCloner")]
        public static void ShowWindow()
        {
            GetWindow<MaterialCloner>("MaterialCloner");

            if (BETA_VERSION_BOOL)
            {
                GuiUtils.ShowDialog("このMaterialClonerはベータ版です。マテリアルのクローンを行う前にプロジェクトのバックアップをしておくことを推奨します。");
            }
        }

        private void OnGUI()
        {
            GuiUtils.DrawBigTitle(AUTHOR, CURRENT_VERSION, BETA_VERSION_BOOL);

            #region ルートオブジェクトの選択
            GuiUtils.DrawSection("ルートオブジェクトの選択", isFirst: true);

            targetObject = (GameObject)EditorGUILayout.ObjectField(TargetObjectLabel, targetObject, typeof(GameObject), true);
            #endregion

            #region 操作
            GuiUtils.DrawSection("操作");

            if (targetObject == null)
            {
                EditorGUILayout.HelpBox($"操作可能な項目がありません。", MessageType.Info);
            }
            else
            {
                if (GUILayout.Button("マテリアルクローンされたオブジェクトの作成"))
                {
                    var result = EditorUtility.DisplayDialog("確認", "選択したオブジェクトのマテリアルのクローンが適用された新しいオブジェクトを生成す。よろしいですか？", "はい", "いいえ");
                    if (result)
                    {
                        CloneMaterial(targetObject);
                    }
                }
            }
            #endregion

            #region リセット
            GuiUtils.DrawSection("リセット");

            if (GUILayout.Button("オブジェクトのリセット"))
            {
                targetObject = null;
            }
            #endregion
        }

        private static void CloneMaterial(GameObject targetObject)
        {
            if (targetObject == null)
            {
                GuiUtils.ShowDialog("ルートオブジェクトが選択されていません。");
                return;
            }

            GameObject clonedObject = Instantiate(targetObject);
            clonedObject.name = targetObject.name + " (MaterialCloned)";
            Undo.RegisterCreatedObjectUndo(clonedObject, "Material Cloner - Clone Object");

            SkinnedMeshRenderer[] renderers = SkinnedMeshRendererUtils.GetAllSkinnedMeshRenderers(clonedObject);
            if (renderers.Length == 0)
            {
                GuiUtils.ShowDialog("SkinnedMeshRendererが見つかりませんでした。");
                return;
            }

            string clonedMaterialDirectory = Path.Combine(ClonedMaterialPath, clonedObject.name);
            Directory.CreateDirectory(clonedMaterialDirectory);

            Dictionary<Material, Material> clonedMaterialCache = new();

            foreach (var renderer in renderers)
            {
                if (SkinnedMeshRendererUtils.IsNull(renderer)) continue;

                Material[] originalMaterials = renderer.sharedMaterials;
                Material[] clonedMaterials = new Material[originalMaterials.Length];

                for (int i = 0; i < originalMaterials.Length; i++)
                {
                    Material original = originalMaterials[i];

                    if (original == null)
                    {
                        clonedMaterials[i] = null;
                        continue;
                    }

                    if (clonedMaterialCache.TryGetValue(original, out var cached))
                    {
                        clonedMaterials[i] = cached;
                        continue;
                    }

                    Material cloned = Instantiate(original);

                    cloned.name = original.name + ClonedMaterialSuffix;

                    string basePath = Path.Combine(clonedMaterialDirectory, cloned.name + ".mat");
                    string finalPath = basePath;
                    int count = 1;

                    while (AssetDatabase.LoadAssetAtPath<Material>(finalPath) != null)
                    {
                        finalPath = Path.Combine(clonedMaterialDirectory, $"{cloned.name}_{count}.mat");
                        count++;
                    }

                    AssetDatabase.CreateAsset(cloned, finalPath);

                    clonedMaterialCache[original] = cloned;
                    clonedMaterials[i] = cloned;
                }

                renderer.materials = clonedMaterials;
            }

            Selection.activeGameObject = clonedObject;
            targetObject.SetActive(false);

            GuiUtils.ShowDialog("マテリアルのクローンが適用された新しいオブジェクトを生成しました。");
        }
    }
}
