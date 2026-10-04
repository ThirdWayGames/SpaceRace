using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FieldOfView3D))]
public class FieldOfView3DEditor : Editor
{
    void OnSceneGUI()
    {
        FieldOfView3D fov = (FieldOfView3D)target;
        Handles.color = Color.white;
        Vector3 viewAngleA = fov.DirFromAngle((-fov.ViewAngle / 2) + 90, false);
        Vector3 viewAngleB = fov.DirFromAngle((fov.ViewAngle / 2) + 90, false);

        /*Handles.DrawWireArc(fov.Trainsform.position, Vector3.up, viewAngleA, 360 - fov.ViewAngle, fov.AmbientViewRadius);
        Handles.DrawWireArc(fov.Trainsform.position, Vector3.Left, viewAngleA, 360 - fov.ViewAngle, fov.AmbientViewRadius);*/

        Handles.DrawWireArc(fov.transform.position, Vector3.up, viewAngleB, fov.ViewAngle, fov.ViewRadius);

        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.ViewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.ViewRadius);
    }
}
