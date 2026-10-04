using System.Collections.Generic;
using UnityEngine;

public class FieldOfView3D : MonoBehaviour
{
    public float AmbientViewRadius;

    public float ViewRadius;

    [Range(0, 360)]
    public float ViewAngle;

    public LayerMask targetMask;

    public LayerMask obstacleMask;

    public float MeshResolution;

    [HideInInspector]
    public List<Transform> VisibleTargets = new List<Transform>();

    public MeshFilter ViewMeshFilter;

    public float EdgeDistanceThreshold;

    public float CutAwayDistance = 0.1f;

    public float CutAwayTolerance = 0.01f;

    public bool ApplyEdgeSmoothing;

    private Mesh viewMesh;

    public void Start()
    {
        viewMesh = new Mesh { name = string.Format("View Mesh ({0})", transform.name) };
        ViewMeshFilter.mesh = viewMesh;
    }

    public void Update()
    {
        DrawFieldOfView();
    }

    public int EdgeResolveIterations;

    public Color RayStartColor = Color.green;

    public void DrawFieldOfView()
    {
        List<Vector3> viewPoints = new List<Vector3>();
        int stepCount = Mathf.RoundToInt(360f * MeshResolution);
        float stepAngleSize = 360f / stepCount;
        ViewCastInfo oldViewCast = new ViewCastInfo();
        RayStartColor = Color.green;
        for (int i = 0; i <= stepCount; i++)
        {
            // Get the current angle
            float angle = stepAngleSize * i;
            ViewCastInfo viewCast = ViewCast(angle);
            RayStartColor += new Color(RayStartColor.r, RayStartColor.g, RayStartColor.b + (1f / stepCount));

            if (ApplyEdgeSmoothing)
            {
                if (i > 0)
                {
                    bool edgeDistanceThresholdExceeded = Mathf.Abs(oldViewCast.dist - viewCast.dist) > EdgeDistanceThreshold;
                    if (oldViewCast.hit != viewCast.hit || (oldViewCast.hit && viewCast.hit && edgeDistanceThresholdExceeded))
                    {
                        EdgeInfo edge = FindEdge(oldViewCast, viewCast);
                        if (edge.pointA != Vector3.zero)
                        {
                            viewPoints.Add(edge.pointA);
                        }

                        if (edge.pointB != Vector3.zero)
                        {
                            viewPoints.Add(edge.pointB);
                        }
                    }
                }
            }
            
            viewPoints.Add(viewCast.point);
            oldViewCast = viewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]) + Vector3.forward * CutAwayDistance;
            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    public void FindVisibleTargets()
    {
        VisibleTargets.Clear();
        var targetsInViewRad = Physics.OverlapSphere(transform.position, ViewRadius, targetMask);

        for (int i = 0; i < targetsInViewRad.Length; i++)
        {
            var target = targetsInViewRad[i].transform;
            var dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < ViewAngle/2)
            {
                var distToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    VisibleTargets.Add(target);
                }
            }
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees -= transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Sin(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dist;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dist, float _angle)
        {
            hit = _hit;
            point = _point;
            dist = _dist;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }

    }

    protected EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;

        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;
        
        for (int i = 0; i < EdgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle, true);

            bool edgeDistanceThresholdExceeded = Mathf.Abs(minViewCast.dist - newViewCast.dist) > EdgeDistanceThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDistanceThresholdExceeded)
            {
                minAngle = newViewCast.angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = newViewCast.angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    protected ViewCastInfo ViewCast(float globalAngle, bool findingEdge = false)
    {
        var dir = DirFromAngle(globalAngle, true);
        var localViewDist = this.AmbientViewRadius;

        // If the current angle is in the view angle then cast the ray at view radius as opposed to ambient radius.
        var rayAngle = Vector3.Angle(dir, transform.forward);
        var localViewAngle = ViewAngle;
        if (rayAngle < localViewAngle / 2)
        {
            localViewDist = this.ViewRadius;
        }

        var hit = Physics.RaycastAll(new Ray(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(dir.x, 0, dir.z)), localViewDist, obstacleMask);

        if (hit.Length > 0)
        {
            Debug.DrawLine(transform.position, hit[0].point, Color.yellow);
            return new ViewCastInfo(true, hit[0].point, hit[0].distance, globalAngle);
        }

        Debug.DrawLine(transform.position, transform.position + dir * localViewDist, Color.green);
        return new ViewCastInfo(false, transform.position + dir * localViewDist, localViewDist, globalAngle);
    }
}

