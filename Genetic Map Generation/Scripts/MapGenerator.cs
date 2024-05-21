using Godot;
using System;

public partial class MapGenerator : Node
{
	[Export]
	int width, height, numberOfOstacles;

	[Export]
	public bool randomPlacement;

	[Export]
	public Direction startEdge, exitEdge;

	Map map;
	private Vector3 startPosition, exitPosition;

	public override void _Ready() 
	{
		// DEBUG
		map = new Map(width,height);

		GD.Print("Creo la mappa vuota");
		map.CreateMap();
		map.PrintMapConsole();


		GD.Print("Imposto casualmente entrata ed uscita...");
		MapHelper.RandomlyChooseAndSetStartExit(map, ref startPosition, ref exitPosition, false, Direction.Up, Direction.Right);
		map.PrintMapConsole();

		GD.Print("Creo la candidate map con gli ostacoli...");
		CandidateMap candidateMap = new CandidateMap(map, numberOfOstacles);
		candidateMap.FillBoardWithPieces(startPosition, exitPosition ,map, width, height);
		map.PrintMapConsole();
	}
}
