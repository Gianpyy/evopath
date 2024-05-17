using Godot;
using System.Collections.Generic;
using Vector3 = Godot.Vector3;

public class KnightPiece
{
	/// <summary>
	/// Le possibili mosse che può compiere un Knight (Cavallo)
	/// </summary>
	public static List<Vector3> listOfPossibleMoves = new List<Vector3>
	{
		// Mosse lunghe
		new Vector3(-1, 0, 2),  // Giù a destra lunga
		new Vector3(1, 0, 2),   // Su a destra lunga
		new Vector3(-1, 0, -2), // Giù a sinistra lunga
		new Vector3(1, 0, -2),  // Su a sinistra lunga

		// Mosse corte
		new Vector3(-2, 0, -1), // Giù a sinistra corta
		new Vector3(-2, 0, 1),  // Giù a destra corta
		new Vector3(2, 0, -1),  // Su a sinistra corta
		new Vector3(2, 0, 1),   // Su a destra corta

	};

	// Coordinate del Knight nella mappa
	private Vector3 position;

	public KnightPiece(Vector3 position)
	{
		this.position = position;
	}

	// Setters e Getters
    public Vector3 Position { get => position; set => position = value; }

}
