extends TextEdit


func _on_obstacle_weights_value_changed(value):
	self.text = str(value)


func _on_text_changed():
	$"..".value = float($".".text)
