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
    public SurfaceType surfaceType;
}
public class FootstepsSoundManager : MonoBehaviour
{
    [SerializeField] private EventReference footstepAudio;
    private EventInstance footstepEvent;

    public List<FootstepAudioData> FootstepAudioData = new List<FootstepAudioData>();
    public Animator Animator;
    public LayerMask Enviroment;
    private float _lastfootstep;
    
    // track which foot played last: true = right, false = left
    private bool _lastFootWasRight = true;
    
    // minimum time (in seconds) between footsteps to prevent left/right from playing too close
    public float minFootstepInterval = 0.3f;
    private float _lastFootstepTime = -1f;

    private void Start()
    {
        footstepEvent = AudioManager.Instance.CreateInstance(footstepAudio);
        RuntimeManager.AttachInstanceToGameObject(footstepEvent, gameObject, false);
    }
    void OnValidate()
    {
        //if(!Animator)
        //{
        //    Animator = GetComponent<Animator>();
        //}
    }

    // Update is called once per frame
    void Update()
    {
        //if (Application.isPlaying && (SettingManager.Instance.isPaused || DialogueSystem.Instance.isRunningConvo))
        //    return;
        //if(Animator != null)
        //{
        //    var footstep = Animator.GetFloat("Footstep");
        //    if (Math.Abs(footstep) < .000001f) footstep = 0f;
        //    if (_lastfootstep > 0 && footstep < 0 || _lastfootstep < 0 && footstep > 0)
        //    {
        //        PlayFootstep();
        //    }
        //    _lastfootstep = footstep;
        //}
    }

    // internal helper used by animator‑based signaling and external callers
    public void PlayFootstep()
    {
        // enforce minimum interval between footsteps
        if (Time.time - _lastFootstepTime < minFootstepInterval)
            return;

        PLAYBACK_STATE playbackState;
        footstepEvent.setParameterByName("Foot", _lastFootWasRight ? 1 : 0);
        //playerFootsteps.setParameterByName("Surface", UnityEngine.Random.Range(0, 3));
        footstepEvent.setParameterByName("Surface", 0);
        footstepEvent.getPlaybackState(out playbackState);

        if (playbackState == PLAYBACK_STATE.STOPPED)
        {
            RuntimeManager.AttachInstanceToGameObject(footstepEvent, gameObject, false);
            //playerFootsteps.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
            footstepEvent.start();
        }
        
        // alternate between left and right
        //if (_lastFootWasRight)
        //{
        //    PlayLeftFootstep();
        //}
        //else
        //{
        //    PlayRightFootstep();
        //}
        _lastFootWasRight = !_lastFootWasRight;
        _lastFootstepTime = Time.time;
    }

    public void StopFootstep()
    {
        footstepEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void PlayLeftFootstep()
    {
        var clips = GetClipsForSurface(isLeftFoot: true);
        if (clips == null || clips.Count == 0)
            return;

        var randomIndex = UnityEngine.Random.Range(0, clips.Count);
        AudioSource.PlayClipAtPoint(clips[randomIndex], transform.position);
    }

    public void PlayRightFootstep()
    {
        var clips = GetClipsForSurface(isLeftFoot: false);
        if (clips == null || clips.Count == 0)
            return;

        var randomIndex = UnityEngine.Random.Range(0, clips.Count);
        AudioSource.PlayClipAtPoint(clips[randomIndex], transform.position);
    }

    public List<AudioClip> GetClipsForSurface(bool isLeftFoot = true)
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
                        // return left or right specific clips
                        if (isLeftFoot && audioData.leftFootstepSound.Count > 0)
                            return audioData.leftFootstepSound;
                        if (!isLeftFoot && audioData.rightFootstepSound.Count > 0)
                            return audioData.rightFootstepSound;
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
                            // return left or right specific clips
                            if (isLeftFoot && audioData.leftFootstepSound.Count > 0)
                                return audioData.leftFootstepSound;
                            if (!isLeftFoot && audioData.rightFootstepSound.Count > 0)
                                return audioData.rightFootstepSound;
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

public enum GroundSurface
{
    Dress,
    Floor,
    Grass,
    Glass
}
