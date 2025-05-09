extends Node
class_name dash_state

var dash_node: PackedScene = preload("res://Scenes/Player/dash_node.tscn")

@onready var player: Node = get_parent().get_parent()
var pos: bool

func reset_node() -> void:
	pos = player.anim.flip_h
	player.can_dash = false
	if pos:
		player.velocity.x = -2000
	else:
		player.velocity.x = 2000
	await get_tree().create_timer(0.2).timeout
	
	player.change_state("idle")

func _physics_process(delta: float) -> void:
	if player.current_state == "dash":
		
		var dash_temp: Node = dash_node.instantiate()
		if pos:
			dash_temp.direction = -1
		else:
			dash_temp.direction = 1
		dash_temp.global_position = player.global_position
		player.get_parent().add_child(dash_temp)
