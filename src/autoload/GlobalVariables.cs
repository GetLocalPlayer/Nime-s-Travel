using Godot;
using System;
using System.Collections.Generic;


/*
Для передачи данных между сценами. Подробнее тут:
https://docs.godotengine.org/en/stable/tutorials/scripting/singletons_autoload.html
*/
public partial class GlobalVariables : Node
{
	public string SceneGateName = null;
	readonly static Dictionary<string, string> spellCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase){
		{"levitation", "bbg"},
		{"IGNITION", "gbrbg"},
	};

	public static string GetSpellCode(string spellName) =>
		spellCodes.ContainsKey(spellName) ? spellCodes[spellName] : "";

    public override void _Ready()
    {
        base._Ready();
    }
}
