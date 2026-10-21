using Microsoft.Data.Sqlite;

namespace ClaimHub.Infrastructure;

/// <summary>
/// The API, Admin portal and Worker share one SQLite file. Relative paths in configuration are
/// resolved against the folder that contains ClaimHub.sln so every host sees the same data folder.
/// </summary>
public static class DatabasePaths
{
    private const string SolutionFileName = "ClaimHub.sln";

    public static string ResolveFromSolutionRoot(string path)
    {
        if (Path.IsPathRooted(path))
        {
            return path;
        }

        return Path.GetFullPath(Path.Combine(FindSolutionRoot(), path));
    }

    public static string ResolveConnectionString(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (!string.IsNullOrEmpty(builder.DataSource) && builder.DataSource != ":memory:")
        {
            builder.DataSource = ResolveFromSolutionRoot(builder.DataSource);
        }

        return builder.ToString();
    }

    private static string FindSolutionRoot()
    {
        foreach (string start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var directory = new DirectoryInfo(start);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        return Directory.GetCurrentDirectory();
    }
}
