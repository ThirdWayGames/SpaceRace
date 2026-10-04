using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Components
{
    [RequireComponent(typeof(GameObjectEntity))]
    public abstract class MutatableComponent : MonoBehaviour
    {
        public float MaxValue;

        public float CurrentValue;

        public virtual void Start()
        {
            if (PhotonNetwork.inRoom && this.gameObject.GetComponent<PhotonView>() != null)
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

        public virtual void Update()
        {
            // Make sure the current value is always less than the max value.
            if (CurrentValue > MaxValue)
            {
                CurrentValue = MaxValue;
            }
        }
    }
}