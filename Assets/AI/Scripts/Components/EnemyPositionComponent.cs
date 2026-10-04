using UnityEngine;
using UnityEngine.AI;

public class EnemyPositionComponent : MonoBehaviour {
    public Animator anim;
    public NavMeshAgent agent;
    public Vector2 smoothDeltaPosition = Vector2.zero;
    public Vector2 velocity = Vector2.zero;

    // Use this for initialization
    void Start () {
	    anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
    }

    public void OnAnimatorMove()
    {
        //// Update position based on animation movement using navigation surface height
        //if (anim != null)
        //{
        //    Vector3 position = anim.rootPosition;
        //    //Vector3 position = agent.nextPosition;
        //    //      Debug.Log(anim.rootPosition);
        //    //position.y = agent.nextPosition.y;
        //    position.y = 0;
        //    transform.position = position;
        //    transform.rotation = anim.rootRotation;
        //    agent.speed = (anim.deltaPosition / Time.deltaTime).magnitude;
        //}
    }
}
