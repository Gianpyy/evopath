using Godot;
using System;
using System.Collections.Generic;

public class VertexPosition : IEquatable<VertexPosition>, IComparable<VertexPosition>
{
	public static List<Vector2> possibleNeighbours = new List<Vector2>
	{
		new Vector2(0, -1),
		new Vector2(0, 1),
		new Vector2(1, 0),
		new Vector2(-1, 0)
	};

	public float totalCost, estimatedCost;
	public VertexPosition previousVertex = null;
	private Vector2 position;
	private bool isTaken;

	// Getters
	public int X { get => (int) position.X; }

	public int Y { get => (int) position.Y; }

	public bool IsTaken { get => isTaken; }

	public Vector2 Position { get => position; }

	// Costruttore
	public VertexPosition(Vector2 position, bool isTaken = false)
	{
		this.position = position;
		this.isTaken = isTaken;
		this.estimatedCost = 0;
		this.totalCost = 1;
	}

	// Overrides
	public int GetHashCode(VertexPosition obj)
	{
		return obj.GetHashCode();
	}

	public override int GetHashCode()
	{
		return position.GetHashCode();
	} 

	// Metodi delle interfacce
	public int CompareTo(VertexPosition other)
	{
		if (this.estimatedCost < other.estimatedCost) 
			return -1;
		
		if (this.estimatedCost > other.estimatedCost)
			return 1;

		return 0;
	} 

	public bool Equals(VertexPosition other)
	{
		return Position == other.Position;
	}
}
