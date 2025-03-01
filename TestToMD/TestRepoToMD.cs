using ToMD.Core;

namespace TestToMD
{
    public class Tests
    {
        private Option _option;

        [SetUp]
        public void Setup()
        {
            _option = new Option()
            {
                RepoType = "github",
                Username = "JackGuogit",
                Repo = "nodify-avalonia",
                Branch = "main",
                RepoPath = @"P:\CSharp\ToMD\ToMD.Api\bin\Debug\net8.0\nodify-avalonia",
                exclude = new List<string>()
                {
                    ".git\\",
                    "bin\\",
                    "build\\",
                    ".png",
                    ".ico",
                }
            };

        }

        [Test]
        public void TestGetRepoInfo()
        {
            RepoToMD.GetMD(_option);


            Assert.Pass();
        }

        [Test]
        public void TestGetAllFilesPath()
        {
            List<string> list = RepoToMD.GetAllFilesPath(_option);

            string v = RepoToMD.CreateMD(_option, list);
        }

        [Test]
        public void TestFillterFile()
        {
            string filex = "P:\\CSharp\\ToMD\\ToMD.Api\\bin\\Debug\\net8.0\\nodify-avalonia\\.github\\FUNDING.yml";
            string relativePath = Path.GetRelativePath(_option.RepoPath, filex);
            bool v = RepoToMD.FillterFile(_option,relativePath);
        }

    }
}