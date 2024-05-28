using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class MapBrain : Node
{

	// --- Parametri algoritmo genetico --- 
	// Dimensione della popolazione iniziale
	[Export] private int populationSize = 10; 
	// Tasso di crossover
	[Export] private int crossoverRate = 100;
	// Tasso di mutazione
	[Export] private int mutationRate = 0;
	// Limite di generazioni
	[Export] private int generationLimit = 10; 

	// --- Variabili algoritmo genetico ---
	// Controllo se l'algoritmo è in esecuzione
	private bool isAlgorithmRunning = false;
	// Generazione corrente
	private List<CandidateMap> currentGeneration;
	// Fitness totale per la generazione corrente
	private int totalFitnessThisGeneration;
	// Miglior fitness tra tutte le generazioni
	private int bestFitnessScoreAllTime = 0;
	// Miglior individuo tra tutte le generazioni
	private CandidateMap bestMap = null;
	// L'indice della generazione con il miglior individuo
	private int bestMapGenerationNumber = 0;
	// Il numero di generazioni 
	private int generationNumber = 1;

	// --- Variabili della mappa ---
	private Map grid;
	[Export] private int mapWidth = 10, mapHeight = 10;
	private Vector2 startPosition, exitPosition;
	[Export] private Direction startPositionEdge = Direction.Left, exitPositionEdge = Direction.Right;
	[Export] private bool randomStartAndEnd = false;
	[Export] private int numberOfKnightPieces = 7;

    /// <summary>
    /// Avvia l'algoritmo
    /// </summary>
    public void RunAlgorithm()
	{
		
		//isAlgorithmRunning = true;

	}

	/// <summary>
	/// Reimposta le variabili dell'algoritmo
	/// </summary>
	private void ResetAlgorithm()
	{
		totalFitnessThisGeneration = 0;
		bestFitnessScoreAllTime = 0;
		bestMap = null;
		bestMapGenerationNumber = 0;
		generationNumber = 1; 
	}

	/// <summary>
	/// Crea la popolazione iniziale per l'algoritmo genetico
	/// </summary>
	private void CreateStartingPopulation()
	{	
		currentGeneration = new List<CandidateMap>(populationSize);
		
		for (int i = 0; i < populationSize; i++)
		{
			CandidateMap newCandidateMap = new CandidateMap(grid,numberOfKnightPieces);
			newCandidateMap.CreateCandidateMap(startPosition, exitPosition, true);
			currentGeneration.Add(newCandidateMap);
		}
	}

	// Trova la soluzione ottima tramite l'algoritmo genetico
	private void FindOptimalSolution(Map grid)
	{
		CreateStartingPopulation();
		GeneticAlgorithm();

	}

	// Algoritmo genetico per trovare la miglior generazione
	private void GeneticAlgorithm()
	{
		totalFitnessThisGeneration = 0;
		int bestFitnessScoreThisGeneration = 0;
		CandidateMap bestMapThisGeneration = null;

		foreach(CandidateMap candidateMap in currentGeneration)
		{
			candidateMap.FindPath();
			candidateMap.Repair();

			//TODO: il parametro deve ottenere i dati della mappa
			//		Creare metodo in CandidateMap
			int fitness = CalculateFitness(candidateMap);

			
			if(fitness > bestFitnessScoreThisGeneration)
			{
				bestFitnessScoreThisGeneration = fitness;
				bestMapThisGeneration = candidateMap;
			}
		}


	}
		
	//TODO: Da implementare
	private int CalculateFitness(CandidateMap candidateMap)
	{
		throw new NotImplementedException();
	}
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Getters
	public bool IsAlgorithmRunning { get => isAlgorithmRunning; set => isAlgorithmRunning = value; }
}
