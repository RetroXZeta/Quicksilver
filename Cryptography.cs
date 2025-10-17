using System;
using System.Text;
using System.Security.Cryptography;

namespace Quicksilver {
    public static class Cryptography {
        public static byte[] HashBytes(byte[] raw) {
            SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(raw);
            return hash;
        }
        public static byte[] HashBytes(string raw) {
            SHA256 sha256 = SHA256.Create();
            byte[] rawBytes = Encoding.UTF8.GetBytes(raw);
            byte[] hash = sha256.ComputeHash(rawBytes);
            return hash;
        }
        public static string HashString(byte[] raw) {
            return Encoding.UTF8.GetString(HashBytes(raw));
        }
        public static string HashString(string raw) {
            return Encoding.UTF8.GetString(HashBytes(raw));
        }
    }
}