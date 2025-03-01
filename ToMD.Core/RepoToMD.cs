using Aspose.Html.Toolkit.Markdown.Syntax;
using Aspose.Html.Toolkit.Markdown.Syntax.Text;
using LibGit2Sharp;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using ToMD.Core.VO;

namespace ToMD.Core
{
    public class RepoToMD
    {
        public static Reslut GetMD(Option option)
        {
            Reslut reslut = GetRepoInfo(option);

            List<string> list = GetAllFilesPath(option);

            reslut.Res = CreateMD(option, list);
            reslut.IsOk = true;

            return reslut;
        }

        private static Reslut GetRepoInfo(Option option)
        {
            string repoUrl = option.GetRepoApi();
            option.RepoPath = Path.Combine(AppContext.BaseDirectory, "repo", option.Repo);
            string localPath = option.RepoPath;

            Reslut reslut = new Reslut();
            var credentials = new UsernamePasswordCredentials
            {
                Username = option.Username,
                Password = option.Token
            };

            var cloneOptions = new CloneOptions
            {
                BranchName="main"
            };

            try
            {
                if (Directory.Exists(localPath))
                    Directory.Delete(localPath, true);

                Repository.Clone(repoUrl, localPath, cloneOptions);
                reslut.Res = "仓库已克隆至: " + localPath;
                reslut.IsOk = true;
                return reslut;
            }
            catch (LibGit2SharpException ex)
            {
                reslut.Res = $"错误: {ex.Message}";
                reslut.IsOk = false;
                return reslut;
            }
        }

        public static List<string> GetAllFilesPath(Option option)
        {
            List<string> allFiles = Directory.GetFiles(option.RepoPath, "*.*", SearchOption.AllDirectories).ToList();
            return allFiles;
        }




        public static string CreateMD(Option option,List<string> files)
        {

            var markdown = new MarkdownSyntaxTree(new Aspose.Html.Configuration());
            // Create a Markdown syntax factory
            var mdf = markdown.SyntaxFactory;



            markdown.AppendChild(mdf.Text($"# {option.Repo}仓库\n"));


            IEnumerable<string> codeFiles = files.Where(f => Path.GetExtension(f)!=".md");
            foreach (var file in codeFiles) 
            {

                string relativePath = Path.GetRelativePath(option.RepoPath, file);
                if (!FillterFile(option,relativePath)) continue;
                markdown.AppendChild(mdf.Text("## "+ relativePath));
                // Create a start token and an end token that will frame a code block
                var startToken = mdf.Token(SourceText.From("\n```\r"));
                var endToken = mdf.Token(SourceText.From("\r\n```\n"));

                // Create a fenced code element
                var fencedCodeSpan = mdf.FencedCodeBlock(startToken, null, endToken);

                // Create text content for the fenced code element
                fencedCodeSpan.AppendChild(mdf.Text(File.ReadAllText(file)));

                //Add the fenced code element to MD file
                markdown.AppendChild(fencedCodeSpan);
            }

            markdown.AppendChild(mdf.Text($"# {option.Repo}文档\n"));

            IEnumerable<string> mdFiles = files.Where(f => Path.GetExtension(f).Equals(".md"));

            foreach (var mdFile in mdFiles)
            {
                markdown.AppendChild(mdf.Text(File.ReadAllText(mdFile)));
            }



            // Prepare a path for MD file saving 
            string savePath = Path.Combine(option.RepoPath, $"{option.Repo}.md");

            // Save MD file
            markdown.Save(savePath);

            return savePath;
        }
        public static bool FillterFile(Option option,string file)
        {

            if (option.exclude.Any(s=>file.Contains(s)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        



    }

    public class Reslut
    {
        public bool IsOk { get; set; }
        public object Res {  get; set; }
    }


    
    public class Option
    {
        public string RepoType { get; set; }
        public string Repo { get; set; }
        public string Branch { get; set; }
        public string Username { get; set; }
        public string Token {  get; set; } 
        public string RepoPath { get; set; }
        public List<string> exclude { get; set; }
        public string GetRepoApi()
        {
            return $"git@{RepoType}.com:/{Username}/{Repo}.git";
        }

    }


    public class FileContent
    {
        public string Path { get; set; }
        public string Content { get; set; }
    }
}
