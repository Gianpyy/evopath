using Godot;
using System;

public partial class MapVisualizer : Node3D
{
	[Export]
    public PackedScene grassObj, roadObj, curvedRoadObj, startObj, exitObj, knightObj, obstacleObj;
	[Export]
	public PackedScene seasShoreObj, seasShoreCornerObj, seaObj;

	private int mapSize;
	[Export]
	private int outSize = 5;

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

		       
	   	// Prendiamo la width poiché è una grid MxM, quindi non fa differenza
	   	mapSize = map.Width;
		
		// Creazione bordo e esterno della mappa centrale
		GenerateMapBorder();
		GenerateOuterMap();

		
    }

	// Creates the sea around the map of outSize width
	private void GenerateOuterMap()
	{

		for(int i = outSize*(-1); i < mapSize+outSize; i++)
		{
			for(int j = outSize*(-1);j < mapSize+outSize; j++)
			{
				if(!(i >= -1 && j>=-1 && i<=mapSize && j<=mapSize))
				{
		
					Node3D seaNode =(Node3D) seaObj.Instantiate();
					seaNode.Position = new Vector3I(i*4,0,j*4);	
					AddChild(seaNode);
				}
					
			}

		}
	}

	private void GenerateMapBorder()
	{
		// Border angles
		// (0,0) [sopra a sinistra]
		Node3D cornerObj =(Node3D) seasShoreCornerObj.Instantiate();
		cornerObj.Position = new Vector3I(-4,0,-4);	
		((MeshInstance3D)cornerObj.GetChild(0)).RotateY(Mathf.DegToRad(180));
		AddChild(cornerObj);

		// (mapWidth,0) [sopra a destra]
		cornerObj =(Node3D) seasShoreCornerObj.Instantiate();
		cornerObj.Position = new Vector3I(-4,0,mapSize*4);	
		((MeshInstance3D)cornerObj.GetChild(0)).RotateY(Mathf.DegToRad(270));
		AddChild(cornerObj);

		// (0, mapHeight) [sotto a sinistra]
		cornerObj =(Node3D) seasShoreCornerObj.Instantiate();
		cornerObj.Position = new Vector3I(mapSize*4,0,-4);	
		((MeshInstance3D)cornerObj.GetChild(0)).RotateY(Mathf.DegToRad(90));
		AddChild(cornerObj);

		// (mapWidth, mapHeight) [sotto a destra]
		cornerObj =(Node3D) seasShoreCornerObj.Instantiate();
		cornerObj.Position = new Vector3I(mapSize*4,0,mapSize*4);
		AddChild(cornerObj);

		
		// Bordi orizzontali
		for (int i = 0; i < mapSize; i++)
        {
            Node3D spawnObj = (Node3D)seasShoreObj.Instantiate();
			((MeshInstance3D)spawnObj.GetChild(0)).RotateY(Mathf.DegToRad(90));
			spawnObj.Position = new Vector3I(mapSize*4,0,i*4);
			AddChild(spawnObj);

			spawnObj = (Node3D)seasShoreObj.Instantiate();
			((MeshInstance3D)spawnObj.GetChild(0)).RotateY(Mathf.DegToRad(-90));
			spawnObj.Position = new Vector3I(-4,0,i*4);
			AddChild(spawnObj);
		}

		// Bordi verticali
		for (int i = 0; i < mapSize; i++)
        {
            Node3D spawnObj = (Node3D)seasShoreObj.Instantiate();
			spawnObj.Position = new Vector3I(i*4,0,mapSize*4);
			AddChild(spawnObj);

			spawnObj = (Node3D)seasShoreObj.Instantiate();
			((MeshInstance3D)spawnObj.GetChild(0)).RotateY(Mathf.DegToRad(-180));
			spawnObj.Position = new Vector3I(i*4,0,-4);
			AddChild(spawnObj);
		}
	}
}
