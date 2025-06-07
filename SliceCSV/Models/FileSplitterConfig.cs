using System.Text;

namespace SliceCSV.Models
{
    /// <summary>
    /// ファイル分割の設定
    /// </summary>
    public class FileSplitterConfig
    {
        /// <summary>
        /// ファイルあたりの行数
        /// </summary>
        public int RowsPerFile { get; set; } = 1000;

        /// <summary>
        /// ファイルエンコーディング
        /// </summary>
        public string EncodingName { get; set; } = "shift_jis";

        /// <summary>
        /// 分割点を調整する識別子
        /// </summary>
        public string[] SplitPointIdentifiers { get; set; } = { "2000", "2111", "2101" };

        /// <summary>
        /// 進捗報告の間隔（行数）
        /// </summary>
        public int ProgressReportInterval { get; set; } = 1000;

        /// <summary>
        /// 出力ファイル名のフォーマット
        /// </summary>
        public string OutputFileNameFormat { get; set; } = "{0}_part{1:D3}{2}";

        /// <summary>
        /// エンコーディングを取得
        /// </summary>
        public Encoding GetEncoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding(EncodingName);
        }

        /// <summary>
        /// デフォルト設定を取得
        /// </summary>
        public static FileSplitterConfig Default => new();
    }

    /// <summary>
    /// ファイル分割処理の結果
    /// </summary>
    public class FileSplitResult
    {
        /// <summary>
        /// 処理が成功したかどうか
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// エラーメッセージ（失敗時）
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// 処理した総行数
        /// </summary>
        public int TotalProcessedRows { get; }

        /// <summary>
        /// 作成されたファイル数
        /// </summary>
        public int CreatedFileCount { get; }

        private FileSplitResult(bool isSuccess, string? errorMessage, int totalProcessedRows, int createdFileCount)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            TotalProcessedRows = totalProcessedRows;
            CreatedFileCount = createdFileCount;
        }

        /// <summary>
        /// 成功結果を作成
        /// </summary>
        /// <param name="totalProcessedRows">処理した総行数</param>
        /// <param name="createdFileCount">作成されたファイル数</param>
        /// <returns>成功結果</returns>
        public static FileSplitResult Success(int totalProcessedRows, int createdFileCount)
        {
            return new FileSplitResult(true, null, totalProcessedRows, createdFileCount);
        }

        /// <summary>
        /// 失敗結果を作成
        /// </summary>
        /// <param name="errorMessage">エラーメッセージ</param>
        /// <returns>失敗結果</returns>
        public static FileSplitResult Failure(string errorMessage)
        {
            return new FileSplitResult(false, errorMessage, 0, 0);
        }
    }
} 