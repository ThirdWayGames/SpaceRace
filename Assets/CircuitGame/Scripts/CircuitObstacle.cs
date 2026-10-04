using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitObstacle : MonoBehaviour
{

    public GameObject StartPoint;
    public GameObject EndPoint;
    public float speed = 1.0f;

    private GameObject CurrentTarget;

    // Use this for initialization
    void Start ()
    {
        CurrentTarget = EndPoint;
    }
	
	// Update is called once per frame
    void Update()
    {
        if (CurrentTarget != null)
        {
            // Move our position a step closer to the target.
            float step = speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, CurrentTarget.transform.position, step);

            // Check if the position of the cube and sphere are approximately equal.
            if (Vector3.Distance(transform.position, CurrentTarget.transform.position) < 0.001f)
            {
                if (CurrentTarget == EndPoint)
                {
                    CurrentTarget = StartPoint;
                } else if (CurrentTarget == StartPoint)
                {
                    CurrentTarget = EndPoint;
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        CircuitGameManager.Instance.ResetPlayer();
    }
}
