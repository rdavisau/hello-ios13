using ARKitMeetup;
using Foundation;
using HelloiOS13.Helpers.REPL;
using UIKit;

namespace HelloiOS13
{
    [Register("ReplSceneDelegate")]
    public class ReplSceneDelegate : UIWindowSceneDelegate
    {
        public static ReplSceneDelegate Instance { get; set; }
        public ReplPage ReplPage { get; private set; }

        public ReplSceneDelegate()
        {
            Instance = this;
        }

        public override UIWindow Window
        {
            get;
            set;
        }

        public static UISceneSession SceneSession;

        public override void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
        {
            Instance = this;

            // if this is being restored by debugger launch, get rid of it
            if (connectionOptions.UserActivities.AnyObject == null)
            {
                UIApplication.SharedApplication.RequestSceneSessionDestruction(session, new UISceneDestructionRequestOptions(), null);
                return;
            }

            SceneSession = session;

            var root = new ReplVC();
            ReplPage = root.ReplPage;

            Window = new UIWindow(scene as UIWindowScene)
            {
                RootViewController = root
            };

            Window.MakeKeyAndVisible();
        }

        internal void SetActiveScene(UIScene scene)
        {
            var rootVc = (scene as UIWindowScene).Windows[0]?.RootViewController;

            if (rootVc != null)
                ReplPage?.SetContext(rootVc);
        }

        internal void SetActiveScreen(UIViewController viewController)
        {
            if (viewController != null)
                ReplPage?.SetContext(viewController);
        }
    }
}

