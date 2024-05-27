using Godot;
using System;

public partial class MapGenerator : Node
{
	[Export]
	int width, height, numberOfOstacles;

	[Export]
	public bool randomPlacement;

	[Export]
	public bool autoRepair;

	[Export]
	public Direction startEdge, exitEdge;

	private Map map;
	private Vector2 startPosition, exitPosition;

	[Export]
	public MapVisualizer mapVisualizer;

	public override void _Ready() 
	{
		// DEBUG
		map = new Map(width,height);

		GD.Print("Creo la mappa vuota");
		map.CreateMap();
		map.PrintMapConsole();


		GD.Print("Imposto casualmente entrata ed uscita...");


		MapHelper.ChooseAndSetStartExit(map, ref startPosition, ref exitPosition, randomPlacement, startEdge, exitEdge);
		map.PrintMapConsole();

		GD.Print("Creo la candidate map con gli ostacoli...");
		CandidateMap candidateMap = new CandidateMap(map, numberOfOstacles);
		candidateMap.GenerateCandidateMap(startPosition, exitPosition, map, width, height, autoRepair);
		map.PrintMapConsole();

		mapVisualizer.GenerateMap(map);
		GD.Print("Numero di curve: "+candidateMap.CornersList.Count);
		GD.Print("Numero di curve consecutive: " +candidateMap.ConsecutiveCornersCount);
	}
}
