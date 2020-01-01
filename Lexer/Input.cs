using System;
using System.IO;

namespace Lexer
{
	internal class Input : IDisposable
	{
		private StreamReader reader;

		public bool NeedNextSymbol { get; set; } = true;
		public int Symbol { get; private set; } = -1;
		public int PrevRowSize { get; private set; }
		public int RowIndex { get; private set; } = 0;
		public int ColIndex { get; private set; } = -1;

		public Input(string path) => reader = new StreamReader(path);
		public Input(Stream stream) => reader = new StreamReader(stream);

		~Input() => Dispose();

		/// <summary></summary>
		/// <param name="path">Path to file on disk.</param>
		public void SetSource(string path)
		{
			reader?.Dispose();
			reader = new StreamReader(path);
			Reset();
		}

		public void SetSource(Stream stream)
		{
			reader?.Dispose();
			reader = new StreamReader(stream);
			Reset();
		}

		public int Next()
		{
			if (NeedNextSymbol) {
				Symbol = reader.Read();
				CheckLineFeed();
			}
			NeedNextSymbol = true;
			return Symbol;
		}

		private void CheckLineFeed()
		{
			if (Symbol == '\n') {
				ColIndex = -1;
				RowIndex++;
			} else {
				ColIndex++;
			}
		}

		private void Reset()
		{
			NeedNextSymbol = true;
			ColIndex = -1;
			RowIndex = 0;
		}

		public void Dispose()
		{
			reader?.Dispose();
			reader = null;
		}
	}
}
