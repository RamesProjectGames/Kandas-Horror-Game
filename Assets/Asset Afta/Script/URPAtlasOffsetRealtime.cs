using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class URPAtlasOffsetRealtime : MonoBehaviour
{
    [Header("Standard Pipeline Settings")]
    [Tooltip("Default untuk Standard Shader adalah _MainTex")]
    public string texturePropertyName = "_MainTex";

    [Header("Offset & Tiling Control")]
    public Vector2 offset = Vector2.zero;
    public Vector2 tiling = Vector2.one;

    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;
    private int _propID;

    private void OnEnable()
    {
        Init();
    }

    private void OnValidate()
    {
        ApplyOffset();
    }

    // Update diaktifkan agar perubahan di Scene View terlihat instan
    private void Update()
    {
        // Pengecekan null tambahan untuk mencegah error 'dest' di editor
        if (_renderer == null || _propBlock == null) Init();

        ApplyOffset();
    }

    private void Init()
    {
        _renderer = GetComponent<Renderer>();
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        // Di Standard Shader, Tiling & Offset digabung dalam properti _ST (Scale/Translate)
        _propID = Shader.PropertyToID(texturePropertyName + "_ST");
    }

    private void ApplyOffset()
    {
        if (_renderer == null) return;
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        // Mengambil data block agar tidak menimpa settingan shader lain
        _renderer.GetPropertyBlock(_propBlock);

        // Standard Shader Vector4: (Tiling X, Tiling Y, Offset X, Offset Y)
        _propBlock.SetVector(_propID, new Vector4(tiling.x, tiling.y, offset.x, offset.y));

        // Terapkan ke renderer secara individual
        _renderer.SetPropertyBlock(_propBlock);
    }
}
