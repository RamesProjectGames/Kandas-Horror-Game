using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public enum ManequinType
{
    Stationary,
    Posing
}
[System.Serializable]
public enum MuscleGroup { Spine, Neck, Arm, Leg, Finger, Other }
public class Manequin : MonoBehaviour
{
    public ManequinType type;
    public Animator targetAnimator;
    public List<MuscleData> muscleList = new List<MuscleData>();
    
    private HumanPoseHandler _poseHandler;
    private HumanPose _currentPose;

    void Start()
    {
        if (targetAnimator == null) targetAnimator = GetComponent<Animator>();
        
        _poseHandler = new HumanPoseHandler(targetAnimator.avatar, targetAnimator.transform);
        _poseHandler.GetHumanPose(ref _currentPose);

        string[] names = HumanTrait.MuscleName;
        for (int i = 0; i < names.Length; i++)
        {
            MuscleGroup group = CategorizeMuscle(names[i]);
            muscleList.Add(new MuscleData(i, names[i], _currentPose.muscles[i], group));
        }
    }

    private MuscleGroup CategorizeMuscle(string name)
    {
        if (name.Contains("Spine") || name.Contains("Chest")) return MuscleGroup.Spine;
        if (name.Contains("Neck") || name.Contains("Head")) return MuscleGroup.Neck;
        if (name.Contains("UpperArm") || name.Contains("Forearm") || name.Contains("Shoulder")) return MuscleGroup.Arm;
        if (name.Contains("Thigh") || name.Contains("Calf") || name.Contains("Foot")) return MuscleGroup.Leg;
        if (name.Contains("Finger")) return MuscleGroup.Finger;
        return MuscleGroup.Other;
    }

    void LateUpdate()
    {
        if (_poseHandler == null) return;

        _poseHandler.GetHumanPose(ref _currentPose);
        foreach (var m in muscleList)
        {
            _currentPose.muscles[m.index] = m.currentValue;
        }
        _poseHandler.SetHumanPose(ref _currentPose);
    }

    [ContextMenu("Randomize Natural Pose")]
    public void RandomizePose()
    {
        foreach (var m in muscleList)
        {
            float range = m.group switch
            {
                MuscleGroup.Spine => 0.15f,
                MuscleGroup.Neck => 0.3f,
                MuscleGroup.Leg => 0.2f,
                MuscleGroup.Arm => 0.5f,
                MuscleGroup.Finger => 0.8f,
                _ => 0.3f
            };
            
            // Randomize within a humanly possible threshold from the default "T-Pose"
            m.currentValue = Random.Range(-range, range);
        }
    }

    [ContextMenu("Reset All Muscles")]
    public void ResetAll()
    {
        foreach (var m in muscleList) m.Reset();
    }
}

[System.Serializable]
public class MuscleData
{
    public string muscleName;
    public int index;
    public MuscleGroup group;
    [Range(-1f, 1f)]public float currentValue;
    public float defaultValue; // Captured at initialization

    public MuscleData(int index, string name, float initialValue, MuscleGroup group)
    {
        this.index = index;
        this.muscleName = name;
        this.defaultValue = initialValue;
        this.currentValue = initialValue;
        this.group = group;
    }

    public void Reset()
    {
        currentValue = defaultValue;
    }
}
