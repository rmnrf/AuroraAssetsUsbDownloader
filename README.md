# Aurora Assets USB Downloader

A console application for downloading Xbox 360 game covers and preparing them for the Aurora Dashboard — **without FTP, without Xbox internet, only PC + USB Drive**.

## How it works

The application uses the source code from [AuroraAssetEditor](https://github.com/XboxUnity/AuroraAssetEditor), but instead of FTP-uploading to the Xbox, it saves `.asset` files **locally to a USB drive**.

## Requirements

- Windows 10/11
- .NET 10 Runtime
- **AuroraAsset.dll** from [AuroraAssetEditor](https://github.com/XboxUnity/AuroraAssetEditor/releases) (copy it next to the .exe)
  - Without this DLL, covers will be saved as PNG files (not .asset)

## Usage

### Step 1: Connect your Xbox 360 USB Drive

You will need the USB flash drive where your Aurora is installed. Connect this USB drive to your PC. The application will automatically find and read the `Content.db` file from your Aurora folder.

### Step 2: Run the application

```bash
# From the command line, simply pass the USB drive letter (e.g., E):
AuroraAssetsUsbDownloader.exe E

# Or run the .exe without parameters — it will ask for the drive letter interactively
AuroraAssetsUsbDownloader.exe
```

Upon launching, the program will prompt you to **select a language (English / Русский)**. After that, an **interactive menu** will appear where you can use the arrow keys and spacebar to select which specific assets to download (Boxart, Background, Icon/Banner, Screenshots).

### Step 3: Insert the USB drive back into the Xbox 360

Since the program saves the covers directly into the native Aurora folder structure on your USB drive, **you don't need to manually copy anything**.
Simply eject the USB drive from your PC, insert it into your Xbox 360, and launch Aurora — all covers will load automatically!

## Features

- **Bilingual Interface:** Full support for English and Russian languages.
- **Interactive Menu:** A convenient menu for selecting downloadable assets without needing command-line arguments.
- **Safe Cancel (Ctrl+C):** If you interrupt the program by pressing `Ctrl+C`, it won't close instantly (which could corrupt a file). Instead, it will carefully finish downloading the current game and display the final statistics.

## Download Statuses

While running, the program displays statuses for each game:
- **`✓ resources downloaded: N`** — the program successfully found, downloaded, and saved new assets from the internet.
- **`skipped (already exists)`** — all requested assets are already present on your USB drive. The program skipped the game instantly without making network requests.
- **`✗ not found`** — the program checked Xbox Live and Archive.org, but no matching materials were found online.
- **`✗ error (see error.log)`** — an unexpected error occurred during download.

## File Structure

The application creates the exact same file structure as an FTP upload via AuroraAssetEditor:

```text
Data/GameData/
├── 415607E6_00000001/
│   └── GC415607E6.asset     ← cover (boxart)
├── 4D5307E6_00000002/
│   └── GC4D5307E6.asset
└── ...
```

## Building from source

```bash
dotnet build
```

## Credits

- `.asset` format and native DLL: **MaesterRowen (Phoenix)**
- AuroraAssetEditor: **Swizzy / XboxUnity**
- Console adaptation: Aurora Assets USB Downloader
