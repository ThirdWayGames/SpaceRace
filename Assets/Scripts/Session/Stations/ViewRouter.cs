using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class ViewRouter : MonoBehaviour
{
    string engagedStation = string.Empty;

    readonly List<Camera> playerCameras = new List<Camera>();

    readonly List<AudioListener> playerListeners = new List<AudioListener>();

    BasePlayerController3D engagedPlayer;

    public void Update()
    {
        var standing = LocalPlayer();
        if (standing != null)
        {
            var cameras = standing.GetComponentsInChildren<Camera>(true);
            for (int i = 0; i < cameras.Length; i++)
            {
                StripGalaxyLayer(cameras[i]);
            }
        }

        if (Camera.main != null)
        {
            StripGalaxyLayer(Camera.main);
        }

        var board = StationBoard.FindBoard();
        var stationId = board == null ? string.Empty : board.Occupancy.StationFor(StationBoard.LocalActorId());
        if (stationId == engagedStation && engagedPlayer != null)
        {
            Glue(CrewStation.Find(stationId));
            return;
        }

        Release();
        engagedStation = stationId;
        if (string.IsNullOrEmpty(stationId))
        {
            return;
        }

        Engage(CrewStation.Find(stationId));
    }

    void Engage(CrewStation station)
    {
        if (station == null)
        {
            engagedStation = string.Empty;
            return;
        }

        var player = LocalPlayer();
        if (player == null)
        {
            engagedStation = string.Empty;
            return;
        }

        engagedPlayer = player;
        player.SetDisableMovement(true);
        RememberPlayerViews(player.gameObject);
        SetPlayerViews(false);
        if (station.StationCamera != null)
        {
            station.StationCamera.enabled = true;
            var listener = station.StationCamera.GetComponent<AudioListener>();
            if (listener == null)
            {
                listener = station.StationCamera.gameObject.AddComponent<AudioListener>();
            }

            listener.enabled = true;
        }

        Glue(station);
    }

    void Release()
    {
        if (engagedPlayer != null)
        {
            engagedPlayer.SetDisableMovement(false);
        }

        SetPlayerViews(true);
        if (!string.IsNullOrEmpty(engagedStation))
        {
            var station = CrewStation.Find(engagedStation);
            if (station != null && station.StationCamera != null)
            {
                station.StationCamera.enabled = false;
                var listener = station.StationCamera.GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = false;
                }
            }
        }

        engagedPlayer = null;
        engagedStation = string.Empty;
        playerCameras.Clear();
        playerListeners.Clear();
    }

    void Glue(CrewStation station)
    {
        if (station == null || station.Seat == null || engagedPlayer == null)
        {
            return;
        }

        var body = engagedPlayer.transform;
        body.position = station.Seat.position;
        body.rotation = Quaternion.Euler(0f, station.Seat.eulerAngles.y, 0f);
        var rigidbody = engagedPlayer.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.position = body.position;
            rigidbody.rotation = body.rotation;
        }
    }

    static BasePlayerController3D LocalPlayer()
    {
        var manager = FindObjectOfType<PlayerManager3D>();
        if (manager != null && manager.LocalPlayerInstance != null)
        {
            return manager.LocalPlayerInstance.GetComponent<BasePlayerController3D>();
        }

        var players = FindObjectsOfType<BasePlayerController3D>();
        for (int i = 0; i < players.Length; i++)
        {
            var view = players[i].GetComponent<PhotonView>();
            if (!PhotonNetwork.inRoom || (view != null && view.isMine))
            {
                return players[i];
            }
        }

        return null;
    }

    void RememberPlayerViews(GameObject player)
    {
        playerCameras.Clear();
        playerListeners.Clear();
        var cameras = player.GetComponentsInChildren<Camera>(true);
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i].enabled)
            {
                playerCameras.Add(cameras[i]);
                StripGalaxyLayer(cameras[i]);
            }
        }

        var listeners = player.GetComponentsInChildren<AudioListener>(true);
        for (int i = 0; i < listeners.Length; i++)
        {
            if (listeners[i].enabled)
            {
                playerListeners.Add(listeners[i]);
            }
        }

        var sceneCamera = Camera.main;
        if (sceneCamera != null && sceneCamera.enabled && !playerCameras.Contains(sceneCamera))
        {
            playerCameras.Add(sceneCamera);
            StripGalaxyLayer(sceneCamera);
            var sceneListener = sceneCamera.GetComponent<AudioListener>();
            if (sceneListener != null && sceneListener.enabled && !playerListeners.Contains(sceneListener))
            {
                playerListeners.Add(sceneListener);
            }
        }
    }

    static void StripGalaxyLayer(Camera camera)
    {
        var layer = LayerMask.NameToLayer("GalaxyView");
        if (layer < 0)
        {
            return;
        }

        camera.cullingMask &= ~(1 << layer);
    }

    void SetPlayerViews(bool enabled)
    {
        for (int i = 0; i < playerCameras.Count; i++)
        {
            if (playerCameras[i] != null)
            {
                playerCameras[i].enabled = enabled;
            }
        }

        for (int i = 0; i < playerListeners.Count; i++)
        {
            if (playerListeners[i] != null)
            {
                playerListeners[i].enabled = enabled;
            }
        }
    }
}
