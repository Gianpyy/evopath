using Godot;
using System;


public enum Selection
{
	RouletteWheel,
	Truncation,
}

public enum Crossover
{
	SinglePoint,
	Uniform,
	Heuristic,
	BlockSwap
}

public enum Mutation
{
	BitFlip,
	Block,
	SimulatedAnnealing
}

