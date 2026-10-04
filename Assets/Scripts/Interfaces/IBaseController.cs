using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IBaseController
    {
        /// <summary>
        /// Finds the child.
        /// </summary>
        /// <param name="gameObjectName">Name of the game object.</param>
        /// <returns>
        /// The gameobject found.
        /// </returns>
        Transform FindChild(string gameObjectName);

        /// <summary>
        /// Gets the component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The component found on the object.</returns>
        T GetComponent<T>();

        Transform GetTransform();
    }
}