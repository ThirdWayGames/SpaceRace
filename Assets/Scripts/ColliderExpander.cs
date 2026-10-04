using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SphereCollider), typeof(CapsuleCollider))]
public class ColliderExpander : MonoBehaviour
{
    public float TransitionTime;

    public float StartRadius;

    public float FinishRadius;

    public Collider Collider;

    private SphereCollider SphereCollider;

    private CapsuleCollider CapsuleCollider;

    private string ColliderType;

    private bool Expanding;

    public void Awake()
    {
        Expanding = false;

        if (Collider != null)
        {
            SphereCollider = Collider as SphereCollider;
            CapsuleCollider = Collider as CapsuleCollider;
            if (SphereCollider != null)
            {
                ColliderType = "sphere";
                SphereCollider.radius = StartRadius;
            }
            else if (CapsuleCollider != null)
            {
                ColliderType = "capsule";
                CapsuleCollider.radius = StartRadius;
            }
            else
            {
                Debug.LogWarning(string.Format("Collider '{0}' is not compatible with this script.", Collider.name));
                ColliderType = "incompatible";
            }
        }
        else
        {
            ColliderType = "none";
        }
    }

	// Update is called once per frame
	public void Update ()
    {
        StartCoroutine("Expand");
    }

    private IEnumerator Expand()
    {
        switch (ColliderType)
        {
            case "sphere":
                {
                    if (SphereCollider.radius < FinishRadius)
                    {
                        SphereCollider.radius += FinishRadius / TransitionTime;
                        SphereCollider.radius = Mathf.Clamp(SphereCollider.radius, StartRadius, FinishRadius);
                    } else
                    {
                        StopCoroutine("Expand");
                    }

                    break;
                }
            case "capsule":
                {
                    if (CapsuleCollider.radius < FinishRadius)
                    {
                        CapsuleCollider.radius += FinishRadius / TransitionTime;
                        CapsuleCollider.radius = Mathf.Clamp(CapsuleCollider.radius, StartRadius, FinishRadius);
                    }
                    else
                    {
                        StopCoroutine("Expand");
                    }

                    break;
                }
            default:
                break;
        }

        yield return null;
    }
}
