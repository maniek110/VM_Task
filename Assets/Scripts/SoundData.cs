using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "ScriptableObjects/SoundData", order = 3)]
public class SoundData : ScriptableObject
{
    public AudioClip Sound;
    public AudioType AudioType;

}
public enum AudioType
{
    Select,
    Destroy,
    Cannot
}

