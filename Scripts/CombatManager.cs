using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// In order to make scenes be part of the combat system
// They need to be in the Combatant group
public partial class CombatManager : Node3D
{
	public List<CharacterBase> characters = new List<CharacterBase>();

	public override void _Ready()
	{
		// Register all characters in the scene
		var characters = GetTree().GetNodesInGroup("Combatant");
		foreach (var character in characters)
		{
			RegisterCharacter((CharacterBase)character);
		}
	}

	public void RegisterCharacter(CharacterBase character)
	{
		characters.Add(character);
		character.ShotFired += OnShotFired;
	}

	public void UnregisterCharacter(CharacterBase character)
	{
		characters.Remove(character);
		character.ShotFired -= OnShotFired;
	}
	public void OnShotFired(Vector3 origin, Vector3 direction)
	{
		// Send a raycast from the direction the player is facing
		var spaceState = GetWorld3D().DirectSpaceState; // Can't use GetWorld3D in a singleton not attached to the scene tree
		var query = PhysicsRayQueryParameters3D.Create(origin, origin +direction * 1000);
		// Not sure if we need to exclude the source character
		//query.Exclude = new Godot.Collections.Array<Rid> { player.GetRid() };
		var result = spaceState.IntersectRay(query);

		if (result.Count > 0)
		{
			// Draw small sphere at hitPosition
			var hitPosition = (Vector3)result["position"];
			var sphere2 = new SphereMesh();
			sphere2.Radius = 0.1f;
			sphere2.Height = 0.1f;
			var sphereInstance2 = new MeshInstance3D();
			sphereInstance2.Mesh = sphere2;
			sphereInstance2.Position = hitPosition;
			AddChild(sphereInstance2);

			var collider = result["collider"].AsGodotObject();

			if (collider is CollisionObject3D collisionObject)
			{
				if (collisionObject is CharacterBase hitCharacter)
					hitCharacter?.ReduceHealth(20);
			}
		}
	}
}
