using Assets.Scripts.Interfaces;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public abstract class BaseController : MonoBehaviour, IBaseController
    {
        /// <summary>
        /// Finds the child.
        /// </summary>
        /// <param name="gameObjectName">Name of the game object.</param>
        /// <returns>
        /// The gameobject found.
        /// </returns>
        public virtual Transform FindChild(string gameObjectName)
        {
            return this.transform.GetComponentsInChildren<Transform>().FirstOrDefault(x => x.name == gameObjectName);
        }

        public Transform GetTransform()
        {
            return this.transform;
        }
    }
}