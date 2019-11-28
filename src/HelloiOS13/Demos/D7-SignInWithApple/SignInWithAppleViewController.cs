using System;
using System.Diagnostics;
using ARKitMeetup.Helpers;
using ARKitMeetup.Models;
using AuthenticationServices;
using Foundation;
using Newtonsoft.Json;
using UIKit;

namespace HelloiOS13.Demos.D7.D1
{
    [DisplayInMenu(DisplayName = "Sign In With Apple", DisplayDescription = "Use it or get banned")]
    public class SignInWithAppleViewController : BaseViewController, IASAuthorizationControllerDelegate, IASAuthorizationControllerPresentationContextProviding
    {
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            View.BackgroundColor = UIColor.White;  

            var button = new ASAuthorizationAppleIdButton(
                ASAuthorizationAppleIdButtonType.SignIn,
                ASAuthorizationAppleIdButtonStyle.Black); 

            button.TouchUpInside += delegate { SignInWithAppl(); };

            View.FillWith(button, 200, 500); 
        }

        [Export("authorizationController:didCompleteWithError:")] 
        public void DidComplete(ASAuthorizationController controller, NSError error)
        {
            Debug.WriteLine(error.Description);
        }
        
        private void SignInWithAppl()
        {
            var provider = new ASAuthorizationAppleIdProvider();
            var request = provider.CreateRequest();

            request.RequestedScopes = new[]
            {
                ASAuthorizationScope.Email,
                ASAuthorizationScope.FullName
            };

            var authorizationController =
                new ASAuthorizationController(new[] { request })
                {
                    Delegate = this,
                    PresentationContextProvider = this
                };

            authorizationController.PerformRequests();
        }

        [Export("authorizationController:didCompleteWithAuthorization:")]
        public void DidComplete(ASAuthorizationController _, ASAuthorization auth)
        {
            var cred = auth.GetCredential<ASAuthorizationAppleIdCredential>();

            var userIdentifier = cred.User;
            var fullName = cred.FullName;
            var email = cred.Email;
            var identityToken = cred.IdentityToken;
            var authCode = cred.AuthorizationCode;

            // send details to the server 

            View.FillWith(new UITextView
            {
                Text =
                $"\r\n\r\nUser Identifier\r\n {userIdentifier}\r\n\r\n" + 
                $"Full Name\r\n{fullName.GivenName} {fullName.FamilyName}\r\n\r\n" + 
                $"Email\r\n{email}\r\n\r\n" +
                $"IdentityToken\r\n{identityToken.ToString().Substring(0, 200)}\r\n\r\n" +
                $"AuthCode\r\n{authCode.ToString().Substring(0, 40)}",

                TextAlignment = UITextAlignment.Center,
                Font = UIFont.GetMonospacedSystemFont(16, UIFontWeight.Regular),   
            }, 40, 50);   
        }

        public UIWindow GetPresentationAnchor(ASAuthorizationController controller)
        {
            return this.View.Window;
        }
    }
}
