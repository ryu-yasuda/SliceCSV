using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;

namespace SliceCSV
{
    // メインフォームクラス
    public partial class MainForm : Form
    {
        private const int ROWS_PER_FILE = 1000;
        private TextBox inputFileTextBox = null!;
        private TextBox outputDirTextBox = null!;
        private Button browseInputButton = null!;
        private Button browseOutputButton = null!;
        private Button processButton = null!;
        private ProgressBar progressBar = null!;
        private Label statusLabel = null!;
        private RichTextBox logTextBox = null!;
        private BackgroundWorker backgroundWorker = null!;

        public MainForm()
        {
            InitializeComponent();
            SetupBackgroundWorker();
        }

        private void InitializeComponent()
        {
            this.Text = "インポートファイル分割ツール";
            this.Size = new System.Drawing.Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // 入力ファイル選択
            var inputLabel = new Label
            {
                Text = "分割対象ファイル:",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(120, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            inputFileTextBox = new TextBox
            {
                Location = new System.Drawing.Point(20, 45),
                Size = new System.Drawing.Size(420, 23),
                ReadOnly = true
            };

            browseInputButton = new Button
            {
                Text = "参照",
                Location = new System.Drawing.Point(450, 45),
                Size = new System.Drawing.Size(100, 23)
            };
            browseInputButton.Click += BrowseInputButton_Click;

            // 出力ディレクトリ選択
            var outputLabel = new Label
            {
                Text = "出力先ディレクトリ:",
                Location = new System.Drawing.Point(20, 80),
                Size = new System.Drawing.Size(120, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            outputDirTextBox = new TextBox
            {
                Location = new System.Drawing.Point(20, 105),
                Size = new System.Drawing.Size(420, 23),
                ReadOnly = true
            };

            browseOutputButton = new Button
            {
                Text = "参照",
                Location = new System.Drawing.Point(450, 105),
                Size = new System.Drawing.Size(100, 23)
            };
            browseOutputButton.Click += BrowseOutputButton_Click;

            // 処理実行ボタン
            processButton = new Button
            {
                Text = "分割を実行",
                Location = new System.Drawing.Point(20, 140),
                Size = new System.Drawing.Size(530, 35),
                BackColor = System.Drawing.Color.LightBlue,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold)
            };
            processButton.Click += ProcessButton_Click;

            // プログレスバー
            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(20, 185),
                Size = new System.Drawing.Size(530, 23),
                Visible = false
            };

            // ステータスラベル
            statusLabel = new Label
            {
                Text = "処理待機中",
                Location = new System.Drawing.Point(20, 215),
                Size = new System.Drawing.Size(530, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            // ログ表示
            var logLabel = new Label
            {
                Text = "処理ログ:",
                Location = new System.Drawing.Point(20, 245),
                Size = new System.Drawing.Size(100, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            logTextBox = new RichTextBox
            {
                Location = new System.Drawing.Point(20, 270),
                Size = new System.Drawing.Size(530, 180),
                ReadOnly = true,
                BackColor = System.Drawing.Color.WhiteSmoke
            };

            // コントロールを追加
            this.Controls.AddRange(new Control[] {
                inputLabel, inputFileTextBox, browseInputButton,
                outputLabel, outputDirTextBox, browseOutputButton,
                processButton, progressBar, statusLabel,
                logLabel, logTextBox
            });
        }

        private void SetupBackgroundWorker()
        {
            backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false
            };
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }

        private void BrowseInputButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "TXTファイル (*.txt)|*.txt|すべてのファイル (*.*)|*.*";
                openFileDialog.Title = "分割するTXTファイルを選択してください";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    inputFileTextBox.Text = openFileDialog.FileName;
                }
            }
        }

        private void BrowseOutputButton_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "分割されたファイルの保存先ディレクトリを選択してください";
                folderBrowserDialog.ShowNewFolderButton = true;

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    outputDirTextBox.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void ProcessButton_Click(object sender, EventArgs e)
        {
            // 入力検証
            if (string.IsNullOrWhiteSpace(inputFileTextBox.Text))
            {
                MessageBox.Show("入力TXTファイルを選択してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(outputDirTextBox.Text))
            {
                MessageBox.Show("出力先ディレクトリを選択してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(inputFileTextBox.Text))
            {
                MessageBox.Show("選択された入力ファイルが存在しません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // UIを処理中状態に変更
            processButton.Enabled = false;
            browseInputButton.Enabled = false;
            browseOutputButton.Enabled = false;
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Visible = true;
            statusLabel.Text = "処理中...";
            logTextBox.Clear();

            // バックグラウンドで処理を開始
            var args = new ProcessArgs
            {
                InputFilePath = inputFileTextBox.Text,
                OutputDirectory = outputDirTextBox.Text
            };
            backgroundWorker.RunWorkerAsync(args);
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var args = e.Argument as ProcessArgs;
            
            try
            {
                ProcessTxtFile(args.InputFilePath, args.OutputDirectory, worker);
                e.Result = "処理が正常に完了しました。";
            }
            catch (Exception ex)
            {
                e.Result = $"エラーが発生しました: {ex.Message}";
            }
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string message)
            {
                logTextBox.AppendText(message + Environment.NewLine);
                logTextBox.ScrollToCaret();
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // UIを通常状態に戻す
            processButton.Enabled = true;
            browseInputButton.Enabled = true;
            browseOutputButton.Enabled = true;
            progressBar.Visible = false;
            
            string result = e.Result as string;
            statusLabel.Text = result;
            
            if (result.StartsWith("エラー"))
            {
                MessageBox.Show(result, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(result, "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ProcessTxtFile(string inputTxtPath, string outputDir, BackgroundWorker worker)
        {
            // 出力ディレクトリの作成
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                worker.ReportProgress(0, $"出力ディレクトリを作成しました: {outputDir}");
            }

            string inputFileName = Path.GetFileNameWithoutExtension(inputTxtPath);
            string extension = Path.GetExtension(inputTxtPath);
            
            // Shift_JISエンコーディングを指定してファイルを読み込み
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var encoding = Encoding.GetEncoding("shift_jis");
            
            using (var reader = new StreamReader(inputTxtPath, encoding))
            {
                int currentFileIndex = 1;
                int totalProcessedRows = 0;
                List<string> currentBuffer = new List<string>();

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
                        
                        worker.ReportProgress(0, $"新しいファイルを作成: {inputFileName}_part{currentFileIndex:D3}{extension} ({splitIndex}行)");
                        
                        // 残りの行を次のバッファへ
                        currentBuffer = currentBuffer.Skip(splitIndex).ToList();
                        currentFileIndex++;
                        
                        worker.ReportProgress(0, $"{totalProcessedRows} 行処理済み（分割点調整: {splitIndex}行で分割）");
                    }
                    
                    // 進捗表示（1000行ごと）
                    if (totalProcessedRows % 1000 == 0)
                    {
                        worker.ReportProgress(0, $"{totalProcessedRows} 行処理済み");
                    }
                }

                // 残りのバッファを最終ファイルに書き出し
                if (currentBuffer.Count > 0)
                {
                    WriteBufferToFile(currentBuffer.ToArray(), 
                                    inputFileName, extension, outputDir, encoding, currentFileIndex);
                    worker.ReportProgress(0, $"最終ファイルを作成: {inputFileName}_part{currentFileIndex:D3}{extension} ({currentBuffer.Count}行)");
                }

                // 最終的な進捗表示
                worker.ReportProgress(0, $"処理完了: 合計 {totalProcessedRows} 行を {currentFileIndex} ファイルに分割しました");
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
        /// TXTファイルの最初のフィールド（識別子）を取得
        /// </summary>
        static string GetIdentifier(string line)
        {
            if (string.IsNullOrEmpty(line))
                return "";
                
            // TXTファイルの最初のフィールドを取得（ダブルクォートを考慮）
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
        }
    }

    // 処理パラメータクラス
    public class ProcessArgs
    {
        public string InputFilePath { get; set; } = null!;
        public string OutputDirectory { get; set; } = null!;
    }

    // プログラムエントリーポイント
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
