using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioBank", menuName = "ScriptableObjects/AudioBank", order = 2)]
public class AudioBank : ScriptableObject
{
    [SerializeField]
    List<SoundData> soundDatas = new List<SoundData>();

    public SoundData GetSound(AudioType type)
    {
        return soundDatas.Find(x => x.AudioType == type);
    }
}