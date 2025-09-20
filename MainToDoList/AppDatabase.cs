using System.IO;
using Microsoft.Maui.Storage;

namespace MainToDoList
{
    public static class AppDatabase
    {
        public static string DbPath => Path.Combine(FileSystem.AppDataDirectory, "tasks.db");
    }
}