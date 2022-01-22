using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Delaunay.Geo;

public class Ambulance : MonoBehaviour
{
    bool init = false;
    float[,] map;
    List<LineSegment> m_edges;
    int HEIGHT;
    int WIDTH;
    List<GameObject> bikes;

    public void Init(float[,] passedMap, List<LineSegment> edges, int height, int width)
    {
        map = passedMap;
        m_edges = edges;
        HEIGHT = height;
        WIDTH = width;
        init = true;
        bikes = new List<GameObject>();
    }

    void OnTriggerEnter(Collider collider)
    {
        if (bikes.Count > 0 && collider.gameObject == bikes[0])
        {
            bikes.RemoveAt(0);
            if (bikes.Count > 0)
            { GetComponent<NavMeshAgent>().destination = bikes[0].transform.position; }
            else
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
    }

    public void CallAmbulance(GameObject bike)
    {
        bikes.Add(bike);
        if (bikes.Count > 0)
        { GetComponent<NavMeshAgent>().destination = bikes[0].transform.position; }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(bikes.Count);
        if (init && bikes.Count > 0 && GetComponent<NavMeshAgent>().destination == transform.position)
        { GetComponent<NavMeshAgent>().destination = bikes[0].transform.position; }
    }

    Vector3 CoordMap2Plane(Vector2 vec)
    { return new Vector3(vec.y / WIDTH * 10 - 5, 0, 5 - vec.x / HEIGHT * 10); }
}
