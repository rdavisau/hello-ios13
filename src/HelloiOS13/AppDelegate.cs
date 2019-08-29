using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using CSharpForMarkup;
using Foundation;
using UIKit;
using Xamarin.Forms;

namespace HelloiOS13
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Xamarin.Forms.FormsMaterial.Init();
            Forms.SetFlags("CollectionView_Experimental");
            Xamarin.Forms.Forms.Init(); 
            ListIPAddresses();

            Window = new UIWindow
            {
                RootViewController = new MenuViewController()
            };

            Window.MakeKeyAndVisible();

            return true;
        }

        private void ListIPAddresses()
        {
            try
            {
                var inet =
                    NetworkInterface
                        .GetAllNetworkInterfaces()
                        .SelectMany(x =>
                            x.GetIPProperties()
                             .UnicastAddresses.Where(y => y.Address.AddressFamily == AddressFamily.InterNetwork))
                        .Select(y => y.Address);

                Debug.WriteLine(String.Join(Environment.NewLine, inet));

            }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }
    }
}

