extends Control


# Called when the node enters the scene tree for the first time.
func _ready():
	$"CornerWeight/CornerWeightText".text = str($"/root/MapGenerator".fitnessCornerWeight)
	$CornerWeight.change_slider_value($"/root/MapGenerator".fitnessCornerWeight)
	
	$"ObstacleWeight/ObstacleWeightText".text = str($"/root/MapGenerator".fitnessObstacleWeight)
	$ObstacleWeight.change_slider_value($"/root/MapGenerator".fitnessObstacleWeight)
	
	$"PathWeight/PathWeightText".text = str($"/root/MapGenerator".fitnessPathWeight)
	$PathWeight.change_slider_value($"/root/MapGenerator".fitnessPathWeight)
	

