using Godot;
using System;



public abstract partial class State
{
	public event EventHandler<Node> StateFinished;
	
	abstract public void Enter(Node context);
	abstract public void Exit(Node context);
	abstract public void Update(Node context, double delta);

	protected void RaiseStateFinished(Node context)
	{
		StateFinished?.Invoke(this, context);
	}
}
