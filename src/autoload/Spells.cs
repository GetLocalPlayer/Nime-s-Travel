using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class Spells: Node
{
    /* Я рассматриваю всякое обратное
	заклинание как просто заклинание со своим
	уникальным именем, однако помечаю его как
	имя оригинального заклинания с префиксом "-".
	Так обратное заклинание для "LEVITATION" 
	это "-LEVITATION". Именя/коды обратных
	заклинаний генерируются внутри _Ready(). */
	readonly Dictionary<string, string> spellCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase){
		{"TEST", "bbb"},
		{"LEVITATION", "bbg"},
		{"IGNITION", "gbrbg"},
	};

	public string GetSpellCode(string spellName) =>
		spellCodes.ContainsKey(spellName) ? spellCodes[spellName].ToLower() : null;

	public string GetSpellName(string code) =>
		spellCodes.FirstOrDefault(entry => entry.Value == code).Key;

    public override void _Ready()
    {
        /* Генерация обратных заклинаний. */
		var revSpells = new Dictionary<string, string>();
		foreach (var entry in spellCodes)
		{
			var reverse = entry.Value.Reverse().ToString();
			/* Нет смысла добавлять заклинания с 
			симитричнвм кодом. */
			if (reverse != entry.Value)
				revSpells.Add("-" + entry.Key, reverse);
		}

		foreach (var entry in revSpells)
			spellCodes.Add(entry.Key, entry.Value);
    }
}