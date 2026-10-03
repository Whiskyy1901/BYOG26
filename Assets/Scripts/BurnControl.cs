using UnityEngine;

public class BurnControl : MonoBehaviour
{
    static readonly int DissolveAmountID = Shader.PropertyToID("_DissolveAmount");

    const float SecondsPerFullBurn = 60f; // Time for a full burn at burnRate = 1

    [Tooltip("1 = normal burn (1 minute), 2 = twice as fast, 0.5 = half as fast")]
    [SerializeField] private float burnRate = 1f;
    [SerializeField] private float startAmount = 0f;
    [SerializeField] private float endAmount = 1f;
    [SerializeField] private bool playOnStart = true;

    public float Amount { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool IsFinished { get; private set; }

    public float SpeedMultiplier = 1f;

    float ConvertedBurnRate =>
        (endAmount - startAmount) / SecondsPerFullBurn * Mathf.Max(burnRate, 0f);
    public float CurrentRate => ConvertedBurnRate * SpeedMultiplier;

    Renderer rend;
    MaterialPropertyBlock mpb;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
        SetAmount(startAmount);
    }

    void Start()
    {
        if (playOnStart) Play();
    }

    void Update()
    {
        if (!IsPlaying) return;

        float step = ConvertedBurnRate * SpeedMultiplier * Time.deltaTime;
        float newAmount = Amount + step;

        // Clamp between start and end, whichever order they're in
        float min = Mathf.Min(startAmount, endAmount);
        float max = Mathf.Max(startAmount, endAmount);
        SetAmount(Mathf.Clamp(newAmount, min, max));

        // Finished only when the end is reached; reversing back to start just stops at start
        IsFinished = Mathf.Approximately(Amount, endAmount);
        if (IsFinished) IsPlaying = false;
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
        SetAmount(startAmount);
    }

    public void SetAmount(float amount)
    {
        Amount = amount;
        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(DissolveAmountID, Amount);
        rend.SetPropertyBlock(mpb);
    }
}