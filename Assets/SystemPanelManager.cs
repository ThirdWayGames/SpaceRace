using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SystemPanelManager : MonoBehaviour
{
    /// <summary>
    /// The system buttons
    /// </summary>
    public List<Button> SystemButtons;

    public void AlertSuccess(Button buttonClicked)
    {
        ApplyAlert(buttonClicked, SpaceRaceTheme.Success);
    }

    public void AlertFailure(Button buttonClicked)
    {
        ApplyAlert(buttonClicked, SpaceRaceTheme.Failure);
    }

    static void ApplyAlert(Button buttonClicked, Color color)
    {
        if (buttonClicked == null)
        {
            return;
        }

        var cb = buttonClicked.colors;
        cb.normalColor = color;
        cb.highlightedColor = Color.Lerp(color, Color.white, 0.28f);
        cb.pressedColor = Color.Lerp(color, Color.black, 0.2f);
        cb.disabledColor = color;
        buttonClicked.colors = cb;
        SetStatusMark(buttonClicked, color);
    }

    static void SetStatusMark(Button button, Color color)
    {
        var mark = button.transform.Find("StatusMark");
        Image image;
        if (mark == null)
        {
            var go = new GameObject("StatusMark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(button.transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-6f, 0f);
            rect.sizeDelta = new Vector2(12f, 12f);
            image = go.GetComponent<Image>();
        }
        else
        {
            image = mark.GetComponent<Image>();
            mark.gameObject.SetActive(true);
        }

        if (image != null)
        {
            image.color = color;
            image.raycastTarget = false;
        }
    }

    public void AlertComplete()
    {
        Reset();
        this.gameObject.SetActive(false);
    }

    public void Reset()
    {
        foreach (var systemButton in SystemButtons)
        {
            var cb = systemButton.colors;
            cb.normalColor = SpaceRaceTheme.ButtonNormal;
            cb.highlightedColor = SpaceRaceTheme.ButtonHighlight;
            cb.pressedColor = SpaceRaceTheme.ButtonPressed;
            cb.disabledColor = SpaceRaceTheme.ButtonDisabled;
            systemButton.colors = cb;
            var mark = systemButton.transform.Find("StatusMark");
            if (mark != null)
            {
                mark.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Gets a random list of failing systems.
    /// </summary>
    /// <returns></returns>
    public List<string> GetRandomSystemFails(int failCount, int? upperBound = null)
    {
        var results = new List<string>();

        // Make sure the fail count is clamped between the min and max amount of fails.
        var clampedUpperBound = SystemButtons.Count();
        if (upperBound.HasValue)
        {
            clampedUpperBound = Mathf.Clamp(upperBound.Value, 1, SystemButtons.Count);
        }

        failCount = Mathf.Clamp(failCount, 1, clampedUpperBound);

        for (var i = 0; i < failCount; i++)
        {
            var newEntry = false;
            while (!newEntry)
            {
                var selectedItem = SystemButtons[Random.Range(0, clampedUpperBound)];
                var selectedItemText = selectedItem.GetComponentInChildren<Text>().text;
                if (!results.Contains(selectedItemText))
                {
                    results.Add(selectedItemText);
                    newEntry = true;
                }
            }
        }

        ShuffleSystemButtons(clampedUpperBound);

        return results;
    }

    /// <summary>
    /// Shuffles the system buttons to make it harder.
    /// </summary>
    public void ShuffleSystemButtons(int? upperBound = null)
    {
        // Keep track of the items we have already shuffled.
        var shuffledIndicies = new List<int>();

        // Determine how many items we are going to shuffle between 1 to half the amount of buttons - 1.
        var clampedUpperBound = SystemButtons.Count();
        if (upperBound.HasValue)
        {
            clampedUpperBound = Mathf.Clamp(upperBound.Value, 1, SystemButtons.Count);
        }

        var shuffleAmount = clampedUpperBound / 2 - 1;
        var randomShuffleAmount = Random.Range(1, shuffleAmount > 0 ? shuffleAmount : 1);
        
        for (var i = 0; i < randomShuffleAmount; i++)
        {
            var shuffled = false;
            int shuffleAttempts = 0;
            while (!shuffled && shuffleAttempts < 3)
            {
                var availableRange = Enumerable.Range(0, SystemButtons.Count).Except(shuffledIndicies).ToArray();
                var firstRandomIndex = availableRange[Random.Range(0, availableRange.Length)];
                var secondRandomIndex = availableRange.Where(x => x != firstRandomIndex).ToArray()[Random.Range(0, availableRange.Length - 1)];
                if (firstRandomIndex != secondRandomIndex)
                {
                    // Shuffle the two buttons at the indices.
                    var firstItemVector = SystemButtons[firstRandomIndex].transform.position;
                    var secondItemVector = SystemButtons[secondRandomIndex].transform.position;

                    SystemButtons[firstRandomIndex].transform.position = secondItemVector;
                    SystemButtons[secondRandomIndex].transform.position = firstItemVector;

                    // Log the fact that they have been shuffled.
                    shuffledIndicies.Add(firstRandomIndex);
                    shuffledIndicies.Add(secondRandomIndex);
                    shuffled = true;
                }
                else
                {
                    Debug.Log("Failed to generate to distinct indicies.");
                }

                shuffleAttempts += 1;
            }

            if (shuffleAttempts >= 3)
            {
                Debug.Log("Exit occurred due to shuffle attempts exceeding attempt amount.");
            }
        }
    }
}
