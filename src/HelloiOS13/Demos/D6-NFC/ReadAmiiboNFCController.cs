using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ARKitMeetup.Models;
using CoreNFC;
using Foundation;
using HelloiOS13.D6.D2;
using HelloiOS13.Helpers;
using Refit;
using UIKit;

namespace HelloiOS13.D6.D1
{
    [DisplayInMenu(DisplayName = "Amiibo Reader", DisplayDescription = "Capture the souls of Amiibo")]
    public class ReadAmiiboNFCController : BaseNFCViewController
    {
        protected IAmiiboAPI AmiiboApi = RestService.For<IAmiiboAPI>("https://www.amiiboapi.com/api/");

        public List<DetectedAmiibo> DetectedAmiibo = AppDelegate.Amiibo;
        public DetectedAmiibo SelectedAmiibo;

        public UILabel NameLabel { get; set; } = new UILabel
        {
            Frame = new CoreGraphics.CGRect(0, 0, 250, 33),
            TextAlignment = UITextAlignment.Center,
            Font = UIFont.PreferredTitle3,
            TextColor = UIColor.White,
            BackgroundColor = UIColor.Black.ColorWithAlpha(.75f),
            Alpha = 0,
        };

        public async Task<DetectedAmiibo> FakeAmiibo(string game, string character)
        {
            var lookup = await AmiiboApi.GetAmiibo(game, character);
            var metadata = lookup.Amiibo.FirstOrDefault();

            // dump metadata
            Debug.WriteLine(
                $"== Metadata : https://www.amiiboapi.com/api/ ==\r\n" +
                metadata.ToJson());

            var imageData = await new HttpClient().GetByteArrayAsync(metadata.Image);

            return new DetectedAmiibo
            {
                UID = Guid.NewGuid().ToString(),
                CharacterId = $"{game}-{character}",
                Metadata = metadata,
                TagData = new byte[] { },
                ImageData = imageData,
            };
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.White.ColorWithAlpha(.85f);
            View.AddSubviews(NameLabel);
            NameLabel.Layer.CornerRadius = 10;

            NameLabel.AddGestureRecognizer(new UITapGestureRecognizer(() => ShareAmiibo(SelectedAmiibo)));

            DetectedAmiibo = AppDelegate.Amiibo;

            foreach (var amiibo in AppDelegate.Amiibo.ToList())
                await OnAmiibo(amiibo, true);
        }

        protected override async Task OnTagDetected(NFCTagReaderSession session, INFCTag tag)
        {
            var amiibo = await TryReadAmiibo(session, tag);

            if (amiibo != null)
                await OnAmiibo(amiibo, false);
        }

        const int size = 55;
        public async Task OnAmiibo(DetectedAmiibo amiibo, bool withDelay)
        {
            var initialPos = new CoreGraphics.CGRect(0, 0, size * 5, size * 5);

            var imageView = new UIImageView
            {
                ContentMode = UIViewContentMode.ScaleAspectFit,
                Image = UIImage.LoadFromData(NSData.FromArray(amiibo.ImageData)),
                Frame = initialPos,
                Center = View.Center,
                UserInteractionEnabled = true,
            };

            imageView.Layer.ShadowRadius = 1f;
            imageView.Layer.ShadowOpacity = .5f;

            imageView.AddGestureRecognizer(new UITapGestureRecognizer(gr => AmiiboSelected(amiibo)));
            imageView.AddGestureRecognizer(new UILongPressGestureRecognizer(gr => ShareAmiibo(amiibo)));

            amiibo.ImageView = imageView;
            View.AddSubview(imageView);

            if (!DetectedAmiibo.Contains(amiibo))
                DetectedAmiibo.Add(amiibo);

            if (!withDelay)
            {
                imageView.Alpha = 0;
                UIView.Animate(.5, () =>
                {
                    imageView.Alpha = 1;
                    imageView.Center = new CoreGraphics.CGPoint(View.Center.X, View.Center.Y - 150);
                });

                await Task.Delay(TimeSpan.FromSeconds(3.5));
            }

            MoveToRestingPosition(amiibo, withDelay);
        }

        private void AmiiboSelected(DetectedAmiibo amiibo)
        {
            foreach (var a in DetectedAmiibo)
                if (!a.Equals(amiibo))
                    MoveToRestingPosition(a, false);

            if (!amiibo.Equals(SelectedAmiibo))
            {
                SelectedAmiibo = amiibo;
                MoveToFocusPosition(amiibo, false);

                NameLabel.Center = new CoreGraphics.CGPoint(View.Center.X, SelectedAmiibo.ImageView.Center.Y + size * 3);
                NameLabel.Text = amiibo.Metadata.Name;
                UIView.Animate(.15, () => NameLabel.Alpha = 1);
            }
            else
            {
                SelectedAmiibo = null;
                MoveToRestingPosition(amiibo, false);

                UIView.Animate(.15, () => NameLabel.Alpha = 0);
            }
        }

        public void MoveToFocusPosition(DetectedAmiibo amiibo, bool withDelay)
        {
            var idx = DetectedAmiibo.IndexOf(amiibo);
            var targetPos = new CoreGraphics.CGRect(View.Frame.Width - size - 10, 60 + ((size + 10) * idx), size * 4, size * 4);

            UIView.Animate(.3, withDelay ? idx * .07 : 0, UIViewAnimationOptions.CurveEaseOut, () =>
            {
                amiibo.ImageView.Frame = targetPos;
                amiibo.ImageView.Center = new CoreGraphics.CGPoint(View.Center.X, View.Center.Y - 200);
            },
            null);
        }

        public void MoveToRestingPosition(DetectedAmiibo amiibo, bool withDelay)
        {
            var idx = DetectedAmiibo.IndexOf(amiibo);
            var targetPos = new CoreGraphics.CGRect(View.Frame.Width - size - 10, 60 + ((size + 10) * idx), size, size);

            UIView.Animate(.3, withDelay ? idx * .05 : 0, UIViewAnimationOptions.CurveEaseOut, () =>
            {
                amiibo.ImageView.RemoveConstraints(amiibo.ImageView.Constraints);
                amiibo.ImageView.Frame = targetPos;
            },
            null);
        }

        public async Task<DetectedAmiibo> TryReadAmiibo(NFCTagReaderSession session, INFCTag tag)
        {
            // dump tag data
            Debug.WriteLine($"Found a tag: {tag}");

            // get mifare representation
            var mifareTag = tag.GetNFCMiFareTag();
            var identifier = mifareTag.Identifier.ToArray();

            // connect w h y
            await session.ConnectToAsync(tag);

            // read entire tag contents
            var tagData =
                await mifareTag.DoFastRead(
                    from: 0x0, to: 0x86,
                    batchSize: 0x20
                );

            // dump tag data
            Debug.WriteLine($"== Raw tag data ==\r\n" +
                $"{Utils.HexDump(tagData, 16)}");

            // extract character identifiers
            var identificationHeader = tagData.Slice(0x15 * 4, 0x2 * 4);

            var uid = identifier.String();
            var game = identificationHeader.SliceStr(0, 4);
            var character = identificationHeader.SliceStr(4, 4);

            // print identifiers
            Debug.WriteLine(
                $"== Identifiers ==\r\n" +
                $"Nfc Uid: {uid}\r\n" +
                $"Game Id: {game}\r\n" +
                $"Char Id: {character}\r\n");

            // get metadata from amiibo api 
            session.AlertMessage = $"Looking up Amiibo...";

            var lookup = await AmiiboApi.GetAmiibo(game, character);
            var metadata = lookup.Amiibo.FirstOrDefault();

            // dump metadata
            Debug.WriteLine(
                $"== Metadata : https://www.amiiboapi.com/api/ ==\r\n" +
                metadata.ToJson());

            // we did it!
            session.AlertMessage = $"It's {metadata.Name}!";

            var imageData = await new HttpClient().GetByteArrayAsync(metadata.Image);

            return new DetectedAmiibo
            {
                UID = uid,
                CharacterId = $"{game}-{character}",
                Metadata = metadata,
                TagData = tagData,
                ImageData = imageData,
            };
        }

        public void ShareAmiibo(DetectedAmiibo amiibo)
        {
            var (characterId, name, uid, data)
                = (amiibo.CharacterId, amiibo.Metadata.Name, amiibo.UID, amiibo.TagData);

            var fn = $"({characterId}-{name}-{uid}.bin".ToLower();
            var path = Path.Combine(Xamarin.Essentials.FileSystem.CacheDirectory, fn);

            // don't :b:elete this before the share dialog goes away
            File.WriteAllBytes(path, data);

            InvokeOnMainThread(async
                () => await Xamarin.Essentials.Share.RequestAsync(
                new Xamarin.Essentials.ShareFileRequest
                {
                    Title = fn,
                    File = new Xamarin.Essentials.ShareFile(path)
                }));
        }

    }
}
