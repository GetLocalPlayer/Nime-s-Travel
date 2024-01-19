using Godot;
using System;


/* A node to switch between scenes with loading if needed.

Root
|--SpawnPoint		// - the position assigned to Nime after a new scene is
|					// loaded (global coords).
|--SpawnWayPoint	// - the position Nime walks to on the NavigationRegion2D
|					// after the scene switched .
|--ExitWayPoint		// - the position Nime walks to to switch to another scene,
|					// it must lead to the collider.
|--Collider			// - collider that must detect the collision with Nime
|					// to switch the scene.
|--Clickable		// - simply a click catcher that commands Nime on click to
					// move to ExitWayPoint.
*/
public partial class SceneGate : Node2D
{
	bool _active = true;
	[Export] public bool Active {
		get => _active;
		set {
			_active = value;
			if (IsNodeReady())
			{
				GetNode<Area2D>("Clickable").Monitoring = value;
				GetNode<Area2D>("Clickable").InputPickable = value;
				GetNode<Area2D>("Collider").Monitoring = value;
			}
		}
	}

	/* The path in the project directory to the scene
	the current scene must be changed to. Loaded once
	and then stored	in 'storedScene' field. */
	[Export] public string ToScenePath;
	Node storedScene;

	/* The gate cane be blocked which causes specific
	interaciton. */
	[ExportGroup("Way Blocking")]
	[Export] public bool IsBlocked = false;
	[Export] public string[] BlockedInteractionLines;

	public Vector2 SpawnPoint { get => GetNode<Marker2D>("SpawnPoint").GlobalPosition; }
	public Vector2 SpawnWayPoint { get => GetNode<Marker2D>("SpawnWayPoint").GlobalPosition; }
	public Vector2 ExitWayPoint { get => GetNode<Marker2D>("ExitWayPoint").GlobalPosition; }

	public void Disable() => Active = false;
	public void Enable() => Active = true;


    public override void _Ready()
    {
		GetNode<Area2D>("Clickable").Monitoring = Active;
		GetNode<Area2D>("Clickable").InputPickable = Active;
		GetNode<Area2D>("Collider").Monitoring = Active;	

        var clickable = GetNode<Area2D>("Clickable");
		clickable.InputEvent += (viewport, @event, shapeIdx) =>
		{
			if (@event is InputEventMouseButton btn)
				if (btn.IsPressed() && btn.ButtonIndex == MouseButton.Left)
				{
					GetTree().CallGroup("Player", "SceneGateClicked", this);
					((Viewport)viewport).SetInputAsHandled();
				}
		};

		var cursorSetup = GetTree().Root.GetNode<CursorSetup>("CursorSetup");

		clickable.MouseEntered += () =>
			cursorSetup.SetCursorIcon(CursorSetup.Shape.Exit);

		clickable.MouseExited += () =>
			cursorSetup.SetCursorIcon(CursorSetup.Shape.Default);

		var collider = GetNode<Area2D>("Collider");
		collider.AreaEntered += (area) =>
		{
			if (area.IsInGroup("PlayerCollider"))
				if (IsBlocked)
				{
					GetTree().Root.GetNode<UI>("UI").RunInteraction(BlockedInteractionLines);
					GetTree().CallGroup("Player", "LookAt", SpawnWayPoint);
				}
				else
				{
					/* Scene changing must be performed in the "idle"
					frame, otherwise the game engine can causes problems
					since it still has its loop process running. */
					CallDeferred("ChangeScene");
				}
		};
    }

	private void ChangeScene()
	{
		/* I do not delete the current scene from the memory,
		I just remove it from the tree, saving it in
		'storedScene' field of the gates that lead back to
		the removed scene. "Removed" here doesn't mean
		"deleted". */
		
		/* GetTree() is a method of the current node that runs
		inside the scene I'm removing, so, right after removing
		the scene from the tree, each call of 'GetTree' will 
		return 'null' value. Hence, need to store it localy
		before removing the scene. */
		var tree = GetTree();

		/* Removing the scene ('RemoveChild' does NOT delete the
		node from the memory). */
		var removedScene = GetTree().CurrentScene;
		tree.Root.RemoveChild(removedScene);
		/* Get Nime from the current scene to pass her in the
		next one. */
		var nime = removedScene.GetNode<Nime>("Nime");

		/* Durning testing, I added its own instance of Nime in
		each scene. It appeared to be quite conviniet and I decided
		not to delete the unused instance of Nime from the scene
		I'm currently loading, but keep it with a name "NimeParams"
		since it has already set, during testing, parameters, such
		as 'MoveSpeed'. I only delete all children of the unused
		instance, groups, stop processes. */

		if (!removedScene.HasNode("NimeParams"))
		{
			var copy = nime.Duplicate((int)DuplicateFlags.Scripts) as Nime;
			copy.Disable();
			copy.Hide();
			copy.Name = "NimeParams";
			foreach (var child in copy.GetChildren())
			{
				copy.RemoveChild(child);
				child.QueueFree();
			}
			removedScene.AddChild(copy);
		}
		removedScene.RemoveChild(nime);

		/* Loading the next scene from the path specified in the
		exported field 'ToScenePath' if it wasn't loaded before
		and storing the loaded scene in 'storedScene' field not to
		load it twice. */
		var loadedScene = storedScene ?? ResourceLoader.Load<PackedScene>(ToScenePath).Instantiate();
		storedScene = loadedScene;
		/* Each scene has it's own instance of Nime. That means
		the instance of Nime inside the scene the game starts
		with is the Nime we actually play and transfer from one
		scene to another. But we still need a local instance of
		Nime for parameters to take from if Nime returns to this
		scene, hence, I must create a copy of nime with name
		"NimeParams" if the scene we're removing is the scene the
		game starts with. */
		if (!loadedScene.HasNode("NimeParams"))
		{
			var n = loadedScene.GetNode<Nime>("Nime");
			n.Disable();
			n.Hide();
			n.Name = "NimeParams";
			foreach (var child in n.GetChildren())
			{
				n.RemoveChild(child);
				child.QueueFree();
			}
			foreach (var group in n.GetGroups())
				n.RemoveFromGroup(group);
		}
		nime.MoveSpeed = loadedScene.GetNode<Nime>("NimeParams").MoveSpeed;
		/* Setting current scene manually. The disadventage of this approach
		is 'GetTree().CurrrentScene' returns 'null' when '_Ready' methods
		of the loaded scene's nodes invoked. Must keep it in mind and delay
		nodes initialization to the "idle" frame. */
		tree.Root.AddChild(loadedScene);
		tree.CurrentScene = loadedScene;

		/* Setting 'storedScene' field of the gates in the loaded scene to the
		reference of the removed scene. By default, I expect that the gates of
		the loaded scene and the gates of the removed scene have the same name
		marked as unique, for instance "Street" scene has SceneGate that leads
		to "Graveyard" scene, and it's name "ToStreetToGraveyard" and marked as
		unique name. Hence, "Graveyard" scene must have a SceneGate node with
		the same "ToStreetToGraveyard" name marked as unique name. */
		var loadedGate = loadedScene.GetNodeOrNull<SceneGate>(new NodePath("%" + Name));
		if (loadedGate != null)
			loadedGate.storedScene = removedScene;
		/* Time to pass playable instance of Nime to the loaded scene, via
		deferred call not to break anything in the process. (P.S. The engine's
		API is written in snake_case, while its C# wrapper is written in
		CamelCase, hence, to call funciton by name from the original API
		we must use the original name written in snake_case).*/
		loadedScene.CallDeferred("add_child", nime);
		if (loadedGate != null)
			tree.CallDeferred("call_group", "Player", "EnterScene", loadedGate);

		tree.Root.GetNode<CursorSetup>("CursorSetup").SetCursorIcon(CursorSetup.Shape.Default);
	}
}
