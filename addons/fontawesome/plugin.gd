@tool
extends EditorPlugin

var finder: Window

func _enter_tree():
	add_custom_type("FontAwesome", "Label", preload("res://addons/fontawesome/FontAwesome.gd"), preload("res://addons/fontawesome/flag-solid.svg"))

	finder = preload("finder/window.tscn").instantiate()
	finder.theme = get_editor_interface().get_base_control().theme
	get_editor_interface().get_base_control().add_child(finder)
	add_tool_menu_item("FontAwesome Icons", _on_finder_pressed)


func _exit_tree():
	remove_custom_type("FontAwesome")
	remove_tool_menu_item("FontAwesome Icons")
	if is_instance_valid(finder):
		finder.queue_free()


func _on_finder_pressed():
	finder.show_window()
