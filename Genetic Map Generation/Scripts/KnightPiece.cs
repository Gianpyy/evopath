using Godot;
using System.Collections.Generic;
using Vector2 = Godot.Vector2;

public class KnightPiece
{
	/// <summary>
	/// Le possibili mosse che può compiere un Knight (Cavallo)
	/// </summary>
	public static List<Vector2> listOfPossibleMoves = new List<Vector2>
	{
		// Mosse lunghe
		new Vector2(-1, 2),  // Giù a destra lunga
		new Vector2(1, 2),   // Su a destra lunga
		new Vector2(-1, -2), // Giù a sinistra lunga
		new Vector2(1, -2),  // Su a sinistra lunga

		// Mosse corte
		new Vector2(-2, -1), // Giù a sinistra corta
		new Vector2(-2, 1),  // Giù a destra corta
		new Vector2(2, -1),  // Su a sinistra corta
		new Vector2(2, 1),   // Su a destra corta

	};

	// Coordinate del Knight nella mappa
	private Vector2 position;

	public KnightPiece(Vector2 position)
	{
		this.position = position;
	}

	// Setters e Getters
    public Vector2 Position { get => position; set => position = value; }

}
