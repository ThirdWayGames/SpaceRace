using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class CircuitGameManager : MonoBehaviour, IConsoleSceneController
{
    private static CircuitGameManager instance;
    public static CircuitGameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CircuitGameManager>();

                if (instance == null)
                {
                    Debug.LogError("There is no circuit game manager found in the scene");
                }
            }

            return instance;
        }
    }

    public GameObject SpawnPoint;
    public GameObject Player;
    public float TimeRemaining = 30;
    public Text TimeRemainingText;
    public Text LivesRemainingText;
    public Text CountDownText;
    public Image PassImage;
    public Image FailImage;

    public IConsole CurrentConsole { get; set; }
    private CircuitGameDifficultySettings DifficultySettings;
    public int RemainingTries = 1;
    public float TimeShowingResultImage = 1;

    public bool CountingDown = true;
    public bool FirstCountDownFinished = false;
    public float CountDownTime = 3;
    public float CountDownRemaining = 3;

	// Update is called once per frame
	void Update ()
	{
	    if (CountingDown)
        { 
	        UpdateCountdown();
	    }

        UpdateTimeRemaining();
        UpdateRemainingLivesText();
    }

    public void UpdateTimeRemaining()
    {
        if (TimeRemainingText != null)
        {
            if (TimeRemaining >= 0.00)
            {
                TimeRemainingText.text = string.Format("{0}s", TimeRemaining.ToString("0.00"));

                if (FirstCountDownFinished)
                {
                    TimeRemaining -= Time.deltaTime;
                }
            }
            else
            {
                TimeRemainingText.text = string.Format("{0}s", "0.00");
                OnFail();
            }
        }
    }

    public void OnSuccess()
    {
        // Show the pass image
        StartCoroutine(EndGame(PassImage, () =>
        {
            // If the console is set and not locked.
            if (CurrentConsole != null && !CurrentConsole.IsLocked())
            {
                // Send the failed console result.
                CurrentConsole.SetConsoleResult(1f);
            }
        }));
    }

    IEnumerator EndGame(Image image, Action callback)
    {
        // Destroy the player
        Destroy(Player);

        // If image is not null
        if (image != null)
        {
            // Show image
            image.enabled = true;
        }

        yield return new WaitForSeconds(TimeShowingResultImage);

        callback.Invoke();
    }

    public void OnFail()
    {
        // Show the fail image
        StartCoroutine(EndGame(FailImage, () =>
        {
            // If the console is set and not locked.
            if (CurrentConsole != null && !CurrentConsole.IsLocked())
            {
                // Send the failed console result.
                CurrentConsole.SetConsoleResult(-1f);
            }
        }));
    }

    public void OnWallCollision()
    {
        RemainingTries--;
        if (RemainingTries <= 0)
        {
            OnFail();
        }
        else
        {
            ResetPlayer();
            StartCountdown();
        }
    }

    public void StartCountdown()
    {
        CountDownRemaining = CountDownTime + 0.49f;
        CountDownText.enabled = true;
        CountingDown = true;
    }

    public void UpdateCountdown()
    {
        if (CountDownText != null)
        {
            CountDownText.text = ((int)CountDownRemaining).ToString();

            if (CountDownRemaining <= 1)
            {
                CountDownText.text = "1";
                CountDownText.enabled = false;
                CountingDown = false;
                FirstCountDownFinished = true;
            }

            CountDownRemaining -= Time.deltaTime;
        }
    }

    public void UpdateRemainingLivesText()
    {
        if (LivesRemainingText != null)
        {
            LivesRemainingText.text = RemainingTries.ToString();
        }
    }

    public void ResetPlayer()
    {
        if (Player != null)
        {
            if (SpawnPoint != null)
            {
                var trailRenderer = Player.GetComponent<TrailRenderer>();
                var circuitPlayerController = Player.GetComponent<CircuitPlayerController>();
                if (circuitPlayerController != null && trailRenderer != null)
                {
                    if (circuitPlayerController.Trails != null && circuitPlayerController.Trails.Any())
                    {
                        foreach (var trail in circuitPlayerController.Trails)
                        {
                            Destroy(trail);
                        }
                    }

                    circuitPlayerController.Trails = new List<GameObject>();
                    circuitPlayerController.Direction = null;
                    trailRenderer.Clear();
                    trailRenderer.enabled = false;
                    Player.transform.position = SpawnPoint.transform.position;
                    trailRenderer.enabled = true;
                }
            }
        }
    }

    public void InitaliseMiniGame(IConsole currentConsole)
    {
        var root = GameObject.Find("Root");
        if (root != null)
        {
            root.transform.position = new Vector3(500, 500, 500);
            CurrentConsole = currentConsole;
            if (CurrentConsole != null)
            {
                DifficultySettings = CurrentConsole.GetDifficultySettings() as CircuitGameDifficultySettings;
                if (DifficultySettings == null)
                {
                    Debug.LogError(string.Format("No Difficulty Settings detected for {0}", this.gameObject.name));
                    DifficultySettings = new CircuitGameDifficultySettings();
                }

                TimeRemaining = DifficultySettings.TimeAllowed;
                UpdateTimeRemaining();

                RemainingTries = DifficultySettings.AllowedAmountOfTries;
                StartCountdown();

                var playerManager = Player.GetComponent<CircuitPlayerController>();

                if (playerManager != null)
                {
                    playerManager.MoveSpeed = DifficultySettings.PlayerSpeed;
                }
            }
            else
            {
                Debug.LogError(string.Format("No Current Console set for {0} when activated", this.gameObject.name));
            }
        }
    }
}
