using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

	// -- Variabili funzione di fitness --

	// Il numero minimo di curve
	[Export] private int fitnessCornerMin = 6;
	// Il numero massimo di curve
	[Export] private int fitnessCornerMax = 12;
	// Il peso che ha il numero di curve sul valore di fitness 
	[Export] private int fitnessCornerWeight = 1;
	// Il peso che ha il numero di ostacoli sul valore di fitness 
	[Export] private int fitnessObstacleWeight = 1;
	// Il peso che ha la lunghezza del percorso sul valore di fitness 
	[Export] private int fitnessPathWeight = 1;

	// --- Variabili della mappa ---

	private Map grid;
	[Export] private int mapWidth = 10, mapHeight = 10;
	private Vector2 startPosition, exitPosition;
	[Export] private Direction startPositionEdge = Direction.Left, exitPositionEdge = Direction.Right;
	[Export] private bool randomStartAndEnd = false;
	[Export] private int numberOfKnightPieces = 7;

	// DEBUG
	[Export] private MapVisualizer mapVisualizer;

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

		//Controllo per selezionare il miglior individuo tra tutte le generazioni
		if(bestFitnessScoreThisGeneration > bestFitnessScoreAllTime)
		{
			bestFitnessScoreAllTime = bestFitnessScoreThisGeneration;
			bestMap = bestMapThisGeneration.DeepClone();
			bestMapGenerationNumber = generationNumber;
		}

		generationNumber++;


		if(!IsOutOfResources())
		{
			List<CandidateMap> nextGeneration = new List<CandidateMap>();
		
			while(nextGeneration.Count < populationSize)
			{
				CandidateMap parent1 = currentGeneration[RouletteWheelSelection()];
				CandidateMap parent2 = currentGeneration[RouletteWheelSelection()];

				// TODO: Crossover e mutation
				
			}
		}


	}

	/// <summary>
	/// Controlla se si ha ancora a disposizione del budget di ricerca
	/// </summary>
	/// <returns>true: se il budget è finito, false altrimenti</returns>
	private bool IsOutOfResources()
	{
		// Controllo per il numero di generazioni come budget
		if(generationNumber < generationLimit)
			return false;
		else
			return true;
		
	}
		
	/// <summary>
	/// Calcola la fitness per una mappa candidata.
	/// La funzione di fitness tiene conto della lunghezza del percorso, del numero di ostacoli e del numero di curve 
	/// Nello specifico:
	/// <list type="bullet"> 
	/// <item><description>MASSIMIZZIAMO la lunghezza del percorso</description></item>
	/// <item><description>MASSIMIZZIAMO il numero di ostacoli</description></item>
	/// <item><description>MASSIMIZZIAMO il numero di curve</description></item>
	/// </list>
	/// </summary>
	/// <param name="candidateMap">La mappa candidata per cui vogliamo calcolare il valore di fitness</param>
	/// <returns>Il valore di fitness associato ad una mappa candidata</returns>
	public int CalculateFitness(CandidateMap candidateMap) // Il metodo deve essere private, è public per DEBUG
	{
		int numberOfObstacles = candidateMap.Obstacles.Where(isObstacle => isObstacle).Count();
		int numberOfCorners = candidateMap.CornersList.Count;

		// -- Calcolo del valore di fitness --

		// Il valore finale della funzione di fitness
		int fitnessScore = 0; 
		
		// Aggiunta la lunghezza del percorso
		fitnessScore += candidateMap.Path.Count * fitnessPathWeight;

		// Aggiunto il numero degli ostacoli 
		fitnessScore += numberOfObstacles * fitnessObstacleWeight;

		// -- Numero di curve --
		// Se il numero di curve rientra nel range prestabilito la mappa candidata viene premiata
		// aggiungendo il numero di curve al valore di fitness
		if (numberOfCorners >= fitnessCornerMin && numberOfCorners <= fitnessCornerMax)
		{
			fitnessScore += numberOfCorners * fitnessCornerWeight;
		}

		// Se il numero di curve eccede il range prestabilito la mappa candidata viene penalizzata
		// rimuovendo dal valore di fitness le curve in eccesso
		else if (numberOfCorners > fitnessCornerMax)
		{
			fitnessScore -= (numberOfCorners - fitnessCornerMax) * fitnessCornerWeight;
		}

		// Se il numero di curve è al di sotto del valore minimo la mappa candidata viene penalizzata
		// rimuovendo il valore minimo del range dal valore di fitness (viene penalizzata di più perché bisogna massimizzare il numero di curve)
		else if (numberOfCorners < fitnessCornerMin)
		{
			fitnessScore -= fitnessCornerMin * fitnessCornerWeight;
		}

		return fitnessScore;
	}

	/// <summary>
	/// Seleziona un individuo dalla popolazione corrente utilizzando la roulette wheel selection.
	/// La probabilità di selezionare un individuo è proporzionale alla sua fitness.
	/// </summary>
	/// <returns>
	/// L'indice dell'individuo selezionato nella popolazione corrente.
	/// </returns>
	private int RouletteWheelSelection()
	{
		Random rand = new Random();
		
		int randomValue = rand.Next(0, totalFitnessThisGeneration);

		for (int i = 0; i < populationSize; i++)
		{
			randomValue -= CalculateFitness(currentGeneration[i]);

			if(randomValue <= 0)
				return i;
		}

		return populationSize - 1;
	}

	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// DEBUG
		grid = new Map(mapWidth, mapHeight);

		GD.Print("Creo la mappa vuota");
		grid.CreateMap();
		grid.PrintMapConsole();


		GD.Print("Imposto casualmente entrata ed uscita...");


		MapHelper.ChooseAndSetStartExit(grid, ref startPosition, ref exitPosition, randomStartAndEnd, startPositionEdge, exitPositionEdge);
		grid.PrintMapConsole();

		GD.Print("Creo la candidate map con gli ostacoli...");
		CandidateMap candidateMap = new CandidateMap(grid, numberOfKnightPieces);
		candidateMap.GenerateCandidateMap(startPosition, exitPosition, grid, mapWidth, mapHeight, true);
		grid.PrintMapConsole();

		mapVisualizer.GenerateMap(grid);
		GD.Print("Lunghezza percorso: "+candidateMap.Path.Count);
		GD.Print("Numero di ostacoli: "+candidateMap.Obstacles.Where(isObstacle => isObstacle).Count());
		GD.Print("Numero di curve: "+candidateMap.CornersList.Count);
		GD.Print("Numero di curve consecutive: " +candidateMap.ConsecutiveCornersCount);

		GD.Print("Valore di fitness: "+CalculateFitness(candidateMap));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Getters
	public bool IsAlgorithmRunning { get => isAlgorithmRunning; set => isAlgorithmRunning = value; }
}
