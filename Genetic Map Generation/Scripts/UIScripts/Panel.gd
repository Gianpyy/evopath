extends Panel


func _on_generate_map_button_pressed():
	visible = false

func _on_close_button_pressed():
	self.visible = false
	
func _on_change_params_button_pressed():
	visible = true
