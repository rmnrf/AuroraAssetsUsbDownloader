namespace AuroraAssetsUsbDownloader.Locales
{
    public class RussianLocalization : ILocalization
    {
        public string HeaderTitle =>       "  ║                   Aurora Assets USB Downloader                   ║";
        public string HeaderSubtitle =>    "  ║         Скачивание обложек для Aurora Xbox 360 на USB            ║";
        public string HeaderDescription => "  ║       Без FTP • Без интернета на Xbox • Только ПК + флешка       ║";

        public string DllNotFound => "  ⚠  AuroraAsset.dll не найдена!";
        public string DllCopyInstruction => "     Скопируйте AuroraAsset.dll из папки AuroraAssetEditor рядом с этим .exe";
        public string DllPngWarning => "     Без неё будут сохранены только PNG файлы (не .asset)";
        public string DllFound => "  ✓  AuroraAsset.dll найдена — будут создаваться .asset файлы";

        public string EnterDriveLetter => "\n  Введите букву флешки (например E): ";
        public string InvalidDriveLetter => "  ✗ Неверная буква диска. Попробуйте еще раз.";
        public string DbFoundInRoot => "\n  ⚠  Content.db найден в корне диска: {0}";
        public string CoversWillBeSavedIn => "     Обложки будут сохранены в: {0}";
        public string DeviceOrDbNotFound => "  ✗ Устройство или Content.db не найдены на диске {0}:";
        public string EnsureUsbInserted => "    Убедитесь, что флешка вставлена и содержит папку Aurora.";

        public string ReadingDb => "\n  Чтение Content.db...";
        public string NoGamesFound => "  ✗ В Content.db не найдено ни одной игры";
        public string GamesFound => "\n  Найдено игр: {0}";

        public string DownloadCoversForAll => "\n  Скачать обложки для всех игр? (Y/n): ";
        public string EnterGameNumbers => "  Введите номера игр через запятую (например 1,3,5-10): ";
        public string NothingSelected => "  Ничего не выбрано.";

        public string OverwriteExisting => "\n  Перезаписывать уже существующие обложки (force update)? (y/N): ";
        public string StartingDownload => "\n  Начинаю скачивание обложек для {0} игр...";

        public string SkippedAlreadyExists => "пропущено (уже есть)";
        public string ResourcesDownloaded => "✓ загружено ресурсов: {0} ({1})";
        public string NotFound => "✗ не найдено";
        public string ErrorSeeLog => "✗ ошибка (см. error.log)";

        public string ProgramFinished => "  РАБОТА ПРОГРАММЫ ЗАВЕРШЕНА!";
        public string DownloadedStats => "  Скачано:    {0} ({1})";
        public string NotFoundStats => "  Не найдено: {0}";
        public string SkippedStats => "  Пропущено:  {0}";
        public string EjectUsb => "  Теперь вы можете извлечь флешку и вставить её обратно в Xbox 360.";
        public string CoversLoadAutomatically => "  После запуска Aurora новые обложки подтянутся автоматически!";

        public string CoversSavedAsPng => "  ⚠  Обложки сохранены как PNG. Чтобы Aurora их подтянула,";
        public string ConvertToAsset => "     сконвертируйте их в .asset через AuroraAssetEditor.";

        public string CancelRequested => "\n  [Ctrl+C] Остановка после текущей игры...";

        public string PressAnyKeyToExit => "  Нажмите любую клавишу для выхода...";

        public string ByteSymbol => " Б";
        public string KiloByteSymbol => " КБ";
        public string MegaByteSymbol => " МБ";

        public string SelectWhatToDownload => "\n  Выберите, что скачивать (Стрелки - выбор, Пробел - вкл/выкл, Enter - продолжить):";
        public string ErrorDownloading => "Ошибка при скачивании";
        
        public string SelectLanguagePrompt => "\n  Select language / Выберите язык (1/2): ";
    }
}
