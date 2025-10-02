@tool
extends Label
class_name FontAwesome

@export_category("FontAwesome")
@export_range(1, 16384) var icon_size: int = 16: set = set_icon_size
@export_enum("solid", "regular", "brands") var icon_type: String = "solid": set = set_icon_type
@export var icon_name: String = "circle-question": set = set_icon_name

const icon_fonts: Dictionary = {
	"solid": "res://addons/fontawesome/fonts/fa-solid-900.woff2",
	"regular": "res://addons/fontawesome/fonts/fa-regular-400.woff2",
	"brands": "res://addons/fontawesome/fonts/fa-brands-400.woff2"
}

const cheatsheet: Dictionary = preload("res://addons/fontawesome/All.gd").all

func _init():
	horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	# disable some things, this is icon not text
	auto_translate = false
	localize_numeral_system = false

	set_icon_type(icon_type)
	set_icon_size(icon_size)
	set_icon_name(icon_name)

func set_icon_size(new_size: int):
	icon_size = clamp(new_size, 1, 16384)
	add_theme_font_size_override("font_size", icon_size)
	queue_redraw()

func set_icon_type(new_type: String):
	icon_type = new_type
	match icon_type:
		"solid", "regular", "brands":
			add_theme_font_override("font", load(icon_fonts[icon_type]))

func set_icon_name(new_name: String):
	icon_name = new_name
	var iconcode = ""
	if icon_name in cheatsheet[icon_type]:
		iconcode = cheatsheet[icon_type][icon_name]
		set_text(iconcode)
