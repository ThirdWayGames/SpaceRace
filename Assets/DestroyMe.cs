using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestroyMe : Photon.MonoBehaviour
{
    public float DestroyTimer = 0f;

    protected float CurrentTimer = 0f;

    // List if Dispose call backs
    protected List<IDisposeCallback> DisposeCallbacks;

    public void Awake()
    {
        CurrentTimer = DestroyTimer;
    }

    public void Start()
    {
        // Get a list of any items that implement the dispose callback interface for when this GO is destroyed.
        DisposeCallbacks = GetComponentsInChildren<IDisposeCallback>().ToList();
    }

    // Update is called once per frame
    void Update ()
    {
        CurrentTimer -= Time.deltaTime;

        // If the current timer is <= 0
        if (CurrentTimer <= 0)
        {
            // If we have any GOs to perform a dispose callback on.
            if (DisposeCallbacks != null && DisposeCallbacks.Any())
            {
                // For each GO that implements the IDisposeCallBack interface.
                foreach (var disposeCallBack in DisposeCallbacks)
                {
                    // Call the dispose item method.
                    disposeCallBack.DisposeItem(CurrentTimer);
                }
            }

            // if we are not in a room
            if (photonView == null || !PhotonNetwork.inRoom)
            {
                // Standard destroy
                Destroy(gameObject);
            }
            else
            {
                // If we are in a room and the photon view is mine/
                if (PhotonNetwork.inRoom && photonView.isMine)
                {
                    // Network destroy.
                    PhotonNetwork.Destroy(this.gameObject);
                }
            }
        }
    }
}
