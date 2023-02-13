using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;

namespace 文字列置換ツール
{
    public partial class Form1 : Form
    {
        private Logger logger;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }



        /// <summary>
        /// フォームShownイベント
        /// </summary>
        private void From1_Shown(object sender, EventArgs e)
        {
            // ログ操作用インスタンスを生成
            logger = new Logger();

            // 設定ファイルからログ出力先を取得
            logger.GetLogFolderPath();

            // 過去ログ削除
            logger.Delete();
        }


        /// <summary>
        /// 実行ボタンClickイベント
        /// </summary>
        private void RunButton_Click(object sender, EventArgs e)
        {
            logger.Write("INFO", "■■■■■■■処理開始■■■■■■■");


            // 元ファイルからファイルのコピーを作成
            // 元ファイルのパスを取得してチェック
            string sourceFilePath = FileNameTextBox.Text;
            string targetFilePath;
            if (sourceFilePath == "")
            {
                MessageBox.Show("ファイルを選択してください。",
                                "エラー",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            // コピーを実行
            try
            {
                targetFilePath = CopyFile(sourceFilePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("出力用ファイルの作成エラー");
                logger.Write("ERROR", $"出力用ファイルの作成エラー\n{ex}");
                MessageBox.Show($"出力用ファイルの作成中にエラーが発生しました。\n\n{ex}",
                                "エラー",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }


            // 設定ファイルから置換文字列を取得
            // 置換前文字列、置換後文字列を保持するコレクション
            Dictionary<string, string> dic = new Dictionary<string, string>();
            try
            {
                dic = GetReplaceValues();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"設定ファイルから置換文字列取得エラー\n{ex}");
                logger.Write("ERROR", $"設定ファイルから置換文字列取得エラー\n{ex}");
                MessageBox.Show($"設定ファイルから置換文字列取得中にエラーが発生しました。\n\n{ex}",
                                "エラー",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }


            // コピーしたファイルの文字列を置換
            try
            {
                foreach (KeyValuePair<string, string> kvp in dic)
                {
                    Debug.WriteLine($"置換前文字列：{kvp.Key}");
                    Debug.WriteLine($"置換後文字列：{kvp.Value}");
                    ReplaceStr(kvp.Key, kvp.Value, targetFilePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"文字列置換エラー\n{ex}");
                logger.Write("ERROR", $"文字列置換エラー\n{ex}");
                MessageBox.Show($"文字列置換エラー\n\n{ex}",
                                "エラー",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }



            // 置換完了
            Debug.WriteLine("正常終了");
            logger.Write("INFO", "■■■■■■■正常終了■■■■■■");
            MessageBox.Show("置換が完了しました。",
                            "完了",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            Process.Start("EXPLORER.EXE", $@"/select,{targetFilePath}");
        }



        /// <summary>
        /// ファイル選択ボタンClickイベント
        /// </summary>
        private void SelectFileButton_Click(object sender, EventArgs e)
        {
            // ファイルダイアログのオプションを設定
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            ofd.Filter = "すべてのファイル(*.*)|*.*";
            ofd.FilterIndex = 0;
            ofd.Title = "ファイルを選択";

            // ファイルダイアログを開き、OKボタンが選択された場合、選択されたファイル名を取得する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                FileNameTextBox.Text = ofd.FileName;
            }
        }



        /// <summary>
        /// 元ファイルからコピーを作成するメソッド
        /// </summary>
        private string CopyFile(string sourcePath)
        {
            // コピー先パスを取得
            string sourceFileName = sourcePath.Substring(sourcePath.LastIndexOf(@"\") + 1);
            string targetPath = sourcePath.Substring(0, sourcePath.LastIndexOf(@"\") + 1) + "replace_" + sourceFileName;

            // 元ファイルのフォルダ直下にコピーファイルを作成
            try
            {
                File.Copy(sourcePath, targetPath, true);
                return targetPath;
            }
            catch
            {
                throw;
            }
        }



        /// <summary>
        /// 設定ファイルから置換文字列情報を取得するメソッド
        /// </summary>
        private Dictionary<string, string> GetReplaceValues()
        {
            // 戻り値用コレクション　Key：置換前文字列、Value：置換後文字列
            Dictionary<string, string> dic = new Dictionary<string, string>();

            // XMLファイルを読み込み
            XElement xml = XElement.Load(Program.configPath);

            // 取得したい要素の存在チェック
            if (Utility.IsElement("ReplaceStr") == false)
            {
                Utility.MakeError($"設定ファイルに<ReplaceStr>タグが見つかりませんでした。\nファイルを確認してください。\n{Program.configPath}");
            }


            // 要素の値を取得する
            try
            {
                IEnumerable<XElement> replaceInfo = xml.Elements("ReplaceStr");
                foreach (XElement replace in replaceInfo)
                {
                    string oldValue = replace.Element("OldValue").Value;
                    string newValue = replace.Element("NewValue").Value;
                    dic.Add(oldValue, newValue);
                }
                return dic;
            }
            catch
            {
                throw;
            }
        }



        /// <summary>
        /// 文字列を置換するメソッド
        /// </summary>
        private void ReplaceStr(string oldValue, string newValue, string targetPath)
        {
            // ファイル書き込みの際に使用する書き出し用文字列を保持
            StringBuilder sb = new StringBuilder();

            // 元ファイルを読み込み
            try
            {
                using (StreamReader sr = new StreamReader(targetPath))
                {
                    string line;

                    // 全行読み込み
                    while ((line = sr.ReadLine()) != null)
                    {
                        // 置換対象の文字列が含まれていた場合、置換して書き出し用文字列に追加
                        if (line.Contains(oldValue))
                        {
                            sb.Append(line.Replace(oldValue, newValue) + "\n");
                        }
                        else
                        {
                            // 置換対象の文字列が含まれていなかった場合、そのまま書き出し用文字列に追加
                            sb.Append(line + "\n");
                        }
                    }
                }
            }
            catch
            {
                throw;
            }


            // 出力ファイルに書き出し用文字列を書き込む
            try
            {
                Debug.WriteLine($"書き込み文字列：{sb}");
                using (StreamWriter sw = new StreamWriter(targetPath))
                {
                    // 書き込む
                    sw.Write(sb);
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
