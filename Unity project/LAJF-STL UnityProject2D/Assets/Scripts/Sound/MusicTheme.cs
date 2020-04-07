﻿using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class MusicTheme
{
    public string name;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;

    public bool loop;

    public AudioMixerGroup audioMixerGroup;

    [HideInInspector]
    public AudioSource source;
}

