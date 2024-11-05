using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class Node
{
    public Vector2 Position { get; init; }
    private List<Connection> _connections = new();

    public IList<Connection> GetConnections() => 
        _connections.AsReadOnly();

    public void AddConnection(Connection connection) =>
        _connections.Add(connection);

    public Node(Vector2 position)
    {
        Position = position;
    }
}

[System.Serializable]
public class Connection
{
    public Node From { get; init; }
    public Node To { get; init; }
    public float Cost { get; init; }

    public Connection(Node from, Node to, float cost)
    {
        From = from;
        To = to;
        Cost = cost;
    }
}

[System.Serializable]
public class TileGraph
{
    public Vector2 Origin { get; init; }
    public int ColumnCount { get; init; } = 12;
    public int RowCount { get; init; } = 12;
    public float ColumnWidth { get; init; } = 1.5f;
    public float RowHeight { get; init; } = 1.5f;

    private Dictionary<Vector2Int, Node> _nodes = new();

    public Node GetNode(Vector2 position)
    {
        int tileX = Mathf.FloorToInt(Mathf.Abs(position.x - Origin.x) / ColumnWidth);
        int tileY = Mathf.FloorToInt(Mathf.Abs(position.y - Origin.y) / RowHeight);

        if (tileX >= ColumnCount || tileX < 0 || tileY >= RowCount || tileY < 0)
            return null;

        Vector2Int tile = new Vector2Int(tileX, tileY);
        if (!_nodes.ContainsKey(tile))
            return null;

        return _nodes[tile];
    }

    public Node GetNodeByTile(Vector2Int tile)
    {
        if (tile.x >= ColumnCount || tile.x < 0 || tile.y >= RowCount || tile.y < 0)
            return null;

        if (!_nodes.ContainsKey(tile))
            return null;

        return _nodes[tile];
    }

    public void AddNode(Node node)
    {
        int tileX = Mathf.FloorToInt(Mathf.Abs(node.Position.x - Origin.x) / ColumnWidth);
        int tileY = Mathf.FloorToInt(Mathf.Abs(node.Position.y - Origin.y) / RowHeight);

        Vector2Int tile = new(tileX, tileY);
        if (_nodes.ContainsKey(tile))
            throw new ArgumentException();

        _nodes.Add(tile, node);
    }


    public TileGraph(Vector2 origin, int columnCount, int rowCount, float columnWidth, float rowHeight)
    {
        Origin = origin;
        ColumnCount = columnCount;
        RowCount = rowCount;
        ColumnWidth = columnWidth;
        RowHeight = rowHeight;
    }
}
