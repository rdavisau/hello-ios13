using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ARKitMeetup.Models;
using CoreFoundation;
using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace HelloiOS13.D1.D4
{
    [DisplayInMenu(DisplayName = "Advanced theme handling", DisplayDescription = "How perform custom work in response to theme changes")]
    public class CustomBehaviourViewController : BaseListViewController<UIView>
    {
        UIImageView ImageView = new UIImageView { ContentMode = UIViewContentMode.ScaleAspectFit };
        UIImageView MyFeelingsView = new UIImageView { ContentMode = UIViewContentMode.ScaleAspectFit };
        UILabel Label = new UILabel { Font = UIFont.SystemFontOfSize(22), TextAlignment = UITextAlignment.Center };

        public override List<UIView> Items => new List<UIView> { ImageView, MyFeelingsView, Label };

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

            UpdateUIForThemeChange();
            Animate();
        }

        public override void TraitCollectionDidChange(UITraitCollection prev)
        {
            base.TraitCollectionDidChange(prev);

            if (TraitCollection.HasDifferentColorAppearanceComparedTo(prev))
                UpdateUIForThemeChange();
        }

        public void UpdateUIForThemeChange()
        {
            UIView.Transition(View, .25, UIViewAnimationOptions.TransitionCrossDissolve, () =>
            {
                ImageView.Image = ImageForCurrentMode;
                MyFeelingsView.Image = MyFeelingsForCurrentMode;
                Label.Text = TextForCurrentMode;
                TableView.ReloadData();
            }, null);
        }

        public UIImage ImageForCurrentMode
            => IsDarkMode
            ? UIImage.FromFile("moon.png")
            : UIImage.FromFile("sun.png");

        public UIImage MyFeelingsForCurrentMode
            => IsDarkMode
            ? UIImage.FromFile("owook.png")
            : UIImage.FromFile("owoweird.png");

        public string TextForCurrentMode
            => IsDarkMode
            ? "thank u"
            : "excuse me turn that off";

        bool IsDarkMode => TraitCollection.UserInterfaceStyle.HasFlag(UIUserInterfaceStyle.Dark);

        Random Random = new Random();

        public void Animate()
        {
            // let's not get too hung up on this yeah
            Task.Run(async () =>
            {
                var duration = .1;
                var shudder = 2;
                while (!gone)
                {
                    DispatchQueue.MainQueue.DispatchAsync(() =>
                    {
                        if (IsDarkMode)
                            return;

                        UIView.Animate(duration, 0, UIViewAnimationOptions.Autoreverse, () => MyFeelingsView.Transform = CGAffineTransform.MakeTranslation(Random.Next(-shudder, shudder), Random.Next(-shudder, shudder)), null);
                        UIView.Animate(duration, 0, UIViewAnimationOptions.Autoreverse, () => Label.Transform = CGAffineTransform.MakeTranslation(Random.Next(-shudder, shudder), Random.Next(-shudder, shudder)), null);
                    });

                    await Task.Delay(TimeSpan.FromSeconds(duration * 2));
                }
            });
        }

        bool gone;
        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            gone = true;
        }
    }
}

