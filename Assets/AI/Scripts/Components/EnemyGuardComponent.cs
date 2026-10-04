using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGuardComponent : MonoBehaviour
{
    void Start()
    {
        if (GuardLocation != null)
        {
            LookAtPos = GuardLocation.transform.GetChild(0).position;
        }
    }

    public GameObject GuardLocation;

    public Vector3 LookAtPos;

    public bool InPosition;
}
