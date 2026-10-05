using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestroyMe : Photon.MonoBehaviour
{
    public float DestroyTimer = 0f;

    protected float CurrentTimer = 0f;

    // List if Dispose call backs
    protected List<IDisposeCallback> DisposeCallbacks;

    bool expired;

    bool disposeInvoked;

    bool destroyRequested;

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
    void Update()
    {
        if (!expired)
        {
            CurrentTimer -= Time.deltaTime;

            if (CurrentTimer > 0f)
            {
                return;
            }

            expired = true;
            InvokeDispose();
        }

        TryDestroy();
    }

    void InvokeDispose()
    {
        if (disposeInvoked || DisposeCallbacks == null)
        {
            return;
        }

        disposeInvoked = true;

        for (int i = 0; i < DisposeCallbacks.Count; i++)
        {
            var disposeCallBack = DisposeCallbacks[i];
            if (disposeCallBack != null)
            {
                disposeCallBack.DisposeItem(CurrentTimer);
            }
        }
    }

    void TryDestroy()
    {
        if (destroyRequested)
        {
            return;
        }

        var view = photonView;
        var action = FlareLighting.ChooseDestroy(PhotonNetwork.inRoom, view != null, view != null && view.isMine, view != null ? view.instantiationId : 0);
        if (action == TimedDestroyAction.WaitForOwner)
        {
            return;
        }

        destroyRequested = true;
        if (action == TimedDestroyAction.NetworkDestroy)
        {
            PhotonNetwork.Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }

    public static TimedDestroyAction ChooseDestroy(bool inRoom, bool hasPhotonView, bool isMine, int instantiationId)
    {
        return FlareLighting.ChooseDestroy(inRoom, hasPhotonView, isMine, instantiationId);
    }
}
