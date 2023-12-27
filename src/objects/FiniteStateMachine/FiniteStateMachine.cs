using Godot;
using System;

public abstract partial class FiniteStateMachine : Node
{
	protected State currentState;

    public override void _PhysicsProcess(double delta)
    {
        currentState?.Update(GetContext(), delta);
    }

	virtual protected Node GetContext() => GetParent();
	
    public void SetState(State newState)
	{
		if (newState == currentState) return;
		currentState?.Exit(GetContext());
		newState?.Enter(GetContext());
		currentState = newState;
	}
}
