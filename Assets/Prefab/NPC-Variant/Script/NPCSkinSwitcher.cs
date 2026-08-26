using UnityEngine;
using System.Collections.Generic;


public class NPCSkinSwitcher : MonoBehaviour
{
    public enum Gender { Male, Female }
    public enum BottomType { Short, Long } // Celana/Rok Pendek vs Panjang

    [System.Serializable]
    public struct SkinVariant
    {
        public string variantName;
        public Material bodyMaterial; // Material gabungan Badan + Baju
        public Material earsMaterial; // Material Telinga
    }

    [Header("1. Gender & Bottom Type")]
    public Gender currentGender = Gender.Male;
    public BottomType bottomType = BottomType.Short;

    [Header("2. Renderers (Body & Ears)")]
    public Renderer maleBodyRenderer;
    public Renderer femaleBodyRenderer;
    public Renderer earsRenderer; // 1 Mesh Telinga untuk semua

    [Header("3. Bottom / Outfit Mesh Objects (Toggle)")]
    [Tooltip("Mesh Celana Pendek Pria")]
    public GameObject maleShortPants;
    [Tooltip("Mesh Celana Panjang Pria")]
    public GameObject maleLongPants;
    [Tooltip("Mesh Rok Pendek Wanita")]
    public GameObject femaleShortSkirt;
    [Tooltip("Mesh Rok Panjang / Hijab Wanita")]
    public GameObject femaleLongSkirt;

    [Header("4. Hair Objects")]
    public List<GameObject> maleHairObjects;
    public List<GameObject> femaleHairObjects;

    [Header("5. Accessories (Toggle ON/OFF)")]
    public List<GameObject> accessoryObjects;
    public bool showAccessories = false;

    [Header("Current Selection")]
    public int currentHairIndex = 0;
    public int currentSkinIndex = 0;

    [Header("6. Material Presets (Body + Ears)")]
    public List<SkinVariant> maleSkinVariants;
    public List<SkinVariant> femaleSkinVariants;

    void Start()
    {
        ApplyAllCustomizations();
    }

    public void ApplyAllCustomizations()
    {
        ApplyGenderAndMeshes();
        ApplySkinMaterials();
        ApplyHair();
        ApplyAccessories();
    }

    private void ApplyGenderAndMeshes()
    {
        bool isMale = (currentGender == Gender.Male);

        // Toggle Body
        if (maleBodyRenderer != null) maleBodyRenderer.gameObject.SetActive(isMale);
        if (femaleBodyRenderer != null) femaleBodyRenderer.gameObject.SetActive(!isMale);

        // Toggle Bottoms / Outfit
        bool isShort = (bottomType == BottomType.Short);

        if (maleShortPants != null) maleShortPants.SetActive(isMale && isShort);
        if (maleLongPants != null) maleLongPants.SetActive(isMale && !isShort);

        if (femaleShortSkirt != null) femaleShortSkirt.SetActive(!isMale && isShort);
        if (femaleLongSkirt != null) femaleLongSkirt.SetActive(!isMale && !isShort);
    }

    private void ApplySkinMaterials()
    {
        bool isMale = (currentGender == Gender.Male);
        List<SkinVariant> activeVariants = isMale ? maleSkinVariants : femaleSkinVariants;
        Renderer activeBody = isMale ? maleBodyRenderer : femaleBodyRenderer;

        if (activeVariants == null || activeVariants.Count == 0) return;

        int index = Mathf.Clamp(currentSkinIndex, 0, activeVariants.Count - 1);
        SkinVariant variant = activeVariants[index];

        // Apply Material Badan & Baju
        if (activeBody != null && variant.bodyMaterial != null)
        {
            activeBody.sharedMaterial = variant.bodyMaterial;
        }

        // Apply Material Telinga
        if (earsRenderer != null && variant.earsMaterial != null)
        {
            earsRenderer.sharedMaterial = variant.earsMaterial;
        }
    }

    private void ApplyHair()
    {
        foreach (var hair in maleHairObjects) if (hair != null) hair.SetActive(false);
        foreach (var hair in femaleHairObjects) if (hair != null) hair.SetActive(false);

        List<GameObject> activeHairList = (currentGender == Gender.Male) ? maleHairObjects : femaleHairObjects;

        if (activeHairList != null && activeHairList.Count > 0)
        {
            int hairIndex = Mathf.Clamp(currentHairIndex, 0, activeHairList.Count - 1);
            if (activeHairList[hairIndex] != null)
            {
                activeHairList[hairIndex].SetActive(true);
            }
        }
    }

    private void ApplyAccessories()
    {
        if (accessoryObjects == null) return;
        foreach (var acc in accessoryObjects)
        {
            if (acc != null) acc.SetActive(showAccessories);
        }
    }

    void OnValidate()
    {
        ApplyAllCustomizations();
    }
}
