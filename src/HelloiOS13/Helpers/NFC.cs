using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreNFC;
using Foundation;
using Newtonsoft.Json;

namespace HelloiOS13.Helpers
{
    public static class MiFareTagExtensions
    {
        public static Task<MifareCommandResult> SendMifareCommand(
            this INFCMiFareTag tag, params byte[] command)
        {
            var tcs = new TaskCompletionSource<MifareCommandResult>();

            tag.SendMiFareCommand(
                NSData.FromArray(command),
                (data, err) => tcs.SetResult(MifareCommandResult.From(data, err)));

            return tcs.Task;
        }

        public static async Task<byte[]> DoFastRead(
            this INFCMiFareTag tag,
            byte from,
            byte to,
            byte batchSize)
        {
            var results = new List<MifareCommandResult>();
            while (from < to)
            {
                var thisTo = (byte)Math.Min(from + batchSize, to);
                var data = await tag.SendMifareCommand(0x3a, from, thisTo);
                results.Add(data);

                from = (byte)(from + (byte)(batchSize + 0x1));
            }

            if (results.Any(x => x.Error != null))
                throw new Exception($"Failed to fast read; first error: {results.First(x => x.Error != null).Error}");

            var all = MifareCommandResult.Combine(results);
            return all.Data;
        }

        public static async Task<byte[]> WritePage(
            this INFCMiFareTag tag,
            byte page,
            byte[] data)
        {
            var cmd = new byte[] { 0xa2, (byte)(page) }.Concat(data).ToArray();
            var result = await tag.SendMifareCommand(cmd);

            if (result.Error != null)
            {
                Debug.WriteLine(result.Error);
            }

            return result.Data;
        }

        public static byte[] FastRead(byte fromPage, byte toPage)
            => new byte[] { 0x3a, fromPage, toPage };

        public static byte[] Slice(this byte[] bs, byte start, byte len)
            => bs.Skip(start).Take(len).ToArray();

        public static string String(this byte[] bs)
            => BitConverter.ToString(bs).Replace("-", "");

        public static string SliceStr(this byte[] bs, byte start, byte len)
            => BitConverter.ToString(bs, start, len).Replace("-", "");

        public static string ToJson(this object o) => JsonConvert.SerializeObject(o, Formatting.Indented);
    }

    public class MifareCommandResult
    {
        public static MifareCommandResult From(NSData data, NSError error)
            => new MifareCommandResult { Data = data?.ToArray(), Error = error?.Description };

        public byte[] Data { get; set; }
        public string Error { get; set; }

        public override string ToString()
            => Error != null
                ? $"ERR: {Error}"
                : "DAT:" + Environment.NewLine + Utils.HexDump(Data, 8);

        public static MifareCommandResult Combine(ICollection<MifareCommandResult> results)
            => new MifareCommandResult
            {
                Data = results.SelectMany(r => r.Data).ToArray()
            };
    }

    public class Utils
    {
        public static string HexDump(byte[] bytes, int bytesPerLine = 16)
        {
            if (bytes == null) return "<null>";
            int bytesLength = bytes.Length;

            char[] HexChars = "0123456789ABCDEF".ToCharArray();

            int firstHexColumn =
                  8                   // 8 characters for the address
                + 3;                  // 3 spaces

            int firstCharColumn = firstHexColumn
                + bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
                + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                + 2;                  // 2 spaces 

            int lineLength = firstCharColumn
                + bytesPerLine           // - characters to show the ascii value
                + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            char[] line = (new String(' ', lineLength - Environment.NewLine.Length) + Environment.NewLine).ToCharArray();
            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            StringBuilder result = new StringBuilder(expectedLines * lineLength);

            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];

                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = (b < 32 ? '·' : (char)b);
                    }
                    hexColumn += 3;
                    charColumn++;
                }
                result.Append(line);
            }
            return result.ToString();
        }
    }
}