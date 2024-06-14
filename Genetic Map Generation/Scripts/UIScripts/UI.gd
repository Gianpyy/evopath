extends Control


func _on_check_button_toggled(toggled_on):
	if toggled_on:
		$"../Camera3D".SideViewCamera();
	else:
		$"../Camera3D".TopViewCamera();


func _on_button_pressed():
	
	$ShowDataButton/Panel.visible = !$ShowDataButton/Panel.visible


func _on_generate_map_button_pressed():
	$"..".RunAlgorithm();



func _on_data_to_excel_button_pressed():
	$"..".WriteDataToExcel();
