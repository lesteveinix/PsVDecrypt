using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace PsVDecrypt
{
    public class Program
    {
        private static readonly string OutputDir = Path.Combine(Directory.GetCurrentDirectory(), "output");
        private static SQLiteConnection _dbConn;

        private static void Main(string[] args)
        {
            var coursesdir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Pluralsight", "courses");
            if (!Directory.Exists(coursesdir))
            {
                Console.WriteLine("Pluralsight courses directory not found");
                Environment.Exit(-1);
            }

            var dbdir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Pluralsight", "pluralsight.db");

            if (!File.Exists(dbdir))
            {
                Console.WriteLine("Pluralsight database not found");
                Environment.Exit(-1);
            }

            _dbConn = new SQLiteConnection("Data Source=" + dbdir + ";Version=3;");
            _dbConn.Open();

            Console.WriteLine("Courses directory: " + coursesdir);
            Console.WriteLine("Output Directory: " + OutputDir);

            var subdirs = Directory.GetDirectories(coursesdir);
            Console.WriteLine("\nFound " + subdirs.Length + " course(s):");

            foreach (var subdir in subdirs)
            {
                Console.WriteLine(" > " + Path.GetFileName(subdir));
            }

            if (!Directory.Exists(OutputDir))
            {
                Util.CreateDirectory(OutputDir);
            }

            System.Threading.Thread.Sleep(500);
            Console.WriteLine("\nPress any key to start decrypting all courses..\n");
            Console.ReadKey();

            foreach (var subdir in subdirs)
            {
                DecryptCourse(subdir);
            }

            Console.WriteLine(" > All done.\n");
        }

        private static void DecryptCourse(string courseSrcDir)
        {
            var courseName = Path.GetFileName(courseSrcDir);
            var courseDstDir = Path.Combine(OutputDir, courseName);
            var hasTranscript = false;
            Console.WriteLine("Processing course " + courseName + " ..");

            // Reset Directory
            if (Directory.Exists(courseDstDir))
            {
                Util.DeleteDirectory(courseDstDir);
            }
            Util.CreateDirectory(courseDstDir);


            // Copy Image
            File.Copy(Path.Combine(courseSrcDir, "image.jpg"), Path.Combine(courseDstDir, "image.jpg"));
            Console.WriteLine(" > Done copying course image.");

            // Read Course Info
            var command =
                new SQLiteCommand("select * from Course where Name=@Name", _dbConn) {CommandType = CommandType.Text};
            command.Parameters.Add(new SQLiteParameter("@Name", courseName));
            var reader = command.ExecuteReader();
            var dataTable = new DataTable();
            dataTable.Load(reader);
            if (dataTable.Rows.Count == 0)
            {
                Console.WriteLine(" > Error: cannot find course in database.");
                return;
            }

            hasTranscript = (long) dataTable.Rows[0]["HasTranscript"] == 1;

            // Save Course Info to JSON
            File.WriteAllText(Path.Combine(courseDstDir, "course-info.json"),
                JsonConvert.SerializeObject(dataTable, Formatting.Indented));
            Console.WriteLine(" > Done saving course info.");

            // Read Module Info
            command = new SQLiteCommand("select * from Module where CourseName=@CourseName", _dbConn)
            {
                CommandType = CommandType.Text
            };
            command.Parameters.Add(new SQLiteParameter("@CourseName", courseName));
            reader = command.ExecuteReader();
            dataTable = new DataTable();
            dataTable.Load(reader);
            Console.WriteLine(" > Found " + dataTable.Rows.Count + " module(s).");
            var dataTableAsList =
                JsonConvert.DeserializeObject<List<object>>(JsonConvert.SerializeObject(dataTable));

            // Process Each Module
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                var moduleItem = dataTable.Rows[i];
                Console.WriteLine("   > Processing module: " + moduleItem["Title"]);

                // Get Module Dir
                var moduleHash = Util.GetModuleHash(moduleItem["Name"] as string,
                    moduleItem["AuthorHandle"] as string);
                var moduleSrcDir = Path.Combine(courseSrcDir, moduleHash);
                var moduleDstDir = Path.Combine(courseDstDir,
                    (moduleItem["ModuleIndex"].ToString()).PadLeft(2, '0') + "." +
                    Util.TitleToFileName(moduleItem["Title"] as string));
                if (!Directory.Exists(moduleDstDir))
                    Util.CreateDirectory(moduleDstDir);

                // Save Module Info to JSON
                File.WriteAllText(Path.Combine(moduleDstDir, "module-info.json"),
                    JsonConvert.SerializeObject(dataTableAsList[i], Formatting.Indented));
                Console.WriteLine("     > Done saving module info.");

                // Read Clip Info
                var clipsCommand =
                    new SQLiteCommand("select * from Clip where ModuleId=@ModuleId", _dbConn)
                    {
                        CommandType = CommandType.Text
                    };
                clipsCommand.Parameters.Add(new SQLiteParameter("@ModuleId", moduleItem["Id"]));
                var clipsReader = clipsCommand.ExecuteReader();
                var clipsDataTable = new DataTable();
                clipsDataTable.Load(clipsReader);

                // Save Clips Info to JSON
                File.WriteAllText(Path.Combine(moduleDstDir, "clips-info.json"),
                    JsonConvert.SerializeObject(clipsDataTable, Formatting.Indented));
                Console.WriteLine("     > Done saving clips info.");

                // Process Each Clip
                for (var j = 0; j < clipsDataTable.Rows.Count; j++)
                {
                    var clipItem = clipsDataTable.Rows[j];
                    Console.WriteLine("     > Processing clip: " + clipItem["Title"]);
                    var clipSrc = Path.Combine(moduleSrcDir, (string) clipItem["Name"]) + ".psv";
                    var clipDst = Path.Combine(moduleDstDir,
                                      (clipItem["ClipIndex"].ToString()).PadLeft(2, '0') + "." +
                                      Util.TitleToFileName((string) clipItem["Title"])) + ".mp4";

                    // Decrypt Clip
                    Util.DecryptFile(clipSrc, clipDst);
                    Console.WriteLine("       > Done decrypting clip.");

                    // Save Transcript
                    if (!hasTranscript) continue;
                    var transcriptsCommand =
                        new SQLiteCommand("select * from ClipTranscript where ClipId=@ClipId", _dbConn)
                        {
                            CommandType = CommandType.Text
                        };
                    transcriptsCommand.Parameters.Add(new SQLiteParameter("@ClipId", clipItem["Id"]));
                    var transcriptsReader = transcriptsCommand.ExecuteReader();
                    var transcriptsDataTable = new DataTable();
                    transcriptsDataTable.Load(transcriptsReader);
                    if (transcriptsDataTable.Rows.Count == 0) continue;

                    // Generate Srt File
                    var sb = new StringBuilder();
                    var sequenceI = 0;
                    foreach (DataRow transcriptItem in transcriptsDataTable.Rows)
                    {
                        sequenceI++;
                        sb.Append(sequenceI + "\n");

                        var startMs = (long) transcriptItem["StartTime"];
                        var endMs = (long) transcriptItem["EndTime"];
                        var startTime = TimeSpan.FromMilliseconds(startMs);
                        var endTime = TimeSpan.FromMilliseconds(endMs);
                        sb.Append(startTime.ToString(@"hh\:mm\:ss") + "," + (startMs % 1000));
                        sb.Append(" --> ");
                        sb.Append(endTime.ToString(@"hh\:mm\:ss") + "," + (endMs % 1000));
                        sb.Append("\n");

                        sb.Append(string.Join("\n",
                            ((string) transcriptItem["Text"]).Replace("\r", "").Split('\n')
                            .Select(text => "- " + text)));
                        sb.Append("\n\n");
                    }


                    File.WriteAllText(clipDst + ".srt", sb.ToString());
                    Console.WriteLine("       > Done saving subtitles.");
                }
            }
        }
    }
}