using UnityEngine;

[RequireComponent(typeof(Renderer))]
[ExecuteAlways]
public class URPAtlasOffsetRealtime : MonoBehaviour
{
    [Header("URP Settings")]
    public string texturePropertyName = "_BaseMap";

    [Header("Offset Control")]
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
        // OnValidate dipanggil saat nilai di inspector diubah
        ApplyOffset();
    }

    private void Update()
    {
        // Tetap panggil ApplyOffset agar perubahan terlihat realtime di Scene
        ApplyOffset();
    }

    private void Init()
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();

        // Inisialisasi PropertyBlock jika masih null
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        _propID = Shader.PropertyToID(texturePropertyName + "_ST");
    }

    private void ApplyOffset()
    {
        // Pastikan semua komponen sudah siap sebelum eksekusi
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        // Ambil data block saat ini (Ini baris yang tadi error)
        _renderer.GetPropertyBlock(_propBlock);

        // Set Tiling dan Offset: (TilingX, TilingY, OffsetX, OffsetY)
        _propBlock.SetVector(_propID, new Vector4(tiling.x, tiling.y, offset.x, offset.y));

        // Terapkan kembali ke renderer
        _renderer.SetPropertyBlock(_propBlock);
    }
}
