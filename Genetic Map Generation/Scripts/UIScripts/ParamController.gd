extends Control

func change_slider_value(value):
	get_child(2).value = value
	
func _on_slider_value_changed(value):
	get_child(1).text = str(value)




#func _on_text_edit_text_changed():
#	if float($"TextEdit".text) < 1:
#		print("2")
#		$"Slider".value = 1
#		$"TextEdit".text = "1"
#	else:
#		$"Slider".value = float($"TextEdit".text)
