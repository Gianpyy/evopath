using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public static class AStar 
{
	public static List<Vector2> GetPath(Vector2 start, Vector2 exit, bool[] obstacles, Map map)
	{
		VertexPosition startVertex = new VertexPosition(start);
		VertexPosition exitVertex = new VertexPosition(exit);

		List<Vector2> path = new List<Vector2>();

		List<VertexPosition> openedList = new List<VertexPosition>();
		HashSet<VertexPosition> closedList = new HashSet<VertexPosition>();

		startVertex.estimatedCost = ManhattanDistance(startVertex, exitVertex);

		openedList.Add(startVertex);

		VertexPosition currentVertex = null;

		while(openedList.Count > 0)
		{
			openedList.Sort();
			currentVertex = openedList[0];

			if(currentVertex.Equals(exitVertex))
			{
				while(currentVertex != startVertex)
				{
					path.Add(currentVertex.Position);
					currentVertex = currentVertex.previousVertex;
				}
				path.Reverse();
				break;
			}
			
			VertexPosition[] neighbours = FindNeighbours(currentVertex, map, obstacles);
			foreach(VertexPosition neighbour in neighbours)
			{
				if (neighbour == null || closedList.Contains(neighbour))
					continue;
				
				if (neighbour.IsTaken == false)
				{
					float totalCost = currentVertex.totalCost + 1;
					float neighbourEstimatedCost = ManhattanDistance(neighbour, exitVertex);
					neighbour.totalCost = totalCost;
					neighbour.previousVertex = currentVertex;
					neighbour.estimatedCost = totalCost + neighbourEstimatedCost;

					if(openedList.Contains(neighbour) == false)
					{
						openedList.Add(neighbour);
					}
				}
			}
			closedList.Add(currentVertex);
			openedList.Remove(currentVertex);
		}

		return path;
	}

	private static VertexPosition[] FindNeighbours(VertexPosition currentVertex, Map map, bool[] obstacles)
	{
		VertexPosition[] neighbours = new VertexPosition[4];
		
		int arrayIndex = 0;
		foreach(Vector2 neighbour in VertexPosition.possibleNeighbours)
		{
			Vector2 position = new Vector2(currentVertex.X + neighbour.X, currentVertex.Y + neighbour.Y);
			if (map.IsCellValid(position.X, position.Y))
			{
				int index = map.CalculateIndexFromCoordinates(position.X, position.Y);
				neighbours[arrayIndex] = new VertexPosition(position, obstacles[index]);
				arrayIndex++;
			}
		}

		return neighbours;
	}

	private static float ManhattanDistance(VertexPosition startVertex, VertexPosition exitVertex)
	{
		return Mathf.Abs(startVertex.X - exitVertex.X) + Mathf.Abs(startVertex.Y - exitVertex.Y);
	}
}
