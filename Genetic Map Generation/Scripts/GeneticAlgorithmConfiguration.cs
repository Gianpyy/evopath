public struct GeneticAlgorithmConfiguration 
{
	public int populationSize;
	public int generationLimit;
	public int crossverRate;
	public int mutationRate;
	public int fitnessCornerMin;
	public int fitnessCornerMax;
	public float fitnessCornerWeight;
	public float fitnessObstacleWeight;
	public float fitnessPathWeight;
	public int mapWidth;
	public int mapHeight;
	public int numberOfPieces;
	public int maxGenerationsWithoutImprovements;
	public Selection selectionMethod;
	public Crossover crossoverMethod;
	public Mutation mutationMethod;
}
