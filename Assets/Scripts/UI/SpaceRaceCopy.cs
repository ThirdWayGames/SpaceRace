using System.Collections.Generic;
using System.Globalization;

public static class SpaceRaceCopy
{
    static readonly Dictionary<string, string> Exact = new Dictionary<string, string>
    {
        { "Netflix Subscription", "Auxiliary Power Bus" },
        { "Flange Wangler", "Coolant Flange" },
        { "Dyametric Gyroscope", "Diametric Gyroscope" },
        { "Plasma Lubticator", "Plasma Lubricator" },
        { "Parabolic Stabalisers", "Parabolic Stabilisers" },
        { "Energy Emmiter", "Energy Emitter" },
        { "Gamma Ray Emmiter", "Gamma Ray Emitter" },
        { "DEFENDERS LASTED {0} MINS", "Match result pending." },
        { "LOADING", "Loading match" },
    };

    static readonly Dictionary<string, string> Fragments = new Dictionary<string, string>
    {
        { "sythesise", "synthesise" },
        { "carriables", "equipment" },
        { "your teams strategy", "your team's strategy" },
        { "your opponents Power Core", "the other team's power core" },
    };

    public const string HowToPlay =
        "MOVE\n" +
        "WASD  Move\n" +
        "Shift  Sprint\n" +
        "Ctrl  Duck\n" +
        "F  Flashlight\n\n" +
        "FIGHT\n" +
        "Mouse  Aim\n" +
        "Left click  Left hand\n" +
        "Right click  Right hand\n" +
        "Q  Left fire mode\n" +
        "E  Right fire mode\n\n" +
        "CONSOLES\n" +
        "R  Use a console\n" +
        "Science console  Synthesise a pathogen, then infect the other team.\n" +
        "Equipment console  Change what your team carries.\n\n" +
        "MAP\n" +
        "M  Expand the minimap";

    public const string WinCondition = "Destroy the other team's power core before they destroy yours.";

    public const string WaitingForCrew = "Waiting for the crew to ready up.";

    public const string MildSevereLegend = "Mild signs are green. Severe signs are red.";

    public static string FormatTimeRemaining(int totalSeconds)
    {
        if (totalSeconds < 0)
        {
            totalSeconds = 0;
        }

        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;
        return string.Format("TIME: {0:00}:{1:00}", minutes, seconds);
    }

    public static string ConnectionStatus(string photonState)
    {
        switch (photonState)
        {
            case "Joining":
            case "Joined":
            case "JoinedLobby":
            case "Authenticated":
            case "ConnectedToGameserver":
            case "ConnectingToGameserver":
                return "Joining match";
            case "Disconnected":
            case "Disconnecting":
            case "DisconnectingFromMasterserver":
            case "DisconnectingFromGameserver":
            case "DisconnectingFromNameServer":
                return "Could not connect";
            default:
                return "Connecting";
        }
    }

    public static string WinAnnouncement(int winningTeam)
    {
        if (winningTeam == 1)
        {
            return "RED TEAM WINS\nThe blue core was destroyed.";
        }

        if (winningTeam == 2)
        {
            return "BLUE TEAM WINS\nThe red core was destroyed.";
        }

        return "MATCH OVER";
    }

    public static string ConsoleInstruction(string sceneName)
    {
        switch (sceneName)
        {
            case "ScienceConsole":
                return "Choose a lab. Synthesise a pathogen, research a disease, or create a treatment.";
            case "SynthLab":
                return "Place the elements in the sockets, then test the mix.";
            case "CureLab":
                return "Match the treatment elements, then test the cure.";
            case "FlameTurretBuild":
            case "BlastWallBuild":
            case "HeavyBlasterTurretBuild":
            case "RadiationBuild":
            case "DataCache":
                return "Restore every listed system before the timer runs out.";
            case "EquipmentSelector":
                return "Choose what each hand carries, then save and exit.";
            case "Database":
                return MildSevereLegend;
            case "DatabaseEquipment":
                return "Select a device to read its station notes.";
            default:
                return null;
        }
    }

    public const string RespawnLabel = "RESPAWNING:";

    public static string FormatRespawnCountdown(float seconds)
    {
        if (float.IsNaN(seconds) || seconds < 0f)
        {
            seconds = 0f;
        }

        if (seconds > 99.99f)
        {
            seconds = 99.99f;
        }

        return seconds.ToString("00.00", CultureInfo.InvariantCulture);
    }

    public static string Rewrite(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        string mapped;
        if (Exact.TryGetValue(value.Trim(), out mapped))
        {
            return mapped;
        }

        var result = value;
        foreach (var pair in Fragments)
        {
            result = result.Replace(pair.Key, pair.Value);
        }

        return result;
    }
}
