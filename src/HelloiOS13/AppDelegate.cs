using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Foundation;
using HelloiOS13.D6.D2;
using LightSwitch;
using UIKit;
using Xamarin.Forms;

namespace HelloiOS13
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public static AppDelegate Instance { get; set; }
        public string CurrentUser { get; set; }

        public static List<DetectedAmiibo> Amiibo = new List<DetectedAmiibo>();
        
        public override UIWindow Window { get; set; }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Instance = this;

            Xamarin.Forms.FormsMaterial.Init();
            Forms.SetFlags("CollectionView_Experimental");
            Xamarin.Forms.Forms.Init(); 
            ListIPAddresses();

            new Continuous.Server.HttpServer().Run();

            LightSwitchAgent.Init(new LightSwitch.Agent.Implementation.AgentOptions
            {
                TargetElementGetter = () => new MultiWindowGetter()
            });
            
            return true;
        }

        public override UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
        {
            if (options.UserActivities.AnyObject?.ActivityType == "repl")
                return new UISceneConfiguration("REPL Configuration", connectingSceneSession.Role);

            return new UISceneConfiguration("Default Configuration", connectingSceneSession.Role);
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

