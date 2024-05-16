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
					map[row, col] = new Cell(row, col); // !

					//DEBUG
						Array values = Enum.GetValues(typeof(Piece));
						Random random = new Random();
						Piece randomBar = (Piece)values.GetValue(random.Next(values.Length));
						map[row,col].PieceType = randomBar;

				}
			}
		}

		// Debug
		public void PrintMapConsole()
		{
			string s = "";
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					s+=map[i,j].PieceType;
				}
				s+="\n";
			}
			GD.Print(s);

			GD.Print(map[3,5].PieceType);	
			GD.Print(map[3,5].X);
			GD.Print(map[3,5].Z);
			
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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

}

