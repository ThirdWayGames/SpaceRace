using Assets.Scripts.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[CustomEditor(typeof(EnemyTargetComponent))]
public class EnemyEditor : Editor {

    void OnSceneGUI()
    {
        EnemyTargetComponent enemyController = (EnemyTargetComponent)target;
        var enemyAttackComp = enemyController.transform.GetComponent<EnemyAttackComponent>();
        var enemyMoveComp = enemyController.transform.GetComponent<EnemyMovementComponent>();
        var enemyNavAgent = enemyController.transform.GetComponent<NavMeshAgent>();
        IWeaponary weapon = null;
        if (enemyAttackComp != null)
        {
            enemyAttackComp.GetWeapon();
        }

        Handles.color = Color.red;
        Handles.DrawWireArc(enemyController.transform.position, Vector3.up, Vector3.forward, 360, enemyController.LookRadius);
        Vector3 viewAngleA = enemyController.DirFromAngle(-enemyController.ViewAngle / 2, false);
        Vector3 viewAngleB = enemyController.DirFromAngle(enemyController.ViewAngle / 2, false);
        Handles.DrawLine(enemyController.transform.position, enemyController.transform.position + viewAngleA * enemyController.ViewAngle);
        Handles.DrawLine(enemyController.transform.position, enemyController.transform.position + viewAngleB * enemyController.ViewAngle);
        
        var currentThreat = enemyController.GetCurrentThreat();
        if (currentThreat != null)
        {
            Handles.color = Color.blue;
            Handles.DrawWireArc(enemyController.LastKnowLocation, Vector3.up, Vector3.forward, 360, .5f);

            if (enemyMoveComp != null)
            {
                if (enemyMoveComp.CurrentDestination != Vector3.zero)
                {
                    Handles.color = Color.yellow;
                    // Draw Debug from me to alt attack pos selected.
                    //// Handles.DrawLine(enemyController.transform.position, enemyMoveComp.CurrentDestination);
                    Handles.DrawWireArc(enemyMoveComp.CurrentDestination, Vector3.up, Vector3.forward, 360, .5f);
                }
            }

            /*
            // If the target is in LoS
            if (currentThreat.IsInLos)
            {
                // Set the targeting handles to red.
                Handles.color = Color.red;

                // Cast a ray at the target (ignoring collisions on TransparentObstacles layer).
                var raycastTransform = enemyController.RayCastFrom != null ? enemyController.RayCastFrom.transform : enemyController.transform;

                // If the raycast transform is higher than the entity base transform
                if (raycastTransform.transform.position.y > enemyController.transform.position.y)
                {
                    // Calculate the 3 positions we need to cast from.
                    var rayPos = new float[] {
                        enemyController.transform.position.y,
                        (raycastTransform.transform.position.y - enemyController.transform.position.y) / 2,
                        raycastTransform.transform.position.y
                    };

                    // Create a temp ray vector from the entities base vector.
                    var tmpTargetVector = new Vector3(enemyController.Target.transform.position.x, enemyController.Target.transform.position.y, enemyController.Target.transform.position.z);

                    // For 3 times.
                    for (int i = 0; i < 3; i++)
                    {
                        // Adjust the y co-ords of the two vectors we are raycasting between.
                        tmpTargetVector.y = rayPos[i];

                        // Draw the line
                        // Handles.DrawLine(raycastTransform.position, tmpTargetVector);
                    }
                }
                else
                {
                    // Draw the line
                    Handles.DrawLine(raycastTransform.position, enemyController.Target.transform.position);
                }
            }

            // Draw a line to the target regardless of being in LoS.
            Handles.color = Color.black;
            if (enemyAttackComp != null && weapon != null)
            {
                if (Vector3.Distance(enemyController.transform.position, enemyController.Target.transform.position) <= weapon.GetEffectiveRange())
                {
                    Handles.color = Color.red;
                }
            }
            
            Handles.DrawLine(enemyController.transform.position, enemyController.Target.transform.position);
            */
        }
        
        // Draw the effective weapon arc.
        if (enemyAttackComp != null && weapon != null)
        {
            Vector3 rotatedVector = Quaternion.AngleAxis(-90, Vector3.up) * enemyController.transform.forward;
            Handles.color = Color.green;
            Handles.DrawWireArc(enemyController.transform.position, Vector3.up, rotatedVector, 180, weapon.GetEffectiveRange());
        }
    }
}
