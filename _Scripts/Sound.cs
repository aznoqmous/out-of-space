using UnityEngine.Audio;
using UnityEngine;
using System;

[Serializable]
public class Sound
{
    public SoundType Type;
    public AudioClip Clip;
    [HideInInspector] public AudioSource AudioSource;
}

public enum SoundType
{
    Speak
}