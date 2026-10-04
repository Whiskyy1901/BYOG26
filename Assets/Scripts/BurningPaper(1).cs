using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class BurningPaper : MonoBehaviour
{
    const float StartProgress = -0.02f;
    const float EndProgress = 1.001f;

    [Header("Timing")]
    public float burnDuration = 120f;
    public AnimationCurve progressCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public bool burnOnStart = false;

    [Header("Burn Shape")]
    public int maskResolution = 256;
    [Range(0f, 1f)] public float noiseStrength = 0.5f;
    public float noiseScale = 3f;
    [Range(1, 6)] public int noiseOctaves = 3;
    [Tooltip("0 = random pattern every play")]
    public int seed = 0;

    [Header("Look")]
    public float charWidth = 0.06f;

    [Header("Events")]
    public UnityEvent onPaperBurned;

    public float Progress { get; private set; }
    public float NormalizedTime => Mathf.Clamp01(elapsed / burnDuration);
    public float TimeRemaining => Mathf.Max(0f, burnDuration - elapsed);
    public bool IsBurning { get; private set; }
    public bool IsFullyBurned { get; private set; }
    public float SpeedMultiplier { get; set; } = 1f;

    public float RemainingFraction
    {
        get
        {
            if (sortedValues == null || sortedValues.Length == 0) return 0f;
            int burned = LowerBound(sortedValues, Progress);
            return 1f - (float)burned / sortedValues.Length;
        }
    }

    SpriteRenderer sr;
    MaterialPropertyBlock mpb;
    Texture2D mask;
    float[] burnValues;
    float[] sortedValues;
    int maskWidth, maskHeight;
    float elapsed;

    static readonly int BurnMaskID = Shader.PropertyToID("_BurnMask");
    static readonly int ProgressID = Shader.PropertyToID("_Progress");
    static readonly int CharWidthID = Shader.PropertyToID("_CharWidth");

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();
        GenerateMask();
        SetProgress(StartProgress);
    }

    void Start()
    {
        if (burnOnStart) StartBurning();
    }

    void Update()
    {
        if (!IsBurning) return;

        elapsed += Time.deltaTime * SpeedMultiplier;
        SetProgress(Mathf.Lerp(StartProgress, EndProgress, progressCurve.Evaluate(NormalizedTime)));

        if (elapsed >= burnDuration)
        {
            IsBurning = false;
            IsFullyBurned = true;
            onPaperBurned.Invoke();
        }
    }

    public void StartBurning()
    {
        if (!IsFullyBurned) IsBurning = true;
    }

    public void Pause()
    {
        IsBurning = false;
    }

    public void ResetBurn(bool newPattern = true)
    {
        elapsed = 0f;
        IsBurning = false;
        IsFullyBurned = false;
        if (newPattern) GenerateMask();
        SetProgress(StartProgress);
    }

    public bool IsBurnedAt(Vector2 worldPos)
    {
        if (IsFullyBurned) return true;

        Vector2 local = transform.InverseTransformPoint(worldPos);
        Bounds b = sr.sprite.bounds;
        float u = (local.x - b.min.x) / b.size.x;
        float v = (local.y - b.min.y) / b.size.y;
        if (u < 0f || u > 1f || v < 0f || v > 1f) return true;

        int x = Mathf.Clamp((int)(u * maskWidth), 0, maskWidth - 1);
        int y = Mathf.Clamp((int)(v * maskHeight), 0, maskHeight - 1);
        return burnValues[y * maskWidth + x] <= Progress;
    }

    void SetProgress(float value)
    {
        Progress = value;
        sr.GetPropertyBlock(mpb);
        mpb.SetTexture(BurnMaskID, mask);
        mpb.SetFloat(ProgressID, Progress);
        mpb.SetFloat(CharWidthID, charWidth);
        sr.SetPropertyBlock(mpb);
    }

    void GenerateMask()
    {
        Vector2 size = sr.sprite.bounds.size;
        maskWidth = Mathf.Max(2, maskResolution);
        maskHeight = Mathf.Max(2, Mathf.RoundToInt(maskResolution * size.y / size.x));
        float minSide = Mathf.Min(size.x, size.y);

        var rng = new System.Random(seed == 0 ? Random.Range(1, int.MaxValue) : seed);
        Vector2 offset = new Vector2((float)rng.NextDouble() * 1000f, (float)rng.NextDouble() * 1000f);

        burnValues = new float[maskWidth * maskHeight];
        float max = 0f;

        for (int y = 0; y < maskHeight; y++)
        {
            for (int x = 0; x < maskWidth; x++)
            {
                float u = (x + 0.5f) / maskWidth;
                float v = (y + 0.5f) / maskHeight;

                float dx = Mathf.Min(u, 1f - u) * size.x;
                float dy = Mathf.Min(v, 1f - v) * size.y;
                float dist = Mathf.Min(dx, dy);

                float nx = u * size.x / minSide * noiseScale + offset.x;
                float ny = v * size.y / minSide * noiseScale + offset.y;
                float value = dist * Mathf.Lerp(1f - noiseStrength, 1f + noiseStrength, FractalNoise(nx, ny));

                burnValues[y * maskWidth + x] = value;
                if (value > max) max = value;
            }
        }

        var pixels = new Color[burnValues.Length];
        for (int i = 0; i < burnValues.Length; i++)
        {
            burnValues[i] /= max;
            pixels[i] = new Color(burnValues[i], 0f, 0f, 1f);
        }

        sortedValues = (float[])burnValues.Clone();
        System.Array.Sort(sortedValues);

        if (mask != null) Destroy(mask);
        mask = new Texture2D(maskWidth, maskHeight, TextureFormat.RFloat, false, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            name = "BurnMask"
        };
        mask.SetPixels(pixels);
        mask.Apply(false, true);
    }

    float FractalNoise(float x, float y)
    {
        float sum = 0f, amp = 1f, freq = 1f, norm = 0f;
        for (int i = 0; i < noiseOctaves; i++)
        {
            sum += Mathf.PerlinNoise(x * freq, y * freq) * amp;
            norm += amp;
            amp *= 0.5f;
            freq *= 2f;
        }
        return Mathf.Clamp01(sum / norm);
    }

    static int LowerBound(float[] arr, float value)
    {
        int lo = 0, hi = arr.Length;
        while (lo < hi)
        {
            int mid = (lo + hi) / 2;
            if (arr[mid] <= value) lo = mid + 1;
            else hi = mid;
        }
        return lo;
    }

    void OnDestroy()
    {
        if (mask != null) Destroy(mask);
    }
}
