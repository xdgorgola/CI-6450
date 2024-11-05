using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PathfindingAlgorithms;

public class WorldGrid : MonoBehaviour
{
    [Header("Grid sizes")]
    [SerializeField]
    private Vector2 _origin;
    [Min(1)]
    [SerializeField]
    private int _columnCount = 12;
    [Min(1)]
    [SerializeField]
    private int _rowCount = 12;
    [Min(0.3f)]
    [SerializeField]
    private float _columnWidth = 1.5f;
    [Min(0.3f)]
    [SerializeField]
    private float _rowHeight = 1.5f;

    [SerializeField]
    [HideInInspector]
    private TileGraph _graph = null;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField]
    private LayerMask d_obstacle_layers;
    [SerializeField]
    private Vector2Int d_start_tile = Vector2Int.zero;
    [SerializeField]
    private Vector2Int d_end_tile = Vector2Int.zero;
    private LinkedList<Connection> d_path;

    [SerializeField]
    private bool d_draw_grid = true;
    [SerializeField]
    private bool d_draw_path = true;
    [SerializeField]
    private bool d_draw_nodes = true;
    [SerializeField]
    private bool d_draw_connections = true;
#endif

    private void Awake()
    {
        InitGraph();
    }


    public List<Vector2> CalculatePath(Vector2 start, Vector2 goal)
    {
        LinkedList<Connection> connections = AStar(_graph, _graph.GetNode(start), _graph.GetNode(goal));
        d_path = connections;

        List<Vector2> wps = connections.Select((c) => c.From.Position).ToList();
        wps.Insert(0, start);
        if ((wps[^1] - goal).magnitude > 0.3f)
            wps.Add(goal);

        return wps;
    }

    private void OnDrawGizmos()
    {
        if (d_draw_grid)
        {
            for (int c = 0; c <= _columnCount; ++c)
            {
                Vector2 start = _origin + Vector2.right * (c * _columnWidth);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(start, start + Vector2.down * (_rowCount * _rowHeight));
            }

            for (int r = 0; r <= _rowCount; ++r)
            {
                Vector2 start = _origin + Vector2.down * (r * _rowHeight);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(start, start + Vector2.right * (_columnCount * _columnWidth));
            }
        }

        if (_graph is null)
            return;

              
        for (int r = 0; r < _rowCount; r++)
        {
            for (int c = 0; c < _columnCount; c++)
            {
                Gizmos.color = Color.green;
                Vector2 cellOrigin = _origin + Vector2.right * (_columnWidth * (c + 0.5f)) + Vector2.down * (_rowHeight * (r + 0.5f));
                int tileX = Mathf.FloorToInt(Mathf.Abs(cellOrigin.x - _origin.x) / _columnWidth);
                int tileY = Mathf.FloorToInt(Mathf.Abs(cellOrigin.y - _origin.y) / _rowHeight);

                Node n = _graph.GetNodeByTile(new Vector2Int(tileX, tileY));
                if (n is null)
                    Gizmos.color = Color.red;

                if (d_draw_nodes)
                    Gizmos.DrawSphere(new Vector3(cellOrigin.x, cellOrigin.y, 0f), Mathf.Min(_columnWidth, _rowHeight) / 6f);

                if (n is null || !d_draw_connections)
                    continue;

                Gizmos.color = Color.blue;
                foreach (Connection con in n.GetConnections())
                    Gizmos.DrawLine(n.Position, con.To.Position);
            }
        }

        if (d_path is null || !d_draw_path)
            return;

        Node cur = d_path.First.Value.From;
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(cur.Position, Mathf.Min(_columnWidth, _rowHeight) / 6f);

        foreach (Connection c in d_path)
        {
            cur = c.To;
            Gizmos.DrawSphere(cur.Position, Mathf.Min(_columnWidth, _rowHeight) / 6f);
            Gizmos.DrawLine(c.From.Position, c.To.Position);
        }
    }

    private void InitGraph()
    {
        _graph = new TileGraph(_origin, _columnCount, _rowCount, _columnWidth, _rowHeight);
        Node[,] nodes = new Node[_rowCount, _columnCount];
        for (int r = 0; r < _rowCount; r++)
        {
            for (int c = 0; c < _columnCount; c++)
            {
                Vector2 cellOrigin = _origin + Vector2.right * (_columnWidth * (c + 0.5f)) + Vector2.down * (_rowHeight * (r + 0.5f));
                Collider2D hit = Physics2D.OverlapBox(cellOrigin, new Vector2(_columnWidth - 0.05f, _rowHeight - 0.05f), 0.0f, d_obstacle_layers);
                if (hit)
                    continue;

                int tileX = Mathf.FloorToInt(Mathf.Abs(cellOrigin.x - _origin.x) / _columnWidth);
                int tileY = Mathf.FloorToInt(Mathf.Abs(cellOrigin.y - _origin.y) / _rowHeight);
                nodes[tileY, tileX] = new Node(cellOrigin);
                _graph.AddNode(nodes[tileY, tileX]);
            }
        }

        float xCost = _columnWidth;
        float yCost = _rowHeight;
        float diagCost = Mathf.Sqrt(xCost * xCost + yCost + yCost * yCost);
        for (int r = 0; r < _rowCount; r++)
        {
            for (int c = 0; c < _columnCount; c++)
            {
                Vector2 cellOrigin = _origin + Vector2.right * (_columnWidth * (c + 0.5f)) + Vector2.down * (_rowHeight * (r + 0.5f));
                int tileX = Mathf.FloorToInt(Mathf.Abs(cellOrigin.x - _origin.x) / _columnWidth);
                int tileY = Mathf.FloorToInt(Mathf.Abs(cellOrigin.y - _origin.y) / _rowHeight);
                Vector2Int tile = new(tileX, tileY);

                Node cn = _graph.GetNodeByTile(tile);
                if (cn is null)
                    continue;

                for (int i = -1; i < 2; ++i)
                {
                    for (int j = -1; j < 2; ++j)
                    {
                        Node nb = _graph.GetNodeByTile(tile + Vector2Int.right * i + Vector2Int.down * j);
                        if (nb is null)
                            continue;

                        float cost = i != 0 && j != 0 ? diagCost : (i != 0 ? xCost : yCost);
                        cn.AddConnection(new Connection(cn, nb, cost));
                    }
                }
            }
        }
    }
}
