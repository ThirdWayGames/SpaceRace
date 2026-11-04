using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class GalaxyProxy : MonoBehaviour
{
    public const float ProxyHeight = 5000f;

    Transform root;

    Transform storm;

    Transform ship;

    readonly Dictionary<string, Transform> points = new Dictionary<string, Transform>();

    readonly Dictionary<string, Transform> contacts = new Dictionary<string, Transform>();

    public void Start()
    {
        root = new GameObject("GalaxyProxyRoot").transform;
        root.position = new Vector3(0f, ProxyHeight, 0f);
        storm = CreatePrimitive(PrimitiveType.Cylinder, "Storm", new Color(0.35f, 0.15f, 0.45f, 0.35f));
        ship = CreatePrimitive(PrimitiveType.Cube, "Ship", new Color(0.8f, 0.84f, 0.9f));
        var map = GalaxyMap.CreateSample();
        for (int i = 0; i < map.Points.Count; i++)
        {
            var point = map.Points[i];
            var color = point.IsWormhole ? new Color(0.95f, 0.85f, 0.2f) : new Color(0.4f, 0.75f, 0.9f);
            var marker = CreatePrimitive(PrimitiveType.Sphere, point.Id, color);
            marker.localScale = Vector3.one * 4f;
            marker.localPosition = new Vector3(point.X, 1f, point.Z);
            points[point.Id] = marker;
        }
    }

    public void LateUpdate()
    {
        var session = GalaxySessionFinder.Find();
        if (root == null)
        {
            return;
        }

        root.gameObject.SetActive(session != null && session.Snapshot != null);
        if (!root.gameObject.activeSelf)
        {
            return;
        }

        var snapshot = session.Snapshot;
        storm.localScale = new Vector3(snapshot.StormRadius * 2f, 0.02f, snapshot.StormRadius * 2f);
        ship.localPosition = new Vector3(snapshot.X, 2f, snapshot.Z);
        ship.localRotation = Quaternion.Euler(0f, snapshot.HeadingDegrees, 0f);
        ship.localScale = new Vector3(6f, 2f, 10f);
        UpdateContacts(snapshot.ContactSummary);
        AimStationCameras(ship.position, snapshot.HeadingDegrees);
    }

    void UpdateContacts(string summary)
    {
        var seen = new HashSet<string>();
        if (!string.IsNullOrEmpty(summary))
        {
            var records = summary.Split(';');
            for (int i = 0; i < records.Length; i++)
            {
                var fields = records[i].Split(',');
                if (fields.Length < 6)
                {
                    continue;
                }

                float x;
                float z;
                if (!float.TryParse(fields[1], NumberStyles.Float, CultureInfo.InvariantCulture, out x) || !float.TryParse(fields[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z))
                {
                    continue;
                }

                Transform marker;
                if (!contacts.TryGetValue(fields[0], out marker))
                {
                    var color = fields[3] == GalaxyContactKind.Hostile.ToString() ? new Color(0.9f, 0.25f, 0.2f) : new Color(0.95f, 0.55f, 0.1f);
                    marker = CreatePrimitive(PrimitiveType.Sphere, fields[0], color);
                    marker.localScale = Vector3.one * 3f;
                    contacts[fields[0]] = marker;
                }

                marker.localPosition = new Vector3(x, 1.5f, z);
                marker.gameObject.SetActive(fields[4] == "1");
                seen.Add(fields[0]);
            }
        }

        foreach (var pair in contacts)
        {
            if (!seen.Contains(pair.Key))
            {
                pair.Value.gameObject.SetActive(false);
            }
        }
    }

    void AimStationCameras(Vector3 shipPosition, float heading)
    {
        var stations = FindObjectsOfType<CrewStation>();
        var forward = Quaternion.Euler(0f, heading, 0f) * Vector3.forward;
        for (int i = 0; i < stations.Length; i++)
        {
            var anchor = stations[i].CameraAnchor;
            if (anchor == null || stations[i].Role == StationRole.Airlock)
            {
                continue;
            }

            if (stations[i].Role == StationRole.Scanner)
            {
                anchor.position = shipPosition + (Vector3.up * 40f);
                anchor.rotation = Quaternion.LookRotation(Vector3.down, forward);
            }
            else
            {
                anchor.position = shipPosition - (forward * 18f) + (Vector3.up * 8f);
                anchor.LookAt(shipPosition);
            }
        }
    }

    Transform CreatePrimitive(PrimitiveType type, string name, Color color)
    {
        var primitive = GameObject.CreatePrimitive(type);
        primitive.name = name;
        primitive.transform.SetParent(root, false);
        var collider = primitive.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        var renderer = primitive.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        SetGalaxyLayer(primitive);
        return primitive.transform;
    }

    static void SetGalaxyLayer(GameObject target)
    {
        var layer = LayerMask.NameToLayer("GalaxyView");
        if (layer < 0)
        {
            return;
        }

        var transforms = target.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].gameObject.layer = layer;
        }
    }
}
