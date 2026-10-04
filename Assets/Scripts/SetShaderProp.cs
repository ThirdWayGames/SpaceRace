using UnityEngine;

public class SetShaderProp : MonoBehaviour
{
    public Material mat;
    public string PropertyName;
    public Transform player;

    // Update is called once per frame
    void Update () 
    {
        if (player != null)
        {
            if (mat != null)
            {
                if (!string.IsNullOrEmpty(PropertyName))
                {
                    mat.SetVector(PropertyName, player.position);
                }
                else
                {
                    Debug.Log("Assign the property name.");
                }
            }
            else
            {
                Debug.Log("Assign the material.");
            }
        }
        else
        {
            Debug.Log("Assign the player property.");
        }
    }
}
