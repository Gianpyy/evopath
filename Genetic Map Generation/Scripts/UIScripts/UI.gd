extends Control



#func _ready():
	
	
	#var mapVisualizerNode = $".."
	#var propertyList = mapVisualizerNode.get_property_list()
	#for e in propertyList:
	#	if e["usage"] & PROPERTY_USAGE_SCRIPT_VARIABLE:
			
	#		var propertyName = str(e["name"])
	#		var propertyValueStr = str(mapVisualizerNode.get(propertyName))
			
	#		$ShowDataButton/ItemList.add_item(propertyName+" "+propertyValueStr,null,false)
			
	
	#$TextEdit.text = str($"..".mutationMethod.keys());
	
	

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
