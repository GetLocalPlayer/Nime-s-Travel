using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class Spells: Node
{
	/* Each reversed spell is just a spell but with reversed code of
	its original verions.
	Reversed spells with their codes are generated in '_Ready' method.
	Simply.
	Each reversed spell has a name equeal to its original version but
	with "-" in the beginning. So reversed spell of "LEVITATIN" is 
	"-LEVITATION". Reversed spell is possible only for non-symmetric
	spells, .g. "IGNITION" spell cannot have a reversed version since
	its code written backwards is the same as the original. */

	readonly Dictionary<string, string> spellCodes = new(StringComparer.OrdinalIgnoreCase){
		{"TEST", "bbbgggrrr"},
		{"LEVITATION", "bbg"},
		{"IGNITION", "gbrbg"},
	};

	public string GetSpellCode(string spellName) =>
		spellCodes.ContainsKey(spellName) ? spellCodes[spellName].ToLower() : null;

	public string GetSpellName(string code) =>
		spellCodes.FirstOrDefault(entry => entry.Value == code).Key;

    public override void _Ready()
    {
		var revSpells = new Dictionary<string, string>();
		foreach (var entry in spellCodes)
		{
			var reverse = new string(entry.Value.Reverse().ToArray());
			if (reverse != entry.Value)
				revSpells.Add("-" + entry.Key, reverse);
		}

		foreach (var entry in revSpells)
			spellCodes.Add(entry.Key, entry.Value);
    }
}