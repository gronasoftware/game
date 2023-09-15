using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


interface ICombatScene
{
	void RegisterCharacter(CharacterBase character);
	void UnregisterCharacter(CharacterBase character);
}


public partial class TestEnvironment : Node3D, ICombatScene
{
	public List<CharacterBase> characters = new List<CharacterBase>();

	public void RegisterCharacter(CharacterBase character)
	{
		characters.Add(character);
		character.ShotFired += OnPlayerShotFired;
	}

	public void UnregisterCharacter(CharacterBase character)
	{
		characters.Remove(character);
		character.ShotFired -= OnPlayerShotFired; // ? Can we do this
	}

	public void OnPlayerShotFired(Vector3 origin, Vector3 direction)
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
