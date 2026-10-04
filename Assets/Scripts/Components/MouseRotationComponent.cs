using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Components
{
    [RequireComponent(typeof(PhotonView), typeof(GameObjectEntity))]
    public class MouseRotationComponent : MonoBehaviour
    {
        public float Angle;
        public float AngleVariance;
        public bool DisableRotation;
        public float MouseHorizontal;
        public float RotationSpeed = 0.3f;
        public bool FollowMouse = true;

        public float GetMouseAngle()
        {
            var randomAngleVar = Random.Range(AngleVariance * -1f, AngleVariance);

            return Angle + randomAngleVar;
        }

        public void Start()
        {
            if (PhotonNetwork.inRoom)
            {
                // If this is not my photon view
                if (!this.gameObject.GetComponent<PhotonView>().isMine)
                {
                    // Get the entity manager
                    var entityManager = World.Active.GetExistingManager<EntityManager>();

                    // Get the entity.
                    var entity = this.gameObject.GetComponent<GameObjectEntity>().Entity;

                    // Remove the component from the entity.
                    entityManager.RemoveComponent(entity, GetType());

                    // Remove me as a component from the object as I will interfere with 
                    // network'd values observed by the photon view from the other client.
                    Destroy(this);
                }
            }
        }
    }
}