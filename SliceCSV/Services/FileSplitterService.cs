using System.Text;
using SliceCSV.Models;

namespace SliceCSV.Services
{
    /// <summary>
    /// ファイル分割サービスの実装
    /// </summary>
    public class FileSplitterService : IFileSplitterService
    {
        private readonly FileSplitterConfig _config;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="config">設定</param>
        public FileSplitterService(FileSplitterConfig? config = null)
        {
            _config = config ?? FileSplitterConfig.Default;
        }

        /// <inheritdoc />
        public async Task<FileSplitResult> SplitFileAsync(string inputFilePath, string outputDirectory, IProgress<string>? progressReporter = null)
        {
            try
            {
                ValidateInputs(inputFilePath, outputDirectory);
                await EnsureOutputDirectoryExistsAsync(outputDirectory, progressReporter);

                var result = await ProcessFileAsync(inputFilePath, outputDirectory, progressReporter);
                
                progressReporter?.Report($"処理完了: 合計 {result.TotalProcessedRows} 行を {result.CreatedFileCount} ファイルに分割しました");
                
                return result;
            }
            catch (Exception ex)
            {
                return FileSplitResult.Failure(ex.Message);
            }
        }

        /// <summary>
        /// 入力値の検証
        /// </summary>
        private static void ValidateInputs(string inputFilePath, string outputDirectory)
        {
            if (string.IsNullOrWhiteSpace(inputFilePath))
                throw new ArgumentException("入力ファイルパスが指定されていません。", nameof(inputFilePath));

            if (string.IsNullOrWhiteSpace(outputDirectory))
                throw new ArgumentException("出力ディレクトリが指定されていません。", nameof(outputDirectory));

            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException($"入力ファイルが見つかりません: {inputFilePath}");
        }

        /// <summary>
        /// 出力ディレクトリの作成
        /// </summary>
        private static async Task EnsureOutputDirectoryExistsAsync(string outputDirectory, IProgress<string>? progressReporter)
        {
            if (!Directory.Exists(outputDirectory))
            {
                await Task.Run(() => Directory.CreateDirectory(outputDirectory));
                progressReporter?.Report($"出力ディレクトリを作成しました: {outputDirectory}");
            }
        }

        /// <summary>
        /// ファイル処理のメイン処理
        /// </summary>
        private async Task<FileSplitResult> ProcessFileAsync(string inputFilePath, string outputDirectory, IProgress<string>? progressReporter)
        {
            var encoding = _config.GetEncoding();
            var fileInfo = new FileInfo(inputFilePath);
            var inputFileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            var extension = fileInfo.Extension;

            using var reader = new StreamReader(inputFilePath, encoding);
            
            var currentBuffer = new List<string>();
            var currentFileIndex = 1;
            var totalProcessedRows = 0;

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                currentBuffer.Add(line);
                totalProcessedRows++;

                if (currentBuffer.Count >= _config.RowsPerFile)
                {
                    var splitIndex = FindValidSplitPoint(currentBuffer);
                    
                    await WriteBufferToFileAsync(
                        currentBuffer.Take(splitIndex).ToArray(), 
                        inputFileName, 
                        extension, 
                        outputDirectory, 
                        encoding, 
                        currentFileIndex);

                    progressReporter?.Report($"新しいファイルを作成: {inputFileName}_part{currentFileIndex:D3}{extension} ({splitIndex}行)");
                    
                    currentBuffer = currentBuffer.Skip(splitIndex).ToList();
                    currentFileIndex++;
                }
            }

            // 残りのバッファを最終ファイルに書き出し
            if (currentBuffer.Count > 0)
            {
                await WriteBufferToFileAsync(
                    currentBuffer.ToArray(), 
                    inputFileName, 
                    extension, 
                    outputDirectory, 
                    encoding, 
                    currentFileIndex);
                
                progressReporter?.Report($"新しいファイルを作成: {inputFileName}_part{currentFileIndex:D3}{extension} ({currentBuffer.Count}行)");
            }

            return FileSplitResult.Success(totalProcessedRows, currentFileIndex);
        }

        /// <summary>
        /// 複合仕訳を考慮した適切な分割点を見つける
        /// </summary>
        private int FindValidSplitPoint(List<string> buffer)
        {
            for (int i = buffer.Count - 1; i >= 0; i--)
            {
                var identifier = GetIdentifier(buffer[i]);
                
                if (_config.SplitPointIdentifiers.Contains(identifier))
                {
                    return i + 1;
                }
            }
            
            return buffer.Count;
        }

        /// <summary>
        /// テキストファイルの最初のフィールド（識別子）を取得
        /// </summary>
        private static string GetIdentifier(string line)
        {
            if (string.IsNullOrEmpty(line))
                return string.Empty;
                
            // ダブルクォートで囲まれている場合
            if (line.StartsWith('"'))
            {
                var endQuoteIndex = line.IndexOf('"', 1);
                if (endQuoteIndex > 0)
                {
                    return line[1..endQuoteIndex];
                }
            }
            else
            {
                // カンマ区切りの場合
                var commaIndex = line.IndexOf(',');
                if (commaIndex > 0)
                {
                    return line[..commaIndex];
                }
            }
            
            return line;
        }

        /// <summary>
        /// バッファの内容をファイルに書き出す
        /// </summary>
        private async Task WriteBufferToFileAsync(string[] lines, string inputFileName, string extension, 
                                                 string outputDir, Encoding encoding, int fileIndex)
        {
            var outputFileName = string.Format(_config.OutputFileNameFormat, inputFileName, fileIndex, extension);
            var outputPath = Path.Combine(outputDir, outputFileName);
            
            using var writer = new StreamWriter(outputPath, false, encoding);
            
            foreach (var line in lines)
            {
                await writer.WriteLineAsync(line);
            }
        }
    }
} 