using ARKitMeetup.Helpers;
using ARKitMeetup.Models;
using PencilKit;
using UIKit;

namespace HelloiOS13.D2.D1
{
    [DisplayInMenu(DisplayName = "Basic PencilKit", DisplayDescription = "Draw a thing real easy")]
    public class BasicPencilKitViewController : UIViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            var canvas = new PKCanvasView();
            var tool = PKToolPicker.GetSharedToolPicker(UIApplication.SharedApplication.KeyWindow);

            tool.SetVisible(true, canvas);
            tool.AddObserver(canvas);
            canvas.BecomeFirstResponder();

            View.FillWith(canvas);
        }
    }
}
