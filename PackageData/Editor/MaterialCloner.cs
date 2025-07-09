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
        private const string AUTHOR = "�Ղ����";

        private GameObject targetObject;

        private static readonly GUIContent TargetObjectLabel = GuiUtils.GenerateCustomLabel("���[�g�I�u�W�F�N�g", "���̃I�u�W�F�N�g���̃}�e���A���S�Ă��N���[�����ꂽ�I�u�W�F�N�g���V�K�쐬����܂��B");
        private const string ClonedMaterialSuffix = "_Clone";
        private const string ClonedMaterialPath = "Assets/ClonedMaterials/";

        [MenuItem("Tools/�Ղ��̂[��/MaterialCloner")]
        public static void ShowWindow()
        {
            GetWindow<MaterialCloner>("MaterialCloner");

            if (BETA_VERSION_BOOL)
            {
                GuiUtils.ShowDialog("����MaterialCloner�̓x�[�^�łł��B�}�e���A���̃N���[�����s���O�Ƀv���W�F�N�g�̃o�b�N�A�b�v�����Ă������Ƃ𐄏����܂��B");
            }
        }

        private void OnGUI()
        {
            GuiUtils.DrawBigTitle(AUTHOR, CURRENT_VERSION, BETA_VERSION_BOOL);

            #region ���[�g�I�u�W�F�N�g�̑I��
            GuiUtils.DrawSection("���[�g�I�u�W�F�N�g�̑I��", isFirst: true);

            targetObject = (GameObject)EditorGUILayout.ObjectField(TargetObjectLabel, targetObject, typeof(GameObject), true);
            #endregion

            #region ����
            GuiUtils.DrawSection("����");

            if (targetObject == null)
            {
                EditorGUILayout.HelpBox($"����\�ȍ��ڂ�����܂���B", MessageType.Info);
            }
            else
            {
                if (GUILayout.Button("�}�e���A���N���[�����ꂽ�I�u�W�F�N�g�̍쐬"))
                {
                    var result = EditorUtility.DisplayDialog("�m�F", "�I�������I�u�W�F�N�g�̃}�e���A���̃N���[�����K�p���ꂽ�V�����I�u�W�F�N�g�𐶐����B��낵���ł����H", "�͂�", "������");
                    if (result)
                    {
                        CloneMaterial(targetObject);
                    }
                }
            }
            #endregion

            #region ���Z�b�g
            GuiUtils.DrawSection("���Z�b�g");

            if (GUILayout.Button("�I�u�W�F�N�g�̃��Z�b�g"))
            {
                targetObject = null;
            }
            #endregion
        }

        private static void CloneMaterial(GameObject targetObject)
        {
            if (targetObject == null)
            {
                GuiUtils.ShowDialog("���[�g�I�u�W�F�N�g���I������Ă��܂���B");
                return;
            }

            GameObject clonedObject = Instantiate(targetObject);
            clonedObject.name = targetObject.name + " (MaterialCloned)";
            Undo.RegisterCreatedObjectUndo(clonedObject, "Material Cloner - Clone Object");

            SkinnedMeshRenderer[] renderers = SkinnedMeshRendererUtils.GetAllSkinnedMeshRenderers(clonedObject);
            if (renderers.Length == 0)
            {
                GuiUtils.ShowDialog("SkinnedMeshRenderer��������܂���ł����B");
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

            GuiUtils.ShowDialog("�}�e���A���̃N���[�����K�p���ꂽ�V�����I�u�W�F�N�g�𐶐����܂����B");
        }
    }
}
