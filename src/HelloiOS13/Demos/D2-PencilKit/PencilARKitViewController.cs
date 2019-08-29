using System;
using System.Threading.Tasks;
using ARKitMeetup.Demos.D1;
using ARKitMeetup.Helpers;
using ARKitMeetup.Models;
using CoreGraphics;
using Foundation;
using HelloiOS13.Demos.D2PencilKit;
using PencilKit;
using SceneKit;
using UIKit;

namespace HelloiOS13.D2.D3
{
    [DisplayInMenu(DisplayName = "Pencil Kit in the Real World 😲", DisplayDescription = "Spawn floating boxes, and then?")]
    public class PencilARKitViewController : BaseARViewController
    {
        public SCNNode Box { get; set; }
        public PKEventingCanvasView Canvas { get; private set; }
        public UIView EBDialogView { get; private set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

        private void SetupCanvas()
        {
            Canvas = new PKEventingCanvasView { TranslatesAutoresizingMaskIntoConstraints = false };
            Canvas.ContentSize = new CGSize(160, 160);

            View.AddSubview(Canvas);

            var tool = PKToolPicker.GetSharedToolPicker(UIApplication.SharedApplication.KeyWindow);

            tool.SetVisible(true, Canvas);
            tool.AddObserver(Canvas);

            var dialog = new EBDialogViewController();
            EBDialogView = dialog.View;
            View.AddSubview(dialog.View);

            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(dialog.View, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1, -20),
                NSLayoutConstraint.Create(dialog.View, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1, 0),
                NSLayoutConstraint.Create(dialog.View, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1, 220),
                NSLayoutConstraint.Create(dialog.View, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 220),
            });

            dialog.SetContent(Canvas);

            dialog.View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(Canvas, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 160)
            });

            dialog.View.Transform = CGAffineTransform.MakeTranslation(0, 800);
        }

        public SCNNode SpawnBox()
        {
            var pos = SCNView.PointOfView.ConvertPositionToNode(new SCNVector3(0.01f, -.1f, -.4f), SCNView.Scene.RootNode);

            var geom = new SCNBox { Width = .15f, Height = .15f, Length = .15f };
            geom.FirstMaterial.Diffuse.Contents = UIColor.Black.ColorWithAlpha(.75f);
            geom.FirstMaterial.DoubleSided = true;

            var box = new SCNNode
            {
                Geometry = geom,
                Position = pos
            };

            SCNView.Scene.Add(box);

            return box;
        }

        public async void SetDrawingTarget(SCNNode node)
        {
            UIView.Animate(.33, 0f, UIViewAnimationOptions.CurveEaseOut, () => EBDialogView.Transform = CGAffineTransform.MakeIdentity(), null);

            await Task.Delay(TimeSpan.FromSeconds(.4));

            Canvas.DrawOnto(node);
            Canvas.BecomeFirstResponder();
        }

        public override Task WaitForReady()
            => Task.CompletedTask;

        public override void OnSessionBegan()
        {
            base.OnSessionBegan();

            SetupCanvas();
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            Box = SpawnBox().RotateForever(.1);
            SetDrawingTarget(Box);

            Canvas.BecomeFirstResponder();
            Canvas.SuppressChanges = true;
            Canvas.Drawing = new PKDrawing();
            Canvas.SuppressChanges = false;

        }
    }
}
