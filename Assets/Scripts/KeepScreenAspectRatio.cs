using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepScreenAspectRatio : MonoBehaviour {

    public Camera Camera;

    public float WidthAspect = 16.0f;
    public float HeightAspect = 9.0f;

    // Use this for initialization
    void Start()
    {
        float targetaspect = WidthAspect / HeightAspect;

        float windowaspect = (float)Screen.width / (float)Screen.height;

        float scaleheight = windowaspect / targetaspect;

        // if scaled height is less than current height, add letterbox
        if (scaleheight < 1.0f)
        {
            Rect rect = Camera.rect;

            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;

            Camera.rect = rect;
        }
        else // add pillarbox
        {
            float scalewidth = 1.0f / scaleheight;

            Rect rect = Camera.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            Camera.rect = rect;
        }
    }

}
