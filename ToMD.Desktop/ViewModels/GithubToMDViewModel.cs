using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ToMD.Core;

namespace ToMD.Desktop.ViewModels
{
    public class GithubToMDViewModel:ViewModelBase
    {
        private SQLiteConnection liteConnection;

        private string _repoType;
        private string _repo;
        private string _branch;
        private string _username;
        private string _token;
        private string _repoPath;
        private string _exclude;

        public string RepoType
        {
            get
            {
                if (_repoType == null)
                {
                    string sql = "select * from config where para='RepoType'";
                    SQLiteCommand command = new SQLiteCommand(sql, liteConnection);
                    SQLiteDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        _repoType = reader["value"].ToString();  // 通过列名
                                                                 // 或使用类型方法：reader.GetString(reader.GetOrdinal("value"))
                    }

                }
                return _repoType;
            }
            set => SetProperty(ref _repoType, value);
        }

        public string Repo
        {
            get
            {
                if (_repo == null)
                {
                    string sql = "select * from config where para='Repo'";
                    SQLiteCommand command = new SQLiteCommand(sql, liteConnection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        _repo = reader["value"].ToString();  // 通过列名
                                                             // 或使用类型方法：reader.GetString(reader.GetOrdinal("value"))
                    }
                }
                return _repo;
            }
            set => SetProperty(ref _repo, value);
        }

        public string Branch
        {
            get
            {
                if (_branch == null)
                {
                    string sql = "select * from config where para='Branch'";
                    SQLiteCommand command = new SQLiteCommand(sql, liteConnection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        _branch = reader["value"].ToString();  // 通过列名
                                                               // 或使用类型方法：reader.GetString(reader.GetOrdinal("value"))
                    }
                }
                return _branch;
            }
            set => SetProperty(ref _branch, value);
        }

        public string Username
        {
            get
            {
                if (_username == null)
                {
                    string sql = "select * from config where para='Username'";
                    SQLiteCommand command = new SQLiteCommand(sql, liteConnection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        _username = reader["value"].ToString();  // 通过列名
                                                                 // 或使用类型方法：reader.GetString(reader.GetOrdinal("value"))
                    }
                }
                return _username;
            }
            set => SetProperty(ref _username, value);
        }

        public string Token
        {
            get
            {
                if (_token == null)
                {
                    string sql = "select * from config where para='Token'";
                    SQLiteCommand command = new SQLiteCommand(sql, liteConnection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        _token = reader["value"].ToString();  // 通过列名
                                                              // 或使用类型方法：reader.GetString(reader.GetOrdinal("value"))
                    }
                }
                return _token;
            }
            set => SetProperty(ref _token, value);
        }

        public string RepoPath
        {
            get
            {
                if(_repoPath == null)
                {
                    string sql = "select * from config where para='RepoPath'";
                    SQLiteCommand command = new SQLiteCommand(sql, liteConnection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        _repoPath = reader["value"].ToString();  // 通过列名
                                                                 // 或使用类型方法：reader.GetString(reader.GetOrdinal("value"))
                    }
                }
                return _repoPath;
            }
            set => SetProperty(ref _repoPath, value);
        }

        public string Exclude
        {
            get
            { 
                if(_exclude==null)
                {
                    string sql = "select * from config where para='Exclude'";
                    SQLiteCommand command = new SQLiteCommand(sql, liteConnection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        _exclude = reader["value"].ToString();  // 通过列名
                                                                 // 或使用类型方法：reader.GetString(reader.GetOrdinal("value"))
                    }
                }
                return _exclude;
            }
            set => SetProperty(ref _exclude, value);
        }

        public GithubToMDViewModel()
        {
            CreateNewDatabase();
            CreateNewTable();
            Confirm = new RelayCommand(() =>
            {
                var result = RepoToMD.GetMD(new Option
                {
                    RepoType = RepoType,
                    Repo = Repo,
                    Branch = Branch,
                    Username = Username,
                    Token = Token,
                    RepoPath = RepoPath,
                    exclude = Exclude.Split(',').ToList()
                });
                if (result.IsOk)
                {
                    SaveConfig();
                }
                else
                {
      
                }
            });
        }

        public ICommand Confirm { get; set; }



        private bool SaveConfig()
        {


            FillTable();
            SelectConfig();
            return true;
        }


        private void CreateNewDatabase()
        {
            string baseDirectory = AppContext.BaseDirectory;

            string toMDDB = Path.Combine(baseDirectory, "ToMDDB.db");
            if(Path.Exists(toMDDB))
            {
                return;
            }
            // Create a new database
            SQLiteConnection.CreateFile(Path.Combine(baseDirectory,"ToMD.db"));//可以不要此句
        }


        //在指定数据库中创建一个table
        private void CreateNewTable()
        {
            liteConnection = new SQLiteConnection($"Data Source={Path.Combine(AppContext.BaseDirectory, "ToMDDB.db")};Version=3;");//没有数据库则自动创建
            liteConnection.Open();
            string sql = "create table if not exists config (id int PRIMARY KEY, para varchar(20), value varchar(256))";
            SQLiteCommand command = new SQLiteCommand(sql, liteConnection);
            command.ExecuteNonQuery();
        }

        private void FillTable()
        {

            string sqlx = $"""
                INSERT INTO config (id, para, value) 
                VALUES 
                (1, 'RepoType', '{RepoType}'),
                (2, 'Repo', '{Repo}'),
                (3, 'Branch', '{Branch}'),
                (4, 'Username', '{Username}'),
                (5, 'Token', '{Token}'),
                (6, 'RepoPath', '{RepoPath}'),
                (7, 'Exclude', '{Exclude}')
                ON CONFLICT(id) 
                DO UPDATE SET 
                para = excluded.para, 
                value = excluded.value;
                """;

            SQLiteCommand command = new SQLiteCommand(sqlx, liteConnection);
            command.ExecuteNonQuery();

        }

        //使用sql查询语句，并显示结果
        private void SelectConfig()
        {
            string sql = "select * from config";
            SQLiteCommand command = new SQLiteCommand(sql, liteConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine("Name: " + reader["para"] + reader["value"]);
            }

            Console.ReadLine();
        }

    }
}
