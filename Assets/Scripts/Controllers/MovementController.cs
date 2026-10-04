using Assets.Scripts.Components;
using Assets.Scripts.Interfaces;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public abstract class MovementController : BaseController, IMovementController
    {
        // Movement properties
        public float MaxSpeed = 3f;
        public float RotationSpeed = 0.3f;
        public bool FollowMouse = true;
        public bool DisableMovement = false;

        // Internal movement properties.
        protected float VerticalMovementIndex  = 0f;
        protected float HorizontalMovementIndex = 0f;
        protected Vector3 MousePosition;
        protected Vector3 GameObjectWorldPos;

        /// <summary>
        /// The rigidbody 2d
        /// </summary>
        protected Rigidbody2D Rigidbody2D;

        /// <summary>
        /// Awakes this instance.
        /// </summary>
        public virtual void Awake()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();
            if (Rigidbody2D == null)
            {
                Debug.LogWarning(string.Format("No Rigidbody2D found for '{0}'", transform.name));
            }
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public virtual void Update()
        {
            // Get the vertical and horizontal axis index.
            VerticalMovementIndex = Input.GetAxis("Vertical");
            HorizontalMovementIndex = Input.GetAxis("Horizontal");
        }

        /// <summary>
        /// Fixeds the update.
        /// </summary>
        public virtual void FixedUpdate()
        {
            // Move the object.
            Move(Time.fixedDeltaTime);
        }

        /// <summary>
        /// Sets the maximum speed.
        /// </summary>
        /// <param name="maxSpeed">The maximum speed.</param>
        public virtual void SetMaxSpeed(float maxSpeed)
        {
            MaxSpeed = maxSpeed;
        }

        public void SetDisableMovement(bool disabled)
        {
            var movementComponent = gameObject.GetComponent<MovementComponent>();
            if (movementComponent != null)
            {
                movementComponent.DisableMovement = disabled;
            }

            var mouseRotationComponent = gameObject.GetComponent<MouseRotationComponent>();
            if (mouseRotationComponent != null)
            {
                mouseRotationComponent.DisableRotation = disabled;
            }
        }

        /// <summary>
        /// Moves the specified vertical.
        /// </summary>
        /// <param name="frameTiming">The frame timing.</param>
        protected virtual void Move(float frameTiming)
        { 
            // If the current console is not null and is active
            if (DisableMovement)
            {
                // Return from this method as movement is disabled when we are in a console.
                return;
            }

            if (FollowMouse)
            {
                // Get the mouse position
                MousePosition = Input.mousePosition;

                // Get the current object position
                GameObjectWorldPos = Camera.main.WorldToScreenPoint(transform.position);

                // calculate the mouse pos in relation to object positon.
                MousePosition.x = MousePosition.x - GameObjectWorldPos.x;
                MousePosition.y = MousePosition.y - GameObjectWorldPos.y;

                // Calculate the amount of rotation required to make the object face toward the mouse pointer.
                // compensate by -90 deg so that the x axis points toward the target.
                var angle = Mathf.Atan2(MousePosition.y, MousePosition.x) * Mathf.Rad2Deg - 90;

                // Adjust the rotation 
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0, 0, angle)), RotationSpeed);
            }

            // Are we tranforming the position.
            if (VerticalMovementIndex != 0f || HorizontalMovementIndex != 0f)
            {
                // Reset the velocity.
                if (Rigidbody2D != null)
                {
                    Rigidbody2D.velocity = Vector2.zero;
                }

                // Adjust the velocity of the transform.
                var pos = transform.position;
                pos = new Vector3(HorizontalMovementIndex * GetCurrentSpeed() * frameTiming, VerticalMovementIndex * GetCurrentSpeed() * frameTiming, 0);
                transform.position += pos;
            }
        }

        /// <summary>
        /// Gets the horizontal speed.
        /// </summary>
        /// <returns>
        /// The adjusted horizontal speed.
        /// </returns>
        protected virtual float GetCurrentSpeed()
        {
            return MaxSpeed;
        }
    }
}