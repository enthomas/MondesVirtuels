using UnityEngine;
using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;
using System;

public class VoronoiDemo : MonoBehaviour
{
	public GameObject cube;

    public Material land;
    public Texture2D tx;
    public const int NPOINTS = 500; //centre des cellules de voronoi
    public const int WIDTH = 200;	//résolution image
    public const int HEIGHT = 200;

    private List<Vector2> m_points;
	private List<LineSegment> m_edges = null;
	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;

    private float [,] createMap() //fait carte de densité (zone noire et blanche)
    {
        float [,] map = new float[WIDTH, HEIGHT];
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
                map[i, j] = Mathf.PerlinNoise(0.02f * i + 0.43f, 0.018f * j + 0.22f);	//(amplitude, période) (je crois)
        return map;
    }

	void Start ()
	{
        float [,] map=createMap();
        Color[] pixels = createPixelMap(map);

        /* Create random points points */
		m_points = new List<Vector2> ();
		List<uint> colors = new List<uint> ();
		/* Randomly pick vertices */
		for (int i = 0; i < NPOINTS; i++) {
			colors.Add ((uint)0);
			//Vector2 vec = new Vector2(Random.Range(0, WIDTH-1), Random.Range(0, HEIGHT-1)); 
			//Nouvelles versions qui respecte map
			int x = UnityEngine.Random.Range(0, WIDTH - 1);
			int y = UnityEngine.Random.Range(0, HEIGHT - 1);
			float limit = map[x, y];
			while (UnityEngine.Random.Range(0f, 1f) > limit)
            {
				x = UnityEngine.Random.Range(0, WIDTH - 1);
				y = UnityEngine.Random.Range(0, HEIGHT - 1);
				limit = map[x, y];
			}
			Vector2 vec = new Vector2(x, y);
			m_points.Add (vec);
		}
		/* Generate Graphs */
		Delaunay.Voronoi v = new Delaunay.Voronoi (m_points, colors, new Rect (0, 0, WIDTH, HEIGHT));
		m_edges = v.VoronoiDiagram ();
		m_spanningTree = v.SpanningTree (KruskalType.MINIMUM);
		m_delaunayTriangulation = v.DelaunayTriangulation ();

		Color color = Color.blue;
		/* Shows Voronoi diagram */
		//Debug.Log("nb edge : " + Convert.ToString(m_edges.Count));
		for (int i = 0; i < m_edges.Count; i++) {
			LineSegment seg = m_edges[i];				
			Vector2 left = (Vector2)seg.p0;
			Vector2 right = (Vector2)seg.p1;
			DrawLine(pixels, left, right, color);

			// Get angle and distance of road
			Vector2 alligned_vect = right - left;

			float angle = Mathf.Atan2(alligned_vect.y, alligned_vect.x) * Mathf.Rad2Deg;
			float dist = alligned_vect.magnitude;
			GameObject road = Instantiate(cube, new Vector3(left.y * 10 / WIDTH - 5, 0, left.x * 10 / HEIGHT - 5), Quaternion/*.LookRotation(alligned_vect)*/.Euler(0, angle, 0));
			road.transform.localScale = new Vector3(dist * 10 / WIDTH, 1f, 1f);

			//Vector3 left3 = new Vector3(left.y * 10 / WIDTH - 5, 0, left.x * 10 / HEIGHT - 5);
			//Vector3 right3 = new Vector3(right.y * 10 / WIDTH - 5, 0, right.x * 10 / HEIGHT - 5);
			//float angle = right.x - left.x == 0 ? 0 : (float)(Math.Atan((right.y - left.y) / (right.x - left.y)) * 180 / Math.PI);
			//Vector3 dir = new Vector3(right.y, 0, right.x) - new Vector3(left.y, 0, left.x);
			//Vector3 dir = right3 - left3;
			//if (i % 100 == 0)
			//{
			//	Debug.Log(left3);
			//	Debug.Log(right3);
			//	Debug.Log(dir.magnitude);
			//	road.transform.localScale = new Vector3(dir.magnitude, 1, 1); 
			//}
		}


		color = Color.red;
		/* Shows Delaunay triangulation */
		//if (m_delaunayTriangulation != null) {
		//	for (int i = 0; i < m_delaunayTriangulation.Count; i++) {
		//			LineSegment seg = m_delaunayTriangulation [i];				
		//			Vector2 left = (Vector2)seg.p0;
		//			Vector2 right = (Vector2)seg.p1;
		//			DrawLine (pixels,left, right,color);
		//	}
		//}

        /* Shows spanning tree */
        
		color = Color.black;
		//if (m_spanningTree != null) {
		//	for (int i = 0; i< m_spanningTree.Count; i++) {
		//		LineSegment seg = m_spanningTree [i];				
		//		Vector2 left = (Vector2)seg.p0;
		//		Vector2 right = (Vector2)seg.p1;
		//		DrawLine (pixels,left, right,color);
		//	}
		//}
        /* Apply pixels to texture */
        tx = new Texture2D(WIDTH, HEIGHT);
        land.SetTexture ("_MainTex", tx);
		tx.SetPixels (pixels);
		tx.Apply ();

	}



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
    private void DrawPoint (Color [] pixels, Vector2 p, Color c) {
		if (p.x<WIDTH&&p.x>=0&&p.y<HEIGHT&&p.y>=0) 
		    pixels[(int)p.x*HEIGHT+(int)p.y]=c;
	}
	// Bresenham line algorithm
	private void DrawLine(Color [] pixels, Vector2 p0, Vector2 p1, Color c) {
		int x0 = (int)p0.x;
		int y0 = (int)p0.y;
		int x1 = (int)p1.x;
		int y1 = (int)p1.y;

		int dx = Mathf.Abs(x1-x0);
		int dy = Mathf.Abs(y1-y0);
		int sx = x0 < x1 ? 1 : -1;
		int sy = y0 < y1 ? 1 : -1;
		int err = dx-dy;
		while (true) {
            if (x0>=0&&x0<WIDTH&&y0>=0&&y0<HEIGHT)
    			pixels[x0*HEIGHT+y0]=c;

			if (x0 == x1 && y0 == y1) break;
			int e2 = 2*err;
			if (e2 > -dy) {
				err -= dy;
				x0 += sx;
			}
			if (e2 < dx) {
				err += dx;
				y0 += sy;
			}
		}
	}
}