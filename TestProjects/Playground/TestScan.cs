using LanScanner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playground
{
    class TestScan
    {
        private static readonly char[] CONST_SEPARATOR = new char[] { '|' };

        public List<ScanResult> DeserilizeAdsOutput(StreamReader reader)
        {
            DateTime beginTime = DateTime.Now;
            List<ScanResult> result = new List<ScanResult>();

            char[] buffer = new char[1024];
            int bufferPos = 0;

            while (!reader.EndOfStream)
            {
                int length = reader.Read(buffer, bufferPos, buffer.Length - bufferPos);
                if (length == 0)
                {
                    break;
                }

                //LogConsole?.Invoke(new string(buffer, bufferPos, length));

                if (ScanBuffer(buffer, bufferPos, length, out int boundaryEndPos))
                {
                    int bufferEndPos = bufferPos + length;
                    string line = new string(buffer, 0, boundaryEndPos);

                    Console.WriteLine(line);

                    if (TryDeserilizeLine(line, out ScanResult lineResult))
                    {
                        result.Add(lineResult);
                    }
                    for (int i = boundaryEndPos, ptr = 0; i < bufferEndPos; i++, ptr++)
                    {
                        buffer[ptr] = buffer[i];
                    }
                    bufferPos = bufferEndPos - boundaryEndPos;
                }
                else
                {
                    bufferPos += length;
                }

            }

            if (result.Count > 0)
            {
                result.RemoveAt(0);
            }
            return result;
        }

        private bool ScanBuffer(char[] buffer, int pos, int length, out int boundaryEndPos)
        {
            for (int i = 0; i < length; i++)
            {
                char current = buffer[i + pos];
                if (current == '\n')
                {
                    boundaryEndPos = i + 1 + pos;
                    return true;
                }
            }
            boundaryEndPos = 0;
            return false;
        }

        private bool TryDeserilizeLine(string line, out ScanResult result)
        {
            string[] parts = line?.Split(CONST_SEPARATOR, StringSplitOptions.None);
            if (parts != null && parts.Length >= 7)
            {
                result = new ScanResult()
                {
                    Status = parts[0].Trim(),
                    Name = parts[1].Trim(),
                    IP = parts[2].Trim(),
                    MAC = parts[5].Trim(),
                };
                return true;
            }
            result = null;
            return false;
        }
    }
}
