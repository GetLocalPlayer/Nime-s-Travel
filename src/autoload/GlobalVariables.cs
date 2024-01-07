using Godot;
using System;
using System.Collections.Generic;


/*
Для передачи данных между сценами. Подробнее тут:
https://docs.godotengine.org/en/stable/tutorials/scripting/singletons_autoload.html
*/
public partial class GlobalVariables : Node
{
	public enum SpellType
	{
		LEVITATION,
		IGNITION,
	}

	private Dictionary<SpellType, string> spellBook = new Dictionary<SpellType, string> {
		{SpellType.LEVITATION, "ggb"},
		{SpellType.IGNITION, "brb"},
	};
	
	public string SceneGateName = null;
}
