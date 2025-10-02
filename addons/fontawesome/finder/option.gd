@tool
extends Button

func _pressed() -> void:
	var meta: String = get_meta(&"text", "")
	if not meta.is_empty():
		DisplayServer.clipboard_set(meta)
		var label := $Margin/Box/Name as Label

		if has_theme_color(&"success_color", &"Editor"):
			label.add_theme_color_override(&"font_color",
					get_theme_color(&"success_color", &"Editor"))
		label.text = "Copied to clipboard"

		await get_tree().create_timer(1.5).timeout

		label.remove_theme_color_override(&"font_color")
		label.text = meta

