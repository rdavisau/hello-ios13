using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ARKitMeetup.Models;
using CoreFoundation;
using CoreNFC;
using Foundation;
using HelloiOS13.Helpers;
using UIKit;

namespace HelloiOS13.D6.D1
{
    [DisplayInMenu(DisplayName = "NFC", DisplayDescription = "Reads a MiFare tag and dumps the contents to the console")]
    public class BaseNFCViewController : BaseViewControllerWithCameraStream
    {
        protected NFCTagReaderSession Session;

        public void BeginSession()
        {
            TryInvalidateSession();

            Session = new NFCTagReaderSession(
                NFCPollingOption.Iso14443,
                GetSessionDelegate(),
                DispatchQueue.MainQueue);

            Session.BeginSession();
        }

        public NFCTagReaderSessionDelegate GetSessionDelegate() =>
            new InlineNFCTagReaderSessionDelegate
            {
                _DidBecomeActive = (session) =>
                {
                    Console.WriteLine($"Session {session} active");
                    session.RestartPolling();
                },

                _DidDetectTags = async (session, tags) =>
                {
                    if (tags.Count() > 1)
                        throw new Exception("We only expect to detect one tag at a time ok");

                    try
                    {
                        await OnTagDetected(session, tags.First());

                        session.InvalidateSession();
                        session.RestartPolling();
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex);

                        session.RestartPolling();
                    }
                },

                _DidInvalidate = (session, error) =>
                {
                    Console.WriteLine($"Session {session} invalidated because {error?.Description}");
                }
            };

        protected async virtual Task OnTagDetected(NFCTagReaderSession session, INFCTag tag)
        {
            await ReadTagData(session, tag);
        }

        public async Task<byte[]> ReadTagData(
            NFCTagReaderSession session, INFCTag tag)
        {
            await session.ConnectToAsync(tag);

            var mifare = tag.GetNFCMiFareTag();

            if (mifare is null)
                return new byte[0]; // not a mifare tag

            var tagData =
                await mifare.DoFastRead(
                    from: 0x0, to: 0x86,
                    batchSize: 0x43
                );

            Debug.WriteLine(
                "Tag Data:\r\n" +
                Utils.HexDump(tagData, 8));

            return tagData;
        }

        private void TryInvalidateSession()
        {
            try
            {
                Session?.RestartPolling();
                Session?.InvalidateSession();
            }
            catch { }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            BeginSession();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            try { Session?.RestartPolling(); Session?.InvalidateSession(); } catch { }
        }
    }
}
