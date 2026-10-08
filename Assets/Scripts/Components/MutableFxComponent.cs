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
            var image = OverlayImage();
            if (image == null)
            {
                return;
            }

            var targetAlpha = Mathf.Clamp(UiOverlayColour.a, 0f, 1f);
            if (UiOverlaySprite != null && image.sprite != UiOverlaySprite)
            {
                image.sprite = UiOverlaySprite;
            }

            if (targetAlpha <= 0.02f)
            {
                var hidden = image.color;
                hidden.a = 0f;
                image.color = hidden;
                image.enabled = false;
                if (image.canvasRenderer != null)
                {
                    image.canvasRenderer.cullTransparentMesh = true;
                }

                StopFade();
                return;
            }

            if (!image.enabled)
            {
                image.enabled = true;
            }

            if (Mathf.Abs(image.color.a - targetAlpha) > 0.001f)
            {
                if (!isCoRoutineRunning)
                {
                    StartCoroutine(FadeTo(image, image.color.a, targetAlpha, UiOverlayRevealSpeed));
                }
            }
            else
            {
                StopFade();
            }
        }

        Image OverlayImage()
        {
            if (UiOverlay == null)
            {
                return null;
            }

            var image = UiOverlay.GetComponent<Image>();
            if (image == null && UiOverlay.parent != null)
            {
                image = UiOverlay.parent.GetComponent<Image>();
            }

            return image;
        }

        void StopFade()
        {
            if (isCoRoutineRunning)
            {
                StopAllCoroutines();
                isCoRoutineRunning = false;
            }
        }

        IEnumerator FadeTo(Image image, float fromValue, float toValue, float aTime)
        {
            isCoRoutineRunning = true;
            if (aTime <= 0f)
            {
                aTime = 0.01f;
            }

            for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
            {
                if (image == null)
                {
                    break;
                }

                var newColor = new Color(1f, 1f, 1f, Mathf.Clamp(Mathf.Lerp(fromValue, toValue, t), 0f, 1f));
                image.color = newColor;
                image.enabled = newColor.a > 0.02f;
                yield return null;
            }

            if (image != null)
            {
                var finalColor = new Color(1f, 1f, 1f, Mathf.Clamp(toValue, 0f, 1f));
                image.color = finalColor;
                image.enabled = finalColor.a > 0.02f;
            }

            isCoRoutineRunning = false;
        }
    }
}