using Godot;
using System;

public struct GeneticAlgorithmConfiguration 
{
	public int populationSize;
	public int generationLimit;
	public int crossverRate;
	public int mutationRate;
	public int fitnessCornerMin;
	public int fitnessCornerMax;
	public int fitnessCornerWeight;
	public int fitnessObstacleWeight;
	public int fitnessPathWeight;
	public int mapWidth;
	public int mapHeight;
	public int numberOfPieces;
}
