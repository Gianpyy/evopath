extends Camera3D

func _ready():
	TopViewCamera();

func TopViewCamera():
	var mapWidth = $"..".mapWidth;
	var mapHeight = $"..".mapHeight;
	#(0,0) to center
	position.x = mapWidth*2*-1;
	position.z = mapHeight*2;

	position.y = (mapWidth)*4;
	
	rotation_degrees.x = -90 

func SideViewCamera():
	var mapWidth = $"..".mapWidth;
	var mapHeight = $"..".mapHeight;
	position.x = mapWidth*4.5*-1;
	position.z = mapHeight*2;
	position.y = mapWidth*2;
	rotation_degrees.x = -50 
