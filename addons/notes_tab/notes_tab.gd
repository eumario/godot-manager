###############################################################################
# Notes Tab                                                                   #
# Copyright (C) 2018-2020 Michael Alexsander                                  #
#-----------------------------------------------------------------------------#
# This file is part of Notes Tab.                                             #
#                                                                             #
# Notes Tab is free software: you can redistribute it and/or modify           #
# it under the terms of the GNU General Public License as published by        #
# the Free Software Foundation, either version 3 of the License, or           #
# (at your option) any later version.                                         #
#                                                                             #
# Notes Tab is distributed in the hope that it will be useful,                #
# but WITHOUT ANY WARRANTY; without even the implied warranty of              #
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the               #
# GNU General Public License for more details.                                #
#                                                                             #
# You should have received a copy of the GNU General Public License           #
# along with Notes Tab.  If not, see <http://www.gnu.org/licenses/>.          #
###############################################################################

tool
extends EditorPlugin


var _notes_tab: TextEdit


func _enter_tree() -> void:
	_notes_tab = preload("res://addons/notes_tab/notes_tab.tscn").instance()

	# Load previous text state.
	var editor_settings: EditorSettings =\
			get_editor_interface().get_editor_settings()
	_notes_tab.text =\
			editor_settings.get_project_metadata("notes_tab", "notes", "")
	# If no previous cursor position is found, put it in the end of the text.
	_notes_tab.cursor_set_line(editor_settings.get_project_metadata(
			"notes_tab", "cursor_line", _notes_tab.get_line_count() - 1))
	_notes_tab.cursor_set_column(editor_settings.get_project_metadata(
			"notes_tab", "cursor_column", _notes_tab.get_line(
			_notes_tab.get_line_count() - 1).length()))

	# warning-ignore:return_value_discarded
	add_control_to_bottom_panel(_notes_tab, "Notes")

	# warning-ignore:return_value_discarded
	_notes_tab.connect("visibility_changed", self,
			"_on_notes_tab_visibility_changed")

	_update_settings()
	# warning-ignore:return_value_discarded
	get_editor_interface().get_editor_settings().connect("settings_changed",
			self, "update_settings")


func _exit_tree() -> void:
	remove_control_from_bottom_panel(_notes_tab)

	_notes_tab.queue_free()


func save_external_data() -> void:
	var editor_settings: EditorSettings =\
			get_editor_interface().get_editor_settings()
	editor_settings.set_project_metadata("notes_tab", "notes", _notes_tab.text)
	editor_settings.set_project_metadata("notes_tab", "cursor_line",
			_notes_tab.cursor_get_line())
	editor_settings.set_project_metadata("notes_tab", "cursor_column",
			_notes_tab.cursor_get_column())


func _update_settings() -> void:
	# Use some of the editor settings in the notes, for consistency.
	var editor_settings: EditorSettings =\
			get_editor_interface().get_editor_settings()
	_notes_tab.caret_block_mode =\
			editor_settings.get("text_editor/cursor/block_caret")
	_notes_tab.caret_blink =\
			editor_settings.get("text_editor/cursor/caret_blink")
	_notes_tab.caret_blink_speed =\
			editor_settings.get("text_editor/cursor/caret_blink_speed")
	_notes_tab.caret_moving_by_right_click =\
			editor_settings.get("text_editor/cursor/right_click_moves_caret")


func _on_notes_tab_visibility_changed() -> void:
	if _notes_tab.visible:
		_notes_tab.grab_focus()
