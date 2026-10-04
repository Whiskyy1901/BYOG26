using System;
using UnityEngine;
using Random = UnityEngine.Random;

// SFX: one sound slot you can fill in from the Inspector
[Serializable]
public class SoundEffect
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Tooltip("Random pitch +/- this much each play. A little stops repeated 8-bit sounds sounding robotic. 0 = always the same pitch.")]
    [Range(0f, 0.5f)] public float pitchVariation = 0.05f;
}

// SFX: plays sounds on its own object, so sounds keep playing even after the object that triggered them is destroyed
// (dying enemies, bullets, pickups). You don't need to put this in the scene; it creates itself the first time a sound plays.
// Put one in the scene only if you want to tweak the master volume or voice count.
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
    [Tooltip("How many sounds can overlap, each with its own pitch.")]
    [SerializeField] private int _voices = 16;

    private AudioSource[] _sources;
    private int _next;
    private static bool _quitting;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => _quitting = false; // for Enter Play Mode without domain reload

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _sources = new AudioSource[Mathf.Max(1, _voices)];
        for (int i = 0; i < _sources.Length; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D, so camera distance never changes the volume
            _sources[i] = source;
        }
    }

    private void OnApplicationQuit() => _quitting = true;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Plays a sound. Does nothing if the slot or its clip is empty, so unassigned sounds are safe.</summary>
    public static void Play(SoundEffect sfx)
    {
        if (sfx == null || sfx.clip == null || _quitting) return;

        if (Instance == null)
            new GameObject("SoundManager").AddComponent<SoundManager>();

        Instance.PlayInternal(sfx);
    }

    private void PlayInternal(SoundEffect sfx)
    {
        AudioSource source = GetFreeSource();
        source.pitch = 1f + Random.Range(-sfx.pitchVariation, sfx.pitchVariation);
        source.PlayOneShot(sfx.clip, sfx.volume * _masterVolume);
    }

    private AudioSource GetFreeSource()
    {
        for (int i = 0; i < _sources.Length; i++)
        {
            int index = (_next + i) % _sources.Length;
            if (!_sources[index].isPlaying)
            {
                _next = (index + 1) % _sources.Length;
                return _sources[index];
            }
        }

        // Every voice is busy: reuse the next one in line
        AudioSource stolen = _sources[_next];
        _next = (_next + 1) % _sources.Length;
        return stolen;
    }
}
