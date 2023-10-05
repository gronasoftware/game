using Godot;
using System;

public partial class InstantProjectile : Node3D, IProjectile
{
    public int damage { get; set; } = 10;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("Created projectile");
        // Send a raycast from the direction the player is facing
        var spaceState = GetWorld3D().DirectSpaceState; // Can't use GetWorld3D in a singleton not attached to the scene tree
        var query = PhysicsRayQueryParameters3D.Create(
            Transform.Origin + -Transform.Basis.Z * 0.5f,
            Transform.Origin + -Transform.Basis.Z * 1000
        );
        var result = spaceState.IntersectRay(query);

        if (result.Count > 0)
        {
            // Draw small sphere at hitPosition
            var hitPosition = (Vector3)result["position"];
            //GD.Print(hitPosition);
            DrawProjectile(Transform.Origin, hitPosition, "sphere");

            var collider = result["collider"].AsGodotObject();

            if (collider is ICombatant combatant)
            {
                combatant.Hit(this);
                QueueFree();
            }
        }
    }

    public void DrawProjectile(Vector3 source, Vector3 target, string type = "laser") {
        if (type == "sphere") {
            var sphere = new SphereMesh();
            sphere.Radius = 0.1f;
            sphere.Height = 0.1f;
            var sphereInstance = new MeshInstance3D();
            sphereInstance.Mesh = sphere;
            sphereInstance.Position = target;
            GetTree().Root.AddChild(sphereInstance);
        }
        else if (type == "laser") {
            //var line = new Line3D();
            //line.From = source;
            //line.To = target;
            //var lineInstance = new Line3D();
            //lineInstance.AddLine(line);
            //GetTree().Root.AddChild(lineInstance);
        }
    }
}
