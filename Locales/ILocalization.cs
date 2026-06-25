namespace AuroraOfflineAssets.Locales
{
    public interface ILocalization
    {
        string HeaderTitle { get; }
        string HeaderSubtitle { get; }
        string HeaderDescription { get; }

        string DllNotFound { get; }
        string DllCopyInstruction { get; }
        string DllPngWarning { get; }
        string DllFound { get; }

        string EnterDriveLetter { get; }
        string InvalidDriveLetter { get; }
        string DbFoundInRoot { get; }
        string CoversWillBeSavedIn { get; }
        string DeviceOrDbNotFound { get; }
        string EnsureUsbInserted { get; }

        string ReadingDb { get; }
        string NoGamesFound { get; }
        string GamesFound { get; }

        string DownloadCoversForAll { get; }
        string EnterGameNumbers { get; }
        string NothingSelected { get; }

        string OverwriteExisting { get; }
        string StartingDownload { get; }

        string SkippedAlreadyExists { get; }
        string ResourcesDownloaded { get; }
        string NotFound { get; }
        string ErrorSeeLog { get; }

        string ProgramFinished { get; }
        string DownloadedStats { get; }
        string NotFoundStats { get; }
        string SkippedStats { get; }
        string EjectUsb { get; }
        string CoversLoadAutomatically { get; }

        string CoversSavedAsPng { get; }
        string ConvertToAsset { get; }

        string CancelRequested { get; }

        string PressAnyKeyToExit { get; }

        string ByteSymbol { get; }
        string KiloByteSymbol { get; }
        string MegaByteSymbol { get; }

        string SelectWhatToDownload { get; }
        string ErrorDownloading { get; }
        
        string SelectLanguagePrompt { get; }
    }
}
