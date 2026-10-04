using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class LocomotionSimpleAgentOG : MonoBehaviour
{
    Animator anim;
    NavMeshAgent agent;
    Vector2 smoothDeltaPosition = Vector2.zero;
    Vector2 velocity = Vector2.zero;

    public GameObject AimAtObect;

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
    }

    void Update()
    {
        Aim();
        Vector3 worldDeltaPosition = agent.nextPosition - transform.position;

        // Map 'worldDeltaPosition' to local space
        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        // Low-pass filter the deltaMove
        float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

        // Update velocity if delta time is safe
        if (Time.deltaTime > 1e-5f)
            velocity = smoothDeltaPosition / Time.deltaTime;

        bool shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius;

        // Update animation parameters
        anim.SetBool("move", shouldMove);
        anim.SetFloat("velx", velocity.x);
        anim.SetFloat("vely", velocity.y);

        LookAt lookAt = GetComponent<LookAt>();
        if (lookAt)
            lookAt.lookAtTargetPosition = agent.steeringTarget + transform.forward;

        //		// Pull character towards agent
        if (worldDeltaPosition.magnitude > agent.radius)
            transform.position = agent.nextPosition - 0.9f * worldDeltaPosition;

        //		// Pull agent towards character
        //		if (worldDeltaPosition.magnitude > agent.radius)
        //			agent.nextPosition = Trainsform.position + 0.9f*worldDeltaPosition;
    }

    void OnAnimatorMove()
    {
        // Update postion to agent position
        //Trainsform.position = agent.nextPosition;

        // Update position based on animation movement using navigation surface height
        //Vector3 position = anim.rootPosition;
        //position.y = agent.nextPosition.y;
        //transform.position = position;
    }

    public GameObject Hips;


    void Aim()
    {
        if (Hips != null && anim.GetBool("Aiming"))
        {
            var _direction = (AimAtObect.transform.position - new Vector3((Hips.transform.position.x),
                                  Hips.transform.position.y, Hips.transform.position.z));
            var xangle = Vector3.Angle(new Vector3(Hips.transform.forward.x, 0, Hips.transform.forward.z),
                new Vector3(_direction.x, 0, _direction.z));
            var right = Vector3.Angle(new Vector3(Hips.transform.right.x, 0, Hips.transform.right.z),
                new Vector3(_direction.x, 0, _direction.z));


            // check if angle is Left or Right
            LeftOrRight leftOrRight = (right < 90 ? LeftOrRight.Right : LeftOrRight.Left);
            Debug.Log(leftOrRight);
            var result = anim.GetFloat("aimx");
            // if angle is greater than 0 then increase or decrease the xangle by 1

                if (xangle > 3)
                {
                    if (leftOrRight == LeftOrRight.Right)
                    {

                        result += 0.1f;
                    }
                    else
                    {
                        result -= 0.1f;
                    }
                }

            // if angle is greather or less than 0 repeat

            if (result <= 3f && result >= -3f)
            {
                anim.SetFloat("aimx", result);
            }
            anim.SetFloat("aimy", (_direction.normalized.y * 3.5f));

        }
        else
        {

            anim.SetFloat("aimx", 0);

            anim.SetFloat("aimy", 0);
        }




    }

    public float XOffset = 3f;

}

public enum LeftOrRight
{
    Left,
    Right
}