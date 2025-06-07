using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace SliceCSV
{
    class Program
    {
        private const int ROWS_PER_FILE = 1000;

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("使用法: SliceCSV <inputCsvPath> <outputDir>");
                Console.WriteLine("例: SliceCSV data.csv output");
                return;
            }

            string inputCsvPath = args[0];
            string outputDir = args[1];

            try
            {
                ProcessCsvFile(inputCsvPath, outputDir);
                Console.WriteLine("CSV分割処理が完了しました。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"エラーが発生しました: {ex.Message}");
                Environment.Exit(1);
            }
        }

        static void ProcessCsvFile(string inputCsvPath, string outputDir)
        {
            // 入力ファイルの存在チェック
            if (!File.Exists(inputCsvPath))
            {
                throw new FileNotFoundException($"入力ファイルが見つかりません: {inputCsvPath}");
            }

            // 出力ディレクトリの作成
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                Console.WriteLine($"出力ディレクトリを作成しました: {outputDir}");
            }

            string inputFileName = Path.GetFileNameWithoutExtension(inputCsvPath);
            string extension = Path.GetExtension(inputCsvPath);
            
            // Shift_JISエンコーディングを指定してファイルを読み込み
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var encoding = Encoding.GetEncoding("shift_jis");
            
            using (var reader = new StreamReader(inputCsvPath, encoding))
            {
                int currentFileIndex = 1;
                int totalProcessedRows = 0;
                StreamWriter? currentWriter = null;
                List<string> currentBuffer = new List<string>();

                try
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        currentBuffer.Add(line);
                        totalProcessedRows++;

                        // 1000行に達したら分割点を調整
                        if (currentBuffer.Count >= ROWS_PER_FILE)
                        {
                            int splitIndex = FindValidSplitPoint(currentBuffer);
                            
                            // ファイルに書き出し
                            WriteBufferToFile(currentBuffer.Take(splitIndex).ToArray(), 
                                            inputFileName, extension, outputDir, encoding, currentFileIndex);
                            
                            // 残りの行を次のバッファへ
                            currentBuffer = currentBuffer.Skip(splitIndex).ToList();
                            currentFileIndex++;
                            
                            Console.WriteLine($"{totalProcessedRows} 行処理済み（分割点調整: {splitIndex}行で分割）");
                        }
                        
                        // 進捗表示（1000行ごと）
                        if (totalProcessedRows % 1000 == 0)
                        {
                            Console.WriteLine($"{totalProcessedRows} 行処理済み");
                        }
                    }

                    // 残りのバッファを最終ファイルに書き出し
                    if (currentBuffer.Count > 0)
                    {
                        WriteBufferToFile(currentBuffer.ToArray(), 
                                        inputFileName, extension, outputDir, encoding, currentFileIndex);
                    }

                    // 最終的な進捗表示
                    Console.WriteLine($"{totalProcessedRows} 行処理済み");
                }
                finally
                {
                    currentWriter?.Close();
                }
            }
        }

        /// <summary>
        /// 複合仕訳を考慮した適切な分割点を見つける
        /// </summary>
        static int FindValidSplitPoint(List<string> buffer)
        {
            // 最後から逆順に検索して、適切な終了行を見つける
            for (int i = buffer.Count - 1; i >= 0; i--)
            {
                string line = buffer[i];
                string identifier = GetIdentifier(line);
                
                // 適切な終了行（2000, 2111, 2101）が見つかったらその次の位置で分割
                if (identifier == "2000" || identifier == "2111" || identifier == "2101")
                {
                    return i + 1;
                }
            }
            
            // 適切な分割点が見つからない場合は元の位置で分割
            return buffer.Count;
        }

        /// <summary>
        /// CSVの最初のフィールド（識別子）を取得
        /// </summary>
        static string GetIdentifier(string line)
        {
            if (string.IsNullOrEmpty(line))
                return "";
                
            // CSVの最初のフィールドを取得（ダブルクォートを考慮）
            if (line.StartsWith("\""))
            {
                int endQuoteIndex = line.IndexOf("\"", 1);
                if (endQuoteIndex > 0)
                {
                    return line.Substring(1, endQuoteIndex - 1);
                }
            }
            else
            {
                int commaIndex = line.IndexOf(",");
                if (commaIndex > 0)
                {
                    return line.Substring(0, commaIndex);
                }
            }
            
            return line;
        }

        /// <summary>
        /// バッファの内容をファイルに書き出す
        /// </summary>
        static void WriteBufferToFile(string[] lines, string inputFileName, string extension, 
                                    string outputDir, Encoding encoding, int fileIndex)
        {
            string outputFileName = $"{inputFileName}_part{fileIndex:D3}{extension}";
            string outputPath = Path.Combine(outputDir, outputFileName);
            
            using (var writer = new StreamWriter(outputPath, false, encoding))
            {
                foreach (string line in lines)
                {
                    writer.WriteLine(line);
                }
            }
            
            Console.WriteLine($"新しいファイルを作成: {outputFileName} ({lines.Length}行)");
        }
    }
}
