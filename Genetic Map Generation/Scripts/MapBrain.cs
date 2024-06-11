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
	// Il numero di generazioni massimo senza un miglioramento
	[Export] private int maxGenerationsWithoutImprovements = 5;

	// --- Variabili algoritmo genetico ---

	// Controllo se l'algoritmo è in esecuzione
	private bool isAlgorithmRunning = false;
	// Generazione corrente
	private List<CandidateMap> currentGeneration;
	// Fitness totale per la generazione corrente
	private float totalFitnessThisGeneration;
	// Miglior fitness tra tutte le generazioni
	private float bestFitnessScoreAllTime = 0;
	// Miglior individuo tra tutte le generazioni
	private CandidateMap bestMap = null;
	// L'indice della generazione con il miglior individuo
	private int bestMapGenerationNumber = 0;
	// Il numero di generazioni 
	private int generationNumber = 1;
	// Il numero di generazioni senza 
	private int generationsWithoutImprovements = 0;

	// Il tipo di operatore genetico per la selezione
	[Export] private Selection selectionMethod = Selection.Truncation;
	// Il tipo di operatore genetico per il crossover
	[Export] private Crossover crossoverMethod = Crossover.SinglePoint;
	// Il tipo di operatore genetico per la mutazione
	[Export] private Mutation mutationMethod = Mutation.BitFlip;

	// -- Variabili funzione di fitness --

	// Il numero minimo di curve
	[Export] private int fitnessCornerMin = 6;
	// Il numero massimo di curve
	[Export] private int fitnessCornerMax = 12;
	// Il peso che ha il numero di curve sul valore di fitness 
	[Export] private float fitnessCornerWeight = 1;
	// Il peso che ha il numero di ostacoli sul valore di fitness 
	[Export] private float fitnessObstacleWeight = 1;
	// Il peso che ha la lunghezza del percorso sul valore di fitness 
	[Export] private float fitnessPathWeight = 1;

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
	private GeneticAlgorithmData geneticAlgorithmData;

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
		generationsWithoutImprovements = 0;

		// Debug
		geneticAlgorithmData.fitnessArray = new float[generationLimit];
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

	// Ottiene i parametri impostati dall'UI con gli slider per impostarli nell'algoritmo genetico
	private void GetParametersFromUI()
	{

		Slider obstacleWeightsSlider = GetNode<Slider>("Ui/Panel/ObstacleWeight/Slider");
		Slider cornerWeightSlider = GetNode<Slider>("Ui/Panel/CornerWeight/Slider");
		Slider pathWeightSlider = GetNode<Slider>("Ui/Panel/PathWeight/Slider");
		
		fitnessObstacleWeight = (float)obstacleWeightsSlider.Value;
		fitnessCornerWeight = (float)cornerWeightSlider.Value;
		fitnessPathWeight = (float)pathWeightSlider.Value;
	}

	// Algoritmo genetico per trovare la miglior generazione
	private void GeneticAlgorithm()
	{

		GetParametersFromUI();

		totalFitnessThisGeneration = 0;
		float bestFitnessScoreThisGeneration = 0;
		CandidateMap bestMapThisGeneration = null;

		foreach(CandidateMap candidateMap in currentGeneration)
		{
			candidateMap.ClearPath();
			candidateMap.FindPath();
			candidateMap.Repair();

			float fitness = CalculateFitness(candidateMap);
			
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
			generationsWithoutImprovements = 0;
		}
		else 
		{
			generationsWithoutImprovements++;
		}

		
		// Debug
		GD.Print("Generazione "+generationNumber+ " totale fitness: "+totalFitnessThisGeneration);
		GD.Print("Fitness del miglior individuo: "+bestFitnessScoreThisGeneration);

		geneticAlgorithmData.fitnessArray[generationNumber-1] = bestFitnessScoreThisGeneration;

		generationNumber++;
		

		if(!IsOutOfResources())
		{
			List<CandidateMap> nextGeneration = new List<CandidateMap>();
		
			
			while(nextGeneration.Count < populationSize)
			{
				
				// SELECTION
				CandidateMap parent1 = null;
				CandidateMap parent2 = null;

				switch(selectionMethod)
				{
					case Selection.RouletteWheel:

						parent1 = currentGeneration[RouletteWheelSelection()];
						parent2 = currentGeneration[RouletteWheelSelection()];

					break;

					case Selection.Truncation:

						parent1 = currentGeneration[0].DeepClone();
						parent2 = currentGeneration[1].DeepClone();
						TruncationSelection(ref parent1, ref parent2);
						
					break;
					
				}

				
				// CROSSOVER
				CandidateMap child1 = null;
				CandidateMap child2 = null;

				switch(crossoverMethod)
				{
					case Crossover.SinglePoint:

						SinglePointCrossover(parent1, parent2, out child1, out child2);

					break;
					
				}				

				// MUTATION

				switch(mutationMethod)
				{
					case Mutation.BitFlip:

						BitflipMutation(child1);
						BitflipMutation(child2);

					break;
					
				}

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

		bestMap.RebuildGrid();

		GD.Print("Miglior mappa: ");
		mapVisualizer.GenerateMap(bestMap.Grid, bestMap.Path);
		bestMap.Grid.PrintMapConsole();

		GD.Print("Soluzione migliore alla generazione "+bestMapGenerationNumber+" con il punteggio: "+bestFitnessScoreAllTime);
		GD.Print("Lunghezza del percorso: "+bestMap.Path.Count);
		GD.Print("Numero di curve: "+bestMap.CornersList.Count);
		GD.Print("Numero di ostacoli: "+bestMap.Obstacles.Count(isObstacle => isObstacle));

		endDate = DateTime.Now;
		long elapsedTicks = endDate.Ticks - startDate.Ticks;
		TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
		GD.Print("Tempo di esecuzione "+elapsedSpan);

		geneticAlgorithmData.elapsedTime = elapsedSpan;
		geneticAlgorithmData.bestMapFitness = CalculateFitness(bestMap);
		geneticAlgorithmData.numberOfCorners = bestMap.CornersList.Count;
		geneticAlgorithmData.numberOfObstacles = bestMap.Obstacles.Where(isObstacle => isObstacle).Count();
		geneticAlgorithmData.pathLenght = bestMap.Path.Count;
    }

    /// <summary>
    /// Controlla se si ha ancora a disposizione del budget di ricerca
    /// </summary>
    /// <returns>true: se il budget è finito, false altrimenti</returns>
    private bool IsOutOfResources()
    {
        if (CheckForImprovements())
		{
			return !CheckForGenerationNumber();
		}

		return true;
    }

	// Controllo se non ci sono stati miglioramenti
	private bool CheckForImprovements()
	{
		return generationsWithoutImprovements < maxGenerationsWithoutImprovements;
	}

	// Controllo per il numero di generazioni come budget
    private bool CheckForGenerationNumber()
    {
        return generationNumber < generationLimit;
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
    private float CalculateFitness(CandidateMap candidateMap) 
	{
		int numberOfObstacles = candidateMap.Obstacles.Where(isObstacle => isObstacle).Count();
		int numberOfCorners = candidateMap.CornersList.Count;

		// -- Calcolo del valore di fitness --

		// Il valore finale della funzione di fitness
		float fitnessScore = 0; 
		
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

		float weightSum = fitnessCornerWeight + fitnessObstacleWeight + fitnessPathWeight;

		return fitnessScore/weightSum;
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
		
		float randomValue = rand.Next(0, (int)totalFitnessThisGeneration);

		for (int i = 0; i < populationSize; i++)
		{
			randomValue -= CalculateFitness(currentGeneration[i]);

			if(randomValue <= 0)
				return i;
		}

		return populationSize - 1;
	}

	private void TruncationSelection(ref CandidateMap c1, ref CandidateMap c2)
	{
		CandidateMap[] candidateMaps = currentGeneration.ToArray();

		float c1Fitness = CalculateFitness(c1);
		float c2Fitness = CalculateFitness(c2);

		if(c1Fitness < c2Fitness)
		{
			c1 = candidateMaps[1].DeepClone();
			c2 = candidateMaps[0].DeepClone();
		}
		

		for (int i = 2; i < candidateMaps.Length; i++)
		{

            float ciFitness = CalculateFitness(candidateMaps[i]);
            
            if (ciFitness > c1Fitness)
			{
				c2 = c1.DeepClone();
				c1 = candidateMaps[i].DeepClone();
			}
			else if(ciFitness > c2Fitness)
			{
				c2 = candidateMaps[i].DeepClone();
			}
		
		}

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

	public void WriteDataToExcel()
	{
		GD.Print("-------------------------------------------------");
		GD.Print("Salvataggio dei dati in Excel...");
		DataAnalysis da = new DataAnalysis();

		da.WriteDataInSheet(geneticAlgorithmData, new GeneticAlgorithmConfiguration{
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
			maxGenerationsWithoutImprovements = this.maxGenerationsWithoutImprovements,
			selectionMethod = this.selectionMethod,
			crossoverMethod = this.crossoverMethod,
			mutationMethod = this.mutationMethod,
		});
	}


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//RunAlgorithm();
		//WriteDataToExcel();
		grid = new Map(mapWidth, mapHeight);

		CandidateMap emptyCandidateMap = new CandidateMap(grid,0);
		mapVisualizer.GenerateMap(emptyCandidateMap.grid,emptyCandidateMap.Path);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Getters
	public bool IsAlgorithmRunning { get => isAlgorithmRunning; set => isAlgorithmRunning = value; }
}
