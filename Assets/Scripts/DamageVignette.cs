using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class DamageVignette : MonoBehaviour
{
    public static DamageVignette Instance { get; private set; }

    [SerializeField] private Color _hitColor = new Color(0.8f, 0f, 0f, 1f);
    [SerializeField] private float _maxPulse = 0.6f;        // most extra intensity a hit can add on top of the resting value
    [SerializeField] private float _decayPerSecond = 1.5f;  // how fast it returns to normal

    private Vignette _vignette;
    private Color _baseColor;
    private float _baseIntensity;
    private float _pulse;

    private void Awake()
    {
        Instance = this;

        var volume = GetComponent<Volume>();
        if (!volume.profile.TryGet(out _vignette))
        {
            Debug.LogWarning("DamageVignette: add a Vignette override to this Volume's profile.", this);
            enabled = false;
            return;
        }

        // Store whatever you set in the profile as the "normal" look
        _baseColor = _vignette.color.value;
        _baseIntensity = _vignette.intensity.value;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Pulse(float strength)
    {
        _pulse = Mathf.Min(_maxPulse, _pulse + strength);
    }

    private void Update()
    {
        if (_pulse <= 0f) return;

        _pulse = Mathf.MoveTowards(_pulse, 0f, _decayPerSecond * Time.deltaTime);

        float t = _pulse / _maxPulse; // 1 = full hit look, 0 = back to normal
        _vignette.intensity.Override(_baseIntensity + _pulse);
        _vignette.color.Override(Color.Lerp(_baseColor, _hitColor, t));
    }
}