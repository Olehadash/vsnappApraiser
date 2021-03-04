using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class GlobalSettings
{
    private const string CLOUD_STORAGE_DOWNLOAD_DIR = "CloudStorageDownload";
    private const string CLOUD_STORAGE_UPLOAD_DIR = "CloudStorageUpload";

    public static readonly string storageFolderPath =
        "/storage/emulated/0";

    public static readonly string cloudStorageDownloadPath =
        Path.Combine(Application.persistentDataPath, CLOUD_STORAGE_DOWNLOAD_DIR);
    public static readonly string cloudStorageUploadPath =
        Application.persistentDataPath;

    public static readonly string filesFolderPath =
    "/storage/emulated/0/Android/data/com.Umdpersum.Vsnaap/files";

    public static readonly string cacheFolderPath =
    "/storage/emulated/0/Android/data/com.Umdpersum.Vsnaap/cache";
}
