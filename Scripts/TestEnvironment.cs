using Godot;
using System;

public partial class TestEnvironment : Node3D
{
    public Player player;

    public override void _Ready()
    {
        player = GetNode<Player>("Player");

        player.ShotFired += OnPlayerShotFired;

    }

    public void OnPlayerShotFired(Vector3 origin, Vector3 direction)
    {
        // Send a raycast from the direction the player is facing
        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(origin, origin +direction * 1000);
        query.Exclude = new Godot.Collections.Array<Rid> { player.GetRid() };
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

            if (collider is CollisionObject3D)
            {
                var collisionObject = (CollisionObject3D)collider;
                if (collisionObject.IsInGroup("Enemy"))
                {
                    Enemy enemy = (Enemy)collisionObject;
                    if (enemy != null)
                        enemy.ReduceHealth(20);
                }
            }
        }
    }

}
