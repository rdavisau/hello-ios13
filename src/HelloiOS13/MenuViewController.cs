﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ARKitMeetup.Helpers;
using ARKitMeetup.Models;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using MobileCoreServices;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using MenuItem = ARKitMeetup.Models.MenuItem;

namespace HelloiOS13
{
    public class MenuViewController : BaseViewController, IUIDragInteractionDelegate, IUIAdaptivePresentationControllerDelegate
    {
        const int MenuViewCapInset = 20;
        const int MenuViewPadding = 10;

        public UIView BackgroundView { get; set; }
        public UIView HeaderView { get; set; }
        public UIView HeaderLabel { get; set; }
        public UILabel StartLabel { get; private set; }
        public UISegmentedControl PresentationKindSegmentedControl { get; private set; }
        public UIView MenuView { get; set; }
        public UIView MenuContainerImageView { get; set; }
        public UITableView MenuTableView { get; private set; }

        public List<MenuItem> Scenes { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            SetupViews();
            DetectScenes();
            SetupTableView();

            View.UserInteractionEnabled = true;
            HeaderView.UserInteractionEnabled = true;
            HeaderLabel.UserInteractionEnabled = true;

            HeaderView.AddInteraction(
                new UIDragInteraction(this) { Enabled = true, });  
        }

        private void DetectScenes()
        {
            Scenes =
                Enumerable.Concat(typeof(Application).Assembly.GetTypes(), this.GetType().Assembly.GetTypes())
                .Distinct()
                .Where(x => x.IsSubclassOf(typeof(UIViewController)) || x.IsSubclassOf(typeof(Page)))
                .Where(x => !x.IsAbstract)
                .OrderBy(x => x.Namespace)
                .Select(x =>
                {
                    var att = x.GetCustomAttribute<DisplayInMenuAttribute>();
                    if (att == null)
                        return null;

                    return new MenuItem
                    {
                        Title = att.DisplayName,
                        Description = att.DisplayDescription,
                        Type = x,
                    };
                })
                .Where(x => x != null)
                .ToList();
        }

        private void SetupTableView()
        {
            var selectedIndexPath = NSIndexPath.FromItemSection(0, 0);
            MenuTableView.Source = new InlineTableViewSourceWithoutRowHeight
            { 
                _RowsInSection = (tv, section) => Scenes.Count,
                _GetCell = (tv, indexPath) =>
                {
                    var scene = Scenes[indexPath.Row];

                    var cell = new UITableViewCell(UITableViewCellStyle.Subtitle, "abc") { BackgroundColor = UIColor.Clear };

                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                    cell.TextLabel.Text = scene.Title;
                    cell.TextLabel.Lines = 0;
                    cell.TextLabel.TextColor = UIColor.White;
                    cell.TextLabel.Font = UIFont.SystemFontOfSize(36);

                    cell.DetailTextLabel.Text = scene.Description;
                    cell.DetailTextLabel.TextColor = UIColor.White.ColorWithAlpha(.75f);
                    cell.DetailTextLabel.Font = UIFont.SystemFontOfSize(20);
                    cell.DetailTextLabel.Lines = 0;

                    cell.SeparatorInset = UIEdgeInsets.Zero;

                    cell.BackgroundColor = UIColor.Black.ColorWithAlpha(.35f);

                    cell.ImageView.Image = selectedIndexPath == indexPath
                        ? UIImage.FromFile("selection.png")
                        : UIImage.FromFile("noselection.png");
                    cell.ImageView.Transform = CGAffineTransform.MakeTranslation(-4, 0);

                    return cell;
                },
                _RowSelected = (tv, indexPath) =>
                {
                    var behaviour = (PresentationBehaviour)(int)PresentationKindSegmentedControl.SelectedSegment;

                    switch (behaviour)
                    {
                        case PresentationBehaviour.Present:

                            TransitionToScene(indexPath.Row);
                            break;

                        case PresentationBehaviour.New:
                        case PresentationBehaviour.Reuse:

                            var scene = Scenes[indexPath.Row];
                            var userInfo = new[] { "openScene", scene.Type.AssemblyQualifiedName }
                                .ToNSDictionary();

                            var userActivity =
                                new NSUserActivity(scene.Type.AssemblyQualifiedName)
                                {
                                    UserInfo = userInfo
                                };

                            UIApplication.SharedApplication.RequestSceneSessionActivation(
                                sceneSession: behaviour == PresentationBehaviour.Reuse
                                                ? SceneDelegate.DetailSceneSession
                                                : null,
                                userActivity: userActivity,
                                options: null,
                                errorHandler: null
                            );

                            break;
                    }
                }
            };

            MenuTableView.DragDelegate = new InlineUITableViewDragDelegate
            {
                _GetItemsForBeginningDragSession = (tv, session, indexPath) =>
                {
                    var scene = Scenes[indexPath.Row];
                    var userActivity = new NSUserActivity("IOS13DemoScene") { Title = scene.Type.AssemblyQualifiedName };
                    var itemProvider = new NSItemProvider(userActivity);
                    
                    itemProvider.RegisterObject(userActivity,
                        NSItemProviderRepresentationVisibility.All);
                    
                    return new[] { new UIDragItem(itemProvider) { LocalObject = userActivity } }; 
                },

                _DragSessionIsRestrictedToDraggingApplication = (tv, session) => false
            };
        }

        private void TransitionToScene(int row)
        {
            var scene = Scenes[row];

            var ui = Activator.CreateInstance(scene.Type);
            if (ui is ContentPage cp)
                ui = cp.CreateViewController();

            var vc = ui as UIViewController;
            vc.PresentationController.Delegate = this;

            PresentViewController(vc, true, null);

            ReplSceneDelegate.Instance?.ReplPage?.SetContext(ui);
        }

        bool firstAppearance = true;
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ReplSceneDelegate.Instance?.SetActiveScreen(this);

            if (!animating)
            {
                BackgroundView.Transform = CGAffineTransform.MakeIdentity();
                AnimateBackground();
            }

            StartLabel.Alpha = 1;
            UIView.Animate(1, 0, UIViewAnimationOptions.Autoreverse | UIViewAnimationOptions.Repeat | UIViewAnimationOptions.CurveEaseIn,
                () => StartLabel.Alpha = 0, null);
        }

        protected virtual void SetupViews()
        {
            View.BackgroundColor = UIColor.Black;
            BackgroundView = new UIView { TranslatesAutoresizingMaskIntoConstraints = false };
            BackgroundView.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("light.png"));
            HeaderView = new UIView { TranslatesAutoresizingMaskIntoConstraints = false, };
            HeaderLabel = new UILabel
            {
                Font = UIFont.SystemFontOfSize(80),
                Text = "iOS 13",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                TextColor = UIColor.White,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            
            HeaderLabel.Layer.ShadowOffset = CGSize.Empty;
            HeaderLabel.Layer.ShadowColor = UIColor.Black.CGColor;
            HeaderLabel.Layer.ShadowOpacity = .75f;
            
            StartLabel = new UILabel
            {
                Text = "OUT NOW",
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center,
                Font = UIFont.SystemFontOfSize(24),
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            StartLabel.Layer.ShadowOffset = CGSize.Empty;
            StartLabel.Layer.ShadowColor = UIColor.Black.CGColor;
            StartLabel.Layer.ShadowOpacity = .75f;

            PresentationKindSegmentedControl = new UISegmentedControl("present", "new", "reuse") 
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                TintColor = UIColor.White,
                Transform = CGAffineTransform.MakeScale(.85f, .85f),
                SelectedSegment = 0,
            };

            MenuView = new UIView { TranslatesAutoresizingMaskIntoConstraints = false };
            MenuContainerImageView = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Alpha = 0,
            };

            MenuTableView = new UITableView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine,
                SeparatorColor = UIColor.White.ColorWithAlpha(.1f),
                BackgroundColor = UIColor.Clear,
                TableFooterView = new UIView { }
            };

            HeaderView.AddSubviews(HeaderLabel, StartLabel, PresentationKindSegmentedControl); 
            MenuView.AddSubviews(MenuContainerImageView);
            MenuView.InsertSubviewAbove(MenuTableView, MenuContainerImageView);
            View.AddSubviews(BackgroundView, HeaderView, MenuView);

            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(BackgroundView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View, NSLayoutAttribute.Leading, 100, -100),
                NSLayoutConstraint.Create(BackgroundView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, View, NSLayoutAttribute.Trailing, 100, 100),
                NSLayoutConstraint.Create(BackgroundView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 100, 100),
                NSLayoutConstraint.Create(BackgroundView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 100, -100),

                NSLayoutConstraint.Create(HeaderLabel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, HeaderView, NSLayoutAttribute.CenterY, 1, 5),
                NSLayoutConstraint.Create(HeaderLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, HeaderView, NSLayoutAttribute.CenterX, 1, 0),
                NSLayoutConstraint.Create(HeaderLabel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, HeaderView, NSLayoutAttribute.Leading, 1, -10),
                NSLayoutConstraint.Create(HeaderLabel, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, HeaderView, NSLayoutAttribute.Trailing, 1, 10),

                NSLayoutConstraint.Create(StartLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, HeaderLabel, NSLayoutAttribute.Bottom, 1, -10),
                NSLayoutConstraint.Create(StartLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, HeaderView, NSLayoutAttribute.CenterX, 1, 0),
                NSLayoutConstraint.Create(StartLabel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, HeaderView, NSLayoutAttribute.Leading, 1, -10),
                NSLayoutConstraint.Create(StartLabel, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, HeaderView, NSLayoutAttribute.Trailing, 1, 10),


                NSLayoutConstraint.Create(PresentationKindSegmentedControl, NSLayoutAttribute.Top, NSLayoutRelation.Equal, StartLabel, NSLayoutAttribute.Bottom, 1, 0),
                NSLayoutConstraint.Create(PresentationKindSegmentedControl, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, StartLabel, NSLayoutAttribute.CenterX, 1, 0),

                NSLayoutConstraint.Create(HeaderView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View, NSLayoutAttribute.Leading, 1, 0),
                NSLayoutConstraint.Create(HeaderView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, View, NSLayoutAttribute.Trailing, 1, 0),
                NSLayoutConstraint.Create(HeaderView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1, 0),
                NSLayoutConstraint.Create(HeaderView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 180),

                NSLayoutConstraint.Create(MenuView, NSLayoutAttribute.Leading, NSLayoutRelation.GreaterThanOrEqual, View, NSLayoutAttribute.Leading, 1, 0),
                NSLayoutConstraint.Create(MenuView, NSLayoutAttribute.Trailing, NSLayoutRelation.GreaterThanOrEqual, View, NSLayoutAttribute.Trailing, 1, 0),
                NSLayoutConstraint.Create(MenuView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1, -15),
                NSLayoutConstraint.Create(MenuView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, PresentationKindSegmentedControl, NSLayoutAttribute.Bottom, 1, 0),
                NSLayoutConstraint.Create(MenuView, NSLayoutAttribute.Width, NSLayoutRelation.LessThanOrEqual, 1, 600),
                NSLayoutConstraint.Create(MenuView, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1, 0),

                NSLayoutConstraint.Create(MenuContainerImageView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, MenuView, NSLayoutAttribute.Leading, 1, 0),
                NSLayoutConstraint.Create(MenuContainerImageView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, MenuView, NSLayoutAttribute.Trailing, 1, -0),
                NSLayoutConstraint.Create(MenuContainerImageView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, MenuView, NSLayoutAttribute.Bottom, 1, -0),
                NSLayoutConstraint.Create(MenuContainerImageView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, MenuView, NSLayoutAttribute.Top, 1, 0),

                NSLayoutConstraint.Create(MenuTableView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, MenuContainerImageView, NSLayoutAttribute.Leading, 1, MenuViewPadding),
                NSLayoutConstraint.Create(MenuTableView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, MenuContainerImageView, NSLayoutAttribute.Trailing, 1, -MenuViewPadding),
                NSLayoutConstraint.Create(MenuTableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, MenuContainerImageView, NSLayoutAttribute.Bottom, 1, 20 ),
                NSLayoutConstraint.Create(MenuTableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, MenuContainerImageView, NSLayoutAttribute.Top, 1, MenuViewPadding/ 2 + 2),
            });
        }

        bool animating = false;
        protected virtual void AnimateBackground()
        {
            animating = true;
            // this is so bad but it works for long enough to make it through a meetup presentation
            // i'll be our little secret ok? 
            Task.Run(async () =>
            {
                var duration = 3500;
                while (true)
                {
                    DispatchQueue.MainQueue.DispatchAsync(() => UIView.Animate(duration, 0, UIViewAnimationOptions.CurveLinear, () => BackgroundView.Transform = CGAffineTransform.MakeTranslation(-20000, -20000), null));
                    await Task.Delay(TimeSpan.FromSeconds(duration));
                    BackgroundView.Transform = CGAffineTransform.MakeIdentity();
                }
            });
        }

        public UIDragItem[] GetItemsForBeginningSession(UIDragInteraction interaction, IUIDragSession session)
        {
            Console.WriteLine(interaction);
            Console.WriteLine(session);

            var scene = Scenes[1];
            var activity = new NSUserActivity("IOS13DemoScene") { Title = scene.Type.AssemblyQualifiedName };
            var itemProvider = new NSItemProvider(activity);

            itemProvider.RegisterObject(activity, NSItemProviderRepresentationVisibility.All);

            return new[] { new UIDragItem(itemProvider) { LocalObject = activity } };
        }

        [Export("presentationControllerDidDismiss:")]
        public void DidDismiss(UIPresentationController presentationController)
        {
            ReplSceneDelegate.Instance?.SetActiveScreen(this);
        }
    }

    public enum PresentationBehaviour
    {
        Present = 0,
        New = 1,
        Reuse = 2,
    }
}

