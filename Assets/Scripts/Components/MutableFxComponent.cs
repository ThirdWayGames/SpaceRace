using System.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Components
{
    [RequireComponent(typeof(GameObjectEntity))]
    public class MutableFxComponent : MonoBehaviour
    {
        public Sprite UiOverlaySprite = null;

        public Color UiOverlayColour = new Color(0, 0, 0, 0);

        public float UiOverlayRevealSpeed = 5.0f;

        protected RectTransform UiOverlay;

        bool isCoRoutineRunning = false;

        public virtual void Start()
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

            UiOverlay = GameManager3D.instance.GameStatePanel;
        }

        public void Update()
        {
            if (UiOverlay.GetComponent<Image>() != null)
            {
                if (UiOverlaySprite != null)
                {
                    UiOverlay.GetComponent<Image>().sprite = UiOverlaySprite;
                    if (UiOverlayColour.a != Mathf.Clamp(UiOverlay.GetComponent<Image>().color.a, 0f, 1f))
                    {
                        if (!isCoRoutineRunning)
                        {
                            StartCoroutine(FadeTo(UiOverlay.GetComponent<Image>().color.a, UiOverlayColour.a, UiOverlayRevealSpeed));
                        }
                    }
                    else
                    {
                        StopAllCoroutines();
                        isCoRoutineRunning = false;
                    }
                }
                else
                {
                    if (UiOverlayColour.a != Mathf.Clamp(UiOverlay.GetComponent<Image>().color.a, 0f, 1f))
                    {
                        if (!isCoRoutineRunning)
                        {
                            StartCoroutine(FadeTo(UiOverlay.GetComponent<Image>().color.a, UiOverlayColour.a, UiOverlayRevealSpeed));
                        }
                    }
                    else
                    {
                        UiOverlay.GetComponent<Image>().sprite = UiOverlaySprite;
                        StopAllCoroutines();
                        isCoRoutineRunning = false;
                    }
                }
            }
        }

        IEnumerator FadeTo(float fromValue, float toValue, float aTime)
        {
            isCoRoutineRunning = true;
            for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
            {
                Color newColor = new Color(1, 1, 1, Mathf.Clamp(Mathf.Lerp(fromValue, toValue, t), 0f, 1f));
                UiOverlay.GetComponent<Image>().color = newColor;
                yield return null;
            }

            isCoRoutineRunning = false;
        }
    }
}