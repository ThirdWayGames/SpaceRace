using Assets.Scripts.Interfaces;
using UnityEngine;

public class TeleportController : Photon.MonoBehaviour
{
    public bool Active;
    public float RechargeTime = 60.0f;
    public int TeamId;

    public Light ActiveLight;

    public ParticleSystem SpawnSmoke;

    public Color Ready;
    public Color Charging;

    private float CooldownTimer = 0;

    public virtual Vector3 GetSpawnPos()
    {
        return transform.position + new Vector3(0f, 0.1f, 0f);
    }

    public virtual Quaternion GetSpawnRot()
    {
        var rot = transform.rotation;
        rot.x = 0;
        rot.z = 0;
        return rot;
    }

    /// <summary>
    /// Awakes this instance.
    /// </summary>
    public void Awake()
    {
        if (ActiveLight == null)
        {
            Debug.LogWarning(string.Format("No Active Light associated to the {0} script on {1}", this.GetType().Name, transform.name));
        }
    }

    /// <summary>
    /// Updates this instance.
    /// </summary>
    public void Update()
    {
        if (ActiveLight != null)
        {
            ActiveLight.color = IsAvailable() ? Ready : Charging;
        }

        if (!IsAvailable())
        {
            CooldownTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Spawneds the specified player.
    /// </summary>
    /// <param name="player">The player.</param>
    public void Spawned(IPlayerController player)
    {
        CooldownTimer = RechargeTime;

        if (SpawnSmoke != null)
        {
            SpawnSmoke.Play();
        }
    }

    /// <summary>
    /// Determines whether this instance is available.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if this instance is available; otherwise, <c>false</c>.
    /// </returns>
    public bool IsAvailable()
    {
        return CooldownTimer <= 0;
    }
}
