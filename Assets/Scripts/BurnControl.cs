using UnityEngine;

public class BurnControl : MonoBehaviour
{
    static readonly int DissolveAmountID = Shader.PropertyToID("_DissolveAmount");
    static readonly int DissolveScaleID = Shader.PropertyToID("_DissolveScale");
    static readonly int NoiseStrengthID = Shader.PropertyToID("_Noise_Strength");

    const float SecondsPerFullBurn = 60f;

    [Tooltip("1 = normal burn (1 minute), 2 = twice as fast, 0.5 = half as fast")]
    [SerializeField] private float burnRate = 1f;
    [SerializeField] private float startAmount = 0f;
    [SerializeField] private float endAmount = 1f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = false;

    [Header("Loop Noise")]  
    [Tooltip("How far Dissolve Scale can move from its starting value each loop")]
    [SerializeField] private float scaleVariation = 3f;
    [Tooltip("How far Noise Strength can move from its starting value each loop")]
    [SerializeField] private float strengthVariation = 0.05f;

    public float Amount { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool IsFinished { get; private set; }
    public bool IsReversing => direction < 0;

    public float SpeedMultiplier = 1f;

    float ConvertedBurnRate =>
        (endAmount - startAmount) / SecondsPerFullBurn * Mathf.Max(burnRate, 0f);
    public float CurrentRate => ConvertedBurnRate * SpeedMultiplier * direction;

    Renderer rend;
    MaterialPropertyBlock mpb;
    float baseScale;
    float baseStrength;
    int direction = 1;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();

        Material mat = rend.sharedMaterial;
        baseScale = mat.GetFloat(DissolveScaleID);
        baseStrength = mat.GetFloat(NoiseStrengthID);

        SetAmount(startAmount);
    }

    void Start()
    {
        if (playOnStart) Play();
    }

    void Update()
    {
        if (!IsPlaying) return;

        float min = Mathf.Min(startAmount, endAmount);
        float max = Mathf.Max(startAmount, endAmount);
        SetAmount(Mathf.Clamp(Amount + CurrentRate * Time.deltaTime, min, max));

        if (direction > 0 && Mathf.Approximately(Amount, endAmount))
        {
            if (loop)
            {
                direction = -1;
            }
            else
            {
                IsFinished = true;
                IsPlaying = false;
            }
        }
        else if (direction < 0 && Mathf.Approximately(Amount, startAmount))
        {
            RandomizeNoise();
            direction = 1;
        }
    }

    public void Play()
    {
        if (!IsFinished) IsPlaying = true;
    }

    public void Pause()
    {
        IsPlaying = false;
    }

    public void ResetDissolve()
    {
        IsPlaying = false;
        IsFinished = false;
        direction = 1;
        SetAmount(startAmount);
    }

    public void RandomizeNoise()
    {
        float scale = Mathf.Max(0f, baseScale + Random.Range(-scaleVariation, scaleVariation));
        float strength = Mathf.Clamp01(baseStrength + Random.Range(-strengthVariation, strengthVariation));

        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(DissolveScaleID, scale);
        mpb.SetFloat(NoiseStrengthID, strength);
        rend.SetPropertyBlock(mpb);
    }

    public void SetAmount(float amount)
    {
        Amount = amount;
        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(DissolveAmountID, Amount);
        rend.SetPropertyBlock(mpb);
    }
}