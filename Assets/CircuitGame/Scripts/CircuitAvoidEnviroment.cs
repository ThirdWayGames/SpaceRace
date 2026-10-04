using UnityEngine;

public class CircuitAvoidEnviroment : MonoBehaviour {

    private AudioSource ShortCircuit;
    // Use this for initialization
    void Start () {
        ShortCircuit = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update () {
		
	}

    void OnCollisionEnter2D(Collision2D col)
    {
        if (ShortCircuit != null)
        {
            ShortCircuit.Play();
        }

        CircuitGameManager.Instance.OnWallCollision();
    }
}
