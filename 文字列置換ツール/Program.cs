using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 文字列置換ツール
{
    internal static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }


        // 設定ファイルパス
        public const string configPath = @"C:\Users\kamimoto\Desktop\work\開発\_個人用\文字列置換ツール\文字列置換ツール\Config\Config.xml";
    }
}
