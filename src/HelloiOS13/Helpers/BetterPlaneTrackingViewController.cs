using System;
using ARKit;
using SceneKit;
using UIKit;

namespace ARKitMeetup.Helpers
{
    public class BetterPlaneTrackingViewController : PlaneTrackingViewController
    {
        public override SCNDebugOptions GetDebugOptions()
            => base.GetDebugOptions() | SCNDebugOptions.ShowPhysicsShapes | SCNDebugOptions.ShowPhysicsFields;

        public override SCNScene GetInitialScene()
        {
            var scene = base.GetInitialScene();
            scene.PhysicsWorld.TimeStep = 1.0f / 180.0f;

            return scene;
        }

        public override SCNNode CreateARPlaneNode(ARPlaneAnchor anchor, UIColor color)
        {
            Console.WriteLine($"ADD: {anchor.Alignment}, {anchor.Extent}");

            var material = new SCNMaterial();
            material.Diffuse.Contents = color;

            var geometry = ARSCNPlaneGeometry.Create(SCNView.Device);
            geometry.FirstMaterial = material;
            geometry.Update(anchor.Geometry);

            var planeNode = new SCNNode
            {
                Geometry = geometry,
                Position = new SCNVector3(anchor.Center.X, -.015f, anchor.Center.Z),
                PhysicsBody = CreatePlanePhysics(geometry)
            };

            return planeNode;
        }

        public override void UpdateARPlaneNode(SCNNode node, ARPlaneAnchor anchor)
        {
            var geometry = ARSCNPlaneGeometry.Create(SCNView.Device);
            geometry.FirstMaterial = node.Geometry.FirstMaterial;
            geometry.Update(anchor.Geometry);

            node.Geometry = null;
            node.Geometry = geometry;
            node.PhysicsBody = CreatePlanePhysics(geometry);
        }

        public override SCNPhysicsBody CreatePlanePhysics(SCNGeometry geometry)
        {
            var body = SCNPhysicsBody.CreateStaticBody();
            body.PhysicsShape = SCNPhysicsShape.Create(geometry, new SCNPhysicsShapeOptions { ShapeType = SCNPhysicsShapeType.BoundingBox });
            body.Restitution = 0.5f;
            body.Friction = 0.5f;

            return body;
        }
    }
}