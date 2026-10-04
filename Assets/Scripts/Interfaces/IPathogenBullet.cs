using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IPathogenBullet : IBullet
    {
        void SetPathogen(ScriptableObject pathogen);
    }
}