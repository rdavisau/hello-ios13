﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ARKit;
using ARKitMeetup.Demos.D1;
using Foundation;
using SceneKit;
using UIKit;

namespace ARKitMeetup.Helpers
{
    public class PlaneTrackingViewController : BaseARViewController
    {
        public UIImageView PlaneTrackingButton { get; set; }

        public virtual bool PlaySoundOnNodeDetection() => true;
        public bool PlaneTrackingEnabled = true;

        public List<SCNNode> Planes { get; set; } = new List<SCNNode>();

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AddUI();
        }

        public override void OnNodeAddedForAnchor(ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor)
        {
            if (!(anchor is ARPlaneAnchor planeAnchor))
                return;

            var color = planeAnchor.Alignment == ARPlaneAnchorAlignment.Horizontal
                ? UIColor.Blue : UIColor.Red;

            var planeNode = CreateARPlaneNode(planeAnchor, color.ColorWithAlpha(.5f));
            node.AddChildNode(planeNode);

            Planes.Add(planeNode);
        }

        public override void OnNodeUpdatedForAnchor(ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor)
        {
            if (!(anchor is ARPlaneAnchor planeAnchor))
                return;

            UpdateARPlaneNode(node.ChildNodes[0], planeAnchor);
        }

        public override void OnNodeRemovedForAnchor(ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor)
        {
            if (!(anchor is ARPlaneAnchor planeAnchor))
                return;
        }

        public virtual void UpdateARPlaneNode(SCNNode node, ARPlaneAnchor anchor)
        {
            Console.WriteLine($"UPDATE: {anchor.Alignment}, {anchor.Extent}");
            var geo = node.Geometry as SCNPlane;
            geo.Width = anchor.Extent.X;
            geo.Height = anchor.Extent.Z;

            node.Position = new SCNVector3(anchor.Center.X, 0, anchor.Center.Z);
            node.PhysicsBody = null;
            node.PhysicsBody = CreatePlanePhysics(geo);
        }

        public virtual SCNNode CreateARPlaneNode(ARPlaneAnchor anchor, UIColor color)
        {
            Console.WriteLine($"ADD: {anchor.Alignment}, {anchor.Extent}");

            var material = new SCNMaterial();
            material.Diffuse.Contents = color;

            var geometry = new SCNPlane
            {
                Width = anchor.Extent.X,
                Height = anchor.Extent.Z,
                Materials = new[] { material, material, material, material },
            };

            var rotation = SCNMatrix4.Identity;
            SCNMatrix4.CreateRotationX((float)-Math.PI / 2.0f, out rotation);

            var planeNode = new SCNNode
            {
                Geometry = geometry,
                Position = new SCNVector3(anchor.Center.X, 0, anchor.Center.Z),
                Transform = rotation,
                PhysicsBody = CreatePlanePhysics(geometry)
            };

            return planeNode;
        }

        public virtual SCNPhysicsBody CreatePlanePhysics(SCNGeometry geometry)
        {
            var body = SCNPhysicsBody.CreateStaticBody();
            body.PhysicsShape = SCNPhysicsShape.Create(geometry, new NSDictionary());
            body.Restitution = 0.5f;
            body.Friction = 0.5f;

            return body;
        }

        private void AddUI()
        {
            PlaneTrackingButton = new UIImageView { UserInteractionEnabled = true, TranslatesAutoresizingMaskIntoConstraints = false };
            PlaneTrackingButton.Image = UIImage.FromBundle("sprite238.gif");

            View.AddSubview(PlaneTrackingButton);
            View.AddConstraints(new[] {
              NSLayoutConstraint.Create(PlaneTrackingButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View, NSLayoutAttribute.Leading, 1, 20),
              NSLayoutConstraint.Create(PlaneTrackingButton, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1, -20),
            });

            PlaneTrackingButton.AddGestureRecognizer(new UITapGestureRecognizer(gr =>
            {
                var cfg = SCNView.Session.Configuration as ARWorldTrackingConfiguration;
                if (cfg == null)
                    return; // non arkit;

                var willBeEnabled = cfg.PlaneDetection == ARPlaneDetection.None;
                cfg.PlaneDetection = willBeEnabled
                ? ARPlaneDetection.Horizontal | ARPlaneDetection.Vertical
                : ARPlaneDetection.None;

                SCNView.Session.Run(cfg);
                PlaneTrackingEnabled = willBeEnabled;
                UIView.Animate(.2, () => PlaneTrackingButton.Alpha = willBeEnabled ? 1 : .5f);
            }));

            PlaneTrackingButton.AddGestureRecognizer(new UILongPressGestureRecognizer(gr =>
            {
                if (gr.State != UIGestureRecognizerState.Began)
                    return;

                foreach (var node in Planes)
                    node.Opacity = node.Opacity == 0 ? 1 : 0;
            }));
        }
    }
}