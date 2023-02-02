using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DeformablePlane : MonoBehaviour
{
    public int xResolution, yResolution;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private Vector3[] constantVps;
    private Vector3[] dynamicVPs;
    bool dynamicVPsSet = false;
    
    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        GenerateMesh(xResolution, yResolution);
    }

    void GenerateMesh(int xRes, int yRes) {
        var mesh = new Mesh {
			name = "Plane " + xRes + "x" + yRes
		};
        float xSize = 1f / (xRes - 1);
        float ySize = 1f / (yRes - 1);
        var vertices = new Vector3[xRes * yRes];
        var uvs = new Vector2[xRes * yRes];
        // 1. Define the vertices of the mesh
        for(int y = 0; y < yRes; y++) {
            for(int x = 0; x < xRes; x++) {
                // 2. Define the triangles of the mesh
                vertices[y * yRes + x] = new Vector3(x * xSize - 0.5f, 0, y * ySize - 0.5f);
                // 3. Define the UVs of the mesh
                uvs[y * yRes + x] = new Vector2((float)x / xRes, (float)y / yRes);
            }
        }
        int triCount = (xRes - 1) * (yRes - 1) * 2;
        var triangles = new int[triCount * 3];
        for(int i = 0; i < triCount; i++) {
            if(i % ((xRes - 1) * 2) == 0) {
                triangles[i * 3] = (i / ((xRes - 1) * 2)) * xRes;
                triangles[i * 3 + 1] = triangles[i * 3] + xRes;
                triangles[i * 3 + 2] = triangles[i * 3] + 1;
            } else {
                triangles[i * 3] = triangles[i * 3 - (i % 2 == 0? 1 : 2)];
                triangles[i * 3 + 1] = i % 2 == 0? (triangles[i * 3 - 2]) : (triangles[i * 3] + 1); 
                triangles[i * 3 + 2] = i % 2 == 0? (triangles[i * 3] + 1) : (triangles[i * 3 - 1]); 
            }
        }
        mesh.SetVertices(vertices);
        constantVps = vertices;
        dynamicVPs = new Vector3[vertices.Length];
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    // SMOOTH VERTICES
    private Vector3[] SmoothVertices(ref Vector3[] verts) {
        // assumes vertices ordered in a grid
        for(int l = 0; l < 2; l++) {
            for(int i = 0; i < verts.Length; i++) {
                float avg = verts[i].y;
                int count = 1;
                for(int k = -1; k < 2; k++) {
                    // in bounds and same row
                    if(i + k >= 0 && i + k < verts.Length && (i + k) / xResolution == i / xResolution) {
                        avg += verts[i + k].y;
                        count++;
                    }
                    // in bounds and same column
                    if(i + k * xResolution >= 0 && i + k * xResolution < verts.Length && (i + k * xResolution) % xResolution == i % xResolution) {
                        avg += verts[i + k * xResolution].y;
                        count++;
                    }
                }
                verts[i].y = avg / count;
            }
        }
        return verts;
    }

    // UPDATE MESH
    public void UpdateMesh() {
        var mesh = meshFilter.mesh;
        mesh.SetVertices(dynamicVPsSet? SmoothVertices(ref dynamicVPs) : constantVps);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // meshCollider.sharedMesh = null;
        // meshCollider.sharedMesh = mesh;
        dynamicVPsSet = false;
    }
    
    // CONSTANT MODIFICATION FUNCTIONS
    public void GrabPoint(Vector3 pos, float radius, float normalOffset) {
        pos = transform.InverseTransformPoint(pos);
        Vector2 localPos = new Vector2(pos.x, pos.z);

        var mesh = meshFilter.mesh;
        var vertices = mesh.vertices;
        for(int i = 0; i < vertices.Length; i++) {
            Vector2 localVert = new Vector2(vertices[i].x, vertices[i].z);
            Vector2 toVert = localVert - localPos;
            var distance = toVert.sqrMagnitude;
            if(distance < radius * radius) {
                float slope = (1 - distance / (radius * radius));
                vertices[i].y += normalOffset * slope;
            }
        }
        constantVps = SmoothVertices(ref vertices);
    }

    // DYNAMIC MODIFICATION FUNCTIONS
    public void AddRipple(Vector3 pos, float impactRadius, float magnitude, float waveSize, float speed, float t, float totalTime) {
        Vector3 localPos = transform.InverseTransformPoint(pos);
        float scale = transform.lossyScale.x;
        float inverseScale = 1 / scale;
        impactRadius *= inverseScale;
        magnitude *= inverseScale;

        if(!dynamicVPsSet) {
            Array.Copy(constantVps, dynamicVPs, constantVps.Length);
            dynamicVPsSet = true;
        }

        for(int i = 0; i < dynamicVPs.Length; i++) {
            Vector3 toVert = dynamicVPs[i] - localPos;
            toVert = new Vector3(toVert.x, 0, toVert.z);
            float GetDy(float time) {
                float dy = Ripple(scale * toVert.x, scale * toVert.z, time);
                if(toVert.sqrMagnitude < impactRadius * impactRadius) {
                    float slope = (toVert).sqrMagnitude / (impactRadius * impactRadius);
                    dy *= slope;
                }
                dy *= Mathf.Clamp01(Mathf.Pow(1 - (time / totalTime), 3));
                return dy;
            }
            dynamicVPs[i].y += GetDy(t);
        }

        float Ripple (float x, float z, float t) {
            float d = Mathf.Sqrt(x * x + z * z);
            float y = Mathf.Sin((waveSize * d - speed * t));
            return magnitude * y / (1f + speed * Mathf.Pow(d - t, 2));
        }
    }
}
