// AuroraOfflineAssets — Консольное приложение для скачивания обложек Aurora
// Работает как AuroraAssetEditor, но вместо FTP записывает файлы локально на флешку
//
// Использование:
//   AuroraOfflineAssets.exe <буква флешки>
//
// Пример:
//   AuroraOfflineAssets.exe E
//
// Требует AuroraAsset.dll из AuroraAssetEditor рядом с .exe

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AuroraAssetsUsbDownloader.Classes;

namespace AuroraAssetsUsbDownloader
{
    class Program
    {
        private static bool _hasNativeDll = false;

        static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.Clear();
            Lang.Strings = new Locales.EnglishLocalization();
            PrintHeader();

            Console.WriteLine("  1. English");
            Console.WriteLine("  2. Русский");
            Console.Write("\n  Select language / Выберите язык (1/2): ");

            char langChoice = '\0';
            while (true)
            {
                var langKey = Console.ReadKey(true);
                if (langKey.KeyChar == '1' || langKey.KeyChar == '2')
                {
                    langChoice = langKey.KeyChar;
                    break;
                }
            }
            
            if (langChoice == '2')
            {
                Lang.Strings = new Locales.RussianLocalization();
            }

            Console.Clear();
            PrintHeader();

            // Проверяем наличие нативной DLL
            _hasNativeDll = File.Exists("AuroraAsset.dll");
            if (!_hasNativeDll)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(Lang.Strings.DllNotFound);
                Console.WriteLine(Lang.Strings.DllCopyInstruction);
                Console.WriteLine(Lang.Strings.DllPngWarning);
                Console.ResetColor();
                Console.WriteLine();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(Lang.Strings.DllFound);
                Console.ResetColor();
                Console.WriteLine();
            }

            string driveLetter = "";
            string contentDbPath = "";
            string outputPath = "";

            while (true)
            {
                if (args.Length >= 1 && string.IsNullOrEmpty(driveLetter))
                {
                    driveLetter = args[0];
                }
                else
                {
                    // Интерактивный режим
                    Console.Write(Lang.Strings.EnterDriveLetter);
                    driveLetter = Console.ReadLine()?.Trim() ?? "";
                }

                // Очищаем ввод (могут ввести "E", "E:", "E:\")
                if (driveLetter.Length > 0 && char.IsLetter(driveLetter[0]))
                {
                    driveLetter = driveLetter[0].ToString().ToUpperInvariant();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Lang.Strings.InvalidDriveLetter);
                    Console.ResetColor();
                    driveLetter = "";
                    continue;
                }

                contentDbPath = $@"{driveLetter}:\Aurora\Data\DataBases\Content.db";
                outputPath = $@"{driveLetter}:\Aurora\Data\GameData";

                if (!File.Exists(contentDbPath))
                {
                    string altDbPath = $@"{driveLetter}:\Data\DataBases\Content.db";
                    string rootDbPath = $@"{driveLetter}:\Content.db";

                    if (File.Exists(altDbPath))
                    {
                        contentDbPath = altDbPath;
                        outputPath = $@"{driveLetter}:\Data\GameData";
                        break;
                    }
                    else if (File.Exists(rootDbPath))
                    {
                        contentDbPath = rootDbPath;
                        outputPath = $@"{driveLetter}:\Data\GameData";
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(string.Format(Lang.Strings.DbFoundInRoot, contentDbPath));
                        Console.WriteLine(string.Format(Lang.Strings.CoversWillBeSavedIn, outputPath));
                        Console.ResetColor();
                        break;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(string.Format(Lang.Strings.DeviceOrDbNotFound, driveLetter));
                        Console.WriteLine(Lang.Strings.EnsureUsbInserted);
                        Console.ResetColor();
                        driveLetter = "";
                        continue;
                    }
                }
                
                break;
            }

            // Создаём копию Content.db (AuroraDbManager удаляет файл после чтения!)
            var tempDb = Path.Combine(Path.GetTempPath(), "AuroraContent_" + Guid.NewGuid().ToString("N") + ".db");
            File.Copy(contentDbPath, tempDb, true);

            Console.WriteLine(Lang.Strings.ReadingDb);

            var titles = ContentDbReader.GetDbTitles(tempDb).ToList();

            // Удаляем временную копию
            try { File.Delete(tempDb); } catch { }

            if (titles.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Lang.Strings.NoGamesFound);
                Console.ResetColor();
                return 1;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(string.Format(Lang.Strings.GamesFound, titles.Count));
            Console.ResetColor();
            Console.WriteLine(new string('─', 70));

            // Показываем список игр
            for (int i = 0; i < titles.Count; i++)
            {
                var t = titles[i];
                Console.WriteLine($"  {i + 1,4}. [{t.TitleId}] {t.TitleName}");
            }
            Console.WriteLine(new string('─', 70));

            // Спросить: все или выборочно?
            Console.Write(Lang.Strings.DownloadCoversForAll);
            var answer = Console.ReadLine()?.Trim().ToLowerInvariant();
            
            List<ContentDbReader.ContentItem> selectedTitles;
            if (answer == "n" || answer == "н")
            {
                Console.Write(Lang.Strings.EnterGameNumbers);
                var input = Console.ReadLine()?.Trim() ?? "";
                selectedTitles = ParseSelection(input, titles);
                if (selectedTitles.Count == 0)
                {
                    Console.WriteLine(Lang.Strings.NothingSelected);
                    return 0;
                }
            }
            else
            {
                selectedTitles = titles;
            }

            bool[] downloadOptions = GetDownloadOptions();
            bool wantBoxart = downloadOptions[0];
            bool wantBg = downloadOptions[1];
            bool wantIconBanner = downloadOptions[2];
            bool wantScreenshots = downloadOptions[3];

            Console.Write(Lang.Strings.OverwriteExisting);
            bool forceOverwrite = Console.ReadLine()?.Trim().ToLowerInvariant() == "y";

            // Создаём выходную директорию
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(string.Format(Lang.Strings.StartingDownload, selectedTitles.Count));
            Console.ResetColor();
            Console.WriteLine();

            int success = 0, failed = 0, skipped = 0;
            long totalBytesSaved = 0;

            bool cancelRequested = false;
            Console.CancelKeyPress += (sender, e) =>
            {
                if (!cancelRequested)
                {
                    e.Cancel = true;
                    cancelRequested = true;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(Lang.Strings.CancelRequested);
                    Console.ResetColor();
                }
            };

            for (int i = 0; i < selectedTitles.Count; i++)
            {
                if (cancelRequested)
                    break;

                var title = selectedTitles[i];
                var gameDataDir = Path.Combine(outputPath, title.GameDataPath);

                string indexStr = $"[{i + 1}/{selectedTitles.Count}]";
                string safeTitle = title.TitleName.Length > 45 ? title.TitleName.Substring(0, 42) + "..." : title.TitleName;
                Console.Write($"  {indexStr,-10} {safeTitle,-46} ");

                // Создаём папку GameData для этой игры
                if (!Directory.Exists(gameDataDir))
                    Directory.CreateDirectory(gameDataDir);

                // Helper to check if file is missing or is just a small placeholder (< 20KB)
                bool IsMissingOrPlaceholder(string path)
                {
                    if (forceOverwrite) return true;
                    if (!File.Exists(path)) return true;
                    return new FileInfo(path).Length < 20480; // 20 KB
                }

                bool needsBoxart = wantBoxart && IsMissingOrPlaceholder(Path.Combine(gameDataDir, title.BoxartFileName));
                bool needsBg = wantBg && IsMissingOrPlaceholder(Path.Combine(gameDataDir, title.BackgroundFileName));
                bool needsIconBanner = wantIconBanner && IsMissingOrPlaceholder(Path.Combine(gameDataDir, title.IconBannerFileName));
                bool needsScreenshots = wantScreenshots && IsMissingOrPlaceholder(Path.Combine(gameDataDir, title.ScreenshotsFileName));

                if (!needsBoxart && !needsBg && !needsIconBanner && !needsScreenshots)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(Lang.Strings.SkippedAlreadyExists);
                    Console.ResetColor();
                    skipped++;
                    continue;
                }

                // Индикатор загрузки
                var spinner = new ConsoleSpinner();
                DateTime processStartTime = DateTime.Now;

                // Скачиваем обложку (с archive.org)
                Image? boxart = null;
                Classes.XboxLiveCatalog.CatalogAssets? catalogAssets = null;
                int assetsSaved = 0;

                try
                {
                    if (needsBoxart) boxart = await CoverDownloader.DownloadBoxart(title.TitleId);
                    bool hasBoxart = boxart != null;

                    // Скачиваем дополнительные ресурсы (с Xbox Marketplace)
                    if (needsBg || needsIconBanner || needsScreenshots)
                        catalogAssets = await Classes.XboxLiveCatalog.GetAssets(title.TitleId);
                    
                    // 1. Boxart
                    if (hasBoxart && needsBoxart)
                    {
                        var boxartPath = Path.Combine(gameDataDir, title.BoxartFileName);
                        if (_hasNativeDll)
                        {
                            try
                            {
                                var assetFile = new AuroraAsset.AssetFile();
                                if (assetFile.SetBoxart(boxart!, true))
                                    File.WriteAllBytes(boxartPath, assetFile.FileData);
                                assetsSaved++;
                            }
                            catch { boxart!.Save(Path.Combine(gameDataDir, "boxart.png"), System.Drawing.Imaging.ImageFormat.Png); }
                        }
                        else
                        {
                            boxart!.Save(Path.Combine(gameDataDir, "boxart.png"), System.Drawing.Imaging.ImageFormat.Png);
                            assetsSaved++;
                        }
                        boxart?.Dispose();
                    }

                    // 2. Background
                    if (catalogAssets != null && catalogAssets.Backgrounds.Count > 0 && needsBg)
                    {
                        var bkPath = Path.Combine(gameDataDir, title.BackgroundFileName);
                        var bkImg = await Classes.XboxLiveCatalog.DownloadImage(catalogAssets.Backgrounds[0]);
                        if (bkImg != null)
                        {
                            if (_hasNativeDll)
                            {
                                try
                                {
                                    var assetFile = new AuroraAsset.AssetFile();
                                    if (assetFile.SetBackground(bkImg, true))
                                        File.WriteAllBytes(bkPath, assetFile.FileData);
                                    assetsSaved++;
                                }
                                catch { bkImg.Save(Path.Combine(gameDataDir, "background.png"), System.Drawing.Imaging.ImageFormat.Png); }
                            }
                            else
                            {
                                bkImg.Save(Path.Combine(gameDataDir, "background.png"), System.Drawing.Imaging.ImageFormat.Png);
                                assetsSaved++;
                            }
                            bkImg.Dispose();
                        }
                    }

                    // 3. Icon & Banner
                    if (catalogAssets != null && (catalogAssets.Icons.Count > 0 || catalogAssets.Banners.Count > 0) && needsIconBanner)
                    {
                        var glPath = Path.Combine(gameDataDir, title.IconBannerFileName);
                        Image? iconImg = catalogAssets.Icons.Count > 0 ? await Classes.XboxLiveCatalog.DownloadImage(catalogAssets.Icons[0]) : null;
                        Image? bannerImg = catalogAssets.Banners.Count > 0 ? await Classes.XboxLiveCatalog.DownloadImage(catalogAssets.Banners[0]) : null;

                        if (iconImg != null || bannerImg != null)
                        {
                            if (_hasNativeDll)
                            {
                                try
                                {
                                    var assetFile = new AuroraAsset.AssetFile();
                                    if (iconImg != null) assetFile.SetIcon(iconImg, true);
                                    if (bannerImg != null) assetFile.SetBanner(bannerImg, true);
                                    File.WriteAllBytes(glPath, assetFile.FileData);
                                    assetsSaved++;
                                }
                                catch 
                                { 
                                    iconImg?.Save(Path.Combine(gameDataDir, "icon.png"), System.Drawing.Imaging.ImageFormat.Png); 
                                    bannerImg?.Save(Path.Combine(gameDataDir, "banner.png"), System.Drawing.Imaging.ImageFormat.Png); 
                                }
                            }
                            else
                            {
                                iconImg?.Save(Path.Combine(gameDataDir, "icon.png"), System.Drawing.Imaging.ImageFormat.Png);
                                bannerImg?.Save(Path.Combine(gameDataDir, "banner.png"), System.Drawing.Imaging.ImageFormat.Png);
                                assetsSaved++;
                            }
                            iconImg?.Dispose();
                            bannerImg?.Dispose();
                        }
                    }

                    // 4. Screenshots
                    if (catalogAssets != null && catalogAssets.Screenshots.Count > 0 && needsScreenshots)
                    {
                        var ssPath = Path.Combine(gameDataDir, title.ScreenshotsFileName);
                        if (_hasNativeDll)
                        {
                            try
                            {
                                var assetFile = new AuroraAsset.AssetFile();
                                int count = 0;
                                for (int s = 0; s < catalogAssets.Screenshots.Count && s < 20; s++)
                                {
                                    var ssImg = await Classes.XboxLiveCatalog.DownloadImage(catalogAssets.Screenshots[s]);
                                    if (ssImg != null)
                                    {
                                        assetFile.SetScreenshot(ssImg, s + 1, true);
                                        ssImg.Dispose();
                                        count++;
                                    }
                                }
                                if (count > 0)
                                {
                                    File.WriteAllBytes(ssPath, assetFile.FileData);
                                    assetsSaved++;
                                }
                            }
                            catch { }
                        }
                    }

                    spinner.Dispose();

                    long gameBytesSaved = 0;
                    try
                    {
                        if (Directory.Exists(gameDataDir))
                        {
                            foreach (var file in new DirectoryInfo(gameDataDir).GetFiles())
                            {
                                if (file.LastWriteTime >= processStartTime.AddSeconds(-5))
                                    gameBytesSaved += file.Length;
                            }
                        }
                    }
                    catch { }

                    if (assetsSaved > 0)
                    {
                        totalBytesSaved += gameBytesSaved;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(string.Format(Lang.Strings.ResourcesDownloaded, assetsSaved, FormatSize(gameBytesSaved)));
                        Console.ResetColor();
                        success++;
                    }
                    else
                    {
                        bool hasLocalFiles = (wantBoxart && !needsBoxart) || (wantBg && !needsBg) || (wantIconBanner && !needsIconBanner) || (wantScreenshots && !needsScreenshots);
                        
                        if (hasLocalFiles)
                        {
                            // У нас уже есть какие-то файлы на диске, мы искали недостающие, но не нашли.
                            // Значит папка уже содержит всё, что вообще можно было найти.
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine(Lang.Strings.SkippedAlreadyExists);
                            Console.ResetColor();
                            skipped++;
                        }
                        else
                        {
                            // На диске файлов не было, и в сети тоже ничего не нашлось.
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(Lang.Strings.NotFound);
                            Console.ResetColor();
                            failed++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    spinner.Dispose();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Lang.Strings.ErrorSeeLog);
                    Console.ResetColor();
                    failed++;
                    
                    try
                    {
                        File.AppendAllText("error.log", $"[{DateTime.Now}] {Lang.Strings.ErrorDownloading} {title.TitleName} ({title.TitleId}):\r\n{ex}\r\n\r\n");
                    }
                    catch { }
                }

                // Небольшая пауза чтобы не перегружать сервисы
                await Task.Delay(200);
            }

            // Итоги
            Console.WriteLine();
            Console.WriteLine(new string('═', 70));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(Lang.Strings.ProgramFinished);
            Console.ResetColor();
            Console.WriteLine(string.Format(Lang.Strings.DownloadedStats, success, FormatSize(totalBytesSaved)));
            Console.WriteLine(string.Format(Lang.Strings.NotFoundStats, failed));
            Console.WriteLine(string.Format(Lang.Strings.SkippedStats, skipped));
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Lang.Strings.EjectUsb);
            Console.WriteLine(Lang.Strings.CoversLoadAutomatically);
            Console.ResetColor();
            Console.WriteLine();

            if (!_hasNativeDll && success > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(Lang.Strings.CoversSavedAsPng);
                Console.WriteLine(Lang.Strings.ConvertToAsset);
                Console.ResetColor();
                Console.WriteLine();
            }

            Console.WriteLine(Lang.Strings.PressAnyKeyToExit);
            Console.ReadKey();
            return 0;
        }

        static string FormatSize(long bytes)
        {
            if (bytes < 1024) return bytes + Lang.Strings.ByteSymbol;
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("0.##") + Lang.Strings.KiloByteSymbol;
            return (bytes / 1024.0 / 1024.0).ToString("0.##") + Lang.Strings.MegaByteSymbol;
        }

        static bool[] GetDownloadOptions()
        {
            string[] options = { "Boxart/Cover", "Background", "Icon/Banner", "Screenshots" };
            bool[] selected = { true, true, true, true };
            int cursor = 0;
            
            Console.WriteLine(Lang.Strings.SelectWhatToDownload);
            
            // Резервируем строки, чтобы консоль прокрутилась, если мы в самом низу
            for (int i = 0; i < options.Length; i++)
                Console.WriteLine();
                
            int startTop = Console.CursorTop - options.Length;

            Console.CursorVisible = false;
            
            while (true)
            {
                Console.SetCursorPosition(0, startTop);
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == cursor)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    
                    string checkbox = selected[i] ? "[X]" : "[ ]";
                    Console.WriteLine($"  {checkbox} {options[i].PadRight(30)}");
                    Console.ResetColor();
                }

                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.UpArrow)
                {
                    cursor--;
                    if (cursor < 0) cursor = options.Length - 1;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    cursor++;
                    if (cursor >= options.Length) cursor = 0;
                }
                else if (key == ConsoleKey.Spacebar)
                {
                    selected[cursor] = !selected[cursor];
                }
                else if (key == ConsoleKey.Enter)
                {
                    break;
                }
            }
            
            Console.CursorVisible = true;
            return selected;
        }

        static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine();
            Console.WriteLine("  ╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine(Lang.Strings.HeaderTitle);
            Console.WriteLine(Lang.Strings.HeaderSubtitle);
            Console.WriteLine(Lang.Strings.HeaderDescription);
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        static List<ContentDbReader.ContentItem> ParseSelection(string input, List<ContentDbReader.ContentItem> allTitles)
        {
            var result = new List<ContentDbReader.ContentItem>();
            var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.Contains('-'))
                {
                    // Диапазон: 5-10
                    var range = trimmed.Split('-');
                    if (range.Length == 2 && int.TryParse(range[0].Trim(), out int from) && int.TryParse(range[1].Trim(), out int to))
                    {
                        for (int i = from; i <= to && i <= allTitles.Count; i++)
                        {
                            if (i >= 1)
                                result.Add(allTitles[i - 1]);
                        }
                    }
                }
                else if (int.TryParse(trimmed, out int num) && num >= 1 && num <= allTitles.Count)
                {
                    result.Add(allTitles[num - 1]);
                }
            }

            return result.Distinct().ToList();
        }
    }

    class ConsoleSpinner : IDisposable
    {
        private bool _active;
        private Thread _thread;
        private int _left;
        private int _top;
        private bool _previousCursorVisible;

        public ConsoleSpinner()
        {
            _left = Console.CursorLeft;
            _top = Console.CursorTop;
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    _previousCursorVisible = Console.CursorVisible;
                    Console.CursorVisible = false;
                }
            }
            catch { }

            _active = true;
            _thread = new Thread(Spin);
            _thread.Start();
        }

        private void Spin()
        {
            var spinChars = new[] { '|', '/', '-', '\\' };
            int spinIndex = 0;
            while (_active)
            {
                try
                {
                    lock (Console.Out)
                    {
                        if (!_active) break;
                        Console.SetCursorPosition(_left, _top);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(spinChars[spinIndex]);
                        Console.ResetColor();
                    }
                    spinIndex = (spinIndex + 1) % spinChars.Length;
                    Thread.Sleep(100);
                }
                catch { }
            }
        }

        public void Dispose()
        {
            _active = false;
            _thread.Join();
            try
            {
                lock (Console.Out)
                {
                    Console.SetCursorPosition(_left, _top);
                    Console.Write(" ");
                    Console.SetCursorPosition(_left, _top);
                    
                    if (OperatingSystem.IsWindows())
                    {
                        Console.CursorVisible = _previousCursorVisible;
                    }
                }
            }
            catch { }
        }
    }
}
