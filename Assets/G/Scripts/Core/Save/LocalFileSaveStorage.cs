using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using G.Core.Common;

namespace G.Core.Save
{
    public class LocalFileSaveStorage : ISaveStorage
    {
        private const string FILE_NAME = "save.json";
        private const string TEMP_FILE_NAME = "save.tmp";
        private const string BACKUP_FILE_NAME = "save.bak";

        private readonly string _directory;

        public LocalFileSaveStorage()
        {
            _directory = Path.Combine(Application.persistentDataPath, "Save");
        }

        public string Id => "local";
        public bool IsAvailable => true;

        public string FilePath => Path.Combine(_directory, FILE_NAME);

        public async UniTask<string> ReadAsync(CancellationToken cancellationToken)
        {
            await UniTask.SwitchToThreadPool();

            try
            {
                string json = ReadFileOrNull(FilePath);

                if (string.IsNullOrWhiteSpace(json) == false)
                    return json;

                string backup = ReadFileOrNull(Path.Combine(_directory, BACKUP_FILE_NAME));

                if (string.IsNullOrWhiteSpace(backup) == false)
                {
                    GameDebug.LogWarning("[Save] Основной файл пуст или отсутствует, прочитана резервная копия.");
                    return backup;
                }

                return null;
            }
            catch (IOException exception)
            {
                GameDebug.LogError($"[Save] Не удалось прочитать сохранение: {exception.Message}");
                return null;
            }
            finally
            {
                await UniTask.SwitchToMainThread(cancellationToken);
            }
        }

        public async UniTask WriteAsync(string json, CancellationToken cancellationToken)
        {
            await UniTask.SwitchToThreadPool();

            try
            {
                if (Directory.Exists(_directory) == false)
                    Directory.CreateDirectory(_directory);

                string tempPath = Path.Combine(_directory, TEMP_FILE_NAME);
                string backupPath = Path.Combine(_directory, BACKUP_FILE_NAME);

                File.WriteAllText(tempPath, json);

                if (File.Exists(FilePath))
                    File.Replace(tempPath, FilePath, backupPath);
                else
                    File.Move(tempPath, FilePath);
            }
            catch (IOException exception)
            {
                GameDebug.LogError($"[Save] Не удалось записать сохранение: {exception.Message}");
            }
            catch (UnauthorizedAccessException exception)
            {
                GameDebug.LogError($"[Save] Нет доступа к файлу сохранения: {exception.Message}");
            }
            finally
            {
                await UniTask.SwitchToMainThread(cancellationToken);
            }
        }

        private static string ReadFileOrNull(string path)
        {
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }
    }
}
