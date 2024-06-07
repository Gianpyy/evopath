using System;

public struct GeneticAlgorithmData
{
	public float[] fitnessArray;
    public float bestMapFitness;
    public int pathLenght;
    public int numberOfCorners;
    public int numberOfObstacles;
    public TimeSpan elapsedTime;
}
