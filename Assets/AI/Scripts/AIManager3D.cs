using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager3D : MonoBehaviour {

    private static AIManager3D AIManager;

    public static AIManager3D instance
    {
        get
        {
            if (!AIManager)
            {
                AIManager = FindObjectOfType<AIManager3D>() as AIManager3D;

                if (!AIManager)
                {
                    Debug.LogError("There is no AIManager found in the scene");
                }
            }

            return AIManager;
        }
    }

    // Use this for initialization
    void Start()
    {
        // Assign enemy positions, maybe in a system

        // The state of the map, so attack and locate, guard ect

        // If the map is in attack mode we want some enemies to go and find the players while some stay and guard the important elements

        // 
    }

    // Update is called once per frame
    void Update()
    {

    }

}
