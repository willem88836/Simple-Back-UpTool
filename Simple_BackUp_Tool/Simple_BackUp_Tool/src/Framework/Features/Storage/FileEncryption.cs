using System;

namespace Framework.Storage
{
	/// <summary>
	///		Encrypts and decrypts items using a key.
	/// </summary>
	public static class FileEncryption
	{
		// NOTE: Make sure to change this sequence before building (and not commit it).
		private static byte[] Key = new byte[] { 228, 86, 171, 182, 149, 85, 25, 44, 201, 14, 63, 94, 200, 65, 191, 228, 44, 79, 97, 49, 211, 142, 117, 185, 214, 177, 95, 214, 15, 233, 127, 231, 13, 173, 223, 188, 58, 79, 125, 54, 13, 232, 115, 217, 103, 120, 144, 226, 215, 140, 55, 251, 80, 171, 189, 85, 223, 241, 222, 226, 245, 65, 29, 223 };

		/// <summary>
		///		Generates a random key.
		/// </summary>
		public static void GenerateKey(short length)
		{
			Random rnd = new Random();

			Key = new byte[length];
			for (int i = 0; i < length; i++)
			{
				Key[i] = (byte)rnd.Next(byte.MaxValue);
			}
		}

		public static void SetKey(byte[] key)
		{
			Key = key;
		}


		public static void Encrypt(ref byte[] data)
		{
			if (Key.Length <= 0)
				return;


			for (int i = 0; i < data.Length; i++)
			{
				int j = Key[i % Key.Length];
				data[i] += (byte)j;
			}
		}

		public static void Decrypt(ref byte[] data)
		{
			if (Key.Length <= 0)
				return;

			for (int i = 0; i < data.Length; i++)
			{
				int j = Key[i % Key.Length];
				data[i] -= (byte)j;
			}
		}


		public static void Encrypt(ref string data)
		{
			if (Key.Length <= 0)
				return;

			string encryptedData = "";

			for (int i = 0; i < data.Length; i++)
			{
				int j = Key[i % Key.Length];
				char c = (char)(data[i] + j);
				encryptedData += c;
			}

			data = encryptedData;
		}

		public static void Decrypt(ref string data)
		{
			if (Key.Length <= 0)
				return;

			string decryptedData = "";

			for (int i = 0; i < data.Length; i++)
			{
				int j = Key[i % Key.Length];
				char c = (char)(data[i] - j);
				decryptedData += c;
			}

			data = decryptedData;
		}
	}
}
