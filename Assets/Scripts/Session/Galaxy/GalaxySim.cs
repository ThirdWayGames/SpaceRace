using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GalaxySim
{
    public const float OutsideStormDrainPerSecond = 10f;

    public const float HostileOverlapDrainPerSecond = 10f;

    public ShipPose Pose = new ShipPose();

    public float Hull = 100f;

    public StormFront Storm = new StormFront();

    public GalaxyMap Map = new GalaxyMap();

    public EscapePhaseMachine Phase = new EscapePhaseMachine();

    public CrewRoster Roster = new CrewRoster();

    public PilotCommand Command = new PilotCommand();

    public string CurrentPoiId = string.Empty;

    public readonly List<string> Equipment = new List<string>();

    ShipPose lastInsidePose;

    public static GalaxySim CreateSample()
    {
        var sim = new GalaxySim();
        sim.Storm.Radius = 400f;
        sim.Storm.ShrinkPerSecond = 1f;
        sim.Pose = new ShipPose(0f, 300f, 180f, 0f);
        sim.Map = GalaxyMap.CreateSample();
        sim.Phase.Current = EscapePhase.Transiting;
        return sim;
    }

    public void SetCommand(PilotCommand command)
    {
        Command = command == null ? new PilotCommand() : new PilotCommand(command.Throttle, command.Turn);
    }

    public void Tick(float dt, float powerLevel)
    {
        if (dt < 0f)
        {
            dt = 0f;
        }

        if (!Phase.IsTerminal())
        {
            Storm.Radius = Mathf.Max(0f, Storm.Radius - (Storm.ShrinkPerSecond * dt));
        }

        if (Phase.Current == EscapePhase.Docked || Phase.Current == EscapePhase.Transiting)
        {
            RememberInsidePose();
            Pose = ShipFlight.Integrate(Pose, Command, powerLevel, dt);
        }

        if (Phase.Current == EscapePhase.Transiting)
        {
            var arrived = FindContainingPoint();
            if (arrived != null)
            {
                CurrentPoiId = arrived.Id;
                Phase.NotifyArrived();
            }
        }

        if (Phase.Current == EscapePhase.Docked && !string.IsNullOrEmpty(CurrentPoiId) && !IsInside(Map.FindPoint(CurrentPoiId)))
        {
            var change = Phase.TryUndock(Roster.AllAboard());
            if (change.Accepted)
            {
                CurrentPoiId = string.Empty;
            }
            else if (lastInsidePose != null)
            {
                Pose = lastInsidePose.ClonePose();
            }
        }
        else
        {
            RememberInsidePose();
        }

        if (!Phase.IsTerminal() && !IsInsideStorm())
        {
            Hull -= OutsideStormDrainPerSecond * dt;
        }

        if (!Phase.IsTerminal())
        {
            Hull -= HostileOverlapDrain() * dt;
        }

        if (Hull < 0f)
        {
            Hull = 0f;
        }

        if (!Phase.IsTerminal() && (Storm.Radius <= 0f || Hull <= 0f))
        {
            Phase.NotifyConsumed();
        }
    }

    public bool TryReveal(float arcDegrees, float range)
    {
        var revealed = false;
        var halfArc = arcDegrees * 0.5f;
        for (int i = 0; i < Map.Contacts.Count; i++)
        {
            var contact = Map.Contacts[i];
            if (contact.Revealed)
            {
                continue;
            }

            if (Distance(Pose.X, Pose.Z, contact.X, contact.Z) > range)
            {
                continue;
            }

            var bearing = Mathf.Atan2(contact.X - Pose.X, contact.Z - Pose.Z) * Mathf.Rad2Deg;
            if (Mathf.Abs(Mathf.DeltaAngle(Pose.HeadingDegrees, bearing)) > halfArc)
            {
                continue;
            }

            contact.Revealed = true;
            revealed = true;
        }

        return revealed;
    }

    public void DamageContact(string id, float amount)
    {
        var contact = Map.FindContact(id);
        if (contact == null || contact.Kind != GalaxyContactKind.Hostile)
        {
            return;
        }

        contact.Health -= amount;
        if (contact.Health < 0f)
        {
            contact.Health = 0f;
        }
    }

    public void SetEquipment(string id)
    {
        if (string.IsNullOrEmpty(id) || Equipment.Contains(id))
        {
            return;
        }

        Equipment.Add(id);
    }

    public bool HasEquipment(string id)
    {
        return Equipment.Contains(id);
    }

    public bool TryWormhole()
    {
        var point = Map.FindPoint(CurrentPoiId);
        if (point == null || !point.IsWormhole || !HasEquipment("coil"))
        {
            return false;
        }

        return Phase.TryEscape(Roster.AllAboard(), true).Accepted;
    }

    public string RevealedIds()
    {
        var builder = new StringBuilder();
        for (int i = 0; i < Map.Contacts.Count; i++)
        {
            if (!Map.Contacts[i].Revealed)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append(",");
            }

            builder.Append(Map.Contacts[i].Id);
        }

        return builder.ToString();
    }

    public GalaxySnapshot Capture()
    {
        return new GalaxySnapshot
        {
            X = Pose.X,
            Z = Pose.Z,
            HeadingDegrees = Pose.HeadingDegrees,
            Speed = Pose.Speed,
            StormRadius = Storm.Radius,
            Hull = Hull,
            PhaseName = Phase.Current.ToString(),
            CurrentPoiId = CurrentPoiId ?? string.Empty,
            RevealedContactIds = RevealedIds(),
            ContactSummary = ContactSummary()
        };
    }

    string ContactSummary()
    {
        var builder = new StringBuilder();
        for (int i = 0; i < Map.Contacts.Count; i++)
        {
            var contact = Map.Contacts[i];
            if (builder.Length > 0)
            {
                builder.Append(";");
            }

            builder.Append(contact.Id);
            builder.Append(",");
            builder.Append(contact.X.ToString("0.##"));
            builder.Append(",");
            builder.Append(contact.Z.ToString("0.##"));
            builder.Append(",");
            builder.Append(contact.Kind.ToString());
            builder.Append(",");
            builder.Append(contact.Revealed ? "1" : "0");
            builder.Append(",");
            builder.Append(contact.Health.ToString("0.##"));
        }

        return builder.ToString();
    }

    float HostileOverlapDrain()
    {
        var drain = 0f;
        for (int i = 0; i < Map.Contacts.Count; i++)
        {
            var contact = Map.Contacts[i];
            if (contact.Kind != GalaxyContactKind.Hostile || contact.Health <= 0f)
            {
                continue;
            }

            if (Distance(Pose.X, Pose.Z, contact.X, contact.Z) <= contact.Radius)
            {
                drain += HostileOverlapDrainPerSecond;
            }
        }

        return drain;
    }

    void RememberInsidePose()
    {
        var point = Map.FindPoint(CurrentPoiId);
        if (point != null && IsInside(point))
        {
            lastInsidePose = Pose.ClonePose();
        }
    }

    PointOfInterest FindContainingPoint()
    {
        for (int i = 0; i < Map.Points.Count; i++)
        {
            if (IsInside(Map.Points[i]))
            {
                return Map.Points[i];
            }
        }

        return null;
    }

    bool IsInside(PointOfInterest point)
    {
        if (point == null)
        {
            return false;
        }

        return Distance(Pose.X, Pose.Z, point.X, point.Z) <= point.Radius;
    }

    bool IsInsideStorm()
    {
        return Distance(Pose.X, Pose.Z, Storm.CenterX, Storm.CenterZ) <= Storm.Radius;
    }

    static float Distance(float x1, float z1, float x2, float z2)
    {
        var dx = x1 - x2;
        var dz = z1 - z2;
        return Mathf.Sqrt((dx * dx) + (dz * dz));
    }
}
