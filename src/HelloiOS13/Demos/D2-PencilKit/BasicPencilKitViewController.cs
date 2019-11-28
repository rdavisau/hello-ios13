using System.Linq;
using ARKitMeetup.Helpers;
using ARKitMeetup.Models;
using PencilKit;
using UIKit;

namespace HelloiOS13.D2.D1
{
    [DisplayInMenu(DisplayName = "Basic PencilKit", DisplayDescription = "Draw a thing real easy")]
    public class BasicPencilKitViewController : BaseViewController
    {
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            
            var canvas = new PKCanvasView();
            var window = GetWindow();

            var tool = PKToolPicker.GetSharedToolPicker(window);

            tool.SetVisible(true, canvas);
            tool.AddObserver(canvas);
            canvas.BecomeFirstResponder();

            View.FillWith(canvas);
        }
    } 
}
