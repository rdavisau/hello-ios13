using System.Linq;
using HelloiOS13.Helpers.REPL;
using UIKit;

namespace HelloiOS13
{
    public class BaseViewController : UIViewController
    {
        public BaseViewController()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (IsBeingPresented && !(this is MenuViewController))
                ReplSceneDelegate.Instance?.SetActiveScene(GetScene);
        }

        public UIScene GetScene
            => GetWindowScene().scene;

        public UIWindow GetWindow()
            => GetWindowScene().window;

        public (UIScene scene, UIWindow window, UIViewController[]) GetWindowScene()
        {
            var allVcs =
                UIApplication
                    .SharedApplication
                    .ConnectedScenes.OfType<UIWindowScene>()
                    .SelectMany(s => s.Windows,
                        (s, w) => (s,  w, new UIViewController[] { w.RootViewController, w.RootViewController?.PresentedViewController }))
                    .ToList();

            return allVcs.FirstOrDefault(x => x.Item3.Contains(this));
        }
    }
}

