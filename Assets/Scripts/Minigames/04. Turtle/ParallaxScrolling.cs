using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// <para>Recommended to use in Unity's 2D mode</para>
/// <para>Setup:
/// <list type="number">
///     <item>
///         <para>Textures import(for each):
///             <list type="number">
///                 <item>
///                     <term>Wrap mode</term>
///                     <description>Wrap mode -> Repeat</description>
///                 </item>
///                 <item>
///                     <term>Filter mode (in case of pixel art)</term>
///                     <description>Filter mode -> Point (no filter)</description>
///                 </item>
///                 <item>
///                     <term>Compression (in case of pixel art)</term>
///                     <description>Compression -> None</description>
///                 </item>
///             </list>
///         </para>
///     </item>
///     <item>
///         <term>Sorting</term>
///         <description>Sort layers with order in "Scene window" (by dragging GameObjects up and down in list)</description>
///     </item>
/// </list>
/// </para>
/// </summary>
public class ParallaxScrolling : MonoBehaviour
{
    public float Speed;
    [ValidateInput(nameof(AreLayersValid), "Layers are not valid")]
    public List<ParallaxLayer> Layers;

    private void Update()
    {
        if (!AreLayersValid(Layers)) return;
        Layers.ForEach(layer => layer.SetOffset(layer.GetOffset() + Vector2.right * Time.deltaTime * layer.Speed * (Speed / 100)));
    }

    [Button]
    public void ResetOffset() => Layers.ForEach(layer => layer.SetOffset(Vector2.zero));
    private static bool AreLayersValid(List<ParallaxLayer> input) => !input.Any(layer => layer.LayerRenderer?.mainTexture is null);

    private void OnDisable() => ResetOffset();

    [Serializable]
    public class ParallaxLayer
    {
        [Required] public RawImage LayerRenderer;
        [Range(0,1)] public float Speed;
        internal Rect UV
        {
            get => LayerRenderer.uvRect;
            set => LayerRenderer.uvRect = value;
        }

        public void SetOffset(Vector2 value) => UV = new Rect(value, UV.size);
        public Vector2 GetOffset() => UV.position;
    }
}