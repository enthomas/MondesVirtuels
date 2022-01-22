using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Delaunay.Geo;
using System;

public class Bike : MonoBehaviour
{
    bool init = false;
    float[,] map;
    List<LineSegment> m_edges;
    int HEIGHT;
    int WIDTH;
    bool findingPath = false;
    bool collided = false;
    GameObject ambulance;
    DateTime initTime;

    public void Init(float[,] passedMap, List<LineSegment> edges, int height, int width, GameObject amb)
    {
        map = passedMap;
        m_edges = edges;
        HEIGHT = height;
        WIDTH = width;
        ambulance = amb;
        init = true;
        initTime = DateTime.Now;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (DateTime.Now - initTime > new TimeSpan(0, 0, 2)) 
        {
            if (collider.gameObject.name.Remove(3) == "Car")
            {
                GetComponent<NavMeshAgent>().isStopped = true;
                if (!collided)
                { ambulance.GetComponent<Ambulance>().CallAmbulance(this.gameObject); }
                collided = true;
            }
            else if (collider.gameObject.name.Remove(9) == "Ambulance")
            {
                GetComponent<NavMeshAgent>().isStopped = false;
                collided = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (init && !collided && !findingPath && GetComponent<NavMeshAgent>().remainingDistance < 0.06f)
        {
            findingPath = true;
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
            findingPath = false;
        }
    }

    Vector3 CoordMap2Plane(Vector2 vec)
    { return new Vector3(vec.y / WIDTH * 10 - 5, 0, 5 - vec.x / HEIGHT * 10); }
}
