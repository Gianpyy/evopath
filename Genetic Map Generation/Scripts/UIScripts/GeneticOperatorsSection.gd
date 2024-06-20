extends Control


# Called when the node enters the scene tree for the first time.
func _ready():
	
	$SelectionOption.select($"/root/MapGenerator".selectionMethod)
	$CrossoverOption.select($"/root/MapGenerator".crossoverMethod)
	$MutationOption.select($"/root/MapGenerator".mutationMethod)
	
	$CrossoverRateEdit.placeholder_text = str($"/root/MapGenerator".crossoverRate)
	$MutationRateEdit.placeholder_text = str($"/root/MapGenerator".mutationRate)
