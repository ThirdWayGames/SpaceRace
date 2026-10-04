using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Components;
using ProBuilder2.Common;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyPatrolSpawner : MonoBehaviour
{
    // The patrol objects
    private List<EnemyPatrol> PatrolObjects;

    // the game object containing the patrol objects
    public GameObject PatrolObjectContainer;

    // Safety measure to stop spawn crazyness crashing Unity.
    public int MaxEnemyCount = 50;

    // Current amount of enemies alive
    private int EnemiesAlive;

    // Used to keep track of the running couroutines
    private Dictionary<string, bool> CouroutineStates;

    // Marks whether the couroutines have been finished
    private bool CouroutinesFinished = false;

    /// <summary>
    /// On game start
    /// </summary>
    void Start()
    {
        // Check if we are master client
        if ((PhotonNetwork.inRoom && PhotonNetwork.isMasterClient) || !PhotonNetwork.inRoom)
        {
            // Get patrol objects
            GetPatrolObjects();

            // Initiate couroutines
            CouroutineStates = new Dictionary<string, bool>();

            // Spawn enemies at the spawn points
            foreach (var patrolObject in PatrolObjects)
            {
                // Create guid for couroutines
                var couroutineIdentifier = Guid.NewGuid().ToString();

                // Increase the amount of running couroutines
                CouroutineStates.Add(couroutineIdentifier, false);

                // Start the couroutine
                StartCoroutine(InitiatePatrol(patrolObject, couroutineIdentifier));
            }
        }
    }

    /// <summary>
    /// Initialite the patrols
    /// </summary>
    /// <returns></returns>
    IEnumerator InitiatePatrol(EnemyPatrol patrolObject, string couroutineIdentifier)
    {
        // Check if the courotine is running
        if (CouroutineStates[couroutineIdentifier] == false)
        {
            // Loop the enemy count
            for (int i = 0; i < patrolObject.EnemyCount; i++)
            {
                // Check if the patrol enemy count is less than the 
                if (patrolObject.EnemiesAlive < patrolObject.EnemyCount && EnemiesAlive < MaxEnemyCount)
                {
                    // Spawn an enemy
                    SpawnEnemy(patrolObject);
                }

                // Wait
                yield return new WaitForSeconds(patrolObject.TimeBetweenEnemySpawn);
            }

            // Set couroutine as finished spawning
            CouroutineStates[couroutineIdentifier] = true;
        }
    }

    /// <summary>
    /// On frame update
    /// </summary>
    private void Update()
    {
        if ((PhotonNetwork.inRoom && PhotonNetwork.isMasterClient) || !PhotonNetwork.inRoom)
        {
            // Update couroutine status update
            CouroutineStatusUpdate();
            
            //Check if the couroutines have finished
            if (CouroutinesFinished)
            {
                // Loop all of the patrol objects
                foreach (var patrolObject in PatrolObjects)
                {
                    // Check if the patrol enemy count is less than the 
                    if (patrolObject.EnemiesAlive < patrolObject.EnemyCount && EnemiesAlive < MaxEnemyCount)
                    {
                        // Check if we have any waiting to spawn items
                        if (patrolObject.WaitingToSpawn.Any())
                        {
                            // Loop the items
                            for (int i = 0; i < patrolObject.WaitingToSpawn.Count; i++)
                            {
                                // If we have finished waiting
                                if (patrolObject.WaitingToSpawn[i] <= 0)
                                {
                                    // Remove from the list
                                    patrolObject.WaitingToSpawn.RemoveAt(i);

                                    // Spawn an enemy
                                    SpawnEnemy(patrolObject);
                                }
                                else
                                {
                                    // Decrease the time left waiting
                                    patrolObject.WaitingToSpawn[i] -= Time.deltaTime;
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// Updates if the couroutines have finished
    /// </summary>
    void CouroutineStatusUpdate()
    {
        // If couroutines are not finished
        if (!CouroutinesFinished)
        {
            // If all couroutines have finished
            if (CouroutineStates != null && CouroutineStates.All(x => x.Value))
            {
                // Stop the couroutine
                StopCoroutine("InitiatePatrol");

                // Mark courutines as finished
                CouroutinesFinished = true;
            }
        }
    }


    /// <summary>
    /// Spawns an enemy
    /// </summary>
    /// <param name="patrol">The patrol object</param>
    /// <returns>Game opbject</returns>
    GameObject SpawnEnemy(EnemyPatrol patrol)
    {
        // Increase the enemies alive on the patrol object
        patrol.EnemiesAlive++;

        // Increase the enmies alive variable
        EnemiesAlive++;

        // Get the prefrab we want to spawn
        var prefabToSpawn = patrol.EnemyPrefabs.Where(x => x != null).ToList()[Random.Range(0, patrol.EnemyPrefabs.Count(x => x != null))];

        // Instantiate the object
        var enemy = PhotonNetwork.inRoom ?
            PhotonNetwork.Instantiate(prefabToSpawn.name, new Vector3(patrol.SpawnPoint.transform.position.x, 0, patrol.SpawnPoint.transform.position.z), patrol.SpawnPoint.transform.rotation, 0) :
            Instantiate(prefabToSpawn, new Vector3(patrol.SpawnPoint.transform.position.x, 0, patrol.SpawnPoint.transform.position.z), patrol.SpawnPoint.transform.rotation);

        // Get the movement component
        var movementComponent = enemy.GetComponent<EnemyMovementComponent>();

        // Check if the movement component is null
        if (movementComponent != null)
        {
            // Set the waypoints
            if (patrol.WayPoints != null)
            {
                // Set the waypoints
                movementComponent.Waypoints = new GameObject[patrol.WayPoints.transform.childCount];

                // Loop the waypoints
                for (int i = 0; i < patrol.WayPoints.transform.childCount; i++)
                {
                    // Update the waypoints on the movement component
                    movementComponent.Waypoints[i] = patrol.WayPoints.transform.GetChild(i).gameObject;
                }
            }
        }

        // Set the enemy state
        var stateComponent = enemy.GetComponent<EnemyStateComponent>();

        // Check if the state component is null
        if (stateComponent != null)
        {
            // Set state as patrol
            stateComponent.CurrentState = EnemyStateComponent.State.Patrol;
        }

        // Get the team component
        var teamComponent = enemy.GetComponent<EnemyTeamComponent>();

        // Check if it is null
        if (teamComponent != null)
        {
            // Set the team
            teamComponent.TeamIdentifier = patrol.PatrolTeam;   
            
            // Set the previous team value to max int to the system will force update the sash colours
            teamComponent.PreviousTeam = int.MaxValue;
        }

        // When the enemy dies decrease the enemies alive variable
        var healthComponent = enemy.GetComponent<HealthComponent>();

        // Check if health componnent is null
        if (healthComponent != null)
        {
            // Set death event
            healthComponent.DeathEvent += () =>
            {
                // Add waiting to spawn item
                patrol.WaitingToSpawn.Add(patrol.TimeBetweenEnemySpawn);

                // Reduce the number of enemies alive making sure it never goes below zero.
                EnemiesAlive = Mathf.Clamp(EnemiesAlive - 1, 0, patrol.EnemyCount);
                patrol.EnemiesAlive = Mathf.Clamp(patrol.EnemiesAlive - 1, 0, patrol.EnemyCount);

                // Remove the enemies
                patrol.Enemies.Remove(enemy);
            };
        }

        // Add the enemy
        patrol.Enemies.Add(enemy);

        // Return the game object
        return enemy;
    }

    /// <summary>
    /// Gets the patrol objects
    /// </summary>
    public void GetPatrolObjects()
    {
        // Instantiate the list
        PatrolObjects = new List<EnemyPatrol>();

        // Check if the object container is null
        if (PatrolObjectContainer != null)
        {
            // Loop all the children of the patrol object
            foreach (var childObject in PatrolObjectContainer.transform)
            {
                // Check if the child object is null
                if (childObject != null)
                {
                    // Check if the child object is a game object
                    if (childObject is Transform)
                    {
                        // Try get the enemy patrol componnent
                        var patrolObject = (childObject as Transform).GetComponent<EnemyPatrol>();

                        // Check if the component is null
                        if (patrolObject != null)
                        {
                            // Add the patrol object
                            PatrolObjects.Add(patrolObject);
                        }
                    }
                }
            }
        }
    }
}
