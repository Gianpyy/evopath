using Godot;
using System;

public partial class MapVisualizer : Node3D
{
	 [Export]
    public PackedScene grassObj, roadObj, curvedRoadObj, startObj, exitObj, knightObj, obstacleObj;

    public void GenerateMap(Map map)
    {
       

        for (int i = 0; i < map.Width; i++)
        {
            for (int j = 0; j < map.Height; j++)
            {

				Node3D spawnObj = null;

				switch(map.MapGrid[i, j].CellObjectType)
				{
					case CellObjectType.Empty:
						spawnObj = (Node3D)grassObj.Instantiate();
					break;

					case CellObjectType.Road:
						spawnObj = (Node3D)roadObj.Instantiate();
					break;

					case CellObjectType.Start:
						spawnObj = (Node3D)startObj.Instantiate();
						if(i+1 < map.Height && map.MapGrid[i+1, j].CellObjectType == CellObjectType.Road)
							((MeshInstance3D)spawnObj.GetChild(0)).RotateY(Mathf.DegToRad(90));
						else if(j-1>=0 && map.MapGrid[i, j-1].CellObjectType == CellObjectType.Road)
							((MeshInstance3D)spawnObj.GetChild(0)).RotateY(Mathf.DegToRad(180));
						else if(i-1>=0 && map.MapGrid[i-1, j].CellObjectType == CellObjectType.Road)
						((MeshInstance3D)spawnObj.GetChild(0)).RotateY(Mathf.DegToRad(270));
						
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
					// i - 1 sopra
					// i + 1 sotto
					// j -1 sinistra
					// j + 1 destra

					//TODO: caso ipotetico start e exit affiancate -> genetic algorithm?

					if (map.MapGrid[i, j].CellObjectType == CellObjectType.Road)
					{
						// Controllo sul lato sinistro
						if (j - 1 >= 0 && j + 1 < map.Width &&
							(map.MapGrid[i, j - 1].CellObjectType == CellObjectType.Road ||
							map.MapGrid[i, j - 1].CellObjectType == CellObjectType.Start ||
							map.MapGrid[i, j - 1].CellObjectType == CellObjectType.Exit) &&
							(map.MapGrid[i, j + 1].CellObjectType == CellObjectType.Road ||
							map.MapGrid[i, j + 1].CellObjectType == CellObjectType.Start ||
							map.MapGrid[i, j + 1].CellObjectType == CellObjectType.Exit))
						{
							((MeshInstance3D)spawnObj.GetChild(0)).RotateY(Mathf.DegToRad(90));
						}

						// Da sinistra a sopra
						else if (i - 1 >= 0 && j - 1 >= 0 &&
								(map.MapGrid[i - 1, j].CellObjectType == CellObjectType.Road ||
								map.MapGrid[i - 1, j].CellObjectType == CellObjectType.Start ||
								map.MapGrid[i - 1, j].CellObjectType == CellObjectType.Exit) &&
								(map.MapGrid[i, j - 1].CellObjectType == CellObjectType.Road ||
								map.MapGrid[i, j - 1].CellObjectType == CellObjectType.Start ||
								map.MapGrid[i, j - 1].CellObjectType == CellObjectType.Exit))
						{
							spawnObj = (Node3D)curvedRoadObj.Instantiate();
							((MeshInstance3D)spawnObj.GetChild(0)).RotateY(Mathf.DegToRad(180));
						}

						// Da sotto a destra
						else if (i + 1 < map.Height && j + 1 < map.Width &&
								(map.MapGrid[i + 1, j].CellObjectType == CellObjectType.Road ||
								map.MapGrid[i + 1, j].CellObjectType == CellObjectType.Start ||
								map.MapGrid[i + 1, j].CellObjectType == CellObjectType.Exit) &&
								(map.MapGrid[i, j + 1].CellObjectType == CellObjectType.Road ||
								map.MapGrid[i, j + 1].CellObjectType == CellObjectType.Start ||
								map.MapGrid[i, j + 1].CellObjectType == CellObjectType.Exit))
						{
							spawnObj = (Node3D)curvedRoadObj.Instantiate();
							((MeshInstance3D)spawnObj.GetChild(0)).RotateY(Mathf.DegToRad(0));  // Ruota di 0 gradi, opzionale
						}

						// Da sopra a destra
						else if (i - 1 >= 0 && j + 1 < map.Width &&
								(map.MapGrid[i - 1, j].CellObjectType == CellObjectType.Road ||
								map.MapGrid[i - 1, j].CellObjectType == CellObjectType.Start ||
								map.MapGrid[i - 1, j].CellObjectType == CellObjectType.Exit) &&
								(map.MapGrid[i, j + 1].CellObjectType == CellObjectType.Road ||
								map.MapGrid[i, j + 1].CellObjectType == CellObjectType.Start ||
								map.MapGrid[i, j + 1].CellObjectType == CellObjectType.Exit))
						{
							spawnObj = (Node3D)curvedRoadObj.Instantiate();
							((MeshInstance3D)spawnObj.GetChild(0)).RotateY(Mathf.DegToRad(-90));
						}

						// Da sotto a sinistra
						else if (i + 1 < map.Height && j - 1 >= 0 &&
								(map.MapGrid[i + 1, j].CellObjectType == CellObjectType.Road ||
								map.MapGrid[i + 1, j].CellObjectType == CellObjectType.Start ||
								map.MapGrid[i + 1, j].CellObjectType == CellObjectType.Exit) &&
								(map.MapGrid[i, j - 1].CellObjectType == CellObjectType.Road ||
								map.MapGrid[i, j - 1].CellObjectType == CellObjectType.Start ||
								map.MapGrid[i, j - 1].CellObjectType == CellObjectType.Exit))
						{
							spawnObj = (Node3D)curvedRoadObj.Instantiate();
							((MeshInstance3D)spawnObj.GetChild(0)).RotateY(Mathf.DegToRad(90));
						}
					}

					
					spawnObj.Position = new Vector3I(i*4,0,j*4);
					spawnObj.Name = map.MapGrid[i, j].CellObjectType.ToString() + " ["+i+","+j+"]";
					AddChild(spawnObj);
					
				}
                
            }
        }
    }
}
