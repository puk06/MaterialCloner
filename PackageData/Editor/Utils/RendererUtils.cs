using System.Linq;
using UnityEngine;

namespace MaterialCloner.Utils
{
    internal static class RendererUtils
    {
        /// <summary>
        /// GameObject内の全てのRendererを取得します。
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        internal static Renderer[] GetAllRenderers(GameObject gameObject)
        {
            return gameObject
                .GetComponentsInChildren<Renderer>(true)
                .Where(smr => !IsNull(smr))
                .ToArray();
        }

        /// <summary>
        /// 指定されたRendererがNullかどうかをチェックします。
        /// </summary>
        /// <param name="Renderer"></param>
        /// <returns></returns>
        internal static bool IsNull(Renderer renderer)
            => renderer == null || renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0;
    }
}
