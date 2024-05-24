using Godot;
using System;

public partial class MapVisualizer : Node3D
{
	 [Export]
    public PackedScene grassObj, roadObj, startObj, exitObj, knightObj, obstacleObj;

    public void GenerateMap(Map map)
    {
       

        for (int i = 0; i < map.Width; i++)
        {
            for (int j = 0; j < map.Height; j++)
            {

				Node3D grass = (Node3D)grassObj.Instantiate();
				grass.Position = new Vector3I(i*2,0,j*2);
				AddChild(grass);

				Node3D spawnObj = null;

				switch(map.MapGrid[i, j].CellObjectType)
				{

					case CellObjectType.Road:
						spawnObj = (Node3D)roadObj.Instantiate();
					break;

					case CellObjectType.Start:
						spawnObj = (Node3D)startObj.Instantiate();
					break;

					case CellObjectType.Exit:
						spawnObj = (Node3D)exitObj.Instantiate();
					break;

					case CellObjectType.Knight:
						spawnObj = (Node3D)knightObj.Instantiate();
					break;

					case CellObjectType.Obstacle:
						spawnObj = (Node3D)obstacleObj.Instantiate();
					break;

				}

				if(spawnObj!=null)
				{
					spawnObj.Position = new Vector3I(i*2,-1,j*2);
					
					AddChild(spawnObj);
				}
                
            }
        }
    }
}
