using System.Linq;
using UIKit;

namespace HelloiOS13
{
    public class MultiWindowGetter
    {
        public UIUserInterfaceStyle OverrideUserInterfaceStyle
        {
            set
            {
                foreach (var w in
                    UIApplication
                        .SharedApplication
                        .ConnectedScenes
                        .OfType<UIWindowScene>()
                        .SelectMany(x => x.Windows))

                    w.OverrideUserInterfaceStyle = value;
            }
        }
    }
}

