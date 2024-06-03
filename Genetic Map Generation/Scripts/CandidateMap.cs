using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vector2 = Godot.Vector2;


// Individuo della popolazione
public class CandidateMap
{
	// Griglia associata alla mappa candidata
	public Map grid;
	// Array degli ostacoli ottenuto normalizzando la griglia. griglia -> array
	private bool[] obstacles = null;
	// Inizio e fine del percorso
	private Vector2 startPoint, exitPoint;
	// Numero di pezzi da aggiungere alla mappa (Knights)
	private int nPieces = 0;
	// Lista dei pezzi aggiunti alla mappa
	private List<KnightPiece> knightPiecesList = new List<KnightPiece>();
	// Il percorso che porta dall'inizio alla fine
	private List<Vector2> path = new List<Vector2>();

	// -- Variabili per algoritmo genetico --

	// Lista contenente le coordinate delle curve del percorso
	private List<Vector2> cornersList;
	// Conteggio delle curve consecutive (valore da minimizzare nella funzione di fitness)
	private int consecutiveCornersCount;

	public CandidateMap(Map grid, int nPieces)
	{
		this.grid = grid;
		this.nPieces = nPieces;
	}

	public CandidateMap(CandidateMap candidateMap)
	{
		grid = candidateMap.grid.DeepClone();
		obstacles = (bool[]) candidateMap.obstacles.Clone();
		startPoint = candidateMap.startPoint;
		exitPoint = candidateMap.exitPoint;
		cornersList = new List<Vector2>(candidateMap.cornersList);
		consecutiveCornersCount = candidateMap.consecutiveCornersCount;
		path = new List<Vector2>(candidateMap.path);
	}

	/// <summary>
	/// Crea la mappa candidata con posizione di inizio e fine e Posizionamento degli ostacoli
	/// </summary>
	/// <param name="startPos">Posizione iniziale del percorso </param>
	/// <param name="exitPos">Posizione finale del percorso</param>
	/// <param name="autoRepair"></param>
	public void CreateCandidateMap(Vector2 startPos, Vector2 exitPos, bool autoRepair)
	{
		startPoint = startPos;
		exitPoint = exitPos;
		obstacles = new bool[grid.Width*grid.Height];


		RandomlyPlaceKnightPieces(nPieces);
		PlaceKnightObstacles();
		//FindPath();
		
		//if (autoRepair) 
		//{
		//	Repair();
       // }

		for(int i = 0;i < obstacles.Length; i++)
		{
			if(obstacles[i])
			{
				Vector2 pos = grid.CalculateCoordinatesFromIndex(i);
				grid.SetCell(pos.X,pos.Y,CellObjectType.Obstacle);
			}
		}

		// Debug
		//cornersList = GetListOfCorners(path);
		//consecutiveCornersCount = CalculateConsecutiveCorners(cornersList);
			
	}

	/// <summary>
	/// Controlla la presenza di un ostacolo nella mappa (controlla l'array in base)
	/// </summary>
	/// <param name="position">coordinate da controllare</param>
	/// <returns>false se non è presente un ostacolo, true altrimenti</returns>
	private bool CheckObstacleAtPosition(Vector2 position)
	{
		if(position == startPoint || position == exitPoint)
			return false;

		int index;
		index = grid.CalculateIndexFromCoordinates(position.X,position.Y);
		
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
				Vector2 coordinates = grid.CalculateCoordinatesFromIndex(randomIndex);

				if(coordinates == startPoint || coordinates == exitPoint)
				{
					continue;
				}

				obstacles[randomIndex] = true;
				knightPiecesList.Add(new KnightPiece(coordinates));
				//GD.Print(coordinates.X+" "+coordinates.Y);
				count--;
			}

			knightPlacementTryLimit--;
		}
	}

	/// <summary>
	/// Aggiunge un ostacolo per ogni mossa possibile del pezzo Knight
	/// </summary>
	/// <param name="knight">Il KnighPiece le cui mosse determineranno il posizionamento degli ostacoli</param>
	private void PlaceObstaclesForKnight(KnightPiece knight)
	{
		foreach(Vector2 position in KnightPiece.listOfPossibleMoves)
		{
			Vector2 newPosition = knight.Position + position;

			if(grid.IsCellValid(newPosition.X, newPosition.Y) && CheckObstacleAtPosition(newPosition))
			{
				obstacles[grid.CalculateIndexFromCoordinates(newPosition.X,newPosition.Y)] = true;
			}
		}
	}

	/// <summary>
	/// Aggiunge gli ostacoli per ogni KnightPiece della lista dei KnighPiece
	/// </summary>
	private void PlaceKnightObstacles()
	{
		foreach(KnightPiece knight in knightPiecesList)
		{
			PlaceObstaclesForKnight(knight);
		}
	}

	/// <summary>
	/// Trova il percorso più breve, utilizzando l'algoritmo A*, dal punto di inizio al punto di fine.
	/// Viene creato un array di Vector2 con le coordinate del percorso e viene impostata la cella di tipo "Road" nella griglia
	/// </summary>
	public void FindPath()
	{
		path = AStar.GetPath(startPoint, exitPoint, obstacles, grid);
		cornersList = GetListOfCorners(path);
		consecutiveCornersCount = CalculateConsecutiveCorners(cornersList);

		for(int i = 0; i < path.Count-1; i++)
		{
			grid.SetCell(path[i].X,path[i].Y,CellObjectType.Road);
		}
	}

	/// <summary>
	/// Elimina casualmente un ostacolo finché non viene trovato un percorso dal punto di inizio al punto di fine
	/// Una volta trovato il percorso, gli ostacoli rimossi che non fanno parte del percorso vengono riaggiunti nell'array delgi ostacoli
	/// </summary>
	/// <returns>La lista degli ostacoli rimossi</returns>
	public List<Vector2> Repair()
	{
		int numberOfObstacles = obstacles.Count(obstacle => obstacle);
		List<Vector2> obstaclesToRemove = new List<Vector2>();
		Random rand = new Random();
		if (path.Count <= 0)
		{
			do {
				int obstacleIndextoRemove = rand.Next(0, numberOfObstacles);
				for (int i = 0; i < obstacles.Length; i++) {
					if (obstacles[i]) {
						if (obstacleIndextoRemove == 0) {
							obstacles[i] = false;
							obstaclesToRemove.Add(grid.CalculateCoordinatesFromIndex(i));
							break;
						}
						obstacleIndextoRemove--;
					}
				}

				FindPath();
			} while (path.Count <= 0);
		}

		foreach(Vector2 obstaclePosition in obstaclesToRemove) 
		{
			string s = "Obstacle ("+obstaclePosition.X+","+obstaclePosition.Y+") removed";
			if (path.Contains(obstaclePosition) == false) 
			{
				s+=" and reinserted";
				int index = grid.CalculateIndexFromCoordinates(obstaclePosition.X, obstaclePosition.Y);
                obstacles[index] = true;
			}

			// Debug
			//GD.Print(s);
        }

		return obstaclesToRemove;
	}

	public void ClearPath()
	{
		for(int i = 0; i < path.Count-1; i++)
		{
			grid.SetCell(path[i].X,path[i].Y,CellObjectType.Empty);
		}
	}

	// -- Metodi per algoritmo genetico --

	/// <summary>
	/// Restituisce una lista di tutte le curve (ossia un cambio di direzione) contenute in un percorso.
	/// </summary>
	/// <param name="path">La lista contente le coordinate del percorso</param>
	/// <returns>Le coordinate delle curve contenute nel percorso</returns>
	private List<Vector2> GetListOfCorners(List<Vector2> path)
	{
		List<Vector2> completePath = new List<Vector2>(path);
		completePath.Insert(0, startPoint);
		List<Vector2> corners = new List<Vector2>();
		if (completePath.Count <= 0)
			return corners;

		for (int i = 0; i < completePath.Count-2; i++)
		{
			if(completePath[i+1].X > completePath[i].X || completePath[i+1].X < completePath[i].X)
			{
				if(completePath[i+2].Y > completePath[i+1].Y || completePath[i+2].Y < completePath[i+1].Y)
				{
					corners.Add(completePath[i+1]);
				}
			}
			else if(completePath[i+1].Y > completePath[i].Y || completePath[i+1].Y < completePath[i].Y)
			{
				if(completePath[i+2].X > completePath[i+1].X || completePath[i+2].X < completePath[i+1].X)
				{
					corners.Add(completePath[i+1]);
				}
			}
		}

		return corners;
	}

	/// <summary>
	/// Calcola il numero di curve consecutive a partire da una lista di coordinate di curve
	/// </summary>
	/// <param name="corners">La lista contenente le coordinate delle curve</param>
	/// <returns>Il numero di curve consecutive</returns>
	private int CalculateConsecutiveCorners(List<Vector2> corners)
	{
		int consecutiveCorners = 0;
		for (int i = 0; i < cornersList.Count-1; i++)
		{
			if (cornersList[i].DistanceTo(cornersList[i+1]) <= 1)
				consecutiveCorners++;
		}

		return consecutiveCorners;
	}

	/// <summary>
	/// Controlla se nell'array degli ostacoli alla posizione passato come parametro è presente un ostacolo
	/// </summary>
	/// <param name="index">Posizione dove controllare la presenza di un ostacolo</param>
	/// <returns>true se è presente un ostacolo, false altrimenti</returns>
	public bool IsObstacleAt(int index)
	{
		return obstacles[index];
	}

	/// <summary>
	/// Imposta l'ostacolo nell'array degli ostacoli all'indice passato come parametro
	/// </summary>
	/// <param name="index">Posizione dove posizionare l'ostacolo nell'array degli ostacoli</param>
	/// <param name="isObstacle">Imposta se è un ostacolo oppure no</param>
	public void PlaceObstacle(int index, bool isObstacle)
	{
		obstacles[index] = isObstacle;
	}
	
	// Getters
	public Map Grid {get => grid;}

    public bool[] Obstacles { get => obstacles;}

	public int ConsecutiveCornersCount {get => consecutiveCornersCount;}
	
	public List<Vector2> CornersList {get => cornersList;}

    public List<Vector2> Path { get => path;}

    public CandidateMap DeepClone()
	{
		return new CandidateMap(this);
	}


	//Debug

	/// <summary>
	/// Genera la mappa candidata delle dimensioni specificate e piazza gli ostacoli su di essa
	/// </summary>
	/// <param name="startPos">La posizione di inizio</param>
	/// <param name="exitPos">La posizione di fine</param>
	/// <param name="grid">La griglia associata alla mappa</param>
	/// <param name="width">La larghezza della mappa</param>
	/// <param name="height">L'altezza della mappa</param>
	/// <param name="autoRepair">Se bisogna trovare un percorso eliminando ostacoli finchè non se ne trova uno. Default: false</param>
	public void GenerateCandidateMap(Vector2 startPos,Vector2 exitPos, Map grid, int width, int height, bool autoRepair = false)
	{
		obstacles = new bool[width*height];

		CreateCandidateMap(startPos,exitPos, autoRepair);

		// Possibile miglioria: contraddistinguere i KnightPiece dagli ostacoli (potrebbe tornare utile per mettere asset diversi)
		for(int i = 0;i < obstacles.Length; i++)
		{
			if(obstacles[i])
			{
				Vector2 pos = grid.CalculateCoordinatesFromIndex(i);
				grid.SetCell(pos.X,pos.Y,CellObjectType.Obstacle);
			}
		}

		/*	
		foreach(KnightPiece k in knightPiecesList)
		{
			grid.SetCell(k.Position.X, k.Position.Y, CellObjectType.Knight);
		}
		*/
	}

}
