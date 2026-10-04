using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform myTarget;

    // Update is called once per frame
    public virtual void Update() 
    {
        if (myTarget != null)
        {
            Vector3 tarPos = myTarget.position;
            tarPos.z = transform.position.z;
            transform.position = tarPos;
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            // Adjust the orthographic size of the camera.
            var tempOrthosize = GetComponent<Camera>().orthographicSize - Input.GetAxis("Mouse ScrollWheel");

            // Make sure the camera can't zoom in or out too far.
            if (tempOrthosize >= 1 && tempOrthosize <= 6)
            {
                GetComponent<Camera>().orthographicSize = tempOrthosize;
            }
        }
    }
}
