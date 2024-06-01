using Godot;
using System;
using Vector2 = Godot.Vector2;

public class Map
{
	private int width, height;

	private Cell[,] map;

	public int Width { get => width; set => width = value; }
	public int Height { get => height; set => height = value; }
    public Cell[,] MapGrid { get => map; set => map = value; }

    public Map(int width, int height)
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

	public int CalculateIndexFromCoordinates(float x, float z)
	{
		return (int) x + (int) z * width;
	}

	public Vector2 CalculateCoordinatesFromIndex(int index)
	{
		int x = index % width;
		int z = index / width;

		return new Vector2(x, z);
	}
	
	// DEBUG
	// Stampa la mappa nella console di debug
	public void PrintMapConsole()
	{
		string s = "";
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //char elem = map[i, j].CellObjectType.ToString()[0];
                switch (map[i, j].CellObjectType)
                {
                    case CellObjectType.Empty: // Empty
                        s += "[color=gray]E[/color] ";
                        break;
                    case CellObjectType.Road: // Road
                        s += "[color=blue]R[/color] ";
                        break;
                    case CellObjectType.Obstacle: // Obstacle
                        s += "[color=red]O[/color] ";
                        break;
                    case CellObjectType.Start: // Start
                        s += "[color=green]S[/color] ";
                        break;
                    case CellObjectType.Exit: // Exit
                        s += "[color=yellow]X[/color] ";
                        break;
					case CellObjectType.Knight: // Knight Debug
						s += "[color=violet]K[/color] ";
					break;
                    
                }
            }
            s += "\n";
        }

        GD.PrintRich(s);
	}

	public Map DeepClone()
	{
		Map clonedMap = new Map(width, height);

		for (int row = 0; row < height; row++)
		{
			for (int col = 0; col < width; col++)
			{
				Cell originalCell = map[row,col];
				Cell newCell = new Cell(originalCell.X, originalCell.Y)
				{
					IsTaken = originalCell.IsTaken,
					CellObjectType = originalCell.CellObjectType,
				};

				clonedMap.map[row,col] = newCell;
			}
		}

		return clonedMap;
	}
}

/// <summary>
/// La cella che andrà a formare la griglia
/// </summary>
public class Cell
{
	// Posizione della cella nella griglia
	private int x, y;

	// Se la cella è occupata -> true
	private bool isTaken;

	// Il tipo di cella sulla griglia
	private CellObjectType cellObjectType;

	// Debug
	private Piece pieceType;


	public Cell(int x, int y)
	{
		this.x = x;
		this.y = y;

		isTaken = false;
		cellObjectType = CellObjectType.Empty;

	}

	// Setters e Getters
	public int X { get => x;}
	public int Y { get => y;}
	public bool IsTaken { get => isTaken; set => isTaken = value; }
	public CellObjectType CellObjectType { get => cellObjectType; set => cellObjectType = value; }
	public Piece PieceType { get => pieceType; set => pieceType = value; }
	
	
}

// Il tipo della cella, può essere un ostacolo, una strada ecc.
public enum CellObjectType
{
	Empty,
	Road,
	Obstacle,
	Knight, // Debug
	Start,
	Exit
}


// Debug
public enum Piece
{
	R,
	P,
	Q,
	K

}