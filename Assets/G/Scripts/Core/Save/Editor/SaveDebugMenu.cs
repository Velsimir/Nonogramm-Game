using System.IO;
using UnityEditor;
using UnityEngine;

namespace G.Core.Save.Editor
{
    public static class SaveDebugMenu
    {
        private const string MENU_ROOT = "Tools/Сейвы/";
        private const string DELETE_ITEM = MENU_ROOT + "Удалить сохранение";
        private const string REVEAL_ITEM = MENU_ROOT + "Показать папку";
        private const string PRINT_ITEM = MENU_ROOT + "Вывести в консоль";
        private const string SAVE_FOLDER = "Save";
        private const string SAVE_FILE = "save.json";

        [MenuItem(DELETE_ITEM, false, 0)]
        private static void DeleteSave()
        {
            string directory = GetSaveDirectory();

            if (Directory.Exists(directory) == false)
            {
                Debug.Log($"[Save] Удалять нечего, папки нет: {directory}");
                return;
            }

            string[] files = Directory.GetFiles(directory);

            bool confirmed = EditorUtility.DisplayDialog(
                "Удалить сохранение?",
                $"Папка будет удалена целиком:\n{directory}\n\nФайлов внутри: {files.Length}\n\nДействие необратимо.",
                "Удалить",
                "Отмена");

            if (confirmed == false)
                return;

            try
            {
                Directory.Delete(directory, recursive: true);
                Debug.Log($"[Save] Сохранение удалено: {directory}, файлов {files.Length}.");
            }
            catch (IOException exception)
            {
                Debug.LogError($"[Save] Не удалось удалить сохранение: {exception.Message}");
            }
        }

        [MenuItem(DELETE_ITEM, true)]
        private static bool CanDeleteSave()
        {
            return EditorApplication.isPlayingOrWillChangePlaymode == false;
        }

        [MenuItem(REVEAL_ITEM, false, 1)]
        private static void RevealSaveFolder()
        {
            string directory = GetSaveDirectory();

            if (Directory.Exists(directory) == false)
            {
                Debug.Log($"[Save] Папки сохранений ещё нет: {directory}");
                return;
            }

            EditorUtility.RevealInFinder(directory);
        }

        [MenuItem(PRINT_ITEM, false, 2)]
        private static void PrintSave()
        {
            string path = Path.Combine(GetSaveDirectory(), SAVE_FILE);

            if (File.Exists(path) == false)
            {
                Debug.Log($"[Save] Файла сохранения нет: {path}");
                return;
            }

            Debug.Log($"[Save] {path}\n{File.ReadAllText(path)}");
        }

        private static string GetSaveDirectory()
        {
            return Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
        }
    }
}
