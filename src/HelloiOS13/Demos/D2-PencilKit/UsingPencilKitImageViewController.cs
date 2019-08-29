using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ARKitMeetup.Helpers;
using ARKitMeetup.Models;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using HelloiOS13.Demos.D2PencilKit;
using PencilKit;
using UIKit;

namespace HelloiOS13.D2.D2
{
    [DisplayInMenu(DisplayName = "Using PencilKit Outputs", DisplayDescription = "Draw a thing real easy and do something with it")]
    public class UsingPencilKitImageViewController : UIViewController
    {
        public UIView BackgroundView { get; private set; }
        public TranslationKind Translation { get; set; }
        public PKEventingCanvasView Canvas { get; private set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            Canvas = new PKEventingCanvasView { BackgroundColor = UIColor.LightTextColor, TranslatesAutoresizingMaskIntoConstraints = false };
            View.AddSubview(Canvas);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(Canvas, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1, 0),
                NSLayoutConstraint.Create(Canvas, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.BottomMargin, 1, -50),
                NSLayoutConstraint.Create(Canvas, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1, 300),
                NSLayoutConstraint.Create(Canvas, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 200),
            });

            var tool = PKToolPicker.GetSharedToolPicker(UIApplication.SharedApplication.KeyWindow);
            tool.SetVisible(true, Canvas);
            tool.AddObserver(Canvas);

            Canvas.BecomeFirstResponder();
            Canvas.OnImage = img => BackgroundView.BackgroundColor = UIColor.FromPatternImage(img);

            CreateAndAnimateBackground();
        }

        protected void CreateAndAnimateBackground()
        {
            BackgroundView = new UIView();
            View.FillWith(BackgroundView, -1000000, -1000000);
            View.BringSubviewToFront(Canvas);

            var (x, y) = _translations[Translation];
            Translation = (TranslationKind)((int)(Translation + 1) % Enum.GetValues(typeof(TranslationKind)).Length);

            Task.Run(async () =>
            {
                var duration = 2500;
                while (true)
                {
                    DispatchQueue.MainQueue.DispatchAsync(() => UIView.Animate(duration, 0, UIViewAnimationOptions.CurveLinear | UIViewAnimationOptions.AllowUserInteraction, () => BackgroundView.Transform = CGAffineTransform.MakeTranslation(x, y), null));
                    await Task.Delay(TimeSpan.FromSeconds(duration));
                    BackgroundView.Transform = CGAffineTransform.MakeIdentity();
                }
            });
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            UIView.Animate(.2, () => Canvas.Alpha = 0);
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            CreateAndAnimateBackground();
            Canvas.Drawing = new PKDrawing();

            UIView.Animate(.2, () => Canvas.Alpha = 1);
        }

        private Dictionary<TranslationKind, (float x, float y)> _translations = new Dictionary<TranslationKind, (float x, float y)>
        {
            [TranslationKind.UpLeft] = (-20000, -20000),
            [TranslationKind.UpRight] = (20000, -20000),
            [TranslationKind.DownLeft] = (-20000, 20000),
            [TranslationKind.DownRight] = (20000, 20000),
        };

        public enum TranslationKind
        {
            UpLeft,
            UpRight,
            DownLeft,
            DownRight
        }
    }
}
