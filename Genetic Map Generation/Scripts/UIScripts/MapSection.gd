extends Control

func _ready():
	$MapSizeLineEdit.placeholder_text = str($"/root/MapGenerator".mapWidth)
	$PopSizeLineEdit.placeholder_text = str($"/root/MapGenerator".populationSize)
	$GenerationLimitLineEdit.placeholder_text = str($"/root/MapGenerator".generationLimit)
	$KnightPiecesLineEdit.placeholder_text = str($"/root/MapGenerator".numberOfKnightPieces)
	
	$CheckBox.button_pressed = $"/root/MapGenerator".randomStartAndEnd;
	_on_check_box_toggled($CheckBox.button_pressed)
	$PositionEdgeContainer/StartEdgeOption.select($"/root/MapGenerator".startPositionEdge);
	$PositionEdgeContainer/ExitEdgeOption.select($"/root/MapGenerator".exitPositionEdge);
	
		
		
func _on_check_box_toggled(toggled_on):
	if toggled_on:
		$PositionEdgeContainer.visible = false
	else:
		$PositionEdgeContainer.visible = true
