extends SceneTree

# Run in an empty project so files in the source tree cannot mask a broken pack.
func _initialize() -> void:
	var args := OS.get_cmdline_user_args()
	if args.size() != 1 or not ProjectSettings.load_resource_pack(args[0]):
		push_error("Cannot mount the mod PCK")
		quit(1)
		return
	var failed := false
	for path in [
		"res://HeavySilent/images/card_portraits/card.png",
		"res://HeavySilent/images/card_portraits/big/card.png",
		"res://HeavySilent/images/charui/big_energy.png",
		"res://HeavySilent/images/charui/text_energy.png",
		"res://HeavySilent/mod_image.png"
	]:
		var texture = ResourceLoader.load(path)
		if not texture is Texture2D:
			push_error("Texture cannot be loaded from PCK: " + path)
			failed = true
	for language in ["eng", "zhs"]:
		for table in ["cards", "characters", "ancients"]:
			var path := "res://HeavySilent/localization/%s/%s.json" % [language, table]
			var parsed = JSON.parse_string(FileAccess.get_file_as_string(path))
			if not parsed is Dictionary or parsed.is_empty():
				push_error("Localization cannot be loaded from PCK: " + path)
				failed = true
	if not failed:
		print("PCK_ENGINE_OK: 5 textures and 6 localization tables loaded from the packaged resources.")
	quit(1 if failed else 0)
