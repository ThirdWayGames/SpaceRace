using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.ScriptableObjects;
using Photon;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(PhotonView))]
public class EnemyPatrol : PunBehaviour
{
    void Start()
    {
        WaitingToSpawn = new List<float>();
        OnConsoleExit = new EventTriggerVariable();

        if (AutoAttachSwitchTeamExitTrigger)
        {
            if (AssociatedConsole != null)
            {
                var accessConsole = AssociatedConsole.GetComponent<AccessConsole3D>();
                if (accessConsole != null)
                {
                    if (!accessConsole.OnConsoleExitTriggers.Contains(OnConsoleExit))
                    {
                        accessConsole.OnConsoleExitTriggers.Add(OnConsoleExit);
                    }
                }
            }
        }

        OnConsoleExit.TriggerEvent.AddListener(OnConsoleComplete);
    }

    [Header("Auto Add Switch Team Exit Trigger")]
    public bool AutoAttachSwitchTeamExitTrigger = true;

    [Header("Properties")]
    public GameObject AssociatedConsole;

    public string Name;

    public GameObject WayPoints;

    public GameObject SpawnPoint;

    [NonSerialized]
    public int EnemiesAlive = 0;

    public int EnemyCount = 0;

    // The enemy prefab to spawn
    public List<GameObject> EnemyPrefabs;

    // Time between the enemy spawn
    public int TimeBetweenEnemySpawn = 10;

    [NonSerialized]
    public EventTriggerVariable OnConsoleExit;

    [NonSerialized]
    public List<GameObject> Enemies = new List<GameObject>();

    [Header("Patrol Team")]
    public int PatrolTeam = 2;

    [NonSerialized]
    public List<float> WaitingToSpawn;

    [PunRPC]
    public void SwitchTeam(int newPatrolTeam)
    {
        PatrolTeam = newPatrolTeam;

        foreach (var enemy in Enemies)
        {
            var enemyTeamComponent = enemy.GetComponent<EnemyTeamComponent>();

            if (enemyTeamComponent != null)
            {
                enemyTeamComponent.TeamIdentifier = PatrolTeam;
            }
        }
    }

    public void OnConsoleComplete(FloatTrigger trigger)
    {
        if (AssociatedConsole != null)
        {
            if (trigger.value == 1f)
            {
                var accessConsole = AssociatedConsole.GetComponent<AccessConsole3D>();
                if (accessConsole != null && AssociatedConsole.gameObject.name == trigger.triggeredBy.name)
                {
                    var player = accessConsole.playerReference;

                    if (player != null)
                    {
                        var transform = ((PlayerController3D)player).GetTransform();

                        if (transform != null)
                        {
                            var teamComponent = transform.GetComponent<TeamComponent>();

                            if (teamComponent != null)
                            {
                                if (PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient)
                                {
                                    photonView.RPC("SwitchTeam", PhotonNetworkSettings.EventTarget, new object[] { teamComponent.TeamIdentifier });
                                }
                                else
                                {
                                    this.SwitchTeam(teamComponent.TeamIdentifier);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
