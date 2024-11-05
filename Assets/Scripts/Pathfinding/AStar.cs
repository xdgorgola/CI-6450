using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PathfindingAlgorithms
{
    private class NodeRecord: IComparable<NodeRecord>
    {
        public readonly Node node;
        public Connection connection;
        public float pathCost;
        public float estimatedCost;


        public NodeRecord(Node node, Connection connection, float pathCost, float estimatedCost)
        {
            this.node = node ?? throw new ArgumentNullException(nameof(node));
            this.connection = connection;
            this.pathCost = pathCost;
            this.estimatedCost = estimatedCost;
        }


        public int CompareTo(NodeRecord other)
        {
            return pathCost.CompareTo(other.pathCost);
        }
    }

    public static float TileHeuristic(Vector2 from, Vector2 target, bool allowDiagonals, float xCost, float yCost)
    {
        float dx = Mathf.Abs(target.x - from.x);
        float dy = Mathf.Abs(target.y - from.y);

        if (allowDiagonals)
        {
            float diagCost = Mathf.Sqrt(xCost * xCost + yCost * yCost);
            return (dx + dy) + (diagCost - 2) * Mathf.Min(dx, dy);
            //return xCost * dx + yCost * dy + (diagCost - 2 * Mathf.Min(xCost * dx, yCost * dy)) * Mathf.Min(dx, dy);
        }

        return xCost * dx + yCost * dy;
    }

    public static LinkedList<Connection> AStar(TileGraph graph, Node start, Node end)
    {
        float xCost = graph.ColumnWidth;
        float yCost = graph.RowHeight;

        PriorityQueue<NodeRecord> openQueue = new PriorityQueue<NodeRecord>();
        openQueue.Enqueue(new NodeRecord(
                start,
                null,
                0f,
                TileHeuristic(start.Position, end.Position, true, xCost, yCost))
            );

        Dictionary<Node, NodeRecord> node2record = new Dictionary<Node, NodeRecord> { { start, openQueue.Peek() } };
        HashSet<Node> closedSet = new HashSet<Node>() { node2record[start].node };
        HashSet<Node> openSet = new HashSet<Node>();
        NodeRecord cur = node2record[start];
        float endNodeHeuristic;

        while (openQueue.Count > 0)
        {
            cur = openQueue.Dequeue();
            if (cur.node == end)
                break;

            IList<Connection> connections = cur.node.GetConnections();
            foreach (Connection c in connections)
            {
                Node nn = c.To;
                NodeRecord nnRecord;
                float nnCost = cur.pathCost + c.Cost;
                if (closedSet.Contains(nn))
                {
                    nnRecord = node2record[nn];
                    if (nnRecord.pathCost <= nnCost)
                        continue;

                    closedSet.Remove(nn);
                    endNodeHeuristic = nnRecord.estimatedCost - nnRecord.pathCost;
                }
                else if (openSet.Contains(nn))
                {
                    nnRecord = node2record[nn];
                    if (nnRecord.pathCost <= nnCost)
                        continue;

                    endNodeHeuristic = nnRecord.estimatedCost - nnRecord.pathCost;
                }
                else
                {
                    nnRecord = new NodeRecord(nn, c, 0f, 0f);
                    endNodeHeuristic = TileHeuristic(nn.Position, end.Position, true, xCost, yCost);
                }

                nnRecord.pathCost = nnCost;
                nnRecord.connection = c;
                nnRecord.estimatedCost = nnCost + endNodeHeuristic;

                if (!openSet.Contains(nn))
                {
                    openSet.Add(nn);
                    openQueue.Enqueue(nnRecord);
                    node2record.Add(nn, nnRecord);
                }
            }

            openSet.Remove(cur.node);
            closedSet.Add(cur.node);
        }

        if (cur.node != end)
            return null;

        LinkedList<Connection> path = new LinkedList<Connection>();
        while (cur.node != start)
        {
            path.AddFirst(cur.connection);
            cur = node2record[cur.connection.From];
        }

        return path;
    }
}
