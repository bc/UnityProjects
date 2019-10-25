using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Spectogram3D : MonoBehaviour
{
    private List<Vector3> _vertices;
    private List<int> _triangles;
    private Mesh _currentMesh;
    private float myPlotWidth = 1f;
    private float myPlotHeight = 0.03f;

    void Start()
    {
        CreateMesh();
        _currentMesh = GetComponent<MeshFilter>().mesh;
    }

    private void CreateMesh()
    {

    }

    void Update()
    {
        ApplyNewMesh(_currentMesh, myPlotWidth, myPlotHeight);
    }

    private void ApplyNewMesh(Mesh myMesh, float plotWidth = 1f, float plotHeight = 1f)
    {
        var signalHistory = AudioManager._bandVolumes;
        ComputeDataMesh(out _triangles, out _vertices, signalHistory);
        RefreshMesh(myMesh, _vertices, _triangles);
    }

    private void RefreshMesh(Mesh myMesh, List<Vector3>  myVertices, List<int> myTriangles)
    {
        myMesh.Clear();
        myMesh.vertices = myVertices.ToArray();
        myMesh.triangles = myTriangles.ToArray();
        myMesh.RecalculateNormals();
    }

    /// <summary>
    /// volumes represent the amplitude of the input values.
    /// </summary>
    /// <param name="triangles"></param>
    /// <param name="vertices"></param>
    /// <param name="signalHistory">a float matrix where the first dimension is time and second dimension is the frequencies</param>
    /// <param name="plotWidth"></param>
    /// <param name="plotHeight"></param>
    private static void ComputeDataMesh(out List<int> triangles, out List<Vector3> vertices, float[][] signalHistory)
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        for (int m = 0; m < signalHistory.Length - 1; m++)
        {
            float[] currentVolumes = signalHistory[m];
            float[] previousVolumes = signalHistory[m + 1];
            
           // the signal history.length is the number of observations over time.
            float zBandValue = m/(signalHistory.Length - 1f);
            float zBandNextValue = (m + 1)/(signalHistory.Length - 1f);
            
            var numberOfXBins = currentVolumes.Length;
            for (int i = 0; i < numberOfXBins - 1; i++)
            {
                // calculating x position
                float x = ((float) i / (numberOfXBins - 1));
                float xNext = ((float) (i + 1) / (numberOfXBins - 1));
                float volume = currentVolumes[i];
                float volumeNext = currentVolumes[i + 1];

                // two volumes that was previous
                float volumePrevious = previousVolumes[i];
                float volumeNextPrevious = previousVolumes[i + 1];


                // connection with previous band

                // adding vertices connecting this band with the next one
                vertices.Add(new Vector3(x, volume, zBandValue));
                vertices.Add(new Vector3(xNext, volumeNext, zBandValue));
                vertices.Add(new Vector3(x, volumePrevious, zBandNextValue));
                vertices.Add(new Vector3(xNext, volumeNextPrevious, zBandNextValue));

                int start_point = vertices.Count - 4;
                // adding 2 triangles using this vertex
                triangles.Add(start_point + 0);
                triangles.Add(start_point + 2);
                triangles.Add(start_point + 1);

                triangles.Add(start_point + 2);
                triangles.Add(start_point + 3);
                triangles.Add(start_point + 1);
            }
        }
    }
}