using System;
using ARKitMeetup;
using ARKitMeetup.Helpers;
using UIKit;
using Xamarin.Forms;

namespace HelloiOS13.Helpers.REPL
{
    public class ReplVC : UIViewController
    {
        public ReplPage ReplPage { get; private set; }

        public ReplVC()
        {
            ReplPage = new ReplPage(this);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.Clear;
            View.Opaque = false;

            var repl = ReplPage.CreateViewController();

            repl.View.TranslatesAutoresizingMaskIntoConstraints = false;
            repl.WillMoveToParentViewController(this);
            repl.ViewWillAppear(true);

            var ebd = new EBDialogViewController();
            View.FillWith(ebd.View, 0, -20);
            ebd.SetContent(repl.View);

            ebd.View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(repl.View, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ebd.View, NSLayoutAttribute.Top, 1, 40),
                NSLayoutConstraint.Create(repl.View, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ebd.View, NSLayoutAttribute.Bottom, 1, -40),
            });

            repl.ViewDidAppear(true);
            repl.DidMoveToParentViewController(this);
        }
    }
}
