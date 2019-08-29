using System.Collections.Generic;
using ARKitMeetup.Models;
using UIKit;

namespace HelloiOS13.D1.D1
{
    [DisplayInMenu(DisplayName = "Basic Text Colour", DisplayDescription = "Demonstrates which colours do and don't respond to theme changes")]
    public class BasicTextColoursViewController : BaseListViewController<UILabel>
    {
        public override List<UILabel> Items => new List<UILabel>
        {
            Label("No text colour specified"),
            Label("Black text colour specified", UIColor.Black),
            Label("UIColor.LabelColor specified", UIColor.LabelColor),
            Label("UIColor.PlaceholderTextColor specified", UIColor.PlaceholderTextColor),
            Label("UIColor.SystemGray4Color specified", UIColor.SystemGray5Color),
        };

        public UILabel Label(string text, UIColor colour = null)
        {
            var l = new UILabel { Text = text, Font = UIFont.SystemFontOfSize(20) };
            if (colour != null)
                l.TextColor = colour;

            return l;
        }
    }
}

