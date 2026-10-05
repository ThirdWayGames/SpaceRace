using System.Collections.Generic;
using System.IO;
using System.Text;
using Assets.Scripts.Components;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Plays a 2v2 with the same hit path bullets use: MutationController.AddMutation
/// loads HeavyBlasterBulletDmg and applies it to HealthComponent. The win rule
/// matches GameStateManager: the team whose core is still above zero wins.
/// </summary>
public class TwoTeamCombatMatch
{
    const string WeaponHit = "HeavyBlasterBulletDmg";
    const string HealHit = "HealBlasterBulletDmg";
    const float HitDamage = 66f;

    class Fighter
    {
        public string Name;
        public int Team;
        public bool HuntsPlayers;
        public bool IsCore;
        public bool Alive = true;
        public GameObject Body;
        public HealthComponent Health;
        public MutationController Mutations;
    }

    readonly List<GameObject> spawned = new List<GameObject>();

    [Test]
    public void TwoTeamsOfTwoPlayToCoreKill()
    {
        var log = new StringBuilder();
        try
        {
            var redCore = SpawnCore("Red Core", "PowerCoreRed", 1);
            var blueCore = SpawnCore("Blue Core", "PowerCoreBlue", 2);
            var red1 = SpawnMarine("Red-1", "BasicCloneRed", 1, false);
            var red2 = SpawnMarine("Red-2", "BasicCloneRed", 1, false);
            var blue1 = SpawnMarine("Blue-1", "BasicCloneBlue", 2, true);
            var blue2 = SpawnMarine("Blue-2", "BasicCloneBlue", 2, true);
            var marines = new List<Fighter> { red1, red2, blue1, blue2 };

            log.AppendLine("2v2 combat match");
            log.AppendLine("Weapon hit: " + WeaponHit + " (" + HitDamage + " health)");
            log.AppendLine("Red hunts the blue core. Blue hunts living red marines, then the red core.");
            log.AppendLine(string.Format(
                "Start  red core {0}  blue core {1}  marine health {2}",
                redCore.Health.CurrentValue,
                blueCore.Health.CurrentValue,
                red1.Health.CurrentValue));

            AssertHealDoesNotDamageCore(redCore, log);
            AssertFriendlyFireIsUnfiltered(log);

            var order = new List<Fighter> { red1, red2, blue1, blue2 };
            int winner = 0;
            string killingBlow = "";
            int rounds = 0;
            for (rounds = 1; rounds <= 80 && winner == 0; rounds++)
            {
                log.AppendLine("Round " + rounds);
                bool anyShot = false;
                for (int i = 0; i < order.Count; i++)
                {
                    var shooter = order[i];
                    if (!shooter.Alive)
                    {
                        continue;
                    }

                    var target = ChooseTarget(shooter, marines, redCore, blueCore);
                    if (target == null)
                    {
                        continue;
                    }

                    anyShot = true;
                    float before = target.Health.CurrentValue;
                    target.Mutations.AddMutation(WeaponHit, shooter.Team);
                    float after = target.Health.CurrentValue;
                    bool downedNow = target.Alive && after <= 0f;
                    if (downedNow)
                    {
                        target.Alive = false;
                    }

                    log.AppendLine(string.Format(
                        "  {0} -> {1}  {2} -> {3} ({4}){5}",
                        shooter.Name,
                        target.Name,
                        before,
                        after,
                        after - before,
                        downedNow ? (target.IsCore ? "  CORE DOWN" : "  DOWN") : ""));

                    if (!target.IsCore && target.Alive)
                    {
                        Assert.AreEqual(HitDamage, before - after, shooter.Name + " hit on " + target.Name + " was not a heavy blaster tick");
                    }
                    else if (before > 0f)
                    {
                        Assert.AreEqual(HitDamage, before - after, shooter.Name + " hit on " + target.Name + " was not a heavy blaster tick");
                    }

                    if (target.IsCore && after <= 0f)
                    {
                        winner = redCore.Health.CurrentValue > 0f ? 1 : 2;
                        killingBlow = shooter.Name + " destroyed " + target.Name;
                        break;
                    }
                }

                Assert.IsTrue(anyShot, "Round " + rounds + " had no shots. Combat stalled.");
            }

            log.AppendLine(winner == 1 ? "RED TEAM WINS" : winner == 2 ? "BLUE TEAM WINS" : "NO WINNER");
            log.AppendLine(killingBlow);
            log.AppendLine(string.Format(
                "End  red core {0} ({1})  blue core {2} ({3})",
                redCore.Health.CurrentValue,
                redCore.Alive ? "up" : "down",
                blueCore.Health.CurrentValue,
                blueCore.Alive ? "up" : "down"));
            for (int i = 0; i < marines.Count; i++)
            {
                log.AppendLine(string.Format(
                    "  {0} health {1} {2}",
                    marines[i].Name,
                    marines[i].Health.CurrentValue,
                    marines[i].Alive ? "up" : "down"));
            }

            Assert.AreEqual(2, winner, "Blue should take the core after the red marines are downed.");
            Assert.LessOrEqual(redCore.Health.CurrentValue, 0f);
            Assert.Greater(blueCore.Health.CurrentValue, 0f);
            Assert.IsFalse(red1.Alive);
            Assert.IsFalse(red2.Alive);
            Assert.IsTrue(blue1.Alive);
            Assert.IsTrue(blue2.Alive);
            Assert.AreEqual(redCore.Health.CurrentValue > 0f ? 1 : 2, winner);
        }
        finally
        {
            var text = log.ToString();
            Debug.Log(text);
            Directory.CreateDirectory("/opt/cursor/artifacts");
            File.WriteAllText("/opt/cursor/artifacts/combat-match.txt", text);
            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] != null)
                {
                    Object.DestroyImmediate(spawned[i]);
                }
            }
        }
    }

    static void AssertHealDoesNotDamageCore(Fighter core, StringBuilder log)
    {
        float before = core.Health.CurrentValue;
        core.Mutations.AddMutation(HealHit, 2);
        log.AppendLine(string.Format(
            "Heal shot on {0} (core lists {1} as an immunity): {2} -> {3}",
            core.Name,
            HealHit,
            before,
            core.Health.CurrentValue));
        Assert.AreEqual(before, core.Health.CurrentValue, "A heal-blaster hit changed core health.");
    }

    void AssertFriendlyFireIsUnfiltered(StringBuilder log)
    {
        var ally = SpawnMarine("Red-ally", "BasicCloneRed", 1, false);
        float before = ally.Health.CurrentValue;
        ally.Mutations.AddMutation(WeaponHit, 1);
        log.AppendLine(string.Format(
            "Friendly fire {0} shot by team 1: {1} -> {2}",
            ally.Name,
            before,
            ally.Health.CurrentValue));
        Assert.AreEqual(before - HitDamage, ally.Health.CurrentValue, "Same-team heavy blaster hit was filtered out.");
        ally.Alive = false;
    }

    static Fighter ChooseTarget(Fighter shooter, List<Fighter> marines, Fighter redCore, Fighter blueCore)
    {
        var enemyCore = shooter.Team == 1 ? blueCore : redCore;
        if (!shooter.HuntsPlayers)
        {
            return enemyCore.Alive ? enemyCore : null;
        }

        Fighter lowest = null;
        for (int i = 0; i < marines.Count; i++)
        {
            var marine = marines[i];
            if (marine.Team == shooter.Team || !marine.Alive)
            {
                continue;
            }

            if (lowest == null || marine.Health.CurrentValue < lowest.Health.CurrentValue)
            {
                lowest = marine;
            }
        }

        if (lowest != null)
        {
            return lowest;
        }

        return enemyCore.Alive ? enemyCore : null;
    }

    Fighter SpawnMarine(string name, string prefab, int team, bool huntsPlayers)
    {
        var body = Spawn(prefab, name);
        var teamComponent = body.GetComponent<TeamComponent>();
        Assert.IsNotNull(teamComponent, name + " has no TeamComponent");
        teamComponent.TeamIdentifier = team;
        return Bind(name, team, huntsPlayers, false, body);
    }

    Fighter SpawnCore(string name, string prefab, int team)
    {
        var body = Spawn(prefab, name);
        return Bind(name, team, false, true, body);
    }

    Fighter Bind(string name, int team, bool huntsPlayers, bool isCore, GameObject body)
    {
        var fighter = new Fighter();
        fighter.Name = name;
        fighter.Team = team;
        fighter.HuntsPlayers = huntsPlayers;
        fighter.IsCore = isCore;
        fighter.Body = body;
        fighter.Health = body.GetComponent<HealthComponent>();
        fighter.Mutations = body.GetComponent<MutationController>();
        Assert.IsNotNull(fighter.Health, name + " has no HealthComponent");
        Assert.IsNotNull(fighter.Mutations, name + " has no MutationController");
        if (fighter.Mutations.Immunities == null)
        {
            fighter.Mutations.Immunities = new List<string>();
        }
        return fighter;
    }

    GameObject Spawn(string prefabName, string name)
    {
        var prefab = Resources.Load<GameObject>(prefabName);
        Assert.IsNotNull(prefab, "Missing Resources prefab " + prefabName);
        var body = Object.Instantiate(prefab);
        body.name = name;
        spawned.Add(body);
        return body;
    }
}
