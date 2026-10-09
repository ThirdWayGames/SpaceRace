using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraFollow3D : MonoBehaviour
{
    public List<Light> LightsToCull;

    public Transform myTarget;
    public Transform camTranform;
    public float SensivityX = 1.5f;
    public float SensivityY = 1.5f;
    public float SensivityScroll = 1.5f;
    public bool ClampCamY = true;
    public float Y_ANGLE_MIN = 45f;
    public float Y_ANGLE_MAX = 85f;
    public float ZOOM_MIN = 3f;
    public float ZOOM_MAX = 19f;
    public bool LinkedToTargetRot;
    public Vector3 CameraXYOffset = new Vector3(0, 0, 10f);
    public bool FixedCamera = true;

    private Camera cam;
    private float CurrentX = 0f;
    private float CurrentY = 0f;
    private bool scoped;
    private float savedZoom;
    private float scopeZoom = 6f;

    public void Start()
    {
        LinkedToTargetRot = false;
        camTranform = transform;
        cam = Camera.main;
    }

    public void Update()
    {
        //// LinkedToTargetRot = !Input.GetKey(KeyCode.LeftAlt);
        if (!FixedCamera)
        {
            CurrentX += Input.GetAxis("Mouse X") * SensivityX;
            CurrentY += Input.GetAxis("Mouse Y") * SensivityY;
        }

        if (ClampCamY)
        {
            CurrentY = Mathf.Clamp(CurrentY, Y_ANGLE_MIN, Y_ANGLE_MAX);
        }

        if (scoped)
        {
            CameraXYOffset.z = Mathf.Lerp(CameraXYOffset.z, scopeZoom, 8f * Time.deltaTime);
        }

        // Apply Mouse zoom.
        if (!scoped && Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            // Adjust the orthographic size of the camera.
            var tempDist = CameraXYOffset.z - Input.GetAxis("Mouse ScrollWheel") * SensivityScroll;

            // Make sure the camera can't zoom in or out too far.
            CameraXYOffset.z = Mathf.Clamp(tempDist, ZOOM_MIN, ZOOM_MAX);
        }
    }

    public void LateUpdate()
    {
        if (myTarget != null)
        {
            var direction = new Vector3(CameraXYOffset.x, CameraXYOffset.y, -CameraXYOffset.z);
            Quaternion rotation;
            if (LinkedToTargetRot)
            {
                rotation = myTarget.rotation*Quaternion.Euler(CurrentY, 0, 0);
            }
            else
            {
                rotation = Quaternion.Euler(CurrentY, CurrentX, 0);
            }

            camTranform.position = myTarget.position + rotation * direction;
            camTranform.LookAt(myTarget);
        }
    }

    public void OnPreCull()
    {
        foreach (var light in LightsToCull.Where(x => x != null && x.gameObject != null))
        {
            light.gameObject.SetActive(false);
        }
    }

    public void OnPreRender()
    {
        foreach (var light in LightsToCull.Where(x => x != null && x.gameObject != null))
        {
            light.gameObject.SetActive(false);
        }
    }

    public void OnPostRender()
    {
        foreach (var light in LightsToCull.Where(x => x != null && x.gameObject != null))
        {
            light.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Sets the cameras target.
    /// </summary>
    /// <param name="transform">The transform to focus the camera on.</param>
    public void SetScoped(bool on, float zoom)
    {
        if (on)
        {
            if (!scoped)
            {
                savedZoom = CameraXYOffset.z;
                scoped = true;
            }

            scopeZoom = zoom;
            return;
        }

        if (!scoped)
        {
            return;
        }

        scoped = false;
        CameraXYOffset.z = savedZoom;
    }

    public void SetTarget(Transform transform)
    {
        // Set the target by default.
        var setTarget = true;

        // Get the targets photon view.
        var targetPhotonView = transform.GetComponent<PhotonView>();

        // If the phon view is not null
        if (targetPhotonView != null)
        {
            // Set the setTarget value based on if the PV belongs to the client.
            setTarget = targetPhotonView.isMine;
        }

        // If the target is mine or we are not in a room.
        if (setTarget || !PhotonNetwork.inRoom)
        {
            // Set the target to be me.
            myTarget = transform;

            // disable the audio listener on the main camera.
            var camAudioListener = GetComponent<AudioListener>();
            if (camAudioListener != null)
            {
                camAudioListener.enabled = false;
            }

            // enable my audio listener.
            var myAudioListener = myTarget.GetComponentInChildren<AudioListener>();
            if (myAudioListener != null)
            {
                myAudioListener.enabled = true;
            }
        }
    }
}
