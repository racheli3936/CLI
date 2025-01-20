////fib bundle --output path
using System.CommandLine;
using System.IO;
using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
#region functions
static void PassOnFiles(string path, string[] lang, string sort, bool note, string author, bool remove)
{
    
    string currentDirectory = Directory.GetCurrentDirectory();
    if (!Directory.Exists(Path.GetDirectoryName(path)))
    {
        //path = currentDirectory + "\\bundle.txt";
        Console.WriteLine("Error: the path is not exists");
    }
    string[] excludedDirectories = { "bin", "debug", "obj", "release", "logs", "temp", "dist", "node_modules", "vendor", ".vs" };
    string[] files = Directory.GetFiles(currentDirectory, "*.*", SearchOption.AllDirectories).Where(
            pathh => !excludedDirectories.Any(dir => path.Contains(Path.Combine(currentDirectory, dir)))).ToArray();
    Sort(files, sort);
    if (author != null)
    {
        File.WriteAllLines(path, ["//author: " + author]);

    }
    foreach (string file in files)
    {
        if (lang.Contains(Path.GetExtension(file)) || lang[0] == "all")
        {
            if (note == true)
            {

                File.AppendAllLines(path, ["//source: " + file]);
            }
            if (remove == true)
            {
                var all_file = File.ReadAllLines(file);
                var non_empty_lines = all_file.Where(line => !string.IsNullOrWhiteSpace(line));
                File.AppendAllLines(path, non_empty_lines);
            }
            else
            {
                File.AppendAllLines(path, File.ReadAllLines(file));
            }

        }
    }
}

#endregion
static void Sort(string[] path, string sort)
{
    Array.Sort(path, (x, y) =>
    {
        string nameX = typeSort(x, sort);
        string nameY = typeSort(y, sort);
        return nameX.CompareTo(nameY);
    });
}
static string typeSort(string path, string sort)
{
    if (sort == "type")
        return Path.GetExtension(path);
    return Path.GetFileName(path);
}
static void CreateRsp()
{
    string lang = "", output, author;
    char sort;
    bool note, remove;
    string command = "bundle";
    try
    {
        Console.WriteLine("enter the first languages you want?");
        lang = Console.ReadLine();
        while (lang != "-1")
        {
            command += " -l" + lang;
            Console.WriteLine("if you want more languages enter, to exit enter -1?");
            lang = Console.ReadLine();
        }
        Console.WriteLine("enter the path to create your bundle, if you don't want enter -1");
        output = Console.ReadLine();
        Console.WriteLine("do you want to sort according type (T) or name (N)");
        sort = char.Parse(Console.ReadLine());
        Console.WriteLine("do you want to save the source of each file? enter true/false");
        note = bool.Parse(Console.ReadLine());
        Console.WriteLine("write the author, if you don't want enter -1");
        author = Console.ReadLine();
        Console.WriteLine("do you want to remove empty lines?true/false");
        remove = bool.Parse(Console.ReadLine());
     
        if (output == "-1") { output = Path.Combine(Directory.GetCurrentDirectory(), "bundle.txt"); }
        command += $" -o \"{output}\"";
        if (sort == 'T') { command += " -s type"; } else { command += " -s name"; }
        if (note) { command += " -n"; }
        if (remove) { command += " -r"; }
        if (author != "-1") { command += $" -a \"{author}\""; }
        string fileName = "response.rsp";
        File.WriteAllText(fileName, command);
        Console.WriteLine("now enter: fib @response.rsp");
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }
}
var bundleLanguage = new Option<string[]>("--languages", "choose languages") { IsRequired = true };
bundleLanguage.FromAmong(".txt", ".java", ".cs", ".cpp", ".py", ".js", ".html", ".css", ".pdf", ".jpg", "all");
bundleLanguage.AddAlias("-l");
var bundleOption = new Option<FileInfo>("--output", "file path and name");
bundleOption.AddAlias("-o");
var bundleSort = new Option<string>("--sort", "sort according to name or type");
bundleSort.FromAmong("name", "type");
bundleSort.AddAlias("-s");
var bundleNote = new Option<bool>("--note", "source in the bundle file");
bundleNote.AddAlias("-n");
var bundleAuthor = new Option<string>("--author", "your name");
bundleAuthor.AddAlias("-a");
var bundle_remove_empty_lines = new Option<bool>("--remove_empty_lines", "remove empty lines");
bundle_remove_empty_lines.AddAlias("-r");
var bundleCommand = new Command("bundle", "bundle code files to a single file");
bundleCommand.AddOption(bundleOption);
bundleCommand.AddOption(bundleLanguage);
bundleCommand.AddOption(bundleSort);
bundleCommand.AddOption(bundleNote);
bundleCommand.AddOption(bundleAuthor);
bundleCommand.AddOption(bundle_remove_empty_lines);
bundleCommand.SetHandler((output, lang, sort, note, author, remove) =>
{
    sort = sort ?? "name";
    string outputName;
    if (output == null)
        outputName = Directory.GetCurrentDirectory() + "\\bundle.txt";
    else
        outputName = output.FullName;
    try
    {
        PassOnFiles(outputName, lang, sort, note, author, remove);
        Console.WriteLine("the file was created succesfully");
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error: {e.Message}");
    }
}, bundleOption, bundleLanguage, bundleSort, bundleNote, bundleAuthor, bundle_remove_empty_lines);
var rspCommand = new Command("create_rsp", "easy way to use bundle command");
rspCommand.SetHandler(() =>
{
    try
    {
        CreateRsp();
    }
    catch (Exception e) { Console.WriteLine(e.Message); }
});
var rootCommand = new RootCommand("root command for file cli");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(rspCommand);
await rootCommand.InvokeAsync(args);
