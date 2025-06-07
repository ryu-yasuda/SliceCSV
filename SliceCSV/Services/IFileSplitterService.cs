using System.ComponentModel;

namespace SliceCSV.Services
{
    /// <summary>
    /// ファイル分割サービスのインターフェース
    /// </summary>
    public interface IFileSplitterService
    {
        /// <summary>
        /// ファイルを指定した行数で分割します
        /// </summary>
        /// <param name="inputFilePath">入力ファイルパス</param>
        /// <param name="outputDirectory">出力ディレクトリ</param>
        /// <param name="progressReporter">進捗レポーター</param>
        /// <returns>分割結果</returns>
        Task<FileSplitResult> SplitFileAsync(string inputFilePath, string outputDirectory, IProgress<string>? progressReporter = null);
    }

    /// <summary>
    /// ファイル分割結果
    /// </summary>
    public class FileSplitResult
    {
        /// <summary>
        /// 成功フラグ
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 処理した行数
        /// </summary>
        public int TotalProcessedRows { get; set; }

        /// <summary>
        /// 作成されたファイル数
        /// </summary>
        public int CreatedFileCount { get; set; }

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 成功結果を作成
        /// </summary>
        public static FileSplitResult Success(int totalRows, int fileCount)
        {
            return new FileSplitResult
            {
                IsSuccess = true,
                TotalProcessedRows = totalRows,
                CreatedFileCount = fileCount
            };
        }

        /// <summary>
        /// 失敗結果を作成
        /// </summary>
        public static FileSplitResult Failure(string errorMessage)
        {
            return new FileSplitResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
} 