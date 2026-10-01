using UnityEngine;

namespace Game.Varginha
{
    // Phase factories populate their maps in Start, after sceneLoaded fires.
    [DefaultExecutionOrder(10000)]
    public sealed class VarginhaWorldGeometryPass : MonoBehaviour
    {
        private void Start() => VarginhaWorldGeometry.EnsureScene(gameObject.scene);
    }
}
