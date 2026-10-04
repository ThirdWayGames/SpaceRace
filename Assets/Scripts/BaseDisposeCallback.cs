using UnityEngine;

public class BaseDisposeCallback : MonoBehaviour, IDisposeCallback
{
    public virtual void DisposeItem(float currentTime)
    {
        throw new System.NotImplementedException();
    }
}
