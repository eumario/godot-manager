@tool
extends AcceptDialog


enum Filters {REGULAR = 0b1, BRANDS = 0b10, SOLID = 0x100}

const cheatsheet: Dictionary = preload("../All.gd").all

var filters: int = Filters.REGULAR
var first_popup: bool = true


func _ready() -> void:
	$Box/Filters/Regular.toggled.connect(_on_filter_toggled.bind(Filters.REGULAR))
	$Box/Filters/Brands.toggled.connect(_on_filter_toggled.bind(Filters.BRANDS))
	$Box/Filters/Solid.toggled.connect(_on_filter_toggled.bind(Filters.SOLID))


func _on_filter_toggled(_button_pressed: bool, bit_flag: int) -> void:
	filters = filters ^ bit_flag
	query_options($Box/Filters/Search.text)


func _on_search_text_changed(new_text: String) -> void:
	query_options(new_text)


func fetch_options() -> void:
	var template := $Box/Option as Button
	var options := $Box/Scroll/Options as HFlowContainer
	for type in cheatsheet.keys():
		for icon in cheatsheet[type].keys():
			var option := template.duplicate() as Button
			option.set_meta(&"text", icon)
			option.set_meta(&"type", type)
			options.add_child(option)
			var glyph: String = cheatsheet[type][icon]
			option.tooltip_text = "Unicode: "+String.num_uint64(glyph.unicode_at(0), 16, false)
			var option_icon := option.get_node(^"Margin/Box/Icon") as Label
			var option_name := option.get_node(^"Margin/Box/Name") as Label
			option_icon.icon_type = type
			option_icon.icon_name = icon
			option_name.text = icon
			option.name = icon.to_pascal_case().validate_node_name()
			option.visible = true



	query_options()

	$Fetching.visible = false


func query_options(search_term: String = "") -> void:
	var filtered_types: Array[String] = []
	if filters & Filters.REGULAR:
		filtered_types.append("regular")
	if filters & Filters.BRANDS:
		filtered_types.append("brands")
	if filters & Filters.SOLID:
		filtered_types.append("solid")

	var options := $Box/Scroll/Options as HFlowContainer
	var results: int = 0
	for option in options.get_children():
		var type: String = option.get_meta(&"type", "")
		var text: String = option.get_meta(&"text", "")
		if filtered_types.find(type) > -1 and ( text.find(search_term.to_lower()) > -1 or search_term.is_empty() ):
			option.visible = true
			results += 1
		else:
			option.visible = false

	$Box/Scroll.visible = results > 0
	$Box/NoResults.visible = results == 0


func show_window() -> void:
	if first_popup:
		popup_centered()
		first_popup = false
		var search := $Box/Filters/Search as LineEdit
		search.placeholder_text = "Filter by name..."
		if has_theme_icon(&"Search", &"EditorIcons"):
			search.right_icon = get_theme_icon(&"Search", &"EditorIcons")
		await get_tree().create_timer(0.25).timeout
		fetch_options()
	else:
		popup()
