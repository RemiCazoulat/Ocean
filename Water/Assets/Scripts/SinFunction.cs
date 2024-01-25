using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class SinFunction : MonoBehaviour
{
  

    struct Wave
    {
        public float amplitude;
        public float frequency;
        public float phase;
        public Vector3 direction;
        public int type;
        public Wave(float amplitude, float waveLength, float speed, Vector3 direction, int type)
        {
            this.amplitude = amplitude;
            this.frequency = 2.0f * Mathf.PI / waveLength;
            this.phase = speed * Mathf.Sqrt(9.8f * 2.0f * Mathf.PI / waveLength);           
            this.direction = direction;
            this.type = type;
        }

       
        
    }

    public int binaryWaveNumber;
    private int waveNumber;
    public int binaryResolution;
    private int resolution;
    public Shader shader;
    
    
    public int windSpeed;
    public int waveHeight;
    
    private Mesh mesh;
    private MeshFilter meshFilter;
    private Material material;
    
    private int xSize;
    private int zSize;
    
    private Vector3[] vertices;
    private Int32[] triangles;
    
    private Wave[] waves;
    private ComputeBuffer wavesBuffer;

    private Vector3 lightDir;
    
    //partie GPU

     void Start()
     { 
         resolution = (int)Mathf.Pow(2, binaryResolution);
         waveNumber = (int)Mathf.Pow(2, binaryWaveNumber);
         mesh = new Mesh();
         GetComponent<MeshFilter>().mesh = mesh;
         material = new Material(shader);
         mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;;
         xSize = resolution;
         zSize = resolution;
         windSpeed = Mathf.Max(1, Mathf.Min(300, windSpeed));
         GeneratePlane();
         GenerateWaves();
         const int waveSize = 4 * 3 + 4 * 3 + 4;
         wavesBuffer = new ComputeBuffer(waveNumber, waveSize);
         wavesBuffer.SetData(waves);
         material.SetBuffer("waves", wavesBuffer);
         material.SetInt("wave_number", waveNumber);
         GetComponent<Renderer>().material = material ;
    }
   
    void Update() {
        windSpeed = Mathf.Max(1, Mathf.Min(300, windSpeed));
        float time = Time.timeSinceLevelLoad;
        material.SetInt("wind_speed",windSpeed);
       
        AssignMesh();

    }

    void OnDestroy()
    {
        wavesBuffer.Release();
    }

    private void GenerateWaves()
    {
        Debug.Log("[WAVES] wave number : "+waveNumber);
        waves = new Wave[waveNumber];
        const float minFreq = 1.05f;
        const float maxFreq = 6f;
        const float basicFreq = 1.1f;
        const float basicAmp = 0.89f;

        float step = (maxFreq - minFreq) / (float)waveNumber;
        Debug.Log("[WAVES] step : "+step);

        float double_pi = 2 * Mathf.PI;
        const float minSpeed = 0.15f;
        const float maxSpeed = 1.5f;
        Vector3 direction = new Vector3(0, 0, 0);
        for (int i = 0; i < waveNumber; i++)
        {
            //float freq = minFreq +  step * i;
            float freq = Mathf.Pow(basicFreq, i);
            Debug.Log("[WAVE "+i+"] freq : "+freq);
            float waveLength =  (float)resolution / 8 * double_pi / freq;
            Debug.Log("[WAVE "+i+"] wave length : "+waveLength);
            float amplitude = waveHeight * Mathf.Pow(basicAmp, i);
            Debug.Log("[WAVE "+i+"] amplitude : "+amplitude);
            float speed = Random.Range(minSpeed, maxSpeed) /* * Mathf.Log(Mathf.Log(i + 1)+1)*/;
            var randomX = Random.Range(-1f, 1f);
            var randomY = Random.Range(-1f, 1f);
            direction.x = randomX;
            direction.z = randomY;
            direction.Normalize();
            waves[i] = new Wave(amplitude, waveLength, speed, direction, 0);
            Debug.Log("[WAVE "+i+"] phase : "+waves[i].phase);

            /*
            Debug.Log("=============== WAVE "+i+"===============");
            Debug.Log("waveLength :"+waveLength);
            Debug.Log("amp : "+waves[i].amplitude);
            Debug.Log("frequency : "+waves[i].frequency);
            Debug.Log("phase : "+waves[i].phase);
            Debug.Log("x dir : "+waves[i].direction.x);
            Debug.Log("y dir : "+waves[i].direction.y);
            Debug.Log("z dir : "+waves[i].direction.z);
            */
            
        }
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
    private void AssignMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }
    /*
    void SumOfSin(float time)
    {
        //creation des buffers
        int byteSize = 4 * 3 + 4 * 3 + 4;
        //init des buffers avec leur taille et la taille de leurs elements
        _verticesBuffer = new ComputeBuffer((xSize + 1) * (zSize + 1), 12);
        var wavesBuffer = new ComputeBuffer(wave_number, byteSize);
      
        //attribution de valeurs aux buffers si besoin
        _verticesBuffer.SetData(vertices);
        wavesBuffer.SetData(waves);

        //association CPU GPU
        sinMovement.SetBuffer(_indexOfKernel, "vertices", _verticesBuffer);
        sinMovement.SetBuffer(_indexOfKernel, "waves", wavesBuffer);
        sinMovement.SetInt("resolution", resolution);
        sinMovement.SetInt("sin_number", wave_number);
        sinMovement.SetFloat("time", time);
        //On parallelise
        sinMovement.Dispatch(_indexOfKernel, xSize / 8, 1, zSize / 8);
        //On recupere les datas
        _verticesBuffer.GetData(vertices);
        //on relache la memoire des buffers
        _verticesBuffer.Release();
        wavesBuffer.Release();
    }
    */

}
