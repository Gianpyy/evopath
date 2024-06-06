extends Control



func _ready():
	
	
	var mapVisualizerNode = $".."
	var propertyList = mapVisualizerNode.get_property_list()
	for e in propertyList:
		if e["usage"] & PROPERTY_USAGE_SCRIPT_VARIABLE:
			
			var propertyName = str(e["name"])
			var propertyValueStr = str(mapVisualizerNode.get(propertyName))
			
			$ItemList.add_item(propertyName+" "+propertyValueStr,null,false)
	
	

func _on_check_button_toggled(toggled_on):
	if toggled_on:
		$"../Camera3D".SideViewCamera();
	else:
		$"../Camera3D".TopViewCamera();


func _on_button_pressed():
	$ItemList.visible = !$ItemList.visible
