using System;
using System.Collections.Generic;
using UnityEngine;

public class CircuitPlayerController : MonoBehaviour
{

    public float MoveSpeed = 1.0f;

    private Rigidbody2D RigedBody;

    public Vector2? Direction;
    public Vector2? lastDirection;

    public GameObject TrailPrefab;

    Vector2 LastTrailEnd;

    private GameObject CurrentTrail;

    [NonSerialized]
    public List<GameObject> Trails;

    public bool UsePhysicalTrails;

    // Use this for initialization
    void Start()
    {
        RigedBody = GetComponent<Rigidbody2D>();
        Trails = new List<GameObject>();
    }

    [NonSerialized]
    public int Count = 1;

    void Update()
    {
        if (!CircuitGameManager.Instance.CountingDown)
        {
            FitTrailBetween(CurrentTrail, LastTrailEnd, transform.position);

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                RigedBody.velocity = Vector2.zero;
                RigedBody.angularVelocity = 0;
                lastDirection = Direction;
                Direction = Vector2.down;
                SpawnTrail();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                RigedBody.velocity = Vector2.zero;
                RigedBody.angularVelocity = 0;
                lastDirection = Direction;
                Direction = Vector2.up;
                SpawnTrail();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                RigedBody.velocity = Vector2.zero;
                RigedBody.angularVelocity = 0;
                lastDirection = Direction;
                Direction = Vector2.left;
                SpawnTrail();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                RigedBody.velocity = Vector2.zero;
                RigedBody.angularVelocity = 0;
                lastDirection = Direction;
                Direction = Vector2.right;
                SpawnTrail();
            }
        }
    }


    /// <summary>
    /// Fits the trails between the two objects 
    /// </summary>
    /// <param name="currentTrail">The current trail</param>
    /// <param name="lastTrail">The last trail position</param>
    /// <param name="pos">Player current psoition</param>
    void FitTrailBetween(GameObject currentTrail, Vector2 lastTrail, Vector2 pos)
    {
        // Do we want to use physical trails
        if (UsePhysicalTrails)
        {
            // Check the current trail is not nill
            if (currentTrail != null)
            {
                //// Calculate the Center Position
                currentTrail.transform.position = lastTrail + (pos - lastTrail) * 0.5f;

                // Scale it (horizontally or vertically)
                float dist = Vector2.Distance(lastTrail, pos);

                // Check if the player direction
                if (Direction == Vector2.left || Direction == Vector2.right)
                {
                    //// Scale the trail
                    currentTrail.transform.localScale = new Vector2(dist + 1f, 1f);
                }
                else
                {
                    // Scale the trail
                    currentTrail.transform.localScale = new Vector2(1f, dist + 1f);
                }
            }
        }
    }

    void FitTrails(GameObject trail)
    {
        if (Trails.Count > 1)
        {
            var i = Trails.IndexOf(trail);
            if (i != 0)
            {
                var recTrans = Trails[i].GetComponent<RectTransform>();
                if (recTrans != null)
                {
                    var firstCorners = new Vector3[4];
                    recTrans.GetWorldCorners(firstCorners);

                    var secRecTrans = Trails[i - 1].GetComponent<RectTransform>();
                    if (secRecTrans != null)
                    {
                        var secCorners = new Vector3[4];
                        secRecTrans.GetWorldCorners(secCorners);

                        Vector3 direction = new Vector3();
                        var smallestDistFloat = float.MaxValue;
                        foreach (var firstCorner in firstCorners)
                        {
                            foreach (var secCorner in secCorners)
                            {
                                var dist = Vector3.Distance(firstCorner, secCorner);

                                if (dist < smallestDistFloat)
                                {
                                    smallestDistFloat = dist;
                                    direction = firstCorner - secCorner;
                                }

                            }
                        }
                        Trails[i].transform.position = Trails[i].transform.position + (direction.normalized * smallestDistFloat);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Spawns a trail behind the player
    /// </summary>
    void SpawnTrail()
    {
        // Do we want to use physical trains
        if (UsePhysicalTrails)
        {
            // Set the last trail end point
            LastTrailEnd = transform.position;
            // Spawn a new Lightwall
            CurrentTrail = (GameObject)Instantiate(TrailPrefab, transform.position, Quaternion.identity);

            // Add the trail
            Trails.Add(CurrentTrail);
            //FitTrails(CurrentTrail); 
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Check direction
        if (Direction != null)
        {
            // Move the player
            RigedBody.MovePosition(RigedBody.position += Direction.Value * MoveSpeed * Time.fixedDeltaTime);
        }
    }

    void OnCollisonEnter2D(Collider2D col)
    {
    }
}
