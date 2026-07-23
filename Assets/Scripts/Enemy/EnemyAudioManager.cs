using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class EnemyAudioManager : MonoBehaviour
{
    [SerializeField] private EventReference monsterAudioEvent;
    [SerializeField] private string actionParameterName = "MonsterAction";
    public List<EnemyActionParameterReference> actionParameters = new List<EnemyActionParameterReference>();
    
    public void PlayActionAudio(string actionName)
    {
        var actionReference = actionParameters.Find(a => a.actionName == actionName);
        if (actionReference == null)
        {
            Debug.LogWarning($"No action parameter reference found for action: {actionName}");
            return;
        }

        if (monsterAudioEvent.IsNull)
        {
            Debug.LogWarning("Monster audio event is not assigned.");
            return;
        }

        actionReference.isCompleted = false;
        actionReference.playVersion++;
        int playVersion = actionReference.playVersion;

        var actionEvent = RuntimeManager.CreateInstance(monsterAudioEvent);
        RuntimeManager.AttachInstanceToGameObject(actionEvent, gameObject, false);
        actionEvent.setParameterByName(actionParameterName, actionReference.parameterValue);
        actionEvent.start();
        StartCoroutine(WaitForActionAudioCompletion(actionEvent, actionReference, playVersion));
    }

    public bool IsActionAudioCompleted(string actionName)
    {
        var actionReference = actionParameters.Find(a => a.actionName == actionName);
        return actionReference != null && actionReference.isCompleted;
    }

    private IEnumerator WaitForActionAudioCompletion(EventInstance actionEvent, EnemyActionParameterReference actionReference, int playVersion)
    {
        PLAYBACK_STATE playbackState;

        do
        {
            actionEvent.getPlaybackState(out playbackState);
            yield return null;
        }
        while (playbackState != PLAYBACK_STATE.STOPPED);

        // Ignore stale completion from an older play call.
        if (actionReference.playVersion == playVersion)
        {
            actionReference.isCompleted = true;
            actionReference.onCompleted?.Invoke();
        }

        actionEvent.release();
    }
}
[System.Serializable]
public class EnemyActionParameterReference
{
    public string actionName;
    public float parameterValue;
    public UnityEvent onCompleted;
    [HideInInspector] public bool isCompleted;
    [HideInInspector] public int playVersion;
}
