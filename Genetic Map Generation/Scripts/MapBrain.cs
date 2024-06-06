using Godot;
using HCoroutines;
using MEC;
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
	[Export] private int mutationRate = 5;
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
	DateTime startDate, endDate;
	private int [] fitnessArray;

    /// <summary>
    /// Avvia l'algoritmo
    /// </summary>
    public void RunAlgorithm()
	{
		ResetAlgorithm();

		grid = new Map(mapWidth, mapHeight);

		MapHelper.ChooseAndSetStartExit(grid, ref startPosition, ref exitPosition, randomStartAndEnd, startPositionEdge, exitPositionEdge);
		
		isAlgorithmRunning = true;

		startDate = DateTime.Now;

		FindOptimalSolution(grid);
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

		// Debug
		fitnessArray = new int[generationLimit];
	}

	/// <summary>
	/// Crea la popolazione iniziale per l'algoritmo genetico
	/// </summary>
	private void CreateStartingPopulation(Map grid)
	{	
		currentGeneration = new List<CandidateMap>(populationSize);
		
		for (int i = 0; i < populationSize; i++)
		{
			CandidateMap newCandidateMap = new CandidateMap(grid.DeepClone(), numberOfKnightPieces);
			newCandidateMap.CreateCandidateMap(startPosition, exitPosition, true);
			currentGeneration.Add(newCandidateMap);
		}
	}

	// Trova la soluzione ottima tramite l'algoritmo genetico
	private void FindOptimalSolution(Map grid)
	{
		CreateStartingPopulation(grid);
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
			candidateMap.ClearPath();
			candidateMap.FindPath();
			candidateMap.Repair();

			int fitness = CalculateFitness(candidateMap);
			
			totalFitnessThisGeneration += fitness;
			
			if(fitness > bestFitnessScoreThisGeneration)
			{
				bestFitnessScoreThisGeneration = fitness;
				bestMapThisGeneration = candidateMap.DeepClone();
			}
		}

		//Controllo per selezionare il miglior individuo tra tutte le generazioni
		if(bestFitnessScoreThisGeneration > bestFitnessScoreAllTime)
		{
			bestFitnessScoreAllTime = bestFitnessScoreThisGeneration;
			bestMap = bestMapThisGeneration.DeepClone();
			bestMapGenerationNumber = generationNumber;
		}

		
		// Debug
		GD.Print("Generazione "+generationNumber+ " totale fitness: "+totalFitnessThisGeneration);
		GD.Print("Fitness del miglior individuo: "+bestFitnessScoreThisGeneration);

		fitnessArray[generationNumber-1] = bestFitnessScoreThisGeneration;

		generationNumber++;
		//yield return Timing.WaitForOneFrame;

		// GD.Print("Miglior mappa della generazione: ");
		// bestMapThisGeneration.Grid.PrintMapConsole();

		


		if(!IsOutOfResources())
		{
			List<CandidateMap> nextGeneration = new List<CandidateMap>();
		
			while(nextGeneration.Count < populationSize)
			{
				// Selection
				CandidateMap parent1 = currentGeneration[RouletteWheelSelection()];
				CandidateMap parent2 = currentGeneration[RouletteWheelSelection()];

				// Crossover
				CandidateMap child1, child2;

				SinglePointCrossover(parent1, parent2, out child1, out child2);

				// Mutation
				BitflipMutation(child1);
				BitflipMutation(child2);

				nextGeneration.Add(child1);
				nextGeneration.Add(child2);
			}

			currentGeneration = nextGeneration;

			GeneticAlgorithm();
		}
		else
		{
			ShowResults();
		}
	}

	private void ShowResults()
    {
        isAlgorithmRunning = false;
		GD.Print("Miglior mappa: ");
		mapVisualizer.GenerateMap(bestMap.Grid);
		bestMap.Grid.PrintMapConsole();

		GD.Print("Soluzione migliore alla generazione "+bestMapGenerationNumber+" con il punteggio: "+bestFitnessScoreAllTime);
		GD.Print("Lunghezza del percorso: "+bestMap.Path.Count);
		GD.Print("Numero di curve: "+bestMap.CornersList.Count);

		endDate = DateTime.Now;
		long elapsedTicks = endDate.Ticks - startDate.Ticks;
		TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
		GD.Print("Tempo di esecuzione "+elapsedSpan);
    }

    /// <summary>
    /// Controlla se si ha ancora a disposizione del budget di ricerca
    /// </summary>
    /// <returns>true: se il budget è finito, false altrimenti</returns>
    private bool IsOutOfResources()
	{
		// Controllo per il numero di generazioni come budget
		if(generationNumber <= generationLimit)
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
	private int CalculateFitness(CandidateMap candidateMap) 
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

	/// <summary>
	/// Esegue un single point crossover tra due mappe candidate genitori per produrre due mappe candidate figli.
	/// Viene scelto casualmente un punto di taglio dal genitore1 e vengono incrociati gli array di ostacoli dopo quel punto per generare i figli
	/// </summary>
	/// <param name="parent1">La prima mappa candidata genitore.</param>
	/// <param name="parent2">La seconda mappa candidata genitore.</param>
	/// <param name="child1">La prima mappa candidata figlio risultante dal crossover.</param>
	/// <param name="child2">La seconda mappa candidata figlio risultante dal crossover.</param>
	private void SinglePointCrossover(CandidateMap parent1, CandidateMap parent2, out CandidateMap child1, out CandidateMap child2)
	{
		
		Random rand = new Random();

		child1 = parent1.DeepClone();
		child2 = parent2.DeepClone();

		if(rand.Next(0, 100) < crossoverRate)
		{
			int numBITs = parent1.Obstacles.Length;
			int crossOverIndex = rand.Next(0, numBITs);

			for (int i = crossOverIndex; i < numBITs; i++)
			{
				child1.PlaceObstacle(i , parent2.IsObstacleAt(i));
				child2.PlaceObstacle(i, parent1.IsObstacleAt(i));
			}
		}
	}

	/// <summary>
	/// Applica la bitflip mutation a una mappa candidata.
	/// </summary>
	/// <param name="candidateMap">La mappa candidata su cui applicare la mutazione.</param>
	private void BitflipMutation(CandidateMap candidateMap)
	{
		Random rand = new Random();

		for (int i = 0; i < candidateMap.Obstacles.Length; i++)
		{
			int rng = rand.Next(0, 100);
			//GD.Print("Random number: "+rng);
			if (rng < mutationRate)
			{
				//GD.Print("Obstacle mutated!");
				bool obstacleAtIndex = candidateMap.IsObstacleAt(i);
				candidateMap.PlaceObstacle(i, !obstacleAtIndex);
			}
		}

	}


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		RunAlgorithm();

		GD.Print("-------------------------------------------------");
		GD.Print("Salvataggio dei dati in Excel...");
		DataAnalysis da = new DataAnalysis();
		da.WriteDataInSheet(fitnessArray, new GeneticAlgorithmConfiguration{
			populationSize = this.populationSize,
			generationLimit = this.generationLimit,
			crossverRate = this.crossoverRate,
			mutationRate = this.mutationRate,
			fitnessCornerMin = this.fitnessCornerMin,
			fitnessCornerMax = this.fitnessCornerMax,
			fitnessCornerWeight = this.fitnessCornerWeight,
			fitnessObstacleWeight = this.fitnessObstacleWeight,
			fitnessPathWeight = this.fitnessPathWeight,
			mapWidth = this.mapWidth,
			mapHeight = this.mapHeight,
			numberOfPieces = this.numberOfKnightPieces,
		});
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Getters
	public bool IsAlgorithmRunning { get => isAlgorithmRunning; set => isAlgorithmRunning = value; }
}
