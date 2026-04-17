using System.Collections;
using UnityEngine;

public class PlayerIgniteAbility : MonoBehaviour
{
    [Header("Cooldown")]
    public float cooldown = 3f;

    [Header("Pulse Settings")]
    public float pulse1Radius = 2f;
    public float pulse2Radius = 4f;
    public float pulse3Radius = 6f;
    public float delayBetweenPulses = 0.2f;
    public float vfxScaleMultiplier = 0.2f;

    [Header("Detection")]
    public LayerMask hitLayers = ~0;

    [Header("VFX")]
    public GameObject ignitePulseVFXPrefab;
    public float vfxHeightOffset = 0f;

    private float _nextUseTime = 0f;

    //Player ability activation with cooldown
    public void TryActivate()
    {
        if (Time.time < _nextUseTime)
            return;

        _nextUseTime = Time.time + cooldown;
        Vector3 castPosition = transform.position;
        StartCoroutine(IgniteRoutine(castPosition));
    }

    //Player ability VFX
    private void SpawnPulseVFX(Vector3 castPosition, float radius)
    {
        if (ignitePulseVFXPrefab == null)
            return;

        Vector3 spawnPosition = castPosition + Vector3.up * vfxHeightOffset;

        GameObject vfx = Instantiate(ignitePulseVFXPrefab, spawnPosition, Quaternion.identity);

        float diameter = radius * 2f * vfxScaleMultiplier;
        vfx.transform.localScale = new Vector3(diameter, 1f, diameter);

        Destroy(vfx, 3f);
    }

    // Creates 3 timed explosions. Aspects are seralized for balance control
    private IEnumerator IgniteRoutine(Vector3 castPosition)
    {
        SpawnPulseVFX(castPosition, pulse1Radius);
        DoIgnitePulse(castPosition, pulse1Radius);
        yield return new WaitForSeconds(delayBetweenPulses);

        SpawnPulseVFX(castPosition, pulse2Radius);
        DoIgnitePulse(castPosition, pulse2Radius);
        yield return new WaitForSeconds(delayBetweenPulses);

        SpawnPulseVFX(castPosition, pulse3Radius);
        DoIgnitePulse(castPosition, pulse3Radius);
    }

    private void DoIgnitePulse(Vector3 castPosition, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(castPosition, radius, hitLayers);

        //Ignite output signal

        for (int i = 0; i < hits.Length; i++)
        {
            hits[i].SendMessage("Ignite", SendMessageOptions.DontRequireReceiver);
        }
    }
}
