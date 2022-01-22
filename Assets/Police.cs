using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Delaunay.Geo;
using System;

public class Police : MonoBehaviour
{
    bool init = false;
    float[,] map;
    List<LineSegment> m_edges;
    int HEIGHT;
    int WIDTH;
    List<GameObject> cars;
    DateTime lastActualised;

    public void Init(float[,] passedMap, List<LineSegment> edges, int height, int width)
    {
        map = passedMap;
        m_edges = edges;
        HEIGHT = height;
        WIDTH = width;
        cars = new List<GameObject>();
        lastActualised = DateTime.Now;
        init = true;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (cars.Count > 0 && collider.gameObject == cars[0])
        { cars.RemoveAt(0); }
        if (cars.Count == 0)
        {
            LineSegment seg = m_edges[UnityEngine.Random.Range(0, m_edges.Count)];
            Vector2 left = (Vector2)seg.p0;
            float limit = map[(int)left.x, (int)left.y];
            while (UnityEngine.Random.Range(0f, 1f) > limit)
            {
                seg = m_edges[UnityEngine.Random.Range(0, m_edges.Count)];
                left = (Vector2)seg.p0;
                limit = map[(int)left.x, (int)left.y];
            }
            GetComponent<NavMeshAgent>().destination = CoordMap2Plane(left);
        }
    }

    public void CallPolice(GameObject car)
    { cars.Add(car); }

    // Update is called once per frame
    void Update()
    {
        if (init && cars.Count > 0)
        {
            if (cars[0] == null)
            { cars.RemoveAt(0); return; }
            GetComponent<NavMeshAgent>().destination = cars[0].transform.position;
        }
    }

    Vector3 CoordMap2Plane(Vector2 vec)
    { return new Vector3(vec.y / WIDTH * 10 - 5, 0, 5 - vec.x / HEIGHT * 10); }
}
