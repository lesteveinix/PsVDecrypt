﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using PsVDecrypt.Domain;

namespace PsVDecrypt
{
    public class Util
    {
        public static void DecryptFile(string srcFile, string dstFile)
        {
            var stream = new VirtualFileStream(srcFile);
            var output = new FileStream(dstFile, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                FileShare.None);
            output.SetLength(0);
            byte[] buffer = stream.ReadAll();
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        public static string GetModuleHash(string name, string authorHandle)
        {
            string s = name + "|" + authorHandle;
            using (MD5 md5 = MD5.Create())
                return Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace('/', '_');
        }

        public static string TitleToFileName(string title)
        {
            var sb = new StringBuilder();
            foreach (char c in title)
            {
                if (c == ' ')
                    sb.Append('-');
                else if (c == '-' || c == '_')
                    sb.Append('-');
                else if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9')
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public static void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);

            while (!Directory.Exists(path))
                System.Threading.Thread.Sleep(200);
        }

        public static void DeleteDirectory(string path)
        {
            Directory.Delete(path, true);

            while (Directory.Exists(path))
                System.Threading.Thread.Sleep(200);
        }
    }
}