using System.Linq;
using UnityEngine;

namespace MaterialCloner.Utils
{
    internal static class SkinnedMeshRendererUtils
    {
        /// <summary>
        /// GameObject内の全てのSkinnedMeshRendererを取得します。
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        internal static SkinnedMeshRenderer[] GetAllSkinnedMeshRenderers(GameObject gameObject)
        {
            return gameObject
                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(smr => !IsNull(smr))
                .ToArray();
        }

        /// <summary>
        /// 指定されたSkinnedMeshRendererがNullかどうかをチェックします。
        /// </summary>
        /// <param name="skinnedMeshRenderer"></param>
        /// <returns></returns>
        internal static bool IsNull(SkinnedMeshRenderer skinnedMeshRenderer)
            => skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMaterials == null || skinnedMeshRenderer.sharedMaterials.Length == 0;
    }
}
