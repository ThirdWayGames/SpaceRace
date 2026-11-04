using Assets.Scripts;
using UnityEngine;

public class CrewStation : MonoBehaviour
{
    public StationRole Role;

    public string StationId;

    public Transform Seat;

    public Transform CameraAnchor;

    public Camera StationCamera;

    public bool DeployRequested;

    float nextCommandTime;

    float nextScanTime;

    float nextShotTime;

    float sentThrottle = 99f;

    float sentTurn = 99f;

    bool localInside;

    public static CrewStation Find(string stationId)
    {
        var stations = FindObjectsOfType<CrewStation>();
        for (int i = 0; i < stations.Length; i++)
        {
            if (stations[i].StationId == stationId)
            {
                return stations[i];
            }
        }

        return null;
    }

    public void Update()
    {
        var board = StationBoard.FindBoard();
        var seated = board != null && board.Occupancy.StationFor(StationBoard.LocalActorId()) == StationId;
        DeployRequested = seated && Role == StationRole.Airlock;
        if (!seated)
        {
            sentThrottle = 99f;
            sentTurn = 99f;
            return;
        }

        if (Role == StationRole.Helm)
        {
            Steer();
        }
        else if (Role == StationRole.Scanner)
        {
            Scan();
        }
        else if (Role == StationRole.Turret)
        {
            Shoot(board);
        }
    }

    public void OnTriggerStay(Collider other)
    {
        var player = PlayerFrom(other);
        if (player == null || !IsLocal(player))
        {
            return;
        }

        localInside = true;
        if (!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        var board = StationBoard.FindBoard();
        if (board == null)
        {
            return;
        }

        if (board.Occupancy.StationFor(StationBoard.LocalActorId()) == StationId)
        {
            if (Role == StationRole.Helm)
            {
                var session = GalaxySessionFinder.Find();
                if (session != null)
                {
                    session.SubmitPilotCommand(0f, 0f);
                }
            }

            board.RequestStand();
            return;
        }

        board.RequestSit(StationId);
    }

    public void OnTriggerExit(Collider other)
    {
        var player = PlayerFrom(other);
        if (player != null && IsLocal(player))
        {
            localInside = false;
        }
    }

    public void OnGUI()
    {
        var board = StationBoard.FindBoard();
        var localStation = board == null ? string.Empty : board.Occupancy.StationFor(StationBoard.LocalActorId());
        if (localStation != StationId && !localInside)
        {
            return;
        }

        if (localStation == StationId)
        {
            DrawSeatHud();
            return;
        }

        GUI.Label(new Rect(16f, Screen.height - 40f, 420f, 24f), "E  " + Role);
    }

    void Steer()
    {
        var session = GalaxySessionFinder.Find();
        if (session == null)
        {
            return;
        }

        var throttle = Input.GetAxis("Vertical");
        var turn = Input.GetAxis("Horizontal");
        var changed = Mathf.Abs(throttle - sentThrottle) > 0.01f || Mathf.Abs(turn - sentTurn) > 0.01f;
        if (!changed && Time.time < nextCommandTime)
        {
            return;
        }

        session.SubmitPilotCommand(throttle, turn);
        sentThrottle = throttle;
        sentTurn = turn;
        nextCommandTime = Time.time + 0.1f;
    }

    void Scan()
    {
        if (Time.time < nextScanTime)
        {
            return;
        }

        nextScanTime = Time.time + 0.5f;
        var session = GalaxySessionFinder.Find();
        if (session != null)
        {
            session.TryReveal(80f, 80f);
        }
    }

    void Shoot(StationBoard board)
    {
        if (!Input.GetButton("Fire1") || Time.time < nextShotTime)
        {
            return;
        }

        nextShotTime = Time.time + 0.25f;
        board.RequestTurretFire();
    }

    void DrawSeatHud()
    {
        var session = GalaxySessionFinder.Find();
        var snapshot = session == null ? null : session.Snapshot;
        if (snapshot == null)
        {
            return;
        }

        var text = string.Format("{0}  hull {1:0}  storm {2:0}  {3}  {4}", Role, snapshot.Hull, snapshot.StormRadius, snapshot.PhaseName, snapshot.CurrentPoiId);
        if (Role == StationRole.Scanner)
        {
            text += "  hidden " + HiddenCount(snapshot.ContactSummary);
        }
        else
        {
            text += "  " + snapshot.RevealedContactIds;
        }

        GUI.Label(new Rect(16f, 16f, 900f, 24f), text);
    }

    static int HiddenCount(string summary)
    {
        if (string.IsNullOrEmpty(summary))
        {
            return 0;
        }

        var hidden = 0;
        var records = summary.Split(';');
        for (int i = 0; i < records.Length; i++)
        {
            var fields = records[i].Split(',');
            if (fields.Length >= 5 && fields[4] == "0")
            {
                hidden += 1;
            }
        }

        return hidden;
    }

    static BasePlayerController3D PlayerFrom(Collider other)
    {
        if (other == null)
        {
            return null;
        }

        var player = other.GetComponent<BasePlayerController3D>();
        if (player == null)
        {
            player = other.GetComponentInParent<BasePlayerController3D>();
        }

        return player;
    }

    static bool IsLocal(BasePlayerController3D player)
    {
        var view = player.GetComponent<PhotonView>();
        if (view == null)
        {
            view = player.GetComponentInParent<PhotonView>();
        }

        if (!PhotonNetwork.inRoom)
        {
            return true;
        }

        return view != null && view.isMine;
    }
}
