using System;
using System.Collections.Generic;
using Godot;
using Vector3 = Godot.Vector3;


// Individuo della popolazione
public class CandidateMap
{
	// Griglia associata alla mappa candidata
	public Map grid;
	// Array degli ostacoli ottenuto normalizzando la griglia. griglia -> array
	private bool[] obstacles = null;
	// Inizio e fine del percorso
	private Vector3 startPoint, exitPoint;
	// Numero di pezzi da aggiungere alla mappa (Knights)
	private int nPieces = 0;
	// Lista dei pezzi aggiunti alla mappa
	private List<KnightPiece> knightPiecesList = new List<KnightPiece>();


	public CandidateMap(Map grid, int nPieces)
	{
		this.grid = grid;
		this.nPieces = nPieces;
	}

	/// <summary>
	/// Crea la mappa candidata con posizione di inizio e fine e Posizionamento degli ostacoli
	/// </summary>
	/// <param name="startPos">Posizione iniziale del percorso </param>
	/// <param name="exitPos">Posizione finale del percorso</param>
	/// <param name="autoRepair"></param>
	public void CreateCandidateMap(Vector3 startPos, Vector3 exitPos, bool autoRepair = false)
	{
		startPoint = startPos;
		exitPoint = exitPos;

		RandomlyPlaceKnightPieces(nPieces);

		PlaceKnightObstacles();
	}

	/// <summary>
	/// Controlla la presenza di un ostacolo nella mappa (controlla l'array in base)
	/// </summary>
	/// <param name="position">coordinate da controllare</param>
	/// <returns>false se non è presente un ostacolo, true altrimenti</returns>
	// TODO: Possibile semplificazione tramite controllo della matrice invece dell'array
	private bool CheckObstacleAtPosition(Vector3 position)
	{
		if(position == startPoint || position == exitPoint)
			return false;

		int index;
		index = grid.CalculateIndexFromCoordinates(position.X,position.Z);
		
		return obstacles[index] == false;

	}

	/// <summary>
	/// Posiziona randomicamente n pezzi nella mappa (inserendoli nell'array degli ostacoli)
	/// </summary>
	/// <param name="nPieces">Numero di pezzi da inserire</param>
	private void RandomlyPlaceKnightPieces(int nPieces)
	{
		int count = nPieces;
		int knightPlacementTryLimit = 100;
		
		Random rand = new Random();

		while(count > 0 && knightPlacementTryLimit > 0)
		{
			int randomIndex = rand.Next(0, obstacles.Length); 

			if(obstacles[randomIndex] == false)
			{
				Vector3 coordinates = grid.CalculateCoordinatesFromIndex(randomIndex);

				if(coordinates == startPoint || coordinates == exitPoint)
				{
					continue;
				}

				obstacles[randomIndex] = true;
				knightPiecesList.Add(new KnightPiece(coordinates));
				GD.Print(coordinates.X+" "+coordinates.Z);
				count--;
			}

			knightPlacementTryLimit--;
		}
	}

	private void PlaceObstaclesForKnight(KnightPiece knight)
	{
		foreach(Vector3 position in KnightPiece.listOfPossibleMoves)
		{
			Vector3 newPosition = knight.Position + position;

			if(grid.IsCellValid(newPosition.X, newPosition.Z) && CheckObstacleAtPosition(newPosition))
			{
				obstacles[grid.CalculateIndexFromCoordinates(newPosition.X,newPosition.Z)] = true;
			}
		}
	}

	private void PlaceKnightObstacles()
	{
		foreach(KnightPiece knight in knightPiecesList)
		{
			PlaceObstaclesForKnight(knight);
		}
	}

	//Setters e Getters
	public Map Grid {get => grid;}
    public bool[] Obstacles { get => obstacles;}

	//Debug
	public void FillBoardWithPieces(Vector3 startPos,Vector3 exitPos,Map grid, int width, int height)
	{
		obstacles = new bool[width*height];

		CreateCandidateMap(startPos,exitPos);

		

		for(int i = 0;i < obstacles.Length; i++)
		{
			if(obstacles[i])
			{
				Vector3 pos = grid.CalculateCoordinatesFromIndex(i);
				grid.SetCell(pos.X,pos.Z,CellObjectType.Obstacle);
			}
		}

		foreach(KnightPiece k in knightPiecesList)
		{
			grid.SetCell(k.Position.X, k.Position.Z, CellObjectType.Knight);
		}
	}


}