using System;
using UnityEngine;

[RequireComponent(typeof(BurnControl), typeof(SpriteRenderer))]
public class BurnZone : MonoBehaviour
{
    static readonly int DissolveScaleID = Shader.PropertyToID("_DissolveScale");
    static readonly int NoiseStrengthID = Shader.PropertyToID("_Noise_Strength");
    static readonly int DistanceMultiplierID = Shader.PropertyToID("_Distance_multiplier");

    [Header("Debug (Scene view, play mode)")]
    [SerializeField] private bool drawDebugGrid = true;
    [SerializeField] private Vector2Int debugGridSize = new Vector2Int(48, 28);
    [SerializeField] private float debugDotSize = 0.08f;
    
    [Header("Player Damage")]
    [SerializeField] private float _playerDPS = 1f;
    private Health _playerHealth;
    private float _lastDamageTime;

    private BurnControl _burn;
    private SpriteRenderer _sprite;

    private void Awake()
    {
        _burn = GetComponent<BurnControl>();
        _sprite = GetComponent<SpriteRenderer>();
        _playerHealth = FindAnyObjectByType<PlayerMovement>().gameObject.GetComponent<Health>();
    }

    private void Update()
    {
        _lastDamageTime += Time.deltaTime;

        if (IsBurnt(_playerHealth.transform.position) && _lastDamageTime >= 1)
        {
            _playerHealth.Damage(_playerDPS);
            _lastDamageTime = 0;
        }
    }

    // True when the point is on burnt ground (in the fire band or past it).
    // Points outside the sprite count as burnt too.
    public bool IsBurnt(Vector2 worldPos)
    {
        return BurnValue(worldPos) > 1f - _burn.Amount;
    }

    // The same value the shader compares against (1 - DissolveAmount).
    public float BurnValue(Vector2 worldPos)
    {
        Vector2 uv = WorldToUV(worldPos);
        Material mat = _sprite.sharedMaterial;

        float distance = Vector2.Distance(uv, new Vector2(0.5f, 0.5f)) * mat.GetFloat(DistanceMultiplierID);
        float noise = SimpleNoise(uv, mat.GetFloat(DissolveScaleID)) * mat.GetFloat(NoiseStrengthID);
        return distance + noise;
    }

    private Vector2 WorldToUV(Vector2 worldPos)
    {
        Vector3 local = transform.InverseTransformPoint(worldPos);
        Bounds b = _sprite.sprite.bounds; // sprite size in local space
        return new Vector2((local.x - b.min.x) / b.size.x, (local.y - b.min.y) / b.size.y);
    }

    // ---- C# copy of Shader Graph's Simple Noise (3 octaves) ----

    private static float SimpleNoise(Vector2 uv, float scale)
    {
        float result = 0f;
        for (int k = 0; k < 3; k++)
        {
            float freq = Mathf.Pow(2f, k);
            float amp = Mathf.Pow(0.5f, 3 - k);
            result += ValueNoise(uv * (scale / freq)) * amp;
        }
        return result;
    }

    private static float ValueNoise(Vector2 uv)
    {
        float fx = Mathf.Floor(uv.x);
        float fy = Mathf.Floor(uv.y);
        float tx = uv.x - fx;
        float ty = uv.y - fy;

        // smoothstep curve, same as the shader
        tx = tx * tx * (3f - 2f * tx);
        ty = ty * ty * (3f - 2f * ty);

        int ix = (int)fx;
        int iy = (int)fy;

        float r0 = Hash(ix, iy);
        float r1 = Hash(ix + 1, iy);
        float r2 = Hash(ix, iy + 1);
        float r3 = Hash(ix + 1, iy + 1);

        float bottom = Mathf.Lerp(r0, r1, tx);
        float top = Mathf.Lerp(r2, r3, tx);
        return Mathf.Lerp(bottom, top, ty);
    }

    // Port of Unity's Hash_Tchou_2_1 (Hashes.hlsl).
    private static float Hash(int x, int y)
    {
        unchecked
        {
            uint vx = (uint)x;
            uint vy = (uint)y;
            vy ^= 1103515245u;
            vx += vy;
            vx *= vy;
            vx ^= vx >> 5;
            vx *= 0x27d4eb2du;
            return vx * (1f / 4294967295f);
        }
    }

    // ---- Debug view: green = safe, red = burnt ----

    private void OnDrawGizmos()
    {
        if (!drawDebugGrid || !Application.isPlaying || _burn == null || _sprite == null) return;

        Bounds b = _sprite.bounds; // world-space bounds
        for (int x = 0; x < debugGridSize.x; x++)
        {
            for (int y = 0; y < debugGridSize.y; y++)
            {
                Vector2 p = new Vector2(
                    Mathf.Lerp(b.min.x, b.max.x, (x + 0.5f) / debugGridSize.x),
                    Mathf.Lerp(b.min.y, b.max.y, (y + 0.5f) / debugGridSize.y));

                Gizmos.color = IsBurnt(p) ? Color.red : Color.green;
                Gizmos.DrawSphere(p, debugDotSize);
            }
        }
    }
}