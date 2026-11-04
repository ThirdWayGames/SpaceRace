using System.Collections.Generic;
using UnityEngine;

public class ShipGreybox : MonoBehaviour
{
    public void Awake()
    {
        EnsureSession();
        var interior = GameObject.Find(EscapeShipBootstrap.InteriorName);
        if (interior == null)
        {
            interior = new GameObject(EscapeShipBootstrap.InteriorName);
        }

        if (interior.GetComponentInChildren<CrewStation>() == null)
        {
            CreateStation(interior.transform, StationRole.Helm, new Vector3(0f, 1f, 4f), new Color(0.25f, 0.45f, 0.75f));
            CreateStation(interior.transform, StationRole.Scanner, new Vector3(-4f, 1f, 1f), new Color(0.2f, 0.6f, 0.45f));
            CreateStation(interior.transform, StationRole.Turret, new Vector3(4f, 1f, 1f), new Color(0.7f, 0.28f, 0.22f));
            CreateStation(interior.transform, StationRole.Airlock, new Vector3(0f, 1f, -4f), new Color(0.75f, 0.62f, 0.2f));
        }

        if (FindObjectOfType<GalaxyProxy>() == null)
        {
            gameObject.AddComponent<GalaxyProxy>();
        }

        if (FindObjectOfType<ViewRouter>() == null)
        {
            gameObject.AddComponent<ViewRouter>();
        }
    }

    static void EnsureSession()
    {
        if (FindObjectOfType<EscapeSessionHost>() != null && FindObjectOfType<StationBoard>() != null)
        {
            return;
        }

        var session = FindObjectOfType<EscapeSessionHost>();
        GameObject hostObject;
        if (session != null)
        {
            hostObject = session.gameObject;
        }
        else
        {
            hostObject = new GameObject("EscapeSession");
            var host = hostObject.AddComponent<EscapeSessionHost>();
            var view = hostObject.AddComponent<PhotonView>();
            view.viewID = 2;
            view.synchronization = ViewSynchronization.Unreliable;
            view.ObservedComponents = new List<Component>();
            view.ObservedComponents.Add(host);
        }

        if (hostObject.GetComponent<StationBoard>() == null)
        {
            hostObject.AddComponent<StationBoard>();
        }
    }

    static void CreateStation(Transform interior, StationRole role, Vector3 localPosition, Color color)
    {
        var stationObject = new GameObject(role.ToString());
        stationObject.transform.SetParent(interior, false);
        stationObject.transform.localPosition = localPosition;
        var collider = stationObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = new Vector3(0f, 1f, 0f);
        collider.size = new Vector3(1.6f, 2.2f, 1.6f);

        var console = GameObject.CreatePrimitive(PrimitiveType.Cube);
        console.name = "Console";
        console.transform.SetParent(stationObject.transform, false);
        console.transform.localPosition = new Vector3(0f, 1f, 0.7f);
        console.transform.localScale = new Vector3(0.8f, 0.6f, 0.3f);
        var consoleCollider = console.GetComponent<Collider>();
        if (consoleCollider != null)
        {
            Destroy(consoleCollider);
        }

        var renderer = console.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        var seat = new GameObject("Seat").transform;
        seat.SetParent(stationObject.transform, false);
        seat.localPosition = new Vector3(0f, 0f, -0.4f);

        var anchor = new GameObject("CameraAnchor").transform;
        anchor.SetParent(stationObject.transform, false);
        anchor.localPosition = new Vector3(0f, 1.6f, 0f);

        var camera = anchor.gameObject.AddComponent<Camera>();
        camera.enabled = false;
        camera.depth = 20f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 8000f;
        var galaxyLayer = LayerMask.NameToLayer("GalaxyView");
        if (role == StationRole.Airlock || galaxyLayer < 0)
        {
            camera.cullingMask = galaxyLayer < 0 ? camera.cullingMask : ~(1 << galaxyLayer);
        }
        else
        {
            camera.cullingMask = 1 << galaxyLayer;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.03f, 0.06f);
        }

        var station = stationObject.AddComponent<CrewStation>();
        station.Role = role;
        station.StationId = role.ToString();
        station.Seat = seat;
        station.CameraAnchor = anchor;
        station.StationCamera = camera;
    }
}
