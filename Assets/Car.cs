using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Delaunay.Geo;
using System;

public class Car : MonoBehaviour
{
    bool init = false;
    float[,] map;
    List<LineSegment> m_edges;
    int HEIGHT;
    int WIDTH;
    bool findingPath = false;
    GameObject police;
    DateTime initTime;

    public void Init(float[,] passedMap, List<LineSegment> edges, int height, int width, GameObject pol)
    {
        map = passedMap;
        m_edges = edges;
        HEIGHT = height;
        WIDTH = width;
        police = pol;
        init = true;
        initTime = DateTime.Now;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (DateTime.Now - initTime > new TimeSpan(0, 0, 2))
        {
            if (collider.gameObject.name.Remove(4) == "Bike")
            { police.GetComponent<Police>().CallPolice(this.gameObject); }
            else if (collider.gameObject.name.Remove(6) == "Police")
            { StartCoroutine(Die()); }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (init && !findingPath && (GetComponent<NavMeshAgent>().remainingDistance < 0.06f || GetComponent<NavMeshAgent>().isStopped || !GetComponent<NavMeshAgent>().hasPath))
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

    IEnumerator Die()
    {
        yield return new WaitForSecondsRealtime(1);
        Destroy(this.gameObject);
    }

    Vector3 CoordMap2Plane(Vector2 vec)
    { return new Vector3(vec.y / WIDTH * 10 - 5, 0, 5 - vec.x / HEIGHT * 10); }
}
