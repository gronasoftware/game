using Godot;
using System;

public partial class InstantWeapon : Weapon
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void Shoot()
	{
			// Send a raycast from the direction the player is facing
			var spaceState = GetWorld3D().DirectSpaceState; // Can't use GetWorld3D in a singleton not attached to the scene tree
			var query = PhysicsRayQueryParameters3D.Create(GlobalTransform.Origin + -GlobalTransform.Basis.Z * 0.5f, GlobalTransform.Origin + -GlobalTransform.Basis.Z * 1000);
			var result = spaceState.IntersectRay(query);

			if (result.Count > 0)
			{
				// Draw small sphere at hitPosition
				var hitPosition = (Vector3)result["position"];
				var sphere = new SphereMesh();
				sphere.Radius = 0.1f;
				sphere.Height = 0.1f;
				var sphereInstance = new MeshInstance3D();
				sphereInstance.Mesh = sphere;
				sphereInstance.Position = hitPosition;
				GetTree().Root.AddChild(sphereInstance);

				var collider = result["collider"].AsGodotObject();

				if (collider is CharacterBase character)
				{
					character.ReduceHealth(10);
				}
			}
	}
}
