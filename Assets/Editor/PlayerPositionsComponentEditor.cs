using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerPositionsComponent))]
public class PlayerPositionsComponentEditor : Editor {

    void OnSceneGUI()
    {
        PlayerPositionsComponent playerPositions = (PlayerPositionsComponent)target;
        Handles.color = Color.green;
        foreach (var playerPos in playerPositions.PlayerPositions)
        {
            if (playerPos != null)
            {
                Handles.DrawLine(playerPositions.transform.position, playerPos.transform.position);
            }
        }
    }
}
