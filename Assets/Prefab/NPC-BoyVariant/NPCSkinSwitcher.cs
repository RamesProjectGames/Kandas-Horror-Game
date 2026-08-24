using UnityEngine;
using System.Collections.Generic;


public class NPCSkinSwitcher : MonoBehaviour
{
    [System.Serializable]
    public struct SkinVariant
    {
        public string variantName;
        public Material bodyMaterial; // Material khusus Badan
        public Material earsMaterial; // Material khusus Telinga
    }

    [Header("Skin Variants (1 Set Body + Ears)")]
    public List<SkinVariant> skinVariants;

    [Header("Target Mesh Renderers")]
    [Tooltip("Drag Mesh Body NPC ke sini")]
    public Renderer bodyRenderer;
    [Tooltip("Drag Mesh Ears NPC ke sini")]
    public Renderer earsRenderer;

    [Header("Current Selection")]
    public int currentSkinIndex = 0;

    void Start()
    {
        ApplySkin();
    }

    public void ApplySkin()
    {
        if (skinVariants == null || skinVariants.Count == 0) return;

        // Ambil preset berdasarkan index
        int index = Mathf.Clamp(currentSkinIndex, 0, skinVariants.Count - 1);
        SkinVariant currentVariant = skinVariants[index];

        // Terapkan material badan
        if (bodyRenderer != null && currentVariant.bodyMaterial != null)
        {
            bodyRenderer.sharedMaterial = currentVariant.bodyMaterial;
        }

        // Terapkan material telinga
        if (earsRenderer != null && currentVariant.earsMaterial != null)
        {
            earsRenderer.sharedMaterial = currentVariant.earsMaterial;
        }
    }

    void OnValidate()
    {
        // Otomatis terupdate di Scene View saat mengganti index atau material
        ApplySkin();
    }
}
