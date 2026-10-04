using Assets.Scripts.Enums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyTargetComponent : MonoBehaviour
{
    public float AltTargetScanRadius = 360f;

    public float AltTargetScanSegments = 360f;

    public List<ThreatLevel> KnownThreats;

    protected AudioSource AudioSource;

    public AudioClip TargetSeenClip;

    public GameObject RayCastFrom;

    //// public GameObject Target;

    public Vector3 LastKnowLocation;

    public float LookRadius = 10f;

    public float ViewAngle = 300f;

    public float targetCooldown = 5f;

    public float lastTimeFoundTarget;

    public float cooldownSpeed = 1f;

    public bool CoverAvailable;

    public GameObject ClosestCover;

    public float TimeTargetIsInView = 0;

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public void Start()
    {
        AudioSource = this.GetComponent<AudioSource>();
    }

    public void PlayAlertClip()
    {
        if (TargetSeenClip != null)
        {
            AudioSource.PlayOneShot(TargetSeenClip);
        }
    }

    public ThreatLevel GetCurrentThreat()
    {
        return KnownThreats.OrderByDescending(x => x.ThreatScore).ThenBy(x => x.Distance).FirstOrDefault();
    }
}
