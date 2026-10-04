using System;
using Assets.Scripts.Enums;
using UnityEngine;

public class GameStateController : MonoBehaviour
{
    public Sprite YouWin;
    public Sprite YouLose;
    public Sprite Draw;

    public void DisplayWinState(Controller winState)
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            throw new Exception("No Sprite Renderer found on GameStateController.");
        }


        if (winState != Controller.Noone)
        {
            spriteRenderer.sprite = winState == Controller.Player ? YouWin : winState == Controller.Computer ? YouLose : Draw;
        }
        else
        {
            spriteRenderer.sprite = null;
        }

    }
}
