using System.Linq;
using System.Collections.Generic;
using ARKit;
using ARKitMeetup.Helpers;
using ARKitMeetup.Models;
using Foundation;
using SceneKit;
using UIKit;
using System.Threading.Tasks;
using System;
using CoreFoundation;

namespace HelloiOS13.D3.D3
{
    [DisplayInMenu(DisplayName = "Multi-Camera Face Tracking", DisplayDescription = "Demonstrates multi-camera tracking - world tracking + face tracking")]
    public class ARWorldFaceTrackingViewController : BetterPlaneTrackingViewController
    {
        public List<FaceNode> Faces { get; private set; } = new List<FaceNode>();

        public override ARConfiguration GetARConfiguration()
        {
            var config = base.GetARConfiguration() as ARWorldTrackingConfiguration;
            config.UserFaceTrackingEnabled = true;

            return config;
        }

        public override SCNScene GetInitialScene()
        {
            var scene = base.GetInitialScene();

            scene.PhysicsWorld.TimeStep = 1.0f / 180.0f;

            return scene;
        }

        public override SCNDebugOptions GetDebugOptions()
            => SCNDebugOptions.ShowBoundingBoxes | SCNDebugOptions.ShowWireframe | ARSCNDebugOptions.ShowFeaturePoints | ARSCNDebugOptions.ShowWorldOrigin
                | SCNDebugOptions.ShowPhysicsShapes | SCNDebugOptions.ShowPhysicsFields;

        public override void OnNodeAddedForAnchor(
            ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor)
        {
            base.OnNodeAddedForAnchor(renderer, node, anchor);

            if (anchor is ARPlaneAnchor planeAnchor)
            {
                var plane = node.ChildNodes.First();
                plane.Geometry.FirstMaterial.Diffuse.Contents = ShaderScene.Random();
                plane.Geometry.TileTexture(2);
            }

            if (anchor is ARFaceAnchor faceAnchor)
            {
                foreach (var child in node.ChildNodes)
                    child.RemoveFromParentNode();

                AddFaceToNode(node);
            }
        }

        public void AddFace()
        {
            var face = new FaceNode(ARSCNFaceGeometry.Create(SCNView.Device));
            Faces.Add(face);

            SCNView.Scene.RootNode.Add(face);

            face.Position =
               SCNView.PointOfView.ConvertPositionToNode(new SCNVector3(0, 0, -1), SCNView.Scene.RootNode);

            var hover = 0.05f;
            face.RunAction(SCNAction.RepeatActionForever(
                SCNAction.Sequence(new[] { SCNAction.MoveBy(0, hover, 0, 2).Ease(SCNActionTimingMode.EaseInEaseOut), SCNAction.MoveBy(0, -hover, 0, 2).Ease(SCNActionTimingMode.EaseInEaseOut) })));
        }

        public void AddFaceToNode(SCNNode node)
        {
            var face = new FaceNode(ARSCNFaceGeometry.Create(SCNView.Device));
            Faces.Add(face);

            SCNView.Scene.RootNode.Add(face);

            face.Position =
               SCNView.PointOfView.ConvertPositionToNode(new SCNVector3(0, 0, -1), SCNView.Scene.RootNode);

        }

        public override void OnNodeUpdatedForAnchor(ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor)
        {
            base.OnNodeUpdatedForAnchor(renderer, node, anchor);

            if (!(anchor is ARFaceAnchor faceAnchor))
                return;

            Console.WriteLine(node);

            try
            {
                foreach (var thing in Faces.OfType<FaceNode>())
                {
                    thing.Update(faceAnchor.Geometry);

                    thing.EulerAngles = node.EulerAngles;
                    thing.RotateBy(0, 0, (float)Math.PI, 0);


                }
            } catch { }

            var blendShapes = faceAnchor.BlendShapes;

            // get the five strongest indicators to display
            var dominantParts = blendShapes.Dictionary
                .GroupBy(x => $"{x.Key}".Split('_')[0], x => (NSNumber)x.Value)
                .Select(x => new { Expression = x.Key, Value = x.Max(y => y.FloatValue) }) // take strongest of left or right when present
                .OrderByDescending(x => x.Value)
                .Select(x => $"{x.Expression}: {x.Value:P0}")
                .Take(5)
                .ToList();

            NaivelyAndIneffecientlyCheckForChangedExpression(node, blendShapes);
        }

        private void NaivelyAndIneffecientlyCheckForChangedExpression(SCNNode node, ARBlendShapeLocationOptions blendShapes)
        {
            if (blendShapes.BrowInnerUp > .6)
                BounceStuff(blendShapes.BrowInnerUp.Value * 3);
        }

        int i = 0;

        public void BounceStuff(float multiplier)
        {
            if (i++ % 4 != 0)
                return;

            foreach (var node in Faces)
            {
                foreach (var i in Enumerable.Range(0, 1))
                {
                    var box = GetBox();

                    box.Position = new SCNVector3(0, .15f, 0);

                    var vec = new SCNVector3(((float)r.NextDouble() - .5f) * multiplier, ((float)r.NextDouble() + (float)2) * multiplier, 0);
                    box.PhysicsBody.ApplyForce(vec, true);
                    node.Add(box);

                    var newPosition = node.ConvertPositionToNode(box.Position, SCNView.Scene.RootNode);
                    SCNView.Scene.RootNode.Add(box);
                    box.Position = newPosition;
                }
            }
        }

        private Random r = new Random();
        private SCNNode _prefab;
        public SCNNode GetBox()
        {
            if (_prefab != null)
                return _prefab.Clone();

            var g = new SCNBox { Height = .066f, Width = .066f, Length = .066f };
            g.FirstMaterial.Diffuse.Contents = UIImage.FromFile("xamagon-fill");

            var box = new SCNNode { Geometry = g };
            box.PhysicsBody = SCNPhysicsBody.CreateBody(SCNPhysicsBodyType.Dynamic, SCNPhysicsShape.Create(g, new SCNPhysicsShapeOptions { ShapeType = SCNPhysicsShapeType.BoundingBox }));
            box.PhysicsBody.ContinuousCollisionDetectionThreshold = g.Width * 2;

            _prefab = box;

            return _prefab.Clone();
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            AddFace();
        }

        public class FaceNode : SCNNode
        {
            public FaceNode(ARSCNFaceGeometry faceGeometry)
            {
                var mat = faceGeometry.FirstMaterial;
                mat.LightingModelName = SCNLightingModel.PhysicallyBased;
                mat.Diffuse.Contents = UIImage.FromFile("tile-small.png");
                mat.Diffuse.ContentsTransform = SCNMatrix4.Scale(32, 32, 0);
                mat.Diffuse.WrapS = SCNWrapMode.Repeat;
                mat.Diffuse.WrapT = SCNWrapMode.Repeat;

                Geometry = faceGeometry;
            }

            public void Update(ARFaceGeometry newGeometry)
            {
                ((ARSCNFaceGeometry)Geometry).Update(newGeometry);
            }
        }

    }
}
