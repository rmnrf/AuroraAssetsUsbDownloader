// Adapted from AuroraAssetEditor - AuroraDbManager.cs by Swizzy
// Reads Aurora's Content.db (SQLite) to get the list of installed games

namespace AuroraOfflineAssets.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SQLite;
    using System.Globalization;
    using System.Linq;

    internal static class ContentDbReader
    {
        private static SQLiteConnection? _content;

        private static void ConnectToContent(string path)
        {
            if (_content != null)
                _content.Close();
            _content = new SQLiteConnection("Data Source=\"" + path + "\";Version=3;");
            _content.Open();
        }

        private static DataTable GetContentDataTable(string sql)
        {
            var dt = new DataTable();
            try
            {
                var cmd = new SQLiteCommand(sql, _content);
                using (var reader = cmd.ExecuteReader())
                    dt.Load(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [ОШИБКА БД] {ex.Message}");
            }
            return dt;
        }

        public static IEnumerable<ContentItem> GetDbTitles(string path)
        {
            ConnectToContent(path);
            var ret = GetContentItems().Select(item => item).ToList();
            _content?.Close();
            return ret;
        }

        private static IEnumerable<ContentItem> GetContentItems()
        {
            return GetContentDataTable("SELECT * FROM ContentItems")
                .Select()
                .Select(row => new ContentItem(row))
                .ToArray();
        }

        internal class ContentItem
        {
            public ContentItem(DataRow row)
            {
                DatabaseId = ((int)((long)row["Id"])).ToString("X08");
                TitleId = ((int)((long)row["TitleId"])).ToString("X08");
                MediaId = ((int)((long)row["MediaId"])).ToString("X08");
                var discNum = (int)((long)row["DiscNum"]);
                if (discNum <= 0)
                    discNum = 1;
                DiscNum = discNum.ToString(CultureInfo.InvariantCulture);
                TitleName = (string)row["TitleName"];
            }

            public string TitleId { get; private set; }
            public string MediaId { get; private set; }
            public string DiscNum { get; private set; }
            public string TitleName { get; private set; }
            public string DatabaseId { get; private set; }

            /// <summary>
            /// GameData folder name: {TitleID}_{DatabaseId}
            /// Same format as used by FTPOperations in AuroraAssetEditor
            /// </summary>
            public string GameDataPath { get { return string.Format("{0}_{1}", TitleId, DatabaseId); } }

            /// <summary>
            /// Boxart asset filename
            /// </summary>
            public string BoxartFileName { get { return string.Format("GC{0}.asset", TitleId); } }

            /// <summary>
            /// Background asset filename
            /// </summary>
            public string BackgroundFileName { get { return string.Format("BK{0}.asset", TitleId); } }

            /// <summary>
            /// Icon/Banner asset filename
            /// </summary>
            public string IconBannerFileName { get { return string.Format("GL{0}.asset", TitleId); } }

            /// <summary>
            /// Screenshots asset filename
            /// </summary>
            public string ScreenshotsFileName { get { return string.Format("SS{0}.asset", TitleId); } }
        }
    }
}
