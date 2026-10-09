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

    public const float ScopePanSpeed = 24f;

    private Camera cam;
    private float CurrentX = 0f;
    private float CurrentY = 0f;
    private bool scoped;
    private float savedZoom;
    private float scopeZoom = 6f;
    private Vector3 scopePan;

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
            CameraXYOffset.z = scopeZoom;
        }

        // Apply Mouse zoom. The wheel changes scope magnification while aimed.
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
        if (myTarget == null)
        {
            return;
        }

        if (cam == null)
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = Camera.main;
            }
        }

        var rotation = AimRotation();
        if (scoped && cam != null)
        {
            PanWhileScoped(rotation);
        }

        Place(rotation);
        if (scoped)
        {
            SniperScopeView.Refresh();
        }
    }

    Quaternion AimRotation()
    {
        if (LinkedToTargetRot && myTarget != null)
        {
            return myTarget.rotation * Quaternion.Euler(CurrentY, 0, 0);
        }

        return Quaternion.Euler(CurrentY, CurrentX, 0);
    }

    public Vector3 BirdseyeOffset()
    {
        return AimRotation() * new Vector3(CameraXYOffset.x, CameraXYOffset.y, -CameraXYOffset.z);
    }

    void Place(Quaternion rotation)
    {
        var look = myTarget.position + scopePan;
        camTranform.position = look + rotation * new Vector3(CameraXYOffset.x, CameraXYOffset.y, -CameraXYOffset.z);
        camTranform.LookAt(look);
    }

    void PanWhileScoped(Quaternion rotation)
    {
        var margin = ScopeEdgeMargin(Screen.width, Screen.height, SniperScopeView.LensRadius);
        var push = EdgePush(Input.mousePosition, Screen.width, Screen.height, margin);
        if (push.sqrMagnitude < 0.001f)
        {
            return;
        }

        var step = ScopePanSpeed * Time.deltaTime;
        var right = rotation * Vector3.right;
        var forward = rotation * Vector3.forward;
        TryPan(ScopePanDelta(new Vector2(push.x, 0f), right, forward, step), rotation, margin);
        TryPan(ScopePanDelta(new Vector2(0f, push.y), right, forward, step), rotation, margin);
    }

    void TryPan(Vector3 delta, Quaternion rotation, float margin)
    {
        if (delta.sqrMagnitude < 0.0000001f)
        {
            return;
        }

        scopePan += delta;
        Place(rotation);
        var screen = cam.WorldToScreenPoint(myTarget.position);
        var inside = screen.z > 0f && PlayerInsideFrame(new Vector2(screen.x, screen.y), Screen.width, Screen.height, margin);
        if (!inside)
        {
            scopePan -= delta;
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
    /// Scoping opens the world camera out to the widest allowed distance. The magnified picture stays in the reticle.
    /// </summary>
    public static float ScopedDistance(float current, float pullback, float min, float max)
    {
        if (max < min)
        {
            return current < min ? min : current;
        }

        return max;
    }

    /// <summary>
    /// Push from a screen edge. Positive X scrolls the view to the right; positive Y scrolls it upward.
    /// </summary>
    public static Vector2 EdgePush(Vector2 mouse, float screenW, float screenH, float margin)
    {
        var push = Vector2.zero;
        if (margin < 0f)
        {
            margin = 0f;
        }

        if (mouse.x <= margin)
        {
            push.x = -1f;
        }
        else if (mouse.x >= screenW - margin)
        {
            push.x = 1f;
        }

        if (mouse.y <= margin)
        {
            push.y = -1f;
        }
        else if (mouse.y >= screenH - margin)
        {
            push.y = 1f;
        }

        return push;
    }

    public static float ScopeEdgeMargin(float screenW, float screenH, float scopeRadius)
    {
        var shorter = screenW < screenH ? screenW : screenH;
        if (shorter < 16f)
        {
            return 8f;
        }

        var margin = scopeRadius;
        var cap = shorter * 0.4f;
        if (margin > cap)
        {
            margin = cap;
        }

        if (margin < 8f)
        {
            margin = 8f;
        }

        return margin;
    }

    public static bool PlayerInsideFrame(Vector2 screen, float screenW, float screenH, float margin)
    {
        const float slack = 1.5f;
        return screen.x >= margin - slack
            && screen.x <= screenW - margin + slack
            && screen.y >= margin - slack
            && screen.y <= screenH - margin + slack;
    }

    public static Vector3 ScopePanDelta(Vector2 push, Vector3 cameraRight, Vector3 cameraForward, float distance)
    {
        var right = cameraRight;
        right.y = 0f;
        if (right.sqrMagnitude < 0.0001f)
        {
            right = Vector3.right;
        }

        var forward = cameraForward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.forward;
        }

        return right.normalized * (push.x * distance) + forward.normalized * (push.y * distance);
    }

    public void SetScoped(bool on, float pullback)
    {
        if (on)
        {
            if (!scoped)
            {
                savedZoom = CameraXYOffset.z;
                scoped = true;
                scopePan = Vector3.zero;
            }

            scopeZoom = ScopedDistance(savedZoom, pullback, ZOOM_MIN, ZOOM_MAX);
            CameraXYOffset.z = scopeZoom;
            return;
        }

        if (!scoped)
        {
            return;
        }

        scoped = false;
        scopePan = Vector3.zero;
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
