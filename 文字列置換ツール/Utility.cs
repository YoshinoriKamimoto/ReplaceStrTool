using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace 文字列置換ツール
{
    /// <summary>
    /// 共通処理を実装するクラス
    /// </summary>
    internal static class Utility
    {
        /// <summary>
        /// 要素が設定ファイルに存在するかチェックするメソッド
        /// </summary>
        /// <param name="tagName">チェックしたい要素のタグ名称</param>
        /// <returns>true：要素あり、false：要素なし</returns>
        public static bool IsElement(string tagName)
        {
            // XMLファイルを読み込む
            XmlDocument doc = new XmlDocument();
            doc.Load(Program.configPath);

            // パラメータで渡されたタグ名称の要素数を取得
            XmlNodeList lists = doc.GetElementsByTagName(tagName);

            // 取得した要素数をチェック
            if (lists.Count > 0)
            {
                // 要素あり
                return true;
            }
            else
            {
                // 要素なし
                return false;
            }
        }



        /// <summary>
        /// 明示的にエラーを発生させ、呼び出し元に投げるメソッド
        /// </summary>
        /// <param name="msg">通知したいエラーメッセージ</param>
        public static void MakeError(string msg)
        {
            Exception ex = new Exception($"\n{msg}\n\n");
            throw ex;
        }

    }
}
