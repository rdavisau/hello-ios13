using System;
using System.Diagnostics;
using ARKit;
using ARKitMeetup.Helpers;
using ARKitMeetup.Models;
using UIKit;

namespace HelloiOS13.D3.D1
{
    [DisplayInMenu(DisplayName="AR Coaching Overlay", DisplayDescription="Automated interactive user guidance for AR experiences")]
    public class ARCoachingViewController : PlaneTrackingViewController
    {
        UIView BlurView;
        ARCoachingOverlayView Coach;
        UISegmentedControl GoalSelector;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            GoalSelector = new UISegmentedControl($"{ARCoachingGoal.Tracking}", $"{ARCoachingGoal.HorizontalPlane}", $"{ARCoachingGoal.VerticalPlane}")
            {
                TranslatesAutoresizingMaskIntoConstraints = false }
            ;

            Coach = new ARCoachingOverlayView
            {
                ActivatesAutomatically = true,
                Delegate = new CoachingDelegate(this),
                Session = SCNView.Session
            };

            BlurView = new UIView
            {
                BackgroundColor = UIColor.Black.ColorWithAlpha(.65f),
                Alpha = 0
            };

            SCNView.FillWith(BlurView);
            SCNView.FillWith(Coach, 200, 400);
            View.AddSubviews(GoalSelector);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(GoalSelector, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.TopMargin, 1, 10),
                NSLayoutConstraint.Create(GoalSelector, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1, 0)
            });

            GoalSelector.ValueChanged += GoalChanged;
        }

        private void GoalChanged(object sender, EventArgs e)
        {
            // reset tracking
            SCNView.Session.Run(GetARConfiguration(), ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors);

            // set new goal
            Coach.Goal = (ARCoachingGoal)(int)GoalSelector.SelectedSegment;
        }

        public void DimUserInterface(bool dim)
        {
            UIView.Animate(.5, () => BlurView.Alpha = dim ? 1 : 0);
        }

        public void ResetTracking()
        {
            SCNView.Session.Run(GetARConfiguration(), ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors);
        }

        public class CoachingDelegate : ARCoachingOverlayViewDelegate
        {
            readonly ARCoachingViewController Parent;

            public CoachingDelegate(ARCoachingViewController parent)
                => Parent = parent;

            public override void WillActivate(ARCoachingOverlayView _)
                => Parent.DimUserInterface(true);

            public override void DidDeactivate(ARCoachingOverlayView _)
                => Parent.DimUserInterface(false);

            public override void DidRequestSessionReset(ARCoachingOverlayView _)
                => Parent.ResetTracking();
        }

    }
}
