using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LanScanner
{
    public class Scanner
    {
        //public event Action<string> LogConsole;
        public event Action<string> LineScanned;

        private static readonly char[] CONST_SEPARATOR = new char[] { '|' };
        private readonly string CONST_SCANNER_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AdvancedScanner");
        //private static readonly string CONST_FAIL_TEXT = "Failed to create";

        //private int _scanFailCharPos = 0;

        public List<ScanResult> Scan(string startIp, string stopIp)
        {
            ProcessStartInfo psi = new ProcessStartInfo(Path.Combine(CONST_SCANNER_PATH, "advanced_ip_scanner_console.exe"), $"/r:{startIp}-{stopIp}");
            psi.WorkingDirectory = CONST_SCANNER_PATH;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;

            Process scannerProc = Process.Start(psi);
            Job job = new Job();
            job.AddProcess(scannerProc.Handle);

            try
            {
                return DeserilizeAdsOutput(scannerProc, 3);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (scannerProc?.HasExited == false)
                {
                    scannerProc.Kill();
                }
            }
            //return DeserilizeAdsOutput(scannerProc.StandardOutput);
        }

        private List<ScanResult> DeserilizeAdsOutput(StreamReader stream)
        {
            List<ScanResult> result = new List<ScanResult>();
            string line = stream.ReadLine();
            LineScanned?.Invoke(line);
            while ((line = stream.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                LineScanned?.Invoke(line);

                string[] parts = line.Split(CONST_SEPARATOR, StringSplitOptions.None);
                if (parts != null && parts.Length >= 7)
                {
                    result.Add(new ScanResult()
                    {
                        Status = parts[0].Trim(),
                        Name = parts[1].Trim(),
                        IP = parts[2].Trim(),
                        MAC = parts[5].Trim(),
                    });
                }
            }
            if (result.Count < 1)
            {
                throw new Exception("AdvancedScanner 扫描结果异常");
            }
            return result;
        }

        private List<ScanResult> DeserilizeAdsOutput(Process proc, int timeOutMinutes)
        {
            Task<List<ScanResult>> work = Task.Run(() => DeserilizeAdsOutput(proc.StandardOutput));
            if (work.Wait(TimeSpan.FromMinutes(timeOutMinutes)))
            {
                return work.Result;
            }
            throw new Exception("AdvancedScanner 失去响应");
        }

        //private List<ScanResult> DeserilizeAdsOutput(Process proc)
        //{
        //    _scanFailCharPos = 0;
        //    DateTime beginTime = DateTime.Now;
        //    List<ScanResult> result = new List<ScanResult>();

        //    if (proc == null)
        //        return result;

        //    char[] buffer = new char[1024];
        //    int bufferPos = 0;

        //    while (proc != null && !proc.HasExited)
        //    {
        //        int length = proc.StandardOutput.Read(buffer, bufferPos, buffer.Length - bufferPos);
        //        if (length == 0)
        //        {
        //            Thread.Sleep(100);
        //            continue;
        //        }

        //        LogConsole?.Invoke(new string(buffer, bufferPos, length));

        //        if (ScanBuffer(buffer, bufferPos, length, out int boundaryEndPos))
        //        {
        //            int bufferEndPos = bufferPos + length;
        //            string line = new string(buffer, 0, boundaryEndPos);

        //            LineScanned?.Invoke(line);

        //            if (TryDeserilizeLine(line, out ScanResult lineResult))
        //            {
        //                result.Add(lineResult);
        //            }
        //            for (int i = boundaryEndPos, ptr = 0; i < bufferEndPos; i++, ptr++)
        //            {
        //                buffer[ptr] = buffer[i];
        //            }
        //            bufferPos = bufferEndPos - boundaryEndPos;
        //        }
        //        else
        //        {
        //            bufferPos += length;
        //        }

        //    }

        //    if (result.Count > 0)
        //    {
        //        result.RemoveAt(0);
        //    }
        //    return result;
        //}

        //private bool ScanBuffer(char[] buffer, int pos, int length, out int boundaryEndPos)
        //{
        //    for (int i = 0; i < length; i++)
        //    {
        //        char current = buffer[i + pos];
        //        if (current == '\n')
        //        {
        //            boundaryEndPos = i + 1 + pos;
        //            return true;
        //        }
        //        if (CONST_FAIL_TEXT[_scanFailCharPos] == current)
        //        {
        //            _scanFailCharPos++;
        //            if (_scanFailCharPos >= CONST_FAIL_TEXT.Length)
        //            {
        //                throw new Exception("运行 AdvancedScanner 失败");
        //            }
        //        }
        //        else
        //        {
        //            _scanFailCharPos = 0;
        //        }
        //    }
        //    boundaryEndPos = 0;
        //    return false;
        //}

        //private bool TryDeserilizeLine(string line, out ScanResult result)
        //{
        //    string[] parts = line?.Split(CONST_SEPARATOR, StringSplitOptions.None);
        //    if (parts != null && parts.Length >= 7)
        //    {
        //        result = new ScanResult()
        //        {
        //            Status = parts[0].Trim(),
        //            Name = parts[1].Trim(),
        //            IP = parts[2].Trim(),
        //            MAC = parts[5].Trim(),
        //        };
        //        return true;
        //    }
        //    result = null;
        //    return false;
        //}
    }
}
