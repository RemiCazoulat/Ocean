using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class DFTWater : MonoBehaviour
{
    
    public int binaryResolution;
    private int resolution;
    public Shader shader;
    public int windSpeed;
    
    private Mesh mesh;
    private MeshFilter meshFilter;
    private Material material;
    
    private int xSize;
    private int zSize;
    
    private Vector3[] vertices;
    private Int32[] triangles;
    
    private ComputeBuffer wavesBuffer;

    private Vector3 lightDir;
    
    //partie GPU

     void Start()
     { 
         resolution = (int)Mathf.Pow(2, binaryResolution);
         mesh = new Mesh();
         GetComponent<MeshFilter>().mesh = mesh;
         material = new Material(shader);
         mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;;
         xSize = resolution;
         zSize = resolution;
         //windSpeed = Mathf.Max(1, Mathf.Min(300, windSpeed));
         GeneratePlane();
         GetComponent<Renderer>().material = material ;
    }
   
    void Update() {
        
        AssignMesh();

    }

    void OnDestroy()
    {
    }
    
    private void GeneratePlane()
    {
        Int32 verticesSize = (xSize + 1) * (zSize + 1);
        vertices = new Vector3[verticesSize];
        Int32 i = 0;
        for(int z = 0; z < zSize + 1 ; z ++) {
            for (int x = 0; x < xSize + 1; x ++) {
                vertices[i] = new Vector3(x, 0, z);
                i++;
            }
        }
        long trianglesSize = (xSize ) * (zSize ) * 6;

        triangles = new Int32[trianglesSize];
        Int32 vert = 0;
        Int32 tris = 0;
        for (int z = 0; z < zSize; z++) {
            for (int x = 0; x < xSize; x++) {
                triangles[tris + 0] = vert;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + xSize + 1;
                triangles[tris + 4] = vert + xSize + 2;
                triangles[tris + 5] = vert + 1;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }


    private float philips(float k, float V, float g)
    {
        float L = (V * V) / g;
        float kL = (float)k * L;
        float num = Mathf.Exp(-1 / (kL * kL));
        float denum = k * k * k * k;
        return num / denum;
    }
    
    private void GenerateSpectrum()
    {
            
    }
    private void AssignMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }

}
