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

	Map.GameMap map;
	private Vector3 startPosition, exitPosition;

	public override void _Ready() 
	{
		// DEBUG
		map = new Map.GameMap(width,height);

		GD.Print("Creo la mappa vuota");
		map.CreateMap();
		map.PrintMapConsole();


		GD.Print("Imposto casualmente entrata ed uscita...");
		MapHelper.RandomlyChooseAndSetStartExit(map, ref startPosition, ref exitPosition, randomPlacement, startEdge, exitEdge);
		map.PrintMapConsole();

		GD.Print("Creo la candidate map con gli ostacoli...");
		CandidateMap candidateMap = new CandidateMap(map, numberOfOstacles);
		candidateMap.FillBoardWithPieces(startPosition, exitPosition ,map, width, height);
		map.PrintMapConsole();
	}
}
