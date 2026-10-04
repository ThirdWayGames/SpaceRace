using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorForward : MonoBehaviour
{
    private EnemyPositionComponent locoAgent;

	// Use this for initialization
	void Start ()
	{
	    locoAgent = GetComponentInParent<EnemyPositionComponent>();
	}
	
	void OnAnimatorMove () {
        locoAgent.OnAnimatorMove();
    }
}
