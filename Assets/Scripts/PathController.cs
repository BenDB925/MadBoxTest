using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
struct BezierCurveData
{
    public List<Transform> controlPoints;
}

public class PathController : MonoBehaviour
{
    [SerializeField] private List<BezierCurveData> bezierCurveControlPoints;
    [SerializeField] private List<MeshFilter> meshes;
    public int sampleFrequency = 20;
    
    private Vector3[] vertices;
    private float lineWidth = 3;

    private void Start()
    {
        for (int i = 0; i < bezierCurveControlPoints.Count; i++)
        {
            GenerateMesh(i);
        }
    }

    public Vector3 GetPointOnCourse(float pPercentThroughCourse)
    {
        float scaledPercent = pPercentThroughCourse * bezierCurveControlPoints.Count;
        int curveIndex = (int)Mathf.Floor(scaledPercent);
        float progressionThroughCurrentCurve = scaledPercent - curveIndex;
        return GetPointOnBezierCurve(curveIndex, progressionThroughCurrentCurve);
    }

    public Vector3 GetPlayerRotation(float pPercentThroughCourse, Transform playerTransform)
    {
        float scaledPercent = pPercentThroughCourse * bezierCurveControlPoints.Count;
        int curveIndex = (int)Mathf.Floor(scaledPercent);
        float progressionThroughCurrentCurve = scaledPercent - curveIndex;
        return GetDirection(curveIndex, progressionThroughCurrentCurve, playerTransform);
    }
    
    private Vector3 GetPointOnBezierCurve(int pCurveIndex, float pPercentThroughCurve)
    {
        Vector3 positionOnCurve;
        BezierCurveData currentCurve = bezierCurveControlPoints[pCurveIndex];
        float percentLeftOnCurve = 1 - pPercentThroughCurve;

        //formula for getting a position on a bezier curve
        positionOnCurve = Mathf.Pow(percentLeftOnCurve, 3) * currentCurve.controlPoints[0].position +
                          3 * Mathf.Pow(percentLeftOnCurve, 2) * pPercentThroughCurve * currentCurve.controlPoints[1].position +
                          3 * percentLeftOnCurve * Mathf.Pow(pPercentThroughCurve, 2) * currentCurve.controlPoints[2].position +
                          Mathf.Pow(pPercentThroughCurve, 3) * currentCurve.controlPoints[3].position;

        return positionOnCurve;
    }
    
    Vector3 GetFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) 
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            3f * oneMinusT * oneMinusT * (p1 - p0) +
            6f * oneMinusT * t * (p2 - p1) +
            3f * t * t * (p3 - p2);
    }
    Vector3 GetVelocity (int curveIndex, float t, Transform relativeTransform) 
    {
        return relativeTransform.TransformPoint(
            GetFirstDerivative(bezierCurveControlPoints[curveIndex].controlPoints[0].position, 
                                        bezierCurveControlPoints[curveIndex].controlPoints[1].position, 
                                        bezierCurveControlPoints[curveIndex].controlPoints[2].position, 
                                        bezierCurveControlPoints[curveIndex].controlPoints[3].position, t)) - relativeTransform.position;
    }
    Vector3 GetDirection (int curveIndex, float t, Transform relativeTransform) {
        return GetVelocity(curveIndex, t, relativeTransform).normalized;
    }

    Vector3 PerpendicularRight(Vector3 orig){
        var vec = new Vector3(orig.z, 0, -orig.x);
        vec.Normalize();
        return vec;
    }
    Vector3 PerpendicularLeft(Vector3 orig){
        var vec = new Vector3(orig.z, 0, -orig.x);
        vec.Normalize();
        return vec * -1;
    }


    public void GenerateMesh(int curveIndex)
    {
        vertices = new Vector3[(sampleFrequency + 1) * 2];

        //iterate over our samples adding two vertices for each one
        for(int s = 0, i = 0; s <= sampleFrequency; s++, i += 2){
            float interval = s / (float)sampleFrequency;

            //get point along spline, and translate to local coords from world
            var point = transform.InverseTransformPoint(GetPointOnBezierCurve(curveIndex, interval));
            var direction = GetDirection(curveIndex, interval, transform);

            var perpendicularLeftVec = PerpendicularLeft(direction) * lineWidth;
            var perpendicularRightVec = PerpendicularRight(direction) * lineWidth;
            // var perpendicularVec = turnLeft ? PerpendicularLeft(diffVector) : PerpendicularRight(diffVector);

            vertices[i] = point + (Vector3)perpendicularLeftVec;
            vertices[i + 1] = point + (Vector3)perpendicularRightVec;
        }

        meshes[curveIndex].mesh = new Mesh();
        meshes[curveIndex].mesh.name = "Spline Mesh";
        meshes[curveIndex].mesh.vertices = vertices;

        //now figure out our triangles
        int [] triangles = new int[sampleFrequency * 6];
        for(int s = 0, ti = 0, vi = 0; s < sampleFrequency; s++, ti += 6, vi += 2){
            //first tri
            triangles[ti] = vi;
            triangles[ti + 1] = vi + 3;
            triangles[ti + 2] = vi + 1;
            //second matching tri
            triangles[ti + 3] = vi;
            triangles[ti + 4] = vi + 2;
            triangles[ti + 5] = vi + 3;
        }

        meshes[curveIndex].mesh.triangles = triangles;
        meshes[curveIndex].mesh.RecalculateNormals();

        meshes[curveIndex].GetComponent<MeshCollider>().sharedMesh = meshes[curveIndex].mesh;

        Debug.Log("Generated Spline Mesh");
    }

    private void OnDrawGizmos()
    {
        for (var i = 0; i < bezierCurveControlPoints.Count; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                float amountThroughCurve = (j / 20.0f);
                Vector3 pointOnCurve = GetPointOnBezierCurve(i, amountThroughCurve);
                
                Gizmos.DrawCube(pointOnCurve, Vector3.one);
            }
        }
    }
}
