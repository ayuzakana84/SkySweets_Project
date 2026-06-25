using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStageData", menuName = "SkySweets/StageData")]

public class StageData : ScriptableObject
{
    public string sceneName;
    public string displayStageName;
    public Sprite previewImage;
    //public AudioClip stageBGM;
}