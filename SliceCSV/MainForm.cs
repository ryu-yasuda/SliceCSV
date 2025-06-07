using System.ComponentModel;
using System.Drawing;
using SliceCSV.Services;
using SliceCSV.UI;
using SliceCSV.Models;

namespace SliceCSV
{
    /// <summary>
    /// メインフォーム
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly IFileSplitterService _fileSplitterService;
        
        // UI コントロール
        private TextBox _inputFileTextBox = null!;
        private TextBox _outputDirTextBox = null!;
        private Button _browseInputButton = null!;
        private Button _browseOutputButton = null!;
        private Button _processButton = null!;
        private ProgressBar _progressBar = null!;
        private Label _statusLabel = null!;
        private RichTextBox _logTextBox = null!;
        private BackgroundWorker _backgroundWorker = null!;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainForm() : this(new FileSplitterService())
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fileSplitterService">ファイル分割サービス</param>
        public MainForm(IFileSplitterService fileSplitterService)
        {
            _fileSplitterService = fileSplitterService ?? throw new ArgumentNullException(nameof(fileSplitterService));
            
            InitializeComponent();
            SetupBackgroundWorker();
        }

        /// <summary>
        /// コンポーネント初期化
        /// </summary>
        private void InitializeComponent()
        {
            InitializeWindow();
            InitializeControls();
            LayoutControls();
        }

        /// <summary>
        /// ウィンドウの初期化
        /// </summary>
        private void InitializeWindow()
        {
            Text = UIConstants.Messages.WindowTitle;
            Size = UIConstants.WindowSize;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
        }

        /// <summary>
        /// コントロールの初期化
        /// </summary>
        private void InitializeControls()
        {
            // 入力ファイル関連
            var inputLabel = CreateLabel(UIConstants.Messages.InputFileLabel, new Point(UIConstants.Margins.Standard, 20));
            
            _inputFileTextBox = CreateReadOnlyTextBox(new Point(UIConstants.Margins.Standard, 45));
            
            _browseInputButton = CreateButton(
                UIConstants.Messages.BrowseButtonText, 
                new Point(450, 45), 
                BrowseInputButton_Click);

            // 出力ディレクトリ関連
            var outputLabel = CreateLabel(UIConstants.Messages.OutputDirectoryLabel, new Point(UIConstants.Margins.Standard, 80));
            
            _outputDirTextBox = CreateReadOnlyTextBox(new Point(UIConstants.Margins.Standard, 105));
            
            _browseOutputButton = CreateButton(
                UIConstants.Messages.BrowseButtonText, 
                new Point(450, 105), 
                BrowseOutputButton_Click);

            // 処理実行ボタン
            _processButton = CreateProcessButton();

            // プログレスバー
            _progressBar = CreateProgressBar();

            // ステータスラベル
            _statusLabel = CreateStatusLabel();

            // ログ表示
            var logLabel = CreateLabel(UIConstants.Messages.LogLabel, new Point(UIConstants.Margins.Standard, 245));
            _logTextBox = CreateLogTextBox();

            // コントロールを追加
            Controls.AddRange(new Control[] {
                inputLabel, _inputFileTextBox, _browseInputButton,
                outputLabel, _outputDirTextBox, _browseOutputButton,
                _processButton, _progressBar, _statusLabel,
                logLabel, _logTextBox
            });
        }

        /// <summary>
        /// レイアウト調整（将来的な拡張用）
        /// </summary>
        private void LayoutControls()
        {
            // 必要に応じてレイアウト調整を行う
        }

        /// <summary>
        /// ラベル作成
        /// </summary>
        private static Label CreateLabel(string text, Point location)
        {
            return new Label
            {
                Text = text,
                Location = location,
                Size = UIConstants.ControlSizes.LabelSize,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        /// <summary>
        /// 読み取り専用テキストボックス作成
        /// </summary>
        private static TextBox CreateReadOnlyTextBox(Point location)
        {
            return new TextBox
            {
                Location = location,
                Size = UIConstants.ControlSizes.TextBoxSize,
                ReadOnly = true
            };
        }

        /// <summary>
        /// ボタン作成
        /// </summary>
        private static Button CreateButton(string text, Point location, EventHandler clickHandler)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = UIConstants.ControlSizes.ButtonSize
            };
            button.Click += clickHandler;
            return button;
        }

        /// <summary>
        /// 処理実行ボタン作成
        /// </summary>
        private Button CreateProcessButton()
        {
            var button = new Button
            {
                Text = UIConstants.Messages.ProcessButtonText,
                Location = new Point(UIConstants.Margins.Standard, 140),
                Size = UIConstants.ControlSizes.LargeButtonSize,
                BackColor = UIConstants.Colors.ProcessButtonBackground,
                Font = UIConstants.Fonts.ProcessButtonFont
            };
            button.Click += ProcessButton_Click;
            return button;
        }

        /// <summary>
        /// プログレスバー作成
        /// </summary>
        private static ProgressBar CreateProgressBar()
        {
            return new ProgressBar
            {
                Location = new Point(UIConstants.Margins.Standard, 185),
                Size = UIConstants.ControlSizes.ProgressBarSize,
                Visible = false
            };
        }

        /// <summary>
        /// ステータスラベル作成
        /// </summary>
        private static Label CreateStatusLabel()
        {
            return new Label
            {
                Text = UIConstants.Messages.WaitingForProcess,
                Location = new Point(UIConstants.Margins.Standard, 215),
                Size = UIConstants.ControlSizes.StatusLabelSize,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        /// <summary>
        /// ログテキストボックス作成
        /// </summary>
        private static RichTextBox CreateLogTextBox()
        {
            return new RichTextBox
            {
                Location = new Point(UIConstants.Margins.Standard, 270),
                Size = UIConstants.ControlSizes.LogTextBoxSize,
                ReadOnly = true,
                BackColor = UIConstants.Colors.LogTextBoxBackground
            };
        }

        /// <summary>
        /// BackgroundWorkerの設定
        /// </summary>
        private void SetupBackgroundWorker()
        {
            _backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false
            };
            _backgroundWorker.DoWork += BackgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }

        /// <summary>
        /// 入力ファイル参照ボタンクリック
        /// </summary>
        private void BrowseInputButton_Click(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = UIConstants.Messages.FileDialogFilter,
                Title = UIConstants.Messages.FileDialogTitle
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _inputFileTextBox.Text = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// 出力ディレクトリ参照ボタンクリック
        /// </summary>
        private void BrowseOutputButton_Click(object? sender, EventArgs e)
        {
            using var folderBrowserDialog = new FolderBrowserDialog
            {
                Description = UIConstants.Messages.FolderDialogDescription,
                ShowNewFolderButton = true
            };

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                _outputDirTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        /// <summary>
        /// 処理実行ボタンクリック
        /// </summary>
        private void ProcessButton_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            SetProcessingState(true);
            
            var request = new ProcessRequest
            {
                InputFilePath = _inputFileTextBox.Text,
                OutputDirectory = _outputDirTextBox.Text
            };
            
            _backgroundWorker.RunWorkerAsync(request);
        }

        /// <summary>
        /// 入力値の検証
        /// </summary>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(_inputFileTextBox.Text))
            {
                ShowError(UIConstants.Messages.SelectInputFile);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_outputDirTextBox.Text))
            {
                ShowError(UIConstants.Messages.SelectOutputDirectory);
                return false;
            }

            if (!File.Exists(_inputFileTextBox.Text))
            {
                ShowError(UIConstants.Messages.FileNotExists);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 処理状態の設定
        /// </summary>
        private void SetProcessingState(bool isProcessing)
        {
            _processButton.Enabled = !isProcessing;
            _browseInputButton.Enabled = !isProcessing;
            _browseOutputButton.Enabled = !isProcessing;
            
            if (isProcessing)
            {
                _progressBar.Style = ProgressBarStyle.Marquee;
                _progressBar.Visible = true;
                _statusLabel.Text = UIConstants.Messages.Processing;
                _logTextBox.Clear();
            }
            else
            {
                _progressBar.Visible = false;
            }
        }

        /// <summary>
        /// エラーメッセージ表示
        /// </summary>
        private static void ShowError(string message)
        {
            MessageBox.Show(message, UIConstants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// BackgroundWorker DoWork
        /// </summary>
        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (e.Argument is not ProcessRequest request)
            {
                e.Result = "無効な処理要求です。";
                return;
            }

            try
            {
                var progress = new Progress<string>(message =>
                {
                    _backgroundWorker.ReportProgress(0, message);
                });

                // 同期的にTask.Runを使用して実行
                var task = Task.Run(async () => await _fileSplitterService.SplitFileAsync(
                    request.InputFilePath, 
                    request.OutputDirectory, 
                    progress));

                var result = task.Result; // 同期的に待機

                e.Result = result.IsSuccess 
                    ? UIConstants.Messages.ProcessCompleted 
                    : $"エラーが発生しました: {result.ErrorMessage}";
            }
            catch (Exception ex)
            {
                e.Result = $"予期しないエラーが発生しました: {ex.Message}";
            }
        }

        /// <summary>
        /// BackgroundWorker ProgressChanged
        /// </summary>
        private void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string message)
            {
                _logTextBox.AppendText(message + Environment.NewLine);
                _logTextBox.ScrollToCaret();
            }
        }

        /// <summary>
        /// BackgroundWorker RunWorkerCompleted
        /// </summary>
        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            SetProcessingState(false);
            
            var result = e.Result as string ?? "不明なエラーが発生しました。";
            _statusLabel.Text = result;
            
            // 結果をログに出力
            _logTextBox.AppendText(Environment.NewLine);
            _logTextBox.AppendText("=== 処理結果 ===" + Environment.NewLine);
            _logTextBox.AppendText(result + Environment.NewLine);
            _logTextBox.ScrollToCaret();
            
            if (result.Contains("エラー"))
            {
                ShowError(result);
            }
        }

        /// <summary>
        /// 処理要求
        /// </summary>
        private class ProcessRequest
        {
            public string InputFilePath { get; set; } = string.Empty;
            public string OutputDirectory { get; set; } = string.Empty;
        }
    }
} 