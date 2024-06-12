extends Camera3D

func _ready():
	TopViewCamera();
	target_y = position.y
	
var speed = 20.0
var scroll_speed = 10
var target_y = 0.0
var maxZoomOut
var maxZoomIn = 5

func _process(delta):
	var input_vector = Vector3()

	if Input.is_key_pressed(KEY_W):
		input_vector.x += 1
	if Input.is_key_pressed(KEY_A):
		input_vector.z -= 1
	if Input.is_key_pressed(KEY_S):
		input_vector.x -= 1
	if Input.is_key_pressed(KEY_D):
		input_vector.z += 1


	input_vector = input_vector.normalized() * speed * delta
	position += input_vector
	

	# Interpolazione fluida della posizione y
	position.y = lerp(position.y, target_y, scroll_speed * delta)
	position = position

	
func _input(event):
	if event is InputEventMouseButton and event.is_pressed():
		
			if event.button_index == MOUSE_BUTTON_WHEEL_UP:
				if target_y > maxZoomIn:
					target_y -= 1
			elif event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
				if maxZoomOut != null && target_y < maxZoomOut:
					target_y += 1

	

func TopViewCamera():
	var mapWidth = $"..".mapWidth;
	var mapHeight = $"..".mapHeight;
	#(0,0) to center
	position.x = mapWidth*2*-1;
	position.z = mapHeight*2;
	maxZoomOut = mapWidth*4
	position.y = maxZoomOut;
	target_y = position.y
	rotation_degrees.x = -90

func SideViewCamera():
	var mapWidth = $"..".mapWidth;
	var mapHeight = $"..".mapHeight;
	
	maxZoomOut = mapWidth*2
	position.x = mapWidth*4.5*-1;
	position.z = mapHeight*2;
	position.y = maxZoomOut;
	target_y = position.y
	rotation_degrees.x = -50 
