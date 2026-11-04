// Headless Photon lobby probe for SpaceRace (PUN / Photon 4.1).
// Speaks the same name-server, master, and game-server operations as
// Assets/Photon Unity Networking, then checks the lobby rules in LobbyManager:
// the master assigns PlayerTeam, each client sets PlayerReady, and every
// client must observe the same roster.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ExitGames.Client.Photon;
using Newtonsoft.Json;

internal static class LobbyNetProbe
{
    internal const string AppId = "eed15a19-9d17-4044-aa03-ea8679e1836e";
    internal const string AppVersion = "SpaceRace";
    internal const byte MaxPlayers = 6;

    internal static readonly string[] NameServers = new string[]
    {
        "ns.exitgames.com:5058",
        "ns.photonengine.io:5058"
    };

    static int Main(string[] args)
    {
        try
        {
            var opt = Options.Parse(args);
            if (opt.Mode == "help")
            {
                Options.PrintHelp();
                return 0;
            }
            if (opt.Mode == "survey")
            {
                return new RegionSurvey().Run(opt.OutPath);
            }
            if (opt.Mode == "compare")
            {
                return ReportCompare.Run(opt.ComparePaths);
            }
            return new LobbySession(opt).Run();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FATAL " + ex);
            return 2;
        }
    }

    internal static void Log(string message)
    {
        Console.Error.WriteLine(DateTime.UtcNow.ToString("HH:mm:ss.fff") + " " + message);
    }

    internal static long NowMs()
    {
        return Stopwatch.GetTimestamp() * 1000L / Stopwatch.Frequency;
    }
}

internal sealed class Options
{
    public string Mode = "lobby";
    public string Role = "host";
    public string Room = "lbprobe";
    public string Region = "eu";
    public string Nick = "probe";
    public string Env = "local";
    public int Expect = 2;
    public int WaitSec = 20;
    public int HoldSec = 12;
    public string OutPath = "lobby-report.json";
    public List<string> ComparePaths = new List<string>();

    public static Options Parse(string[] args)
    {
        var opt = new Options();
        if (args == null || args.Length == 0)
        {
            opt.Mode = "help";
            return opt;
        }

        for (int i = 0; i < args.Length; i++)
        {
            string a = args[i];
            if (a == "--help" || a == "-h")
            {
                opt.Mode = "help";
            }
            else if (a == "--survey")
            {
                opt.Mode = "survey";
            }
            else if (a == "--compare")
            {
                opt.Mode = "compare";
            }
            else if (a == "--role")
            {
                opt.Role = Next(args, ref i);
            }
            else if (a == "--room")
            {
                opt.Room = Next(args, ref i);
            }
            else if (a == "--region")
            {
                opt.Region = Next(args, ref i);
            }
            else if (a == "--nick")
            {
                opt.Nick = Next(args, ref i);
            }
            else if (a == "--env")
            {
                opt.Env = Next(args, ref i);
            }
            else if (a == "--expect")
            {
                opt.Expect = int.Parse(Next(args, ref i));
            }
            else if (a == "--wait")
            {
                opt.WaitSec = int.Parse(Next(args, ref i));
            }
            else if (a == "--hold")
            {
                opt.HoldSec = int.Parse(Next(args, ref i));
            }
            else if (a == "--out")
            {
                opt.OutPath = Next(args, ref i);
            }
            else if (opt.Mode == "compare")
            {
                opt.ComparePaths.Add(a);
            }
            else
            {
                throw new ArgumentException("Unknown argument " + a);
            }
        }

        return opt;
    }

    static string Next(string[] args, ref int i)
    {
        i++;
        if (i >= args.Length)
        {
            throw new ArgumentException("Missing value after " + args[i - 1]);
        }
        return args[i];
    }

    public static void PrintHelp()
    {
        Console.WriteLine("LobbyNetProbe");
        Console.WriteLine("  --survey --out regions.json");
        Console.WriteLine("  --role host|join --room NAME --region eu --nick NAME --env LABEL --expect N --wait SEC --hold SEC --out report.json");
        Console.WriteLine("  --compare report.json [more.json ...]");
    }
}

internal sealed class ActorState
{
    public int Id;
    public string Nick = "";
    public string Env = "";
    public string Role = "";
    public int Team;
    public bool Ready;
    public bool Left;
    public bool TeamLatencyRecorded;
    public bool ReadyLatencyRecorded;
    public int LastProbeSeq;
}

internal enum Stage
{
    ConnectingName,
    HopMaster,
    ConnectingMaster,
    OnMaster,
    JoinRetry,
    HopGame,
    ConnectingGame,
    InRoom,
    Leaving,
    Done,
    Failed
}

internal enum ServerKind
{
    Name,
    Master,
    Game
}

internal sealed class LobbySession : IPhotonPeerListener
{
    const byte OpAuthenticate = 230;
    const byte OpCreateGame = 227;
    const byte OpJoinGame = 226;
    const byte OpSetProperties = 252;
    const byte OpLeave = 254;

    const byte EvJoin = 255;
    const byte EvLeave = 254;
    const byte EvProperties = 253;

    const byte PApplicationId = 224;
    const byte PAppVersion = 220;
    const byte PRegion = 210;
    const byte PUserId = 225;
    const byte PSecret = 221;
    const byte PAddress = 230;
    const byte PRoomName = 255;
    const byte PPlayerProperties = 249;
    const byte PBroadcast = 250;
    const byte PGameProperties = 248;
    const byte PActorNr = 254;
    const byte PProperties = 251;
    const byte PTargetActorNr = 253;
    const byte PActorList = 252;
    const byte PCleanupCacheOnLeave = 241;
    const byte PCheckUserOnJoin = 232;
    const byte PEmptyRoomTtl = 236;
    const byte PRoomOptionFlags = 191;

    const byte KMaxPlayers = 255;
    const byte KIsVisible = 254;
    const byte KIsOpen = 253;
    const byte KCleanupInfo = 249;
    const byte KMasterClientId = 248;
    const byte KPlayerName = 255;

    const int GameDoesNotExist = 32758;

    readonly Options opt;
    readonly string userId;
    readonly List<string> warnings = new List<string>();
    readonly List<int> rttSamples = new List<int>();
    readonly List<int> teamLatency = new List<int>();
    readonly List<int> readyLatency = new List<int>();
    readonly List<int> probeLatency = new List<int>();
    readonly Dictionary<int, ActorState> actors = new Dictionary<int, ActorState>();
    readonly List<ActorState> settledActors = new List<ActorState>();
    bool frozen;

    PhotonPeer peer;
    Stage stage = Stage.ConnectingName;
    ServerKind server = ServerKind.Name;
    bool disconnected = true;
    int nameServerIndex;
    string nameServerUsed = "";
    string token;
    string masterAddress = "";
    string gameAddress = "";
    string failure;
    int localActor;
    int masterId = 1;
    bool readySent;
    int probeSeq;
    long nextProbeAt;
    long nextRttAt;
    long connectStarted;
    long started;
    long joinedAt;
    long allPlayersAt;
    long teamAt;
    long allReadyAt;
    long nextJoinAt;

    public LobbySession(Options opt)
    {
        this.opt = opt;
        this.userId = "probe-" + opt.Nick + "-" + Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    public int Run()
    {
        started = LobbyNetProbe.NowMs();
        LobbyNetProbe.Log("start role=" + opt.Role + " room=" + opt.Room + " region=" + opt.Region + " nick=" + opt.Nick + " env=" + opt.Env);
        Connect(LobbyNetProbe.NameServers[0], ServerKind.Name);
        long deadline = started + (opt.WaitSec + opt.HoldSec + 45) * 1000L;

        while (stage != Stage.Done && stage != Stage.Failed && LobbyNetProbe.NowMs() < deadline)
        {
            if (peer != null)
            {
                peer.Service();
            }

            long now = LobbyNetProbe.NowMs();
            if ((stage == Stage.ConnectingName || stage == Stage.ConnectingMaster || stage == Stage.ConnectingGame)
                && now - connectStarted > 12000)
            {
                if (stage == Stage.ConnectingName && nameServerIndex + 1 < LobbyNetProbe.NameServers.Length)
                {
                    nameServerIndex++;
                    Warn("name server timed out, trying " + LobbyNetProbe.NameServers[nameServerIndex]);
                    Connect(LobbyNetProbe.NameServers[nameServerIndex], ServerKind.Name);
                }
                else
                {
                    Fail("timed out connecting to " + server + " at " + (peer == null ? "" : peer.ServerAddress));
                }
            }
            else if (stage == Stage.HopMaster && disconnected)
            {
                Connect(masterAddress, ServerKind.Master);
            }
            else if (stage == Stage.HopGame && disconnected)
            {
                Connect(gameAddress, ServerKind.Game);
            }
            else if (stage == Stage.JoinRetry && now >= nextJoinAt)
            {
                SendJoinOrCreate(false);
                stage = Stage.OnMaster;
            }
            else if (stage == Stage.InRoom)
            {
                TickRoom(now);
            }

            Thread.Sleep(10);
        }

        if (stage != Stage.Done && stage != Stage.Failed)
        {
            Fail("timed out in stage " + stage);
        }

        int code = WriteReport();
        Cleanup();
        return code;
    }

    void TickRoom(long now)
    {
        if (peer != null && peer.RoundTripTime > 0 && now >= nextRttAt)
        {
            rttSamples.Add(peer.RoundTripTime);
            nextRttAt = now + 200;
        }

        if (allPlayersAt == 0 && actors.Count >= opt.Expect)
        {
            allPlayersAt = now;
        }

        if (localActor > 0 && localActor == masterId)
        {
            AssignTeams();
        }

        ActorState self;
        if (!readySent && actors.TryGetValue(localActor, out self) && self.Team != 0)
        {
            var props = new Hashtable();
            props["PlayerReady"] = true;
            props["ReadySetAt"] = peer.ServerTimeInMilliSeconds;
            if (SendActorProps(localActor, props))
            {
                readySent = true;
                self.Ready = true;
                LobbyNetProbe.Log("sent ready actor=" + localActor);
            }
        }

        if (teamAt == 0 && AllHaveTeam())
        {
            teamAt = now;
        }
        if (allReadyAt == 0 && AllReady())
        {
            allReadyAt = now;
            nextProbeAt = now + 300 + localActor * 80;
        }

        if (allReadyAt != 0 && probeSeq < 5 && now >= nextProbeAt)
        {
            probeSeq++;
            var props = new Hashtable();
            props["ProbeSeq"] = probeSeq;
            props["ProbeStamp"] = peer.ServerTimeInMilliSeconds;
            SendActorProps(localActor, props);
            nextProbeAt = now + 350;
        }

        if (!frozen && allReadyAt != 0 && probeSeq >= 5 && now >= nextProbeAt)
        {
            Freeze();
        }

        if (joinedAt != 0 && now - joinedAt >= opt.HoldSec * 1000L)
        {
            if (!frozen)
            {
                if (actors.Count < opt.Expect)
                {
                    Fail("hold ended with " + actors.Count + " players, expected " + opt.Expect);
                }
                else if (!AllHaveTeam())
                {
                    Fail("hold ended before every player had a team");
                }
                else if (!AllReady())
                {
                    Fail("hold ended before every player was ready");
                }
                else
                {
                    Freeze();
                    Leave();
                }
            }
            else
            {
                Leave();
            }
        }
    }

    void Freeze()
    {
        if (frozen)
        {
            return;
        }

        frozen = true;
        var ids = new List<int>(actors.Keys);
        ids.Sort();
        for (int i = 0; i < ids.Count; i++)
        {
            ActorState actor = actors[ids[i]];
            var copy = new ActorState();
            copy.Id = actor.Id;
            copy.Nick = actor.Nick;
            copy.Env = actor.Env;
            copy.Role = actor.Role;
            copy.Team = actor.Team;
            copy.Ready = actor.Ready;
            copy.Left = actor.Left;
            settledActors.Add(copy);
        }
        LobbyNetProbe.Log("settled players=" + settledActors.Count);
    }

    bool AllHaveTeam()
    {
        if (actors.Count < opt.Expect)
        {
            return false;
        }
        foreach (var actor in actors.Values)
        {
            if (!actor.Left && actor.Team == 0)
            {
                return false;
            }
        }
        return true;
    }

    bool AllReady()
    {
        if (!AllHaveTeam())
        {
            return false;
        }
        foreach (var actor in actors.Values)
        {
            if (!actor.Left && !actor.Ready)
            {
                return false;
            }
        }
        return true;
    }

    void AssignTeams()
    {
        var ids = new List<int>(actors.Keys);
        ids.Sort();
        for (int i = 0; i < ids.Count; i++)
        {
            ActorState actor = actors[ids[i]];
            if (actor.Left || actor.Team != 0)
            {
                continue;
            }

            int red = 0;
            int blue = 0;
            foreach (var other in actors.Values)
            {
                if (other.Id == actor.Id || other.Left)
                {
                    continue;
                }
                if (other.Team == 1)
                {
                    red++;
                }
                else if (other.Team == 2)
                {
                    blue++;
                }
            }

            // Same rule as LobbyManager.AssignTeamId when no team was requested.
            int team = red < blue ? 1 : 2;
            var props = new Hashtable();
            props["PlayerTeam"] = team;
            props["TeamSetAt"] = peer.ServerTimeInMilliSeconds;
            if (!SendActorProps(actor.Id, props))
            {
                Warn("failed to send team " + team + " for actor " + actor.Id);
                continue;
            }

            actor.Team = team;
            LobbyNetProbe.Log("assigned actor " + actor.Id + " team " + team);
        }
    }

    bool SendActorProps(int actorNr, Hashtable props)
    {
        var op = new Dictionary<byte, object>();
        op[PProperties] = props;
        op[PActorNr] = actorNr;
        op[PBroadcast] = true;
        return peer.OpCustom(OpSetProperties, op, true, 0, false);
    }

    void Leave()
    {
        LobbyNetProbe.Log("leaving room");
        stage = Stage.Leaving;
        try
        {
            peer.OpCustom(OpLeave, null, true, 0, false);
        }
        catch (Exception ex)
        {
            Warn("leave send failed: " + ex.Message);
        }
        peer.Disconnect();
    }

    void Connect(string address, ServerKind kind)
    {
        CleanupPeer();
        server = kind;
        disconnected = false;
        connectStarted = LobbyNetProbe.NowMs();
        if (kind == ServerKind.Name)
        {
            stage = Stage.ConnectingName;
            nameServerUsed = address;
        }
        else if (kind == ServerKind.Master)
        {
            stage = Stage.ConnectingMaster;
        }
        else
        {
            stage = Stage.ConnectingGame;
        }

        string app = kind == ServerKind.Name ? "ns" : "";
        peer = new PhotonPeer(this, ConnectionProtocol.Udp);
        peer.DebugOut = DebugLevel.ERROR;
        LobbyNetProbe.Log("connect " + kind + " " + address + " app='" + app + "'");
        if (!peer.Connect(address, app))
        {
            Fail("Connect() returned false for " + address);
        }
    }

    void CleanupPeer()
    {
        if (peer == null)
        {
            return;
        }
        try
        {
            peer.Disconnect();
        }
        catch (Exception)
        {
        }
        try
        {
            peer.StopThread();
        }
        catch (Exception)
        {
        }
        peer = null;
    }

    void Cleanup()
    {
        CleanupPeer();
    }

    void SendAuth()
    {
        var op = new Dictionary<byte, object>();
        if (!string.IsNullOrEmpty(token))
        {
            op[PSecret] = token;
            LobbyNetProbe.Log("auth with token on " + server);
            peer.OpCustom(OpAuthenticate, op, true, 0, false);
            return;
        }

        op[PAppVersion] = LobbyNetProbe.AppVersion;
        op[PApplicationId] = LobbyNetProbe.AppId;
        op[PRegion] = opt.Region;
        op[PUserId] = userId;
        LobbyNetProbe.Log("auth app region=" + opt.Region + " on " + server);
        peer.OpCustom(OpAuthenticate, op, true, 0, peer.IsEncryptionAvailable);
    }

    void SendJoinOrCreate(bool onGameServer)
    {
        var op = new Dictionary<byte, object>();
        op[PRoomName] = opt.Room;
        if (onGameServer)
        {
            var player = new Hashtable();
            player[KPlayerName] = opt.Nick;
            player["ProbeEnv"] = opt.Env;
            player["ProbeRole"] = opt.Role;
            op[PPlayerProperties] = player;
            op[PBroadcast] = true;
            if (opt.Role == "host")
            {
                AddRoomOptions(op);
            }
        }

        byte code = opt.Role == "host" ? OpCreateGame : OpJoinGame;
        LobbyNetProbe.Log((opt.Role == "host" ? "create" : "join") + " room=" + opt.Room + " onGame=" + onGameServer);
        peer.OpCustom(code, op, true, 0, false);
    }

    static void AddRoomOptions(Dictionary<byte, object> op)
    {
        var game = new Hashtable();
        game[KMaxPlayers] = LobbyNetProbe.MaxPlayers;
        game[KIsVisible] = false;
        game[KIsOpen] = true;
        game[KCleanupInfo] = true;
        op[PGameProperties] = game;
        op[PCleanupCacheOnLeave] = true;
        op[PCheckUserOnJoin] = true;
        op[PEmptyRoomTtl] = 15000;
        op[PRoomOptionFlags] = 0x01 | 0x02;
    }

    void Fail(string message)
    {
        if (stage == Stage.Failed || stage == Stage.Done)
        {
            return;
        }
        failure = message;
        stage = Stage.Failed;
        LobbyNetProbe.Log("FAIL " + message);
        if (peer != null)
        {
            try
            {
                peer.Disconnect();
            }
            catch (Exception)
            {
            }
        }
    }

    void Warn(string message)
    {
        warnings.Add(message);
        LobbyNetProbe.Log("WARN " + message);
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        if (level == DebugLevel.ERROR || level == DebugLevel.WARNING)
        {
            LobbyNetProbe.Log("photon " + level + " " + message);
        }
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        LobbyNetProbe.Log("status " + statusCode + " stage " + stage + " server " + server);
        if (statusCode == StatusCode.Connect)
        {
            peer.EstablishEncryption();
            return;
        }

        if (statusCode == StatusCode.EncryptionEstablished || statusCode == StatusCode.EncryptionFailedToEstablish)
        {
            if (statusCode == StatusCode.EncryptionFailedToEstablish)
            {
                Warn("encryption was not established on " + server + "; authenticating anyway");
            }
            SendAuth();
            return;
        }

        if (statusCode == StatusCode.Disconnect)
        {
            disconnected = true;
            if (stage == Stage.Leaving)
            {
                stage = Stage.Done;
            }
            else if (stage != Stage.HopMaster && stage != Stage.HopGame && stage != Stage.Failed && stage != Stage.Done)
            {
                if (stage == Stage.ConnectingName && nameServerIndex + 1 < LobbyNetProbe.NameServers.Length)
                {
                    nameServerIndex++;
                    Warn("name server closed the connection, trying " + LobbyNetProbe.NameServers[nameServerIndex]);
                    Connect(LobbyNetProbe.NameServers[nameServerIndex], ServerKind.Name);
                }
                else
                {
                    Fail("disconnected during " + stage);
                }
            }
            return;
        }

        if (statusCode == StatusCode.ExceptionOnConnect
            || statusCode == StatusCode.Exception
            || statusCode == StatusCode.ExceptionOnReceive
            || statusCode == StatusCode.TimeoutDisconnect
            || statusCode == StatusCode.DisconnectByServer
            || statusCode == StatusCode.DisconnectByServerLogic
            || statusCode == StatusCode.DisconnectByServerUserLimit
            || statusCode == StatusCode.SecurityExceptionOnConnect)
        {
            if (stage == Stage.ConnectingName && nameServerIndex + 1 < LobbyNetProbe.NameServers.Length)
            {
                nameServerIndex++;
                Warn(statusCode + " on name server, trying " + LobbyNetProbe.NameServers[nameServerIndex]);
                Connect(LobbyNetProbe.NameServers[nameServerIndex], ServerKind.Name);
                return;
            }
            Fail(statusCode + " during " + stage + " server " + server);
        }
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        LobbyNetProbe.Log("op " + operationResponse.OperationCode + " rc=" + operationResponse.ReturnCode + " " + operationResponse.DebugMessage + " keys=" + KeyList(operationResponse.Parameters));

        if (operationResponse.Parameters != null && operationResponse.Parameters.ContainsKey(PSecret))
        {
            string secret = operationResponse.Parameters[PSecret] as string;
            if (!string.IsNullOrEmpty(secret))
            {
                token = secret;
            }
        }

        if (operationResponse.OperationCode == OpAuthenticate)
        {
            if (operationResponse.ReturnCode != 0)
            {
                Fail("auth failed rc=" + operationResponse.ReturnCode + " " + operationResponse.DebugMessage);
                return;
            }

            if (server == ServerKind.Name)
            {
                masterAddress = operationResponse.Parameters.ContainsKey(PAddress) ? operationResponse.Parameters[PAddress] as string : null;
                if (string.IsNullOrEmpty(masterAddress))
                {
                    Fail("name server auth returned no master address");
                    return;
                }
                LobbyNetProbe.Log("master " + masterAddress);
                stage = Stage.HopMaster;
                peer.Disconnect();
                return;
            }

            if (server == ServerKind.Master)
            {
                stage = Stage.OnMaster;
                SendJoinOrCreate(false);
                return;
            }

            SendJoinOrCreate(true);
            return;
        }

        if (operationResponse.OperationCode == OpCreateGame || operationResponse.OperationCode == OpJoinGame)
        {
            HandleJoinOrCreate(operationResponse);
            return;
        }

        if (operationResponse.OperationCode == OpSetProperties && operationResponse.ReturnCode != 0)
        {
            Warn("set properties rc=" + operationResponse.ReturnCode + " " + operationResponse.DebugMessage);
        }
    }

    void HandleJoinOrCreate(OperationResponse operationResponse)
    {
        bool onGame = server == ServerKind.Game;
        if (!onGame)
        {
            if (operationResponse.ReturnCode != 0)
            {
                if (opt.Role != "host" && operationResponse.ReturnCode == GameDoesNotExist && LobbyNetProbe.NowMs() - started < opt.WaitSec * 1000L)
                {
                    stage = Stage.JoinRetry;
                    nextJoinAt = LobbyNetProbe.NowMs() + 400;
                    return;
                }
                Fail((opt.Role == "host" ? "create" : "join") + " failed on master rc=" + operationResponse.ReturnCode + " " + operationResponse.DebugMessage);
                return;
            }

            gameAddress = operationResponse.Parameters.ContainsKey(PAddress) ? operationResponse.Parameters[PAddress] as string : null;
            if (operationResponse.Parameters.ContainsKey(PRoomName))
            {
                string confirmed = operationResponse.Parameters[PRoomName] as string;
                if (!string.IsNullOrEmpty(confirmed))
                {
                    opt.Room = confirmed;
                }
            }
            if (string.IsNullOrEmpty(gameAddress))
            {
                Fail("master did not return a game server address");
                return;
            }
            LobbyNetProbe.Log("game " + gameAddress);
            stage = Stage.HopGame;
            peer.Disconnect();
            return;
        }

        if (operationResponse.ReturnCode != 0)
        {
            Fail((opt.Role == "host" ? "create" : "join") + " failed on game server rc=" + operationResponse.ReturnCode + " " + operationResponse.DebugMessage);
            return;
        }

        EnterRoom(operationResponse);
    }

    void EnterRoom(OperationResponse operationResponse)
    {
        localActor = operationResponse.Parameters.ContainsKey(PActorNr) ? AsInt(operationResponse.Parameters[PActorNr]) : 0;
        if (operationResponse.Parameters.ContainsKey(PActorList))
        {
            var list = operationResponse.Parameters[PActorList] as int[];
            if (list != null)
            {
                for (int i = 0; i < list.Length; i++)
                {
                    EnsureActor(list[i]);
                }
            }
        }
        EnsureActor(localActor);

        if (operationResponse.Parameters.ContainsKey(PPlayerProperties))
        {
            ApplyActorPropertyMap(operationResponse.Parameters[PPlayerProperties] as Hashtable, 0);
        }
        if (operationResponse.Parameters.ContainsKey(PGameProperties))
        {
            ApplyGameProperties(operationResponse.Parameters[PGameProperties] as Hashtable);
        }

        joinedAt = LobbyNetProbe.NowMs();
        stage = Stage.InRoom;
        LobbyNetProbe.Log("joined actor=" + localActor + " master=" + masterId + " others=" + actors.Count);
    }

    public void OnEvent(EventData photonEvent)
    {
        int sender = -1;
        if (photonEvent.Parameters != null && photonEvent.Parameters.ContainsKey(PActorNr))
        {
            sender = AsInt(photonEvent.Parameters[PActorNr]);
        }

        if (photonEvent.Code == EvJoin)
        {
            if (sender > 0)
            {
                ActorState actor = EnsureActor(sender);
                if (photonEvent.Parameters.ContainsKey(PPlayerProperties))
                {
                    ApplyProps(actor, photonEvent.Parameters[PPlayerProperties] as Hashtable, false);
                }
                if (photonEvent.Parameters.ContainsKey(PActorList))
                {
                    var list = photonEvent.Parameters[PActorList] as int[];
                    if (list != null)
                    {
                        for (int i = 0; i < list.Length; i++)
                        {
                            EnsureActor(list[i]);
                        }
                    }
                }
                LobbyNetProbe.Log("event join actor=" + sender + " count=" + actors.Count);
            }
            return;
        }

        if (photonEvent.Code == EvLeave)
        {
            if (frozen)
            {
                return;
            }
            ActorState actor;
            if (actors.TryGetValue(sender, out actor))
            {
                actor.Left = true;
            }
            Fail("actor " + sender + " left before the lobby settled");
            return;
        }

        if (photonEvent.Code == EvProperties)
        {
            int target = photonEvent.Parameters.ContainsKey(PTargetActorNr) ? AsInt(photonEvent.Parameters[PTargetActorNr]) : 0;
            var props = photonEvent.Parameters.ContainsKey(PProperties) ? photonEvent.Parameters[PProperties] as Hashtable : null;
            if (target == 0)
            {
                ApplyGameProperties(props);
            }
            else
            {
                ApplyProps(EnsureActor(target), props, true);
            }
        }
    }

    ActorState EnsureActor(int id)
    {
        ActorState actor;
        if (!actors.TryGetValue(id, out actor))
        {
            actor = new ActorState();
            actor.Id = id;
            actors[id] = actor;
        }
        return actor;
    }

    void ApplyActorPropertyMap(Hashtable map, int targetActor)
    {
        if (map == null)
        {
            return;
        }
        if (targetActor > 0)
        {
            ApplyProps(EnsureActor(targetActor), map, false);
            return;
        }

        foreach (object key in map.Keys)
        {
            int actorNr;
            if (!TryAsInt(key, out actorNr))
            {
                continue;
            }
            ApplyProps(EnsureActor(actorNr), map[key] as Hashtable, false);
        }
    }

    void ApplyProps(ActorState actor, Hashtable props, bool live)
    {
        if (actor == null || props == null)
        {
            return;
        }

        foreach (object key in props.Keys)
        {
            object value = props[key];
            if (key is byte && (byte)key == KPlayerName && value is string)
            {
                actor.Nick = (string)value;
            }
            else if (key is string && (string)key == "ProbeEnv" && value is string)
            {
                actor.Env = (string)value;
            }
            else if (key is string && (string)key == "ProbeRole" && value is string)
            {
                actor.Role = (string)value;
            }
            else if (key is string && (string)key == "PlayerTeam")
            {
                int team;
                if (TryAsInt(value, out team))
                {
                    actor.Team = team;
                }
            }
            else if (key is string && (string)key == "PlayerReady")
            {
                actor.Ready = AsBool(value);
            }
        }

        int stamp;
        if (live && actor.Id != localActor && !actor.TeamLatencyRecorded && props.ContainsKey("TeamSetAt") && TryAsInt(props["TeamSetAt"], out stamp) && peer != null)
        {
            actor.TeamLatencyRecorded = true;
            teamLatency.Add(peer.ServerTimeInMilliSeconds - stamp);
        }
        if (live && actor.Id != localActor && !actor.ReadyLatencyRecorded && props.ContainsKey("ReadySetAt") && TryAsInt(props["ReadySetAt"], out stamp) && peer != null)
        {
            actor.ReadyLatencyRecorded = true;
            readyLatency.Add(peer.ServerTimeInMilliSeconds - stamp);
        }

        int seq;
        if (live && actor.Id != localActor && props.ContainsKey("ProbeSeq") && props.ContainsKey("ProbeStamp") && TryAsInt(props["ProbeSeq"], out seq) && seq > actor.LastProbeSeq && TryAsInt(props["ProbeStamp"], out stamp) && peer != null)
        {
            actor.LastProbeSeq = seq;
            probeLatency.Add(peer.ServerTimeInMilliSeconds - stamp);
        }

    }

    void ApplyGameProperties(Hashtable props)
    {
        if (props == null)
        {
            return;
        }
        foreach (object key in props.Keys)
        {
            int code;
            if (!TryAsInt(key, out code) || code != KMasterClientId)
            {
                continue;
            }
            int id;
            if (TryAsInt(props[key], out id) && id > 0)
            {
                masterId = id;
            }
        }
    }

    int WriteReport()
    {
        bool stateOk = string.IsNullOrEmpty(failure) && stage != Stage.Failed && frozen && settledActors.Count >= opt.Expect;
        string error = failure;
        if (string.IsNullOrEmpty(error) && !stateOk)
        {
            error = "lobby state incomplete";
        }

        var snapshot = new List<Dictionary<string, object>>();
        var reported = new List<ActorState>(frozen ? (IEnumerable<ActorState>)settledActors : actors.Values);
        reported.Sort(delegate(ActorState a, ActorState b) { return a.Id.CompareTo(b.Id); });
        for (int i = 0; i < reported.Count; i++)
        {
            ActorState actor = reported[i];
            var row = new Dictionary<string, object>();
            row["id"] = actor.Id;
            row["nick"] = actor.Nick;
            row["env"] = actor.Env;
            row["role"] = actor.Role;
            row["team"] = actor.Team;
            row["ready"] = actor.Ready;
            row["left"] = actor.Left;
            snapshot.Add(row);
        }

        var report = new Dictionary<string, object>();
        report["ok"] = stateOk;
        report["error"] = error ?? "";
        report["role"] = opt.Role;
        report["nick"] = opt.Nick;
        report["env"] = opt.Env;
        report["hostname"] = HostName();
        report["region"] = opt.Region;
        report["room"] = opt.Room;
        report["appVersion"] = LobbyNetProbe.AppVersion;
        report["nameServer"] = nameServerUsed;
        report["master"] = masterAddress ?? "";
        report["game"] = gameAddress ?? "";
        report["localActor"] = localActor;
        report["masterId"] = masterId;
        report["expect"] = opt.Expect;
        report["actorCount"] = actors.Count;
        report["connectMs"] = joinedAt == 0 ? -1 : (int)(joinedAt - started);
        report["allPlayersMs"] = allPlayersAt == 0 || joinedAt == 0 ? -1 : (int)(allPlayersAt - joinedAt);
        report["allTeamsMs"] = teamAt == 0 || joinedAt == 0 ? -1 : (int)(teamAt - joinedAt);
        report["allReadyMs"] = allReadyAt == 0 || joinedAt == 0 ? -1 : (int)(allReadyAt - joinedAt);
        report["rtt"] = Stats.Summarize(rttSamples);
        report["teamOneWayMs"] = Stats.Summarize(teamLatency);
        report["readyOneWayMs"] = Stats.Summarize(readyLatency);
        report["probeOneWayMs"] = Stats.Summarize(probeLatency);
        report["actors"] = snapshot;
        report["warnings"] = warnings;

        string json = JsonConvert.SerializeObject(report, Formatting.Indented);
        File.WriteAllText(opt.OutPath, json);
        LobbyNetProbe.Log("wrote " + opt.OutPath + " ok=" + stateOk + " error=" + (error ?? ""));
        return stateOk ? 0 : 1;
    }

    static string HostName()
    {
        try
        {
            return Dns.GetHostName();
        }
        catch (Exception)
        {
            return "unknown";
        }
    }

    static string KeyList(Dictionary<byte, object> parameters)
    {
        if (parameters == null)
        {
            return "";
        }
        var sb = new StringBuilder();
        foreach (var pair in parameters)
        {
            sb.Append(pair.Key);
            sb.Append(':');
            sb.Append(pair.Value == null ? "null" : pair.Value.GetType().Name);
            sb.Append(' ');
        }
        return sb.ToString();
    }

    static int AsInt(object value)
    {
        int n;
        if (!TryAsInt(value, out n))
        {
            throw new InvalidCastException("Expected int, got " + (value == null ? "null" : value.GetType().FullName));
        }
        return n;
    }

    static bool TryAsInt(object value, out int n)
    {
        if (value is int)
        {
            n = (int)value;
            return true;
        }
        if (value is byte)
        {
            n = (byte)value;
            return true;
        }
        if (value is short)
        {
            n = (short)value;
            return true;
        }
        if (value is long)
        {
            n = (int)(long)value;
            return true;
        }
        n = 0;
        return false;
    }

    static bool AsBool(object value)
    {
        if (value is bool)
        {
            return (bool)value;
        }
        int n;
        if (TryAsInt(value, out n))
        {
            return n != 0;
        }
        return false;
    }
}

internal static class Stats
{
    public static Dictionary<string, object> Summarize(List<int> samples)
    {
        var copy = new List<int>(samples);
        copy.Sort();
        var result = new Dictionary<string, object>();
        result["count"] = copy.Count;
        result["min"] = copy.Count == 0 ? -1 : copy[0];
        result["max"] = copy.Count == 0 ? -1 : copy[copy.Count - 1];
        result["median"] = copy.Count == 0 ? -1 : copy[copy.Count / 2];
        result["samples"] = copy;
        return result;
    }
}

internal sealed class RegionSurvey : IPhotonPeerListener
{
    const byte OpGetRegions = 220;
    const byte PApplicationId = 224;
    const byte PRegion = 210;
    const byte PAddress = 230;

    PhotonPeer peer;
    bool done;
    string failure;
    string[] regions = new string[0];
    string[] servers = new string[0];
    int nameIndex;
    long connectStarted;

    public int Run(string outPath)
    {
        ConnectCurrent();
        long deadline = LobbyNetProbe.NowMs() + 20000;
        while (!done && string.IsNullOrEmpty(failure) && LobbyNetProbe.NowMs() < deadline)
        {
            if (peer != null)
            {
                peer.Service();
            }
            if (!done && LobbyNetProbe.NowMs() - connectStarted > 8000 && nameIndex + 1 < NameServersLength())
            {
                nameIndex++;
                LobbyNetProbe.Log("survey retry " + NameServerAt(nameIndex));
                ConnectCurrent();
            }
            Thread.Sleep(10);
        }

        if (peer != null)
        {
            try
            {
                peer.Disconnect();
                peer.StopThread();
            }
            catch (Exception)
            {
            }
        }

        var rows = new List<Dictionary<string, object>>();
        if (regions != null && servers != null)
        {
            int n = Math.Min(regions.Length, servers.Length);
            var pingers = new List<RegionPing>();
            for (int i = 0; i < n; i++)
            {
                if (string.IsNullOrEmpty(regions[i]))
                {
                    continue;
                }
                pingers.Add(new RegionPing(regions[i].ToLowerInvariant(), servers[i]));
            }
            PingAll(pingers);
            for (int i = 0; i < pingers.Count; i++)
            {
                var row = new Dictionary<string, object>();
                row["code"] = pingers[i].Code;
                row["address"] = pingers[i].Address;
                row["rttMs"] = pingers[i].RttMs;
                row["replies"] = pingers[i].Replies;
                rows.Add(row);
                LobbyNetProbe.Log("REGION " + pingers[i].Code + " " + pingers[i].RttMs + "ms replies=" + pingers[i].Replies + " " + pingers[i].Address);
            }
        }

        string best = "";
        string worst = "";
        int bestRtt = int.MaxValue;
        int worstRtt = -1;
        for (int i = 0; i < rows.Count; i++)
        {
            int rtt = (int)rows[i]["rttMs"];
            if (rtt <= 0)
            {
                continue;
            }
            if (rtt < bestRtt)
            {
                bestRtt = rtt;
                best = (string)rows[i]["code"];
            }
            if (rtt > worstRtt)
            {
                worstRtt = rtt;
                worst = (string)rows[i]["code"];
            }
        }

        var report = new Dictionary<string, object>();
        report["ok"] = string.IsNullOrEmpty(failure) && best.Length > 0;
        report["error"] = failure ?? "";
        report["hostname"] = Dns.GetHostName();
        report["best"] = best;
        report["worst"] = worst;
        report["regions"] = rows;
        File.WriteAllText(outPath, JsonConvert.SerializeObject(report, Formatting.Indented));
        LobbyNetProbe.Log("survey best=" + best + " worst=" + worst + " -> " + outPath);
        return report["ok"].Equals(true) ? 0 : 1;
    }

    static int NameServersLength()
    {
        return 2;
    }

    static string NameServerAt(int index)
    {
        return index == 0 ? "ns.exitgames.com:5058" : "ns.photonengine.io:5058";
    }

    void ConnectCurrent()
    {
        if (peer != null)
        {
            try
            {
                peer.Disconnect();
                peer.StopThread();
            }
            catch (Exception)
            {
            }
        }
        connectStarted = LobbyNetProbe.NowMs();
        peer = new PhotonPeer(this, ConnectionProtocol.Udp);
        peer.DebugOut = DebugLevel.ERROR;
        string address = NameServerAt(nameIndex);
        LobbyNetProbe.Log("survey connect " + address);
        if (!peer.Connect(address, "ns"))
        {
            failure = "Connect failed for " + address;
        }
    }

    static void PingAll(List<RegionPing> regions)
    {
        const int attempts = 4;
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            for (int i = 0; i < regions.Count; i++)
            {
                regions[i].Start();
            }
            long begin = LobbyNetProbe.NowMs();
            bool pending = true;
            while (pending && LobbyNetProbe.NowMs() - begin < 800)
            {
                pending = false;
                for (int i = 0; i < regions.Count; i++)
                {
                    if (!regions[i].Poll(attempt > 0))
                    {
                        pending = true;
                    }
                }
                if (pending)
                {
                    Thread.Sleep(5);
                }
            }
            for (int i = 0; i < regions.Count; i++)
            {
                regions[i].Stop();
            }
            Thread.Sleep(50);
        }
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        if (level == DebugLevel.ERROR)
        {
            LobbyNetProbe.Log("survey photon " + message);
        }
    }

    public void OnEvent(EventData photonEvent)
    {
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        LobbyNetProbe.Log("survey status " + statusCode);
        if (statusCode == StatusCode.Connect)
        {
            peer.EstablishEncryption();
            return;
        }
        if (statusCode == StatusCode.EncryptionEstablished || statusCode == StatusCode.EncryptionFailedToEstablish)
        {
            var op = new Dictionary<byte, object>();
            op[PApplicationId] = LobbyNetProbe.AppId;
            peer.OpCustom(OpGetRegions, op, true, 0, peer.IsEncryptionAvailable);
            return;
        }
        if (statusCode == StatusCode.ExceptionOnConnect || statusCode == StatusCode.Exception || statusCode == StatusCode.TimeoutDisconnect)
        {
            if (nameIndex + 1 < NameServersLength())
            {
                nameIndex++;
                ConnectCurrent();
                return;
            }
            failure = "name server status " + statusCode;
            done = true;
        }
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        LobbyNetProbe.Log("survey op " + operationResponse.OperationCode + " rc=" + operationResponse.ReturnCode + " " + operationResponse.DebugMessage);
        if (operationResponse.OperationCode != OpGetRegions)
        {
            return;
        }
        if (operationResponse.ReturnCode != 0)
        {
            failure = "GetRegions rc=" + operationResponse.ReturnCode + " " + operationResponse.DebugMessage;
            done = true;
            return;
        }
        regions = operationResponse.Parameters.ContainsKey(PRegion) ? operationResponse.Parameters[PRegion] as string[] : null;
        servers = operationResponse.Parameters.ContainsKey(PAddress) ? operationResponse.Parameters[PAddress] as string[] : null;
        if (regions == null || servers == null)
        {
            failure = "GetRegions returned no region list";
        }
        done = true;
        peer.Disconnect();
    }
}

internal sealed class RegionPing
{
    public readonly string Code;
    public readonly string Address;
    public int RttMs = -1;
    public int Replies;
    readonly string host;
    PingMono ping;
    Stopwatch watch;
    bool finished;

    public RegionPing(string code, string address)
    {
        Code = code;
        Address = address ?? "";
        host = Resolve(StripPort(Address));
    }

    public void Start()
    {
        finished = false;
        watch = Stopwatch.StartNew();
        ping = new PingMono();
        try
        {
            if (string.IsNullOrEmpty(host))
            {
                finished = true;
                return;
            }
            ping.StartPing(host);
        }
        catch (Exception ex)
        {
            LobbyNetProbe.Log("ping start " + Code + " " + ex.Message);
            finished = true;
        }
    }

    public bool Poll(bool count)
    {
        if (finished)
        {
            return true;
        }
        if (ping == null)
        {
            finished = true;
            return true;
        }
        if (!ping.Done() && watch.ElapsedMilliseconds < 800)
        {
            return false;
        }
        finished = true;
        if (count && ping.Successful && watch.ElapsedMilliseconds < 800)
        {
            Replies++;
            if (RttMs < 0)
            {
                RttMs = (int)watch.ElapsedMilliseconds;
            }
            else
            {
                RttMs = (RttMs * (Replies - 1) + (int)watch.ElapsedMilliseconds) / Replies;
            }
        }
        return true;
    }

    public void Stop()
    {
        if (ping != null)
        {
            try
            {
                ping.Dispose();
            }
            catch (Exception)
            {
            }
            ping = null;
        }
    }

    static string StripPort(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }
        int scheme = value.IndexOf("://", StringComparison.Ordinal);
        if (scheme >= 0)
        {
            value = value.Substring(scheme + 3);
        }
        int colon = value.LastIndexOf(':');
        int first = value.IndexOf(':');
        if (colon > 0 && colon == first)
        {
            return value.Substring(0, colon);
        }
        return value;
    }

    static string Resolve(string host)
    {
        if (string.IsNullOrEmpty(host))
        {
            return "";
        }
        IPAddress parsed;
        if (IPAddress.TryParse(host, out parsed))
        {
            return parsed.ToString();
        }
        try
        {
            IPAddress[] ips = Dns.GetHostAddresses(host);
            for (int i = 0; i < ips.Length; i++)
            {
                if (ips[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    return ips[i].ToString();
                }
            }
            if (ips.Length > 0)
            {
                return ips[0].ToString();
            }
        }
        catch (Exception ex)
        {
            LobbyNetProbe.Log("dns " + host + " " + ex.Message);
        }
        return "";
    }
}

internal static class ReportCompare
{
    public static int Run(List<string> paths)
    {
        if (paths.Count < 2)
        {
            Console.Error.WriteLine("compare needs at least two reports");
            return 2;
        }

        var docs = new List<Newtonsoft.Json.Linq.JObject>();
        for (int i = 0; i < paths.Count; i++)
        {
            docs.Add(Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(paths[i])));
        }

        var failures = new List<string>();
        var notes = new List<string>();
        string region = docs[0]["region"].ToString();
        int master = JInt(docs[0]["masterId"]);
        string signature = Signature(docs[0]);

        for (int i = 0; i < docs.Count; i++)
        {
            var doc = docs[i];
            string nick = doc["nick"].ToString();
            if (!JBool(doc["ok"]))
            {
                failures.Add(nick + " failed: " + doc["error"]);
            }
            if (doc["region"].ToString() != region)
            {
                failures.Add(nick + " region " + doc["region"] + " != " + region);
            }
            if (JInt(doc["masterId"]) != master)
            {
                failures.Add(nick + " master " + doc["masterId"] + " != " + master);
            }
            string sig = Signature(doc);
            if (sig != signature)
            {
                failures.Add(nick + " roster " + sig + " != " + signature);
            }

            int rtt = JInt(doc["rtt"]["median"]);
            int team = JInt(doc["teamOneWayMs"]["median"]);
            int ready = JInt(doc["readyOneWayMs"]["median"]);
            int probe = JInt(doc["probeOneWayMs"]["median"]);
            notes.Add(string.Format(
                "{0} env={1} connect={2}ms players={3}ms teams={4}ms ready={5}ms rttMed={6}ms teamOneWay={7}ms readyOneWay={8}ms probeOneWay={9}ms",
                nick,
                doc["env"],
                doc["connectMs"],
                doc["allPlayersMs"],
                doc["allTeamsMs"],
                doc["allReadyMs"],
                rtt,
                team,
                ready,
                probe));

            if (rtt > 0 && Math.Abs(rtt - JInt(docs[0]["rtt"]["median"])) > 100)
            {
                notes.Add("WARN " + nick + " RTT median differs from the first client by more than 100ms");
            }
        }

        Console.WriteLine("region " + region + " room " + docs[0]["room"] + " master " + master);
        Console.WriteLine("roster " + signature);
        for (int i = 0; i < notes.Count; i++)
        {
            Console.WriteLine(notes[i]);
        }
        if (failures.Count == 0)
        {
            Console.WriteLine("PASS lobby state matches across " + docs.Count + " clients");
            return 0;
        }
        for (int i = 0; i < failures.Count; i++)
        {
            Console.WriteLine("FAIL " + failures[i]);
        }
        return 1;
    }

    static string Signature(Newtonsoft.Json.Linq.JObject doc)
    {
        var sb = new StringBuilder();
        var actors = (Newtonsoft.Json.Linq.JArray)doc["actors"];
        for (int i = 0; i < actors.Count; i++)
        {
            var actor = actors[i];
            if (sb.Length > 0)
            {
                sb.Append(" | ");
            }
            sb.Append(actor["id"]);
            sb.Append(":");
            sb.Append(actor["nick"]);
            sb.Append(":team");
            sb.Append(actor["team"]);
            sb.Append(JBool(actor["ready"]) ? ":ready" : ":idle");
            if (JBool(actor["left"]))
            {
                sb.Append(":left");
            }
        }
        return sb.ToString();
    }

    static int JInt(Newtonsoft.Json.Linq.JToken token)
    {
        return Convert.ToInt32(token.ToString());
    }

    static bool JBool(Newtonsoft.Json.Linq.JToken token)
    {
        return string.Equals(token.ToString(), "true", StringComparison.OrdinalIgnoreCase);
    }
}
