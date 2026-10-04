using UnityEngine;

[RequireComponent (typeof (Animator))]
public class EnemyActions : MonoBehaviour
{
	public Animator animator;

    public float speed = 0.6f;

    public bool IsDead = false;

    public bool readyToFire = false;

    private const int countOfDamageAnimations = 3;

    private int lastDamageAnimation = -1;

    void Awake ()
    {
		animator = GetComponent<Animator> ();
	}

    public void SetFloat(string name, float value)
    {
        animator.SetFloat(name, value);
    }

    public void SetBool(string name, bool value)
    {
        animator.SetBool(name, value);
    }

    public Vector3 RootPosition()
    {
        return animator.rootPosition;
    }

	public void Stay ()
    {
		animator.SetBool("Aiming", false);
		//animator.SetFloat ("Speed", 0f);
	}

	public void Walk ()
    {
	    animator.SetBool("Aiming", false);
        //animator.SetFloat ("Speed", speed);
	}

	public void Run ()
    {
		//animator.SetFloat ("Speed", 1f);
	}

	public void Attack ()
    {
		Aiming ();
		animator.SetTrigger ("Attack");
	}

	public void Death ()
    {
		animator.SetTrigger("Death");
	}

    public void SetIsDead()
    {
        IsDead = true;
    }

    public bool IsAiming()
    {
        return animator.GetBool("Aiming");
    }

    public void IsInAimingPosition()
    {
        readyToFire = true;
    }

	public void Damage ()
    {
		if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Death")) return;
		int id = Random.Range(0, countOfDamageAnimations);
        if (countOfDamageAnimations > 1)
        {
            while (id == lastDamageAnimation)
            {
                id = Random.Range(0, countOfDamageAnimations);
            }
        }

		lastDamageAnimation = id;
		animator.SetInteger ("DamageID", id);
		animator.SetTrigger ("Damage");
	}

	public void Jump ()
    {
		animator.SetBool ("Squat", false);
		//animator.SetFloat ("Speed", 0f);
		animator.SetBool("Aiming", false);
		animator.SetTrigger ("Jump");
	}

	public void Aiming ()
    {
		animator.SetBool ("Squat", false);
		animator.SetBool("Aiming", true);
	}

    public void WalkingAiming()
    {
		animator.SetBool("Aiming", true);
       // animator.SetFloat("Speed", speed);
    }

    public void IdleAiming()
    {
        animator.SetBool("Aiming", true);
       // animator.SetFloat("Speed", 0);
    }

    public void Sitting ()
    {
		animator.SetBool ("Squat", !animator.GetBool("Squat"));
		animator.SetBool("Aiming", false);
	}

    public void CoverAiming()
    {
		animator.SetBool("Cover", true);
		animator.SetBool("Move", false);
    }
}
