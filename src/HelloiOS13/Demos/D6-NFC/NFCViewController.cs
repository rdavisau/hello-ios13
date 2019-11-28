using System.Text;
using System.Threading.Tasks;
using ARKitMeetup.Demos.D1;
using ARKitMeetup.Helpers;
using ARKitMeetup.Models;
using CoreNFC;
using HelloiOS13.D6.D1;
using Newtonsoft.Json;
using UIKit;

namespace HelloiOS13.D6.D2
{
    [DisplayInMenu(DisplayName = "Amiibo Writer", DisplayDescription = "Write the essence of an Amiibo to another tag")]
    public class WriteAmiiboNFCController : ReadAmiiboNFCController
    {
        protected override async Task OnTagDetected(NFCTagReaderSession session, INFCTag tag)
        {
            if (SelectedAmiibo == null)
            {
                session.AlertMessage = "Select an amiibo to clone.";
                return;
            }

            if (SelectedAmiibo != null)
            {
                session.AlertMessage = $"Writing {SelectedAmiibo.Metadata.Name} to this tag...";

                await WriteAmiibo(session, tag.GetNFCMiFareTag(), SelectedAmiibo);

                return;
            }
        }

        public async Task WriteAmiibo(NFCTagReaderSession session, INFCMiFareTag tag, DetectedAmiibo amiibo)
        {
            var dynamicLockBytes = new byte[] { 0x01, 0x00, 0x0F, 0xBD };
            var staticLockBytes = new byte[] { 0x0F, 0xE0, 0x0F, 0xE0 };

            var dump = amiibo.TagData;

            // this is ille🚨al so i took it out, but the flow is:
            // 1. make a copy of the tag dump
            // 2. decrypt the encrypted part of the tag dump using the original tag uid and secret nintendo key material
            // 3. reencrypt the now decrypted parts of the tag dump using the uid of the target tag and secret nintendo key material
            // 4. write the tag dump (not the lock bits) to the tag
            // 5. write the dynamic lock bits to 0x82
            // 6. write the static lock bits to 0x2
            // 7. draw the amiibo on the card for good luck

            // check https://www.3dbrew.org/wiki/Amiibo for amiibo data structure
            // check https://github.com/AcK77/AmiiBomb-uino for a good cloning reference

            await Task.Yield();

            session.AlertMessage = "Various implementation details removed and left to the reader (check the source).";
        }
    }

    public class DetectedAmiibo
    {
        public string UID { get; set; }
        public string CharacterId { get; set; }
        public Helpers.Amiibo Metadata { get; set; }
        public byte[] TagData { get; set; }
        public byte[] ImageData { get; set; }

        [JsonIgnore]
        public UIImageView ImageView { get; set; }
    }
}
