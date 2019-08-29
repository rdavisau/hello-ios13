using System.Linq;
using ARKit;
using ARKitMeetup.Helpers;
using ARKitMeetup.Models;
using AVFoundation;
using CoreImage;
using EventKit;
using Foundation;
using SceneKit;
using UIKit;

namespace HelloiOS13.D3.D2
{
    [DisplayInMenu(DisplayName = "AR Segmentation", DisplayDescription = "Demonstrates detection of people and proper occlusion with virtual content")]
    public class ARPersonSegmentationViewController : PlaneTrackingViewController
    {
        const string SegmentationEnabledText = "SEGMENTATION\r\nENABLED";
        const string SegmentationDisabledText = "SEGMENTATION\r\nDISABLED";

        public bool SegmentationEnabled = false;

        UIImageView DepthImageView = new UIImageView { TranslatesAutoresizingMaskIntoConstraints = false, Alpha = 0 };
        UIImageView SegImageView = new UIImageView { TranslatesAutoresizingMaskIntoConstraints = false, Alpha = 0 };
        UISwitch SegmentationToggle = new UISwitch { TranslatesAutoresizingMaskIntoConstraints = false, };
        UILabel SegmentationLabel = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Font = UIFont.BoldSystemFontOfSize(12),
            TextColor = UIColor.SystemRedColor,
            BackgroundColor = UIColor.Black.ColorWithAlpha(.5f),
            Text = SegmentationDisabledText,
            TextAlignment = UITextAlignment.Center,
            Lines = 0
        };

        public override void OnSessionBegan()
        {
            base.OnSessionBegan();

            var (w, h) = (256, 192);

            View.AddSubviews(DepthImageView, SegImageView, SegmentationToggle, SegmentationLabel);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(DepthImageView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.TopMargin, 1, 40),
                NSLayoutConstraint.Create(DepthImageView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View, NSLayoutAttribute.LeadingMargin, 1, 0),
                NSLayoutConstraint.Create(DepthImageView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, h),
                NSLayoutConstraint.Create(DepthImageView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1, w),

                NSLayoutConstraint.Create(SegImageView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.TopMargin, 1, 40),
                NSLayoutConstraint.Create(SegImageView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, View, NSLayoutAttribute.TrailingMargin, 1, 0),
                NSLayoutConstraint.Create(SegImageView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, h),
                NSLayoutConstraint.Create(SegImageView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1, w),

                NSLayoutConstraint.Create(SegmentationToggle, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.TopMargin, 1, 40),
                NSLayoutConstraint.Create(SegmentationToggle, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1, 0),

                NSLayoutConstraint.Create(SegmentationLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, SegmentationToggle, NSLayoutAttribute.Bottom, 1, 10),
                NSLayoutConstraint.Create(SegmentationLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1, 0),
            });

            DepthImageView.Layer.CornerRadius = 10f;
            SegImageView.Layer.CornerRadius = 10f;

            SegmentationToggle.ValueChanged += SegmentationToggle_ValueChanged;
        }

        private async void SegmentationToggle_ValueChanged(object sender, System.EventArgs e)
        {
            SegmentationEnabled = !SegmentationEnabled;

            UIView.Animate(.25f, () =>
            {
                DepthImageView.Alpha = SegmentationEnabled ? .5f : 0;
                SegImageView.Alpha = SegmentationEnabled ? .5f : 0;
            });

            SegmentationLabel.TextColor =
                SegmentationEnabled
                ? UIColor.SystemGreenColor
                : UIColor.SystemRedColor;

            SegmentationLabel.Text =
                SegmentationEnabled
                ? SegmentationEnabledText
                : SegmentationDisabledText;
            
            SCNView.Session.Run(GetARConfiguration());
        }

        public override ARConfiguration GetARConfiguration()
            => new ARWorldTrackingConfiguration
            {
                PlaneDetection = ARPlaneDetection.Horizontal | ARPlaneDetection.Vertical,
                FrameSemantics =
                    SegmentationEnabled
                    ? ARFrameSemantics.PersonSegmentationWithDepth
                    : ARFrameSemantics.None
            };
                          
        public override void OnNodeAddedForAnchor(ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor)
        {
            base.OnNodeAddedForAnchor(renderer, node, anchor);

            var plane = node.ChildNodes.First();
            plane.Geometry.FirstMaterial.Diffuse.Contents = ShaderScene.Random();
            plane.Geometry.TileTexture(2);
        }
        
        public SCNNode SpawnBox(ARRaycastResult raycast)
        {
            var geom = new SCNBox { Width = .15f, Height = .15f, Length = .15f };
            geom.FirstMaterial.Diffuse.Contents = UIColor.Black.ColorWithAlpha(.75f);
            geom.FirstMaterial.DoubleSided = true;

            var box = new SCNNode { Geometry = geom, Transform = raycast.WorldTransform.ToSCNMatrix4() };

            SCNView.Scene.RootNode.Add(box);

            return box;
        }

        public override void OnFrameUpdate(ARSession session, ARFrame frame)
        {
            base.OnFrameUpdate(session, frame);

            if (frame.EstimatedDepthData == null || frame.SegmentationBuffer == null)
                return;

            var depth = UIImage.FromImage(new CIImage(frame.EstimatedDepthData));
            var seg = UIImage.FromImage(new CIImage(frame.SegmentationBuffer));

            DepthImageView.Image = depth;
            SegImageView.Image = seg;
        }
    }
}
