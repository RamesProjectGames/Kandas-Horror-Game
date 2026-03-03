using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[Serializable]
public class FootstepAudioData
{
    public List<AudioClip> footStepSound = new List<AudioClip>();
    public SurfaceType surfaceType;
}
public class FootstepsSoundManager : MonoBehaviour
{
    public List<FootstepAudioData> FootstepAudioData = new List<FootstepAudioData>();
    public Animator Animator;
    public LayerMask Enviroment;
    private float _lastfootstep;
    void OnValidate()
    {
        if(!Animator)
        {
            Animator = GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Animator != null)
        {
            var footstep = Animator.GetFloat("Footstep");
            if (Math.Abs(footstep) < .000001f) footstep = 0f;
            if (_lastfootstep > 0 && footstep < 0 || _lastfootstep < 0 && footstep > 0)
            {
                PlayFootstep();
            }
            _lastfootstep = footstep;
        }
    }

    // internal helper used by animator‑based signaling and external callers
    public void PlayFootstep()
    {
        var clips = GetClipsForSurface();
        if (clips == null || clips.Count == 0)
            return; // no clips assigned for this surface

        var randomIndex = UnityEngine.Random.Range(0, clips.Count);
        AudioSource.PlayClipAtPoint(clips[randomIndex], transform.position);
    }

    public List<AudioClip> GetClipsForSurface()
    {
        var clips = new List<AudioClip>();
        // Raycast downwards a short distance to determine what we're stepping on.
        var origin = transform.position + Vector3.up * .1f;
        var isHit = Physics.Raycast(origin, Vector3.down, out var hitInfo, 3);
        if (isHit)
        {
            // first, try to fetch a SurfaceIdentifier component from the hit collider.
            var surfaceIdentifier = hitInfo.collider.GetComponent<SurfaceIdentifier>();
            if (surfaceIdentifier != null)
            {
                foreach (var audioData in FootstepAudioData)
                {
                    if (audioData.surfaceType == surfaceIdentifier.surfaceType)
                    {
                        return audioData.footStepSound;
                    }
                }
            }

            // if we didn't find a SurfaceIdentifier, check if the collider is a terrain
            // and determine the terrain layer at the hit point.
            var terrain = hitInfo.collider.GetComponent<Terrain>();
            if (terrain != null)
            {
                string layerName = GetLayerName(hitInfo.point, terrain);
                if (!string.IsNullOrEmpty(layerName) &&
                    Enum.TryParse<SurfaceType>(layerName, true, out var terrainSurface))
                {
                    foreach (var audioData in FootstepAudioData)
                    {
                        if (audioData.surfaceType == terrainSurface)
                        {
                            return audioData.footStepSound;
                        }
                    }
                }
            }
        }

        return clips; // Default footstep sounds (empty list means caller should handle fallback)
    }
    public float[] GetTextureMix(Vector3 playerPos, Terrain t)
    {
        Vector3 tPos = t.transform.position;
        TerrainData terrainData = t.terrainData;
        int mapX = Mathf.FloorToInt((playerPos.x - tPos.x) / terrainData.size.x * terrainData.alphamapWidth);
        int mapZ = Mathf.FloorToInt((playerPos.z - tPos.z) / terrainData.size.z * terrainData.alphamapHeight);
        float[,,] splatMapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        float[] cellMix = new float[splatMapData.GetUpperBound(2) + 1];
        for (int i = 0; i < cellMix.Length; i++)
        {
            cellMix[i] = splatMapData[0, 0, i];
        }
        return cellMix;
    }
    public string GetLayerName(Vector3 playerPos, Terrain t)
    {
        float[] cellMix = GetTextureMix(playerPos, t);
        float strongestMix = 0;
        int maxIndex = 0;
        for (int i = 0; i < cellMix.Length; i++)
        {
            if (cellMix[i] > strongestMix)
            {
                maxIndex = i;
                strongestMix = cellMix[i];
            }
        }
        return t.terrainData.terrainLayers[maxIndex].name;
    }
}
