using System;
using System.Linq;
using Foundation;
using UIKit;

namespace HelloiOS13
{
    [Register("SceneDelegate")]
    public class SceneDelegate : UIWindowSceneDelegate
    {
        public static UISceneSession DetailSceneSession { get; set; }

        public override UIWindow Window
        {
            get;
            set;
        }

        public override void PerformAction(UIWindowScene windowScene, UIApplicationShortcutItem shortcutItem, Action<bool> completionHandler)
        {
            // intercept repl item
            if (shortcutItem.LocalizedTitle.Contains("REPL"))
            {
                UIApplication.SharedApplication.RequestSceneSessionActivation(
                    ReplSceneDelegate.SceneSession,
                    new NSUserActivity("repl"),
                    new UISceneActivationRequestOptions(),
                    err => Console.WriteLine(err));

                completionHandler(false);
            }

            completionHandler(true);
        }

        public override void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
        {
            if (!(scene is UIWindowScene ws))
                return;
            
            var target =
                connectionOptions
                    .UserActivities
                    .OfType<NSUserActivity>()
                    .Select(GetViewControllerForUseractivity)
                    .FirstOrDefault(x => x != null) ?? new MenuViewController();

            if (target.GetType() != typeof(MenuViewController))
                DetailSceneSession = session;

            Window = new UIWindow(ws)
            {
                RootViewController = target
            };

            Window.MakeKeyAndVisible();
        }

        public override void ContinueUserActivity(UIScene scene, NSUserActivity userActivity)
        {
            var vc = GetViewControllerForUseractivity(userActivity);
            vc.View.Alpha = 0;

            UIView.Animate(.75, () => Window.RootViewController.View.Alpha = 0f,
                () =>
                {
                    Window.RootViewController = vc;
                    UIView.Animate(.75, () => vc.View.Alpha = 1);
                });

            Window.RootViewController = vc;
        }

        public override void DidBecomeActive(UIScene scene)
        {
            Console.WriteLine($"SCENE {scene} did become active");

            if (!(scene.Delegate is ReplSceneDelegate))
                ReplSceneDelegate.Instance?.SetActiveScene(scene);
        }

        public override void DidUpdateUserActivity(UIScene scene, NSUserActivity userActivity)
        {
            base.DidUpdateUserActivity(scene, userActivity);
        }

        public UIViewController GetViewControllerForUseractivity(NSUserActivity userActivity)
        {
            return (UIViewController)
                Activator.CreateInstance(
                       Type.GetType(userActivity.ActivityType)
                    ?? Type.GetType(userActivity.Title)
                    ?? typeof(MenuViewController));
        }
    }
}

