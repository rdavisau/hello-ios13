using System.Collections.Generic;
using ARKitMeetup.Models;
using UIKit;

namespace HelloiOS13.D1.D3
{
    [DisplayInMenu(DisplayName = "Custom dynamic colours", DisplayDescription = "How to programmatically create your own dynamic colours")]
    public class CustomDynamicColoursViewController : BaseListViewController<UIColor>
    {
        public override List<UIColor> Items => new List<UIColor>
        {
            DynamicColour(UIColor.Blue, UIColor.Red),
            DynamicColour(UIColor.Yellow, UIColor.Green),
            DynamicColour(UIColor.SystemPinkColor, UIColor.SystemTealColor),
        };

        public UIColor DynamicColour(UIColor light, UIColor dark) =>
            new UIColor(traits =>
                traits.UserInterfaceStyle.HasFlag(UIUserInterfaceStyle.Light)
                ? light
                : dark
            );
    }
}

