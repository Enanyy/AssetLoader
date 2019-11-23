/********************************************************************
	created:	2015/08/19
	created:	19:8:2015   11:17
	filename: 	MD5Hash.cs
	file path:	Assets\Code\FrameWork\Dipper
	file base:	MD5Hash
	file ext:	cs
	author:		来自网络，表示感谢
	
	purpose:	
*********************************************************************/
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


public static class MD5Hash
{
    static StringBuilder stringBuilder;
    public static string Get(string input)
    {
        return Get(Encoding.Default.GetBytes(input));
    }

    public static string Get(byte[] input)
    {
        MD5 md5Hasher = MD5.Create();
        byte[] data = md5Hasher.ComputeHash(input);
        if (stringBuilder == null)
        {
            stringBuilder = new StringBuilder();
        }
        stringBuilder.Remove(0, stringBuilder.Length);
        for (int i = 0; i < data.Length; ++i)
            stringBuilder.Append(data[i].ToString("x2"));

        string md5 = stringBuilder.ToString();
        stringBuilder.Remove(0, stringBuilder.Length);
        return md5;
    }

    public static string Get(Stream stream)
    {
        //MD5 md5 = MD5.Create();
        MD5 md5Hasher = new MD5CryptoServiceProvider();
        byte[] data = md5Hasher.ComputeHash(stream);
        if (stringBuilder == null)
        {
            stringBuilder = new StringBuilder();
        }

        stringBuilder.Remove(0, stringBuilder.Length);
        for (int i = 0; i < data.Length; ++i)
            stringBuilder.Append(data[i].ToString("x2"));

        string md5 = stringBuilder.ToString();
        stringBuilder.Remove(0, stringBuilder.Length);
        return md5;
    }

    public static bool Verify(string input, string hash)
    {
        string hashOfInput = Get(input);
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;
        return (0 == comparer.Compare(hashOfInput, hash));
    }

    public static bool Verify(byte[] input, string hash)
    {
        string hashOfInput = Get(input);
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;
        return (0 == comparer.Compare(hashOfInput, hash));
    }

    public static bool Verify(Stream input, string hash)
    {
        string hashOfInput = Get(input);
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;
        return (0 == comparer.Compare(hashOfInput, hash));
    }
}

