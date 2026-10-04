using System.Linq;
using Assets.Scripts.Interfaces;
using UnityEngine;
using MonoBehaviour = Photon.MonoBehaviour;

namespace Assets.Scripts
{
    public class RayCastActionController : MonoBehaviour
    {
        public void Update()
        {
            // If we are clicking the mouse.
            if (Input.GetButtonDown("Fire1"))
            {
                RaycastHit2D hit;
                // Draw a raycast from the mous to the world and see what we hit.
                var mouseWorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                hit = Physics2D.Raycast(mouseWorldPoint, mouseWorldPoint);

                if (hit.transform != null)
                {
                    if (hit.transform.GetComponents<IRayCastActionTrigger>().Any())
                    {
                        hit.transform.GetComponent<IRayCastActionTrigger>().RayCastHitAction2D(hit);
                    }
                }
            }
        }
    }
}