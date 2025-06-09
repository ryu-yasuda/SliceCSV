using System.Drawing;

namespace SliceCSV.UI
{
    /// <summary>
    /// UI関連の定数
    /// </summary>
    public static class UIConstants
    {
        /// <summary>
        /// ウィンドウサイズ
        /// </summary>
        public static readonly Size WindowSize = new(600, 500);

        /// <summary>
        /// マージン
        /// </summary>
        public static class Margins
        {
            public const int Standard = 20;
            public const int Small = 10;
        }

        /// <summary>
        /// コントロールサイズ
        /// </summary>
        public static class ControlSizes
        {
            public static readonly Size TextBoxSize = new(420, 23);
            public static readonly Size ButtonSize = new(100, 23);
            public static readonly Size LargeButtonSize = new(530, 35);
            public static readonly Size ProgressBarSize = new(530, 23);
            public static readonly Size LogTextBoxSize = new(530, 180);
            public static readonly Size LabelSize = new(120, 23);
            public static readonly Size StatusLabelSize = new(530, 23);
        }

        /// <summary>
        /// カラー
        /// </summary>
        public static class Colors
        {
            public static readonly Color ProcessButtonBackground = Color.LightBlue;
            public static readonly Color LogTextBoxBackground = Color.WhiteSmoke;
        }

        /// <summary>
        /// メッセージ
        /// </summary>
        public static class Messages
        {
            public const string SelectInputFile = "入力インポートファイルを選択してください。";
            public const string SelectOutputDirectory = "出力先ディレクトリを選択してください。";
            public const string FileNotExists = "選択された入力ファイルが存在しません。";
            public const string Processing = "処理中...";
            public const string WaitingForProcess = "処理待機中";
            public const string ProcessCompleted = "処理が正常に完了しました。";
            
            public const string WindowTitle = "インポートファイル分割ツール";
            public const string InputFileLabel = "分割対象ファイル:";
            public const string OutputDirectoryLabel = "出力先ディレクトリ:";
            public const string BrowseButtonText = "参照";
            public const string ProcessButtonText = "分割を実行";
            public const string LogLabel = "処理ログ:";
            
            public const string FileDialogFilter = "インポートファイル (*.txt;*.csv)|*.txt;*.csv|TXTファイル (*.txt)|*.txt|CSVファイル (*.csv)|*.csv|すべてのファイル (*.*)|*.*";
            public const string FileDialogTitle = "分割するインポートファイルを選択してください";
            public const string FolderDialogDescription = "分割されたファイルの保存先ディレクトリを選択してください";
            
            public const string ErrorTitle = "エラー";
            public const string CompletionTitle = "完了";
        }

        /// <summary>
        /// フォント
        /// </summary>
        public static class Fonts
        {
            public static readonly Font ProcessButtonFont = new("Microsoft Sans Serif", 10F, FontStyle.Bold);
        }
    }
} 