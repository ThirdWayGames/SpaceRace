using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class EscapeShipBootstrap : MonoBehaviour
{
    public const string InteriorName = "ShipInterior";

    public const string SpawnPointName = "SpawnPoint";

    const int SceneViewId = 1;

    bool cameraReleased;

    Camera sceneCamera;

    AudioListener sceneListener;

    public void Awake()
    {
        var interior = BuildInterior();
        var spawns = CreateSpawns(interior.transform);
        var manager = EnsureGameManager();
        EnsurePlayerManager();
        AssignSpawns(manager, spawns);
        EnsureSceneCamera(interior.transform);
        if (FindObjectOfType<ShipGreybox>() == null)
        {
            gameObject.AddComponent<ShipGreybox>();
        }
    }

    public void Update()
    {
        if (cameraReleased || sceneCamera == null)
        {
            return;
        }

        var playerManager = FindObjectOfType<PlayerManager3D>();
        if (playerManager == null || playerManager.LocalPlayerInstance == null)
        {
            return;
        }

        if (!ShouldReleaseSceneCamera(playerManager.LocalPlayerInstance))
        {
            return;
        }

        sceneCamera.enabled = false;
        if (sceneListener != null)
        {
            sceneListener.enabled = false;
        }

        cameraReleased = true;
    }

    public void ConfigureOnFootView()
    {
        EnsureSceneCamera(transform);
    }

    public static bool ShouldReleaseSceneCamera(GameObject player)
    {
        if (player == null)
        {
            return false;
        }

        var cameras = player.GetComponentsInChildren<Camera>(true);
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null && cameras[i].enabled)
            {
                return true;
            }
        }

        return false;
    }

    static GameObject BuildInterior()
    {
        var existing = GameObject.Find(InteriorName);
        if (existing != null)
        {
            return existing;
        }

        var root = new GameObject(InteriorName);
        CreateBox(root.transform, "Floor", new Vector3(0f, 0f, 0f), new Vector3(14f, 0.2f, 14f), new Color(0.22f, 0.24f, 0.28f));
        var ceiling = CreateBox(root.transform, "Ceiling", new Vector3(0f, 3f, 0f), new Vector3(14f, 0.2f, 14f), new Color(0.16f, 0.17f, 0.2f));
        var ceilingRenderer = ceiling.GetComponent<Renderer>();
        if (ceilingRenderer != null)
        {
            ceilingRenderer.enabled = false;
        }

        CreateInteriorLight(root.transform);
        CreateBox(root.transform, "WallNorth", new Vector3(0f, 1.5f, 7f), new Vector3(14f, 3f, 0.2f), new Color(0.28f, 0.32f, 0.36f));
        CreateBox(root.transform, "WallSouth", new Vector3(0f, 1.5f, -7f), new Vector3(14f, 3f, 0.2f), new Color(0.28f, 0.32f, 0.36f));
        CreateBox(root.transform, "WallEast", new Vector3(7f, 1.5f, 0f), new Vector3(0.2f, 3f, 14f), new Color(0.28f, 0.32f, 0.36f));
        CreateBox(root.transform, "WallWest", new Vector3(-7f, 1.5f, 0f), new Vector3(0.2f, 3f, 14f), new Color(0.28f, 0.32f, 0.36f));
        return root;
    }

    static TeleportController[] CreateSpawns(Transform interior)
    {
        var red = CreateSpawn(interior, SpawnPointName, new Vector3(0f, 1f, 0f), 1);
        var blue = CreateSpawn(interior, "SpawnPointBlue", new Vector3(1.5f, 1f, 0f), 2);
        return new[] { red, blue };
    }

    static TeleportController CreateSpawn(Transform interior, string name, Vector3 localPosition, int teamId)
    {
        var existing = interior.Find(name);
        GameObject spawnObject;
        if (existing != null)
        {
            spawnObject = existing.gameObject;
        }
        else
        {
            spawnObject = new GameObject(name);
            spawnObject.transform.SetParent(interior, false);
            spawnObject.transform.localPosition = localPosition;
            spawnObject.transform.localRotation = Quaternion.identity;
        }

        var teleport = spawnObject.GetComponent<TeleportController>();
        if (teleport == null)
        {
            teleport = spawnObject.AddComponent<TeleportController>();
        }

        teleport.TeamId = teamId;
        teleport.RechargeTime = 0f;
        return teleport;
    }

    static GameManager3D EnsureGameManager()
    {
        var existing = FindObjectOfType<GameManager3D>();
        if (existing != null)
        {
            EnsureSceneView(existing.gameObject);
            return existing;
        }

        var prefab = Resources.Load<GameObject>("GameManager");
        if (prefab == null)
        {
            Debug.LogError("EscapeShipBootstrap could not load the GameManager prefab.");
            return null;
        }

        var instance = Instantiate(prefab);
        instance.name = "GameManager";
        EnsureSceneView(instance);
        return instance.GetComponent<GameManager3D>();
    }

    static void EnsureSceneView(GameObject managerObject)
    {
        var view = managerObject.GetComponent<PhotonView>();
        if (view != null && view.viewID == 0)
        {
            view.viewID = SceneViewId;
        }
    }

    static void EnsurePlayerManager()
    {
        if (FindObjectOfType<PlayerManager3D>() != null)
        {
            return;
        }

        var playerManager = new GameObject("PlayerManager");
        playerManager.AddComponent<PlayerManager3D>();
    }

    static void AssignSpawns(GameManager3D manager, TeleportController[] spawns)
    {
        if (manager == null)
        {
            Debug.LogError("EscapeShipBootstrap has no GameManager3D to receive spawn points.");
            return;
        }

        manager.SpawnLocations = spawns;
    }

    void EnsureSceneCamera(Transform interior)
    {
        sceneCamera = GetComponent<Camera>();
        if (sceneCamera == null)
        {
            sceneCamera = gameObject.AddComponent<Camera>();
        }

        sceneListener = GetComponent<AudioListener>();
        if (sceneListener == null)
        {
            sceneListener = gameObject.AddComponent<AudioListener>();
        }

        gameObject.tag = "MainCamera";
        transform.position = interior.position + new Vector3(0f, 8f, -6f);
        transform.rotation = Quaternion.Euler(50f, 0f, 0f);
        sceneCamera.clearFlags = CameraClearFlags.SolidColor;
        sceneCamera.backgroundColor = new Color(0.05f, 0.06f, 0.08f);
        sceneCamera.nearClipPlane = 0.05f;
        sceneCamera.farClipPlane = 80f;
        sceneCamera.depth = 0f;
        sceneCamera.targetDisplay = 0;
        sceneCamera.targetTexture = null;
        var galaxyLayer = LayerMask.NameToLayer("GalaxyView");
        if (galaxyLayer >= 0)
        {
            sceneCamera.cullingMask &= ~(1 << galaxyLayer);
        }

        var follow = GetComponent<CameraFollow3D>();
        if (follow == null)
        {
            follow = gameObject.AddComponent<CameraFollow3D>();
        }

        follow.camTranform = transform;
        follow.LightsToCull = new List<Light>();
        follow.ClampCamY = true;
        follow.FixedCamera = true;
        follow.Y_ANGLE_MIN = 50f;
        follow.Y_ANGLE_MAX = 70f;
        follow.ZOOM_MIN = 6f;
        follow.ZOOM_MAX = 16f;
        follow.CameraXYOffset = new Vector3(0f, 0f, 12f);
    }

    static void CreateInteriorLight(Transform parent)
    {
        var lightObject = new GameObject("InteriorLight");
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.localPosition = new Vector3(0f, 2.4f, 0f);
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 24f;
        light.intensity = 1.5f;
        light.color = new Color(0.85f, 0.9f, 1f);
    }

    static GameObject CreateBox(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPosition;
        cube.transform.localScale = scale;
        var renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        return cube;
    }
}
