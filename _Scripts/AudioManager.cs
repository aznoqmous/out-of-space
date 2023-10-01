using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public Sound[] Sounds;
    private Dictionary<SoundType, Sound> _sounds = new Dictionary<SoundType, Sound>();
    [SerializeField] private AudioSource _theme;

    public float _volume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else Destroy(gameObject);
        foreach (Sound sound in Sounds)
        {
            sound.AudioSource = gameObject.AddComponent<AudioSource>();
            sound.AudioSource.playOnAwake = false;
            sound.AudioSource.clip = sound.Clip;
            _sounds.Add(sound.Type, sound);
        }
    }

    private void Start()
    {
        _theme.Play();
    }

    public void SetThemeSpeed(float speed = 1f)
    {
        _theme.pitch = speed;
    }

    public void SetVolume(float volume)
    {
        _volume = volume;
        _theme.volume = volume;
    }

    public void Play(SoundType type, float volume =1f, float pitch=-999f)
    {
        if (pitch == -999f) pitch = Random.Range(0.95f, 1.05f);
        //AudioSource.PlayClipAtPoint(_sounds[type].AudioSource.clip, Camera.main.transform.position);
        _sounds[type].AudioSource.pitch = pitch;
        _sounds[type].AudioSource.volume = volume * _volume;
        _sounds[type].AudioSource.Play();
    }
    public void Play(SoundType type, Vector3 position)
    {
        _sounds[type].AudioSource.pitch = Random.Range(0.95f, 1.05f);
        AudioSource.PlayClipAtPoint(_sounds[type].AudioSource.clip, position);
    }

}