using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Delaunay;
using Delaunay.Geo;
using System;

public class Mapping : MonoBehaviour
{
    public GameObject roadPrefab;
    public GameObject roadBikePrefab;
    public GameObject roadBikeCenterPrefab;
    public GameObject buildingPrefab;
    public GameObject holderSurfBike;
    public GameObject holderSurfCar;
    public GameObject holderBuildings;
    public GameObject carPrefab;
    public GameObject bikePrefab;
    public GameObject policePrefab;
    public GameObject ambulancePrefab;

    GameObject[] cars;
    GameObject[] bikes;
    GameObject police;
    GameObject ambulance;
    float[,] map;

    public Material land;
    public Texture2D tx;
    public const int NPOINTS = 50; //centre des cellules de voronoi
    public const int NBUILDINGS = 100;
    public const int NCAR = 30;
    public const int NBIKE = 15;
    public const int WIDTH = 200;	//résolution image
    public const int HEIGHT = 200;

    private List<Vector2> m_points;
    private List<LineSegment> m_edges = null;
    private List<LineSegment> m_spanningTree;
    private List<LineSegment> m_delaunayTriangulation;

    System.Random rnd = new System.Random();

    private float[,] createMap() //fait carte de densité (zone noire et blanche)
    {
        float[,] map = new float[HEIGHT, WIDTH];
        for (int i = 0; i < HEIGHT; i++)
            for (int j = 0; j < WIDTH; j++)
                map[i, j] = Mathf.PerlinNoise(0.02f * i + 0.43f, 0.018f * j + 0.22f);	//(amplitude, période) (je crois)
        return map;
    }

    // Start is called before the first frame update
    void Start()
    {
        Transform trBike = holderSurfBike.GetComponent<Transform>();
        Transform trCar = holderSurfCar.GetComponent<Transform>();
        Transform trBuilding = holderBuildings.GetComponent<Transform>();

        map = createMap();
        Color[] pixels = createPixelMap(map);

        /* Create random points points */
        m_points = new List<Vector2>();
        List<uint> colors = new List<uint>();
        /* Randomly pick vertices */
        for (int i = 0; i < NPOINTS; i++)
        {
            colors.Add((uint)0);
            int x = UnityEngine.Random.Range(1, HEIGHT - 2);
            int y = UnityEngine.Random.Range(1, WIDTH - 2);
            float limit = map[x, y];
            while (UnityEngine.Random.Range(0f, 1f) > limit)
            {
                x = UnityEngine.Random.Range(1, HEIGHT - 2);
                y = UnityEngine.Random.Range(1, WIDTH - 2);
                limit = map[x, y];
            }
            Vector2 vec = new Vector2(x, y);
            m_points.Add(vec);
        }

        /* Generate Graphs */
        Delaunay.Voronoi v = new Delaunay.Voronoi(m_points, colors, new Rect(0, 0, WIDTH - 1, HEIGHT - 1));
        m_edges = v.VoronoiDiagram();
        m_spanningTree = v.SpanningTree(KruskalType.MINIMUM);
        m_delaunayTriangulation = v.DelaunayTriangulation();

        //Create Roads
        Color color = Color.blue;
        for (int i = 0; i < m_edges.Count; i++)
        {
            LineSegment seg = m_edges[i];
            Vector2 left = (Vector2)seg.p0;
            Vector2 right = (Vector2)seg.p1;
            DrawLine(pixels, left, right, color); // Shows Voronoi diagram 

            //Position
            Vector3 realLeft = CoordMap2Plane(left);
            Vector3 realRight = CoordMap2Plane(right);
            //Angle and distance
            Vector3 allignedVect = realRight - realLeft;
            float angle = Mathf.Atan2(allignedVect.z, allignedVect.x) * Mathf.Rad2Deg;
            float dist = allignedVect.magnitude;

            GameObject road = Instantiate(roadPrefab, realLeft, Quaternion.Euler(0, -angle, 0), trCar);
            road.transform.localScale += new Vector3(dist * 10 - 1, 0f, 0f);

            //Bike road next to it
            Vector3 perpendicular = Vector3.Cross(allignedVect, Vector3.up).normalized;
            perpendicular = Vector3.ClampMagnitude(perpendicular, 0.16f);
            perpendicular *= rnd.Next(2) == 0 ? 1 : -1;
            GameObject roadBike = Instantiate(roadBikePrefab, realLeft + perpendicular, Quaternion.Euler(0, -angle, 0), trBike);
            roadBike.transform.localScale += new Vector3(dist * 10 - 1, 0f, 0f);
            //GameObject roadBikeCenter = Instantiate(roadBikeCenterPrefab, realLeft + perpendicular, Quaternion.Euler(0, -angle, 0), trBike);
            //roadBikeCenter.transform.localScale += new Vector3(dist * 10 - 1, 0f, 0f);
        }

        //Create Buildings
        for (int i = 0; i < NBUILDINGS; i++)
        {
            //choose road to have a house
            LineSegment seg = m_edges[UnityEngine.Random.Range(0, m_edges.Count)];
            Vector2 left = (Vector2)seg.p0;
            Vector2 right = (Vector2)seg.p1;
            int middleX = (int)((left.x + right.x) / 2);
            int middleY = (int)((left.y + right.y) / 2);
            float limit = map[middleX, middleY];
            while (UnityEngine.Random.Range(0f, 1f) > limit)
            {
                seg = m_edges[UnityEngine.Random.Range(0, m_edges.Count)];
                left = (Vector2)seg.p0;
                right = (Vector2)seg.p1;
                middleX = (int)((left.x + right.x) / 2);
                middleY = (int)((left.y + right.y) / 2);
                limit = map[middleX, middleY];
            }
            //extremities of road
            Vector3 realLeft = CoordMap2Plane(left);
            Vector3 realRight = CoordMap2Plane(right);

            //position along the road
            float t = UnityEngine.Random.Range(0.2f, 0.8f);
            Vector3 pos = new Vector3(realLeft.x + t * (realRight.x - realLeft.x), 0, realLeft.z + t * (realRight.z - realLeft.z));

            //which side
            Vector3 allignedVect = realRight - realLeft;
            Vector3 perpendicular = Vector3.Cross(allignedVect, Vector3.up).normalized;
            perpendicular = Vector3.ClampMagnitude(perpendicular, 0.3f);
            perpendicular *= rnd.Next(2) == 0 ? 1 : -1;

            //instantiate
            GameObject building = Instantiate(buildingPrefab, pos + perpendicular, Quaternion.LookRotation(-perpendicular), trBuilding);
            building.transform.localScale += new Vector3(0, limit - 1, 0f);
        }

        //Create surface for navigation
        NavMeshSurface surfaceCar = holderSurfCar.GetComponent<NavMeshSurface>();
        surfaceCar.BuildNavMesh();
        NavMeshSurface surfaceBike = holderSurfBike.GetComponent<NavMeshSurface>();
        surfaceBike.BuildNavMesh();

        


        /* Apply pixels to texture */
        tx = new Texture2D(WIDTH, HEIGHT);
        land.SetTexture("_MainTex", tx);
        tx.SetPixels(pixels);
        tx.Apply();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Create agents
            //police
            Vector3 posPol = CoordMap2Plane((Vector2)m_edges[0].p0);
            police = Instantiate(policePrefab, posPol, Quaternion.identity);
            police.GetComponent<Police>().Init(map, m_edges, HEIGHT, WIDTH);
            //ambulance
            Vector3 posAmb = CoordMap2Plane((Vector2)m_edges[m_edges.Count - 1].p0);
            ambulance = Instantiate(ambulancePrefab, posAmb, Quaternion.identity);
            ambulance.GetComponent<Ambulance>().Init(map, m_edges, HEIGHT, WIDTH);
            //cars
            cars = new GameObject[NCAR];
            for (int i = 0; i < NCAR; i++)
            {
                //choose start
                LineSegment seg = m_edges[UnityEngine.Random.Range(0, m_edges.Count)];
                Vector2 left = (Vector2)seg.p0;
                float limit = map[(int)left.x, (int)left.y];
                while (UnityEngine.Random.Range(0f, 1f) > limit)
                {
                    seg = m_edges[UnityEngine.Random.Range(0, m_edges.Count)];
                    left = (Vector2)seg.p0;
                    limit = map[(int)left.x, (int)left.y];
                }
                cars[i] = Instantiate(carPrefab, CoordMap2Plane(left), Quaternion.identity);
                //choose direction
                seg = m_edges[UnityEngine.Random.Range(0, m_edges.Count)];
                left = (Vector2)seg.p0;
                limit = map[(int)left.x, (int)left.y];
                while (UnityEngine.Random.Range(0f, 1f) > limit)
                {
                    seg = m_edges[UnityEngine.Random.Range(0, m_edges.Count)];
                    left = (Vector2)seg.p0;
                    limit = map[(int)left.x, (int)left.y];
                }
                cars[i].GetComponent<NavMeshAgent>().destination = CoordMap2Plane(left);
                cars[i].GetComponent<Car>().Init(map, m_edges, HEIGHT, WIDTH, police);
            }

            //bikes
            bikes = new GameObject[NBIKE];
            for (int i = 0; i < NBIKE; i++)
            {
                //choose start
                LineSegment seg = m_edges[UnityEngine.Random.Range(0, m_edges.Count)];
                Vector2 left = (Vector2)seg.p0;
                float limit = map[(int)left.x, (int)left.y];
                while (UnityEngine.Random.Range(0f, 1f) > limit)
                {
                    seg = m_edges[UnityEngine.Random.Range(0, m_edges.Count)];
                    left = (Vector2)seg.p0;
                    limit = map[(int)left.x, (int)left.y];
                }
                bikes[i] = Instantiate(bikePrefab, CoordMap2Plane(left), Quaternion.identity);
                //choose direction
                seg = m_edges[UnityEngine.Random.Range(0, m_edges.Count)];
                left = (Vector2)seg.p0;
                limit = map[(int)left.x, (int)left.y];
                while (UnityEngine.Random.Range(0f, 1f) > limit)
                {
                    seg = m_edges[UnityEngine.Random.Range(0, m_edges.Count)];
                    left = (Vector2)seg.p0;
                    limit = map[(int)left.x, (int)left.y];
                }
                bikes[i].GetComponent<NavMeshAgent>().destination = CoordMap2Plane(left);
                bikes[i].GetComponent<Bike>().Init(map, m_edges, HEIGHT, WIDTH, ambulance);
            }
        }
    }

    Vector3 CoordMap2Plane(Vector2 vec)
    { return new Vector3(vec.y / WIDTH * 10 - 5, 0, 5 - vec.x / HEIGHT * 10); }



    /* Functions to create and draw on a pixel array */
    private Color[] createPixelMap(float[,] map)
    {
        Color[] pixels = new Color[WIDTH * HEIGHT];
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
            {
                pixels[i * HEIGHT + j] = Color.Lerp(Color.white, Color.black, map[i, j]);
            }
        return pixels;
    }
    private void DrawPoint(Color[] pixels, Vector2 p, Color c)
    {
        if (p.x < WIDTH && p.x >= 0 && p.y < HEIGHT && p.y >= 0)
            pixels[(int)p.x * HEIGHT + (int)p.y] = c;
    }
    // Bresenham line algorithm
    private void DrawLine(Color[] pixels, Vector2 p0, Vector2 p1, Color c)
    {
        int x0 = (int)p0.x;
        int y0 = (int)p0.y;
        int x1 = (int)p1.x;
        int y1 = (int)p1.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        while (true)
        {
            if (x0 >= 0 && x0 < WIDTH && y0 >= 0 && y0 < HEIGHT)
                pixels[x0 * HEIGHT + y0] = c;

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}
