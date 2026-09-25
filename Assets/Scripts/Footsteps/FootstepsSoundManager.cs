using Dialogue;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[Serializable]
public class FootstepAudioData
{
    public List<AudioClip> leftFootstepSound = new List<AudioClip>();
    public List<AudioClip> rightFootstepSound = new List<AudioClip>();
    public GroundSurface surfaceType;
}
[Serializable]
public class TerrainSoundType
{
    public List<string> terrainLayerNames = new List<string>();
    public GroundSurface surfaceType;
}
public class FootstepsSoundManager : MonoBehaviour
{
    [SerializeField] private EventReference footstepAudio;
    private EventInstance footstepEvent;

    public List<TerrainSoundType> TerrainSoundTypes = new List<TerrainSoundType>();
    public Animator Animator;
    public LayerMask Enviroment;
    private float _lastfootstep;
    
    // track which foot played last: true = right, false = left
    private bool _lastFootWasRight = true;
    
    // minimum time (in seconds) between footsteps to prevent left/right from playing too close
    public float minFootstepInterval = 0.3f;
    public Transform foot;
    [SerializeField] private float groundCheckDistance = 3f;
    private float _lastFootstepTime = -1f;

    private bool HasValidFootstepEvent()
    {
        return footstepEvent.hasHandle() && footstepEvent.isValid();
    }

    private void EnsureFootstepEventCreated()
    {
        if (AudioManager.Instance == null || footstepAudio.IsNull)
            return;

        if (!footstepEvent.hasHandle() || !footstepEvent.isValid())
        {
            footstepEvent = AudioManager.Instance.CreateInstance(footstepAudio);
            RuntimeManager.AttachInstanceToGameObject(footstepEvent, gameObject, false);
        }
    }

    private void Start()
    {
        EnsureFootstepEventCreated();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        Vector3 origin = foot.position - Vector3.down * .5f;
        bool foundGround = HasSurfaceOrTerrainBelowFoot();

        Gizmos.color = foundGround ? new Color(0.12f, 0.56f, 1f, 1f) : new Color(0.96f, 0.65f, 0.14f, 1f);
        Gizmos.DrawWireSphere(origin, 0.18f);
        Gizmos.DrawLine(origin, origin + Vector3.down * 0.6f);

        if (foundGround)
        {
            Gizmos.DrawCube(origin + Vector3.down * 0.45f, new Vector3(0.12f, 0.04f, 0.12f));
        }
        else
        {
            Gizmos.DrawWireCube(origin + Vector3.down * 0.45f, new Vector3(0.18f, 0.04f, 0.18f));
        }
    }

    // internal helper used by animator‑based signaling and external callers
    private bool HasSurfaceOrTerrainBelowFoot()
    {
        Vector3 origin = foot.position - Vector3.down * .5f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo, groundCheckDistance))
        {
            if (hitInfo.collider != null)
            {
                if (hitInfo.collider.GetComponent<SurfaceIdentifier>() != null)
                    return true;

                if (hitInfo.collider.GetComponent<Terrain>() != null)
                    return true;
            }
        }

        return GetTerrainAtWorldPosition(origin) != null;
    }

    public void PlayFootstep()
    {
        EnsureFootstepEventCreated();

        bool hasAudioManager = AudioManager.Instance != null;
        bool hasValidEvent = HasValidFootstepEvent();
        bool eventIsAssigned = !footstepAudio.IsNull;
        bool hasGround = HasSurfaceOrTerrainBelowFoot();

        if (!hasAudioManager || !hasValidEvent || !eventIsAssigned || !hasGround)
        {
            Debug.Log($"Footstep blocked: audioManager={hasAudioManager}, validEvent={hasValidEvent}, eventAssigned={eventIsAssigned}, ground={hasGround}, surfaceId={GetSurfaceIndex()}");
            return;
        }

        PLAYBACK_STATE playbackState;
        int surfaceIndex = GetSurfaceIndex();
        Debug.Log(Enum.GetName(typeof(GroundSurface), surfaceIndex));
        string surfaceName = Enum.GetName(typeof(GroundSurface), surfaceIndex);
        //Debug.Log($"Footstep ground surface: {surfaceName} (index {surfaceIndex})");

        footstepEvent.setParameterByName("Foot", _lastFootWasRight ? 1 : 0);
        footstepEvent.setParameterByName("Surface", surfaceIndex);
        footstepEvent.getPlaybackState(out playbackState);

        if (playbackState == PLAYBACK_STATE.STOPPED)
        {
            RuntimeManager.AttachInstanceToGameObject(footstepEvent, gameObject, false);
            footstepEvent.start();
        }

        _lastFootWasRight = !_lastFootWasRight;
        // _lastFootstepTime = Time.fixedTime;
    }

    public void StopFootstep()
    {
        if (!HasValidFootstepEvent())
            return;

        footstepEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }
    private Terrain GetTerrainAtWorldPosition(Vector3 worldPos)
    {
        Terrain bestTerrain = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < Terrain.activeTerrains.Length; i++)
        {
            Terrain terrain = Terrain.activeTerrains[i];
            if (terrain == null || terrain.terrainData == null)
                continue;

            Vector3 terrainPos = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;

            bool withinX = worldPos.x >= terrainPos.x && worldPos.x <= terrainPos.x + terrainSize.x;
            bool withinZ = worldPos.z >= terrainPos.z && worldPos.z <= terrainPos.z + terrainSize.z;

            if (!withinX || !withinZ)
                continue;

            float distance = Vector3.Distance(worldPos, terrain.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestTerrain = terrain;
            }
        }

        return bestTerrain;
    }

    public int GetSurfaceIndex()
    {
        var surfaceIndex = 0;
        var origin = foot.position - Vector3.down * .5f;
        var isHit = Physics.Raycast(origin, Vector3.down, out var hitInfo, groundCheckDistance);

        if (isHit && hitInfo.collider != null)
        {
            var surfaceIdentifier = hitInfo.collider.GetComponent<SurfaceIdentifier>();
            if (surfaceIdentifier != null)
            {
                surfaceIndex = (int)surfaceIdentifier.surfaceType;
                return surfaceIndex;
            }
        }

        Terrain terrain = GetTerrainAtWorldPosition(origin);
        if (terrain != null)
        {
            string layerName = GetLayerName(transform.position, terrain);
            foreach (var terrainSoundType in TerrainSoundTypes)
            {
                if (terrainSoundType.terrainLayerNames.Contains(layerName))
                {
                    return (int)terrainSoundType.surfaceType;
                }
            }

            if (!string.IsNullOrEmpty(layerName) && Enum.TryParse<GroundSurface>(layerName, true, out var terrainSurface))
            {
                return (int)terrainSurface;
            }
        }

        return surfaceIndex;
    }
    public float[] GetTextureMix(Vector3 playerPos, Terrain t)
    {
        if (t == null || t.terrainData == null)
            return new float[0];

        Vector3 tPos = t.transform.position;
        TerrainData terrainData = t.terrainData;

        float x = (playerPos.x - tPos.x) / Mathf.Max(terrainData.size.x, 0.0001f);
        float z = (playerPos.z - tPos.z) / Mathf.Max(terrainData.size.z, 0.0001f);

        int mapX = Mathf.Clamp(Mathf.FloorToInt(x * terrainData.alphamapWidth), 0, terrainData.alphamapWidth - 1);
        int mapZ = Mathf.Clamp(Mathf.FloorToInt(z * terrainData.alphamapHeight), 0, terrainData.alphamapHeight - 1);

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
        if (t == null || t.terrainData == null)
            return string.Empty;

        float[] cellMix = GetTextureMix(playerPos, t);
        if (cellMix == null || cellMix.Length == 0)
            return string.Empty;

        float strongestMix = -1f;
        int maxIndex = 0;
        for (int i = 0; i < cellMix.Length; i++)
        {
            if (cellMix[i] > strongestMix)
            {
                maxIndex = i;
                strongestMix = cellMix[i];
            }
        }

        if (maxIndex < 0 || maxIndex >= t.terrainData.terrainLayers.Length)
            return string.Empty;

        return t.terrainData.terrainLayers[maxIndex].name;
    }
}

public enum GroundSurface
{
    Dress,
    Floor,
    Grass,
    Glass
}
