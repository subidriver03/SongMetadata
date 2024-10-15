using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TagLib;

namespace SongMetadata
{
    // Class to represent the song editing process
    public class SongEditor
    {
        public string Artist { get; set; } = string.Empty; // Album artist
        public string Genre { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        private byte[]? AlbumCover { get; set; } = null;

        // Helper method to clean the title
        public string CleanTitle(string title)
        {
            // Regex to remove the leading numbers and dash (e.g., "01-Spooky Hallow" becomes "Spooky Hallow")
            return Regex.Replace(title, @"^\d+-", "");
        }

        // Method to load the album cover from a file
        public void LoadAlbumCover(string imagePath)
        {
            if (System.IO.File.Exists(imagePath)) // Explicitly reference System.IO.File
            {
                AlbumCover = System.IO.File.ReadAllBytes(imagePath); // Explicitly reference System.IO.File
            }
            else
            {
                Console.WriteLine("Image file not found. Skipping album cover.");
                AlbumCover = null;
            }
        }

        // Method to edit song metadata
        public void EditSongMetadata(string filePath)
        {
            // Use TagLib.File to edit the song's metadata
            var file = TagLib.File.Create(filePath); // Explicitly reference TagLib.File

            string rawTitle = Path.GetFileNameWithoutExtension(filePath);
            string cleanedTitle = CleanTitle(rawTitle);

            file.Tag.Title = cleanedTitle;
            file.Tag.AlbumArtists = new[] { Artist }; // Edit Album Artist instead of Performers
            file.Tag.Album = Album;
            file.Tag.Genres = new[] { Genre };
            file.Tag.Year = (uint)ReleaseDate.Year;

            // Set album cover if provided
            if (AlbumCover != null)
            {
                file.Tag.Pictures = new IPicture[]
                {
                    new Picture(new ByteVector(AlbumCover)) { Type = PictureType.FrontCover }
                };
            }

            // Save changes
            file.Save();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Create a new SongEditor instance
            SongEditor editor = new SongEditor();

            // Prompt for metadata input
            Console.Write("Enter the album artist's name: ");
            editor.Artist = Console.ReadLine() ?? string.Empty; // Album Artist

            Console.Write("Enter the genre: ");
            editor.Genre = Console.ReadLine() ?? string.Empty;

            Console.Write("Enter the album name: ");
            editor.Album = Console.ReadLine() ?? string.Empty;

            Console.Write("Enter the release date (yyyy-mm-dd): ");
            DateTime releaseDate;
            if (DateTime.TryParse(Console.ReadLine(), out releaseDate))
            {
                editor.ReleaseDate = releaseDate;
            }
            else
            {
                Console.WriteLine("Invalid date format. Setting release date to today.");
                editor.ReleaseDate = DateTime.Today;
            }

            // Prompt for the album cover image file
            Console.Write("Enter the path to the album cover image (or press Enter to skip): ");
            string albumCoverPath = Console.ReadLine() ?? string.Empty;
            if (!string.IsNullOrEmpty(albumCoverPath))
            {
                editor.LoadAlbumCover(albumCoverPath);
            }

            // Prompt the user to enter the folder path where the songs are located
            Console.Write("Enter the folder path where the songs are located: ");
            string folderPath = Console.ReadLine() ?? string.Empty;

            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                // Get all song files from the folder (assuming .mp3 for this example)
                string[] songFiles = Directory.GetFiles(folderPath, "*.mp3");

                if (songFiles.Length == 0)
                {
                    Console.WriteLine("No song files found in the directory.");
                    return;
                }

                Console.WriteLine("The following files will be modified:");
                foreach (string songFile in songFiles)
                {
                    Console.WriteLine(Path.GetFileName(songFile));
                }

                // Confirm action with the user
                Console.Write("Do you want to proceed with editing the metadata for these songs? (y/n): ");
                string confirmation = Console.ReadLine()?.ToLower() ?? "n";

                if (confirmation == "y")
                {
                    foreach (string songFile in songFiles)
                    {
                        try
                        {
                            // Edit metadata for each song
                            editor.EditSongMetadata(songFile);
                            Console.WriteLine($"Successfully updated: {Path.GetFileName(songFile)}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error updating {Path.GetFileName(songFile)}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Operation canceled.");
                }
            }
            else
            {
                Console.WriteLine("Invalid directory path. Please check and try again.");
            }
        }
    }
}
