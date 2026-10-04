using UnityEngine;

public interface IBaseMutation
{
    void Initialise(GameObject parentGameObject);

    bool IsInitialised();

    void CureMutation(GameObject parentGameObject, GameObject gameObject);

    bool IsCured();

    void ExecuteMutation(GameObject parentGameObject);

    bool CanApplyMutation(GameObject parentToCheck);
}