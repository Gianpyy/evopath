using Godot;
using System;
using Vector2 = Godot.Vector2;

public static class MapHelper
{
	/// <summary>
	/// Sceglie casualmente un punto di entrata ed uno di uscita ai bordi della mappa, passando come riferimento le coordinate della posizione di inizio e di uscita
	/// </summary>
	/// <param name="grid">La mappa in cui si vuole impostare inizio e fine</param>
	/// <param name="startPosition">Le coordinate della posizione di entrata</param>
	/// <param name="exitPosition">Le coordinate della posizione di uscita</param>
	/// <param name="randomPlacement">Se il bordo di entrata ed uscita deve essere selezionato casualmente</param>
	/// <param name="startPositionEdge">Il bordo da cui verrà scelta l'entrata. Default: sinistra</param>
	/// <param name="exitPositionEdge">Il bordo da cui verrà scelta l'uscita. Default: destra</param>
	public static void ChooseAndSetStartExit(Map grid, ref Vector2 startPosition, ref Vector2 exitPosition, bool randomPlacement, Direction startPositionEdge = Direction.Left, Direction exitPositionEdge = Direction.Right) 
	{
		if (randomPlacement)
		{
			startPosition = RandomlyChoosePositionOnEdge(grid, startPosition);
			exitPosition = RandomlyChoosePositionOnEdge(grid, exitPosition);
		}
		else 
		{
			startPosition = RandomlyChoosePositionOnEdge(grid, startPosition, startPositionEdge);
			exitPosition = RandomlyChoosePositionOnEdge(grid, exitPosition, exitPositionEdge);
		}
		grid.SetCell(startPosition.X, startPosition.Y, CellObjectType.Start);
		grid.SetCell(exitPosition.X, exitPosition.Y, CellObjectType.Exit);
	}


	// Sceglie casualmente la posizione di entrata/uscita sul bordo passato come parametro. Di default, il bordo è scelto casualmente.
	private static Vector2 RandomlyChoosePositionOnEdge(Map grid, Vector2 position, Direction direction = Direction.None)
	{
		Random rand = new Random();
		if (direction == Direction.None)
		{
			direction = (Direction) rand.Next(0,5);
		}

		Vector2 finalPosition = Vector2.Zero;
		switch (direction)
		{
			case Direction.Right:
				do
				{
					finalPosition = new Vector2(grid.Width - 1, rand.Next(0, grid.Height));
				} while(finalPosition.DistanceTo(position) <= 1);
				break;

			case Direction.Left:
				do
				{
					finalPosition = new Vector2(0 , rand.Next(0, grid.Height));
				} while(finalPosition.DistanceTo(position) <= 1);
				break;

			case Direction.Up:
				do
				{
					finalPosition = new Vector2(rand.Next(0, grid.Width), 0);
				} while(finalPosition.DistanceTo(position) <= 1);
				break;

			case Direction.Down:
				do
				{
					finalPosition = new Vector2(rand.Next(0, grid.Width), grid.Height - 1);
				} while(finalPosition.DistanceTo(position) <= 1);
				break;
			
			default:
				break;
		}

		return finalPosition;
	}
}
