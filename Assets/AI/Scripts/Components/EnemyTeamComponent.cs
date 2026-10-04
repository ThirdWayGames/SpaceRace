using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTeamComponent : MonoBehaviour {

    /// <summary>
    /// Used to indentify each player for each team
    /// </summary>
    public int TeamIdentifier;

    public int? PreviousTeam;

    public GameObject ColourIdentifier;
}
