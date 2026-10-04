using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IPropertyMutation
    {
        void Unmutate(GameObject parent);

        void Mutate(GameObject parent);
    }
}