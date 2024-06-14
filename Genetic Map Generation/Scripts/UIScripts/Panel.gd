extends Panel


func _on_generate_map_button_pressed():
	visible = false

func _on_close_button_pressed():
	self.visible = false
	
func _on_change_params_button_pressed():
	visible = true

func _on_weights_button_pressed():
	if !$WeightsSection.visible:
		$WeightsSection.visible = true
		
	
	$GeneticOperatorsSection.visible = false
	$MapSection.visible = false


func _on_map_button_pressed():
	if !$MapSection.visible:
		$MapSection.visible = true
	
	$WeightsSection.visible = false
	$GeneticOperatorsSection.visible = false

func _on_operators_button_pressed():
	if !$GeneticOperatorsSection.visible:
		$GeneticOperatorsSection.visible = true
		
	$WeightsSection.visible = false
	$MapSection.visible = false


