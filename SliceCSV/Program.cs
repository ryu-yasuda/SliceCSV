using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;

namespace SliceCSV
{
    /// <summary>
    /// プログラムエントリーポイント
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// アプリケーションのメインエントリーポイント
        /// </summary>
        [STAThread]
        static void Main()
        {
            // アプリケーションの初期化
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                // メインフォームを実行
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                // 予期しないエラーの処理
                MessageBox.Show(
                    $"予期しないエラーが発生しました: {ex.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
