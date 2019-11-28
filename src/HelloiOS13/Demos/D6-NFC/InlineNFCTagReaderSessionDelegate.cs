using System;
using CoreNFC;
using Foundation;

namespace HelloiOS13.D6.D1
{
    public class InlineNFCTagReaderSessionDelegate : NFCTagReaderSessionDelegate
    {
        public Action<NFCTagReaderSession> _DidBecomeActive { get; set; }
        public Action<NFCTagReaderSession, INFCTag[]> _DidDetectTags { get; set; }
        public Action<NFCTagReaderSession, NSError> _DidInvalidate { get; set; }

        public override void DidBecomeActive(NFCTagReaderSession session)
            => _DidBecomeActive?.Invoke(session);

        public override void DidDetectTags(NFCTagReaderSession session, INFCTag[] tags)
            => _DidDetectTags?.Invoke(session, tags);

        public override void DidInvalidate(NFCTagReaderSession session, NSError error)
            => _DidInvalidate?.Invoke(session, error);
    }
}
