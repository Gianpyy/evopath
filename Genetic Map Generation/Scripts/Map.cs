using Godot;
using System;
using System.Diagnostics;

public partial class Map : Node
{

	public class GameMap
	{
		private int width, height;

		private Cell[,] map;

		public GameMap(int width, int height)
		{
			this.width = width;
			this.height = height;
			CreateMap();
		}

		/// <summary>
		/// Crea la mappa, costruendo la matrice e settando le Celle che la andranno a formare
		/// </summary>
		public void CreateMap()
		{
			map = new Cell[height, width];

			for(int row = 0; row < height; row++)
			{
				for(int col = 0; col < width; col++)
				{
					map[row, col] = new Cell(col, row); // !

					//DEBUG
					Array values = Enum.GetValues(typeof(Piece));
					Random random = new Random();
					Piece randomBar = (Piece)values.GetValue(random.Next(values.Length));
					map[row,col].PieceType = randomBar;

				}
			}
		}

		/// <summary>
		/// Imposta il tipo di cella alle coordinate specificate
		/// </summary>
		public void SetCell(int x, int z, CellObjectType cellObjectType) 
		{
			map[z, x].CellObjectType = cellObjectType;
			map[z, x].IsTaken = false;
		}

		public void SetCell(float x, float z, CellObjectType cellObjectType)
		{
			SetCell((int) x, (int) z, cellObjectType);
		}

		/// <summary>
		/// Restituisce se la cella è occupata
		/// </summary>
		public bool IsCellTaken(int x, int z)
		{
			return map[z, x].IsTaken;
		}

		public bool IsCellTaken(float x, float z) 
		{
			return map[(int) z, (int) x].IsTaken;
		}

		/// <summary>
		/// Restituisce se la cella è valida
		/// </summary>
		public bool IsCellValid(float x, float z)
		{
			if (x >= width || x < 0 || z >= height || z < 0)
				return false;

			return true;
		}

		/// <summary>
		/// Restituisce la cella nelle coordinate specificate, se valida
		/// </summary>
		public Cell GetCell(int x, int z)
		{
			if (IsCellValid(x, z) == false)
				return null;

			return map[z, x];
		}

		public Cell GetCell(float x, float z) 
		{
			return GetCell((int) x, (int) z);
		}
		
		// Debug
		public int CalculateIndexFromCoordinates(int x, int z)
		{
			return x + z * width;
		}

		public float CalculateIndexFromCoordinates(float x, float z)
		{
			return (int) x + (int) z * width;
		}

		
		public void PrintMapConsole()
		{
			string s = "";
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					string elem = "";
					elem = map[i,j].CellObjectType.ToString();
					s += elem[0];
				}
				s+="\n";
			}
			GD.Print(s);
		}

	}
	
	/// <summary>
	/// La cella che andrà a formare la griglia
	/// </summary>
	public class Cell
	{
		// Posizione della cella nella griglia
		private int x, z;

		// Se la cella è occupata -> true
		private bool isTaken;

		// Il tipo di cella sulla griglia
		private CellObjectType cellObjectType;

		// Debug
		private Piece pieceType;


		public Cell(int x, int z)
		{
			this.x = x;
			this.z = z;

			isTaken = false;
			cellObjectType = CellObjectType.Empty;

		}



		// Setters e Getters
        public int X { get => x;}
        public int Z { get => z;}
        public bool IsTaken { get => isTaken; set => isTaken = value; }
        public CellObjectType CellObjectType { get => cellObjectType; set => cellObjectType = value; }
		public Piece PieceType { get => pieceType; set => pieceType = value; }
		
        
    }

	// Debug
	public enum Piece
	{
		R,
		P,
		Q,
		K

	}
	// Il tipo della cella, può essere un ostacolo, una strada ecc.
	public enum CellObjectType
	{
		Empty,
		Road,
		Obstacle,
		Start,
		Exit
	}
	
	
	public override void _Ready()
	{

		// DEBUG
		GameMap map = new GameMap(8,8);
		map.CreateMap();
		map.PrintMapConsole();

		map.SetCell(3,5, CellObjectType.Obstacle);
		map.PrintMapConsole();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

}

