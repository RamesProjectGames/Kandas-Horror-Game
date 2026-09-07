using UnityEngine;
using System.Collections.Generic;
public class RandomCowDestroy : MonoBehaviour
{
    [Header("Daftar 3 Sapi Utuh di Scene")]
    public List<GameObject> cowIntactList; // Masukkan 3 Sapi Utuh dari Scene ke sini

    [Header("Referensi Prefab Pecahan")]
    public GameObject cowFracturedWithFrog; // Drag Prefab 'SapiPecah_DenganKatak'
    public GameObject cowFracturedEmpty;    // Drag Prefab 'SapiPecah_Kosong'

    [Header("Pengaturan Ledakan")]
    public float explosionForce = 500f;
    public float explosionRadius = 3f;

    private int indexSapiAdaKatak;

    void Start()
    {
        // Acak salah satu sapi dari indeks 0, 1, atau 2 untuk diisi katak
        if (cowIntactList.Count > 0)
        {
            indexSapiAdaKatak = Random.Range(0, cowIntactList.Count);
            Debug.Log("Sapi yang ada kataknya adalah Sapi nomor indeks: " + indexSapiAdaKatak);
        }
    }

    // Panggil fungsi ini saat Sapi tertembak / dipukul / memicu Trigger
    public void HancurkanSapiSpesifik(GameObject sapiYangDihancurkan, Vector3 hitPoint)
    {
        if (!cowIntactList.Contains(sapiYangDihancurkan)) return;

        int indexSapi = cowIntactList.IndexOf(sapiYangDihancurkan);
        Vector3 spawnPos = sapiYangDihancurkan.transform.position;
        Quaternion spawnRot = sapiYangDihancurkan.transform.rotation;

        GameObject prefabYangDiSpawn;

        // Cek apakah sapi yang dihancurkan ini adalah sapi yang terpilih berisi katak
        if (indexSapi == indexSapiAdaKatak)
        {
            prefabYangDiSpawn = cowFracturedWithFrog;
        }
        else
        {
            prefabYangDiSpawn = cowFracturedEmpty;
        }

        // 1. Spawn Pecahan
        GameObject fracturedInstance = Instantiate(prefabYangDiSpawn, spawnPos, spawnRot);

        // 2. Beri efek dorongan ledakan
        Rigidbody[] rbs = fracturedInstance.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            rb.AddExplosionForce(explosionForce, hitPoint, explosionRadius);
        }

        // 3. Hapus sapi utuh
        Destroy(sapiYangDihancurkan);
    }
}
