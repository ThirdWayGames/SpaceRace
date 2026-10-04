using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Components;
using ProBuilder2.Common;
using Random = System.Random;

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave Management")]
    public Wave[] Waves;

    [Header("Spawn Management")]
    public SpawnGroup[] SpawnGroups;

    private float WaveTimer = 0f;

    [Header("Settings")]
    public GameObject EnemyDestination;
    public float TimeBetweenWaves = 5f;


    private float NextWaveCountdown = 0;
    private bool CountingDown = false;
    private Wave CurrentWave;

    void Start()
    {
        // Check values
        if (SpawnGroups == null || !SpawnGroups.All(x => x.SpawnTransforms.Any()))
        {
            throw new Exception("SpawnGroups cannot be null or Empty. All spawn groups must have atleast one spawn transform.");
        }
    }

    void Update()
    {
        // Check for wave objects
        if (Waves != null && Waves.Any())
        {
            // Try countdown next wave
            CountdownNextWave();

            // Check if it is new level or wave is complete
            if (CurrentWave == null || (CurrentWave.WaveComplete && CurrentWave.FinishedSpawning))
            {
                // Check to see if the countdown is currently active
                if (!CountingDown)
                {
                    // Get the next wave
                    CurrentWave = GetNextWave();

                    // Check if there is a current wave
                    if (CurrentWave != null)
                    {
                        // Start spawning the wave
                        StartCoroutine(SpawnWave());
                    }
                    else
                    {
                        // Close the wave spawner
                        CloseWaveSpawner();
                    }
                }
            }

            // Check to see if the wave is complete
            CheckForWaveComplete();
        }
    }

    /// <summary>
    /// Count down the next wave
    /// </summary>
    private void CountdownNextWave()
    {
        // Check if the countdown is greater than 0
        if (NextWaveCountdown >= 0)
        {
            // Countdown the timer
            NextWaveCountdown -= Time.deltaTime;
            NextWaveCountdown = Mathf.Clamp(NextWaveCountdown, 0f, Mathf.Infinity);

            // Check if the next wave countdown is 0
            if ((int)NextWaveCountdown == 0)
            {
                // Set counting down to false
                CountingDown = false;
            }
        }
    }

    /// <summary>
    /// Starts the next wave countdown
    /// </summary>
    private void StartNextWaveCountdown()
    {
        // Set counting down to true
        CountingDown = true;

        // Set the wave countdown
        NextWaveCountdown = TimeBetweenWaves;
    }

    /// <summary>
    /// Closes the wave spawner
    /// </summary>
    private void CloseWaveSpawner()
    {
        // Disable this script
        this.enabled = false;
    }

    private void CheckForWaveComplete()
    {
        // Check if there is a current wave and the wave has completed
        if (CurrentWave != null && CurrentWave.FinishedSpawning && !CurrentWave.WaveComplete)
        {
            // Check if the wave can be completed by eliminating all of the bots
            if (CurrentWave.EliminateWaveToAdvance)
            {
                // Check if enemies are alive on this wave
                if (CurrentWave.FinishedSpawning && !CurrentWave.EnemyTypes.SelectMany(x => x.EnemiesAlive).Any())
                {
                    // Set the wave as complete and start the next countdown
                    CurrentWave.WaveComplete = true;
                    StartNextWaveCountdown();
                }
            }

            // Decrease the wave timer
            WaveTimer += Time.deltaTime;
            WaveTimer = Mathf.Clamp(WaveTimer, 0f, Mathf.Infinity);

            // Check if the wave timer is greater than the timeout
            if (WaveTimer >= CurrentWave.WaveTimeout)
            {
                // Set the wave as complete and start the next countdown// Set the 
                CurrentWave.WaveComplete = true;
                StartNextWaveCountdown();
            }
        }
    }

    /// <summary>
    /// Gets the next wave
    /// </summary>
    /// <returns>Tee wave</returns>
    private Wave GetNextWave()
    {
        // Set default wave as null
        Wave wave = null;

        // Check if current wave is null, if so then get first wave
        if (CurrentWave == null)
        {
            // Get first wave
            wave = Waves[0];
        }
        else
        {
            // Get the current wave index
            var currentWaveIndex = Waves.ToList().IndexOf(CurrentWave);

            // Check if the next wave index is in range
            if (currentWaveIndex + 1 < Waves.Length)
            {
                // Get the range
                wave = Waves[currentWaveIndex + 1];
            }
        }

        // Return the wave
        return wave;
    }

    /// <summary>
    /// Spawns a new wave
    /// </summary>
    /// <returns>Return ienumerator</returns>
    IEnumerator SpawnWave()
    {
        // Set wave timer at 0
        WaveTimer = 0;

        // Get the current wave
        var wave = CurrentWave;

        // Loop the enemy types in the wave
        for (int x = 0; x < wave.EnemyTypes.Length; x++)
        {
            // Get the enemy type
            var enemyType = wave.EnemyTypes[x];

            // Loop each enemy
            for (int i = 0; i < enemyType.EnemyCount; i++)
            {
                // Spawn the enemy
                var enemy = SpawnEnemy(enemyType);

                // Check if the enemies alive list has a value
                if (enemyType.EnemiesAlive == null)
                {
                    // Initialise the list
                    enemyType.EnemiesAlive = new List<GameObject>();
                }

                // Add the enemy gameobject to the list of enemies alive
                enemyType.EnemiesAlive.Add(enemy);

                // Get the enemy target component
                var enemyTargetComponent = enemy.GetComponent<EnemyTargetComponent>();

                // Add the enemy destination as a known threat
                enemyTargetComponent.KnownThreats.Add(new ThreatLevel()
                {
                    Target = EnemyDestination,
                    IsHostile = true
                });

                // Get the enemy state component
                var enemyStateComponent = enemy.GetComponent<EnemyStateComponent>();

                // Set the current state as wave
                enemyStateComponent.CurrentState = EnemyStateComponent.State.Wave;

                // Wait for period between enemy spawn
                yield return new WaitForSeconds(1 * enemyType.RateOfSpawn);
            }

            // Wait for period between enemy types
            yield return new WaitForSeconds(1 * enemyType.TimeUntilNextEnemyType);
        }

        // Mark the current wave as finished spawning
        CurrentWave.FinishedSpawning = true;
    }

    /// <summary>
    /// Spawns an enemy ai
    /// </summary>
    /// <param name="enemyType"></param>
    /// <returns>The gameobject of the spawned enemy</returns>
    GameObject SpawnEnemy(EnemyType enemyType)
    {
        // Get the spawn point name
        var spawnPointName = enemyType.SpawnPoint;

        // Set default spawn group as null
        SpawnGroup selectedSpawnGroup = null;

        // Check if the spawnpoint name is valid
        if (!string.IsNullOrWhiteSpace(spawnPointName) && spawnPointName.ToLower() != "any")
        {
            // Get the first spawn point with the spawn point name
            var spawnGroup = SpawnGroups.FirstOrDefault(x => x.Name.ToLower() == spawnPointName.ToLower());
            if (spawnGroup != null)
            {
                // Set the selected spawn group
                selectedSpawnGroup = spawnGroup;
            }
            else
            {
                // Throw exception letting the developer know that the spawn group does not exist
                throw new Exception("Spawn group name provided that does not exist. Please choose from the list of existing spawn groups.");
            }
        }

        // Check to see if the selected spawn group is still null
        if (selectedSpawnGroup == null)
        {
            // Choose a random spawn group
            selectedSpawnGroup = SpawnGroups[UnityEngine.Random.Range(0, SpawnGroups.Length)];
        }

        // Choose a random spawn point inside of the spawn group
        var selectedTransformSpawn = selectedSpawnGroup.SpawnTransforms[UnityEngine.Random.Range(0, selectedSpawnGroup.SpawnTransforms.Length)];

        // Instantiate the enemy
        var enemy = Instantiate(enemyType.Enemy, new Vector3(selectedTransformSpawn.position.x, 0, selectedTransformSpawn.position.z), selectedTransformSpawn.rotation);

        // Get the enemy health component
        var enemyHealthComponent = enemy.GetComponent<HealthComponent>();
        if (enemyHealthComponent != null)
        {
            // Add on death event function
            enemyHealthComponent.DeathEvent += () =>
            {
                // Trigger enemy type on death event script on death of the ai
                enemyHealthComponent.TriggerOnDeathScript(enemyType);
            };
        }

        // Return the enemy
        return enemy;
    }

    [Serializable]
    public class SpawnGroup
    {
        [Header("Spawn Group Name")]
        public string Name;

        [Header("Spawn Location Objects")]
        public Transform[] SpawnTransforms;
    }
}







