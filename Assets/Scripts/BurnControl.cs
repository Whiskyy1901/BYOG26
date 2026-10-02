using UnityEngine;

public class BurnControl : MonoBehaviour
{
    static readonly int DissolveAmountID = Shader.PropertyToID("_DissolveAmount");
 
    public float duration = 60f;
    public float startAmount = 0f;
    public float endAmount = 1f;
    public bool playOnStart = true;
 
    public float Amount { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool IsFinished { get; private set; }
    public float SpeedMultiplier { get; set; } = 1f;
 
    Renderer rend;
    MaterialPropertyBlock mpb;
    float elapsed;
 
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
 
        elapsed += Time.deltaTime * SpeedMultiplier;
        float t = Mathf.Clamp01(elapsed / duration);
        SetAmount(Mathf.Lerp(startAmount, endAmount, t));
 
        if (t >= 1f)
        {
            IsPlaying = false;
            IsFinished = true;
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
        elapsed = 0f;
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
