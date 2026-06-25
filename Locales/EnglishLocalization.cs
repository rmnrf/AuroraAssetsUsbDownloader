namespace AuroraOfflineAssets.Locales
{
    public class EnglishLocalization : ILocalization
    {
        public string HeaderTitle =>       "  ║                   Aurora Assets USB Downloader                   ║";
        public string HeaderSubtitle =>    "  ║           Downloading covers for Aurora Xbox 360 to USB          ║";
        public string HeaderDescription => "  ║          No FTP • No Xbox Internet • Only PC + USB Drive         ║";

        public string DllNotFound => "  ⚠  AuroraAsset.dll not found!";
        public string DllCopyInstruction => "     Copy AuroraAsset.dll from AuroraAssetEditor next to this .exe";
        public string DllPngWarning => "     Without it, only PNG files will be saved (not .asset)";
        public string DllFound => "  ✓  AuroraAsset.dll found — .asset files will be created";

        public string EnterDriveLetter => "\n  Enter USB drive letter (e.g. E): ";
        public string InvalidDriveLetter => "  ✗ Invalid drive letter. Try again.";
        public string DbFoundInRoot => "\n  ⚠  Content.db found in root: {0}";
        public string CoversWillBeSavedIn => "     Covers will be saved in: {0}";
        public string DeviceOrDbNotFound => "  ✗ Device or Content.db not found on drive {0}:";
        public string EnsureUsbInserted => "    Ensure USB drive is inserted and contains Aurora folder.";

        public string ReadingDb => "\n  Reading Content.db...";
        public string NoGamesFound => "  ✗ No games found in Content.db";
        public string GamesFound => "\n  Games found: {0}";

        public string DownloadCoversForAll => "\n  Download covers for all games? (Y/n): ";
        public string EnterGameNumbers => "  Enter game numbers comma-separated (e.g. 1,3,5-10): ";
        public string NothingSelected => "  Nothing selected.";

        public string OverwriteExisting => "\n  Overwrite existing covers (force update)? (y/N): ";
        public string StartingDownload => "\n  Starting cover download for {0} games...";

        public string SkippedAlreadyExists => "skipped (already exists)";
        public string ResourcesDownloaded => "✓ resources downloaded: {0} ({1})";
        public string NotFound => "✗ not found";
        public string ErrorSeeLog => "✗ error (see error.log)";

        public string ProgramFinished => "  PROGRAM FINISHED!";
        public string DownloadedStats => "  Downloaded: {0} ({1})";
        public string NotFoundStats => "  Not found:  {0}";
        public string SkippedStats => "  Skipped:    {0}";
        public string EjectUsb => "  You can now eject the USB drive and insert it back into Xbox 360.";
        public string CoversLoadAutomatically => "  New covers will load automatically upon launching Aurora!";

        public string CoversSavedAsPng => "  ⚠  Covers saved as PNG. For Aurora to load them,";
        public string ConvertToAsset => "     convert them to .asset using AuroraAssetEditor.";

        public string CancelRequested => "\n  [Ctrl+C] Stopping after current game...";

        public string PressAnyKeyToExit => "  Press any key to exit...";

        public string ByteSymbol => " B";
        public string KiloByteSymbol => " KB";
        public string MegaByteSymbol => " MB";

        public string SelectWhatToDownload => "\n  Select what to download (Arrows - move, Space - toggle, Enter - continue):";
        public string ErrorDownloading => "Error downloading";
        
        public string SelectLanguagePrompt => "\n  Select language / Выберите язык (1/2): ";
    }
}
