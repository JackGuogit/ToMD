using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using ToMD.Core;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.TypeInfoResolver =
        JsonTypeInfoResolver.Combine(
            AppJsonSerializerContext.Default,
            new DefaultJsonTypeInfoResolver()
        );
});

var app = builder.Build();

var sampleTodos = new Todo[] {
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var tomd = app.MapGroup("/tomd");
tomd.MapGet("/a", () => sampleTodos);
tomd.MapGet("/{id}", (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());
tomd.MapPost("/getmd/{token}", (string token, [FromBody] Option option) =>
{
    if (token != "guo502") return "无法验证";

    return RepoToMD.GetMD(option).Res.ToString();



});

tomd.MapGet("/getmd/downlaodmd/{name}", ( string name,string token) =>
{

    string v = Path.Combine(AppContext.BaseDirectory, "repo");
    List<string> list = FindFilesByName(name, v);

    if (list != null && list.Count > 0)
    {
        string filePath = list.First();
        byte[] fileBytes = File.ReadAllBytes(filePath);
        string contentType = "application/octet-stream";
        string fileName = Path.GetFileName(filePath);

        // 返回文件流响应
        return Results.File(
            fileBytes,
            contentType: contentType,
            fileDownloadName: fileName
        );
    }

    return Results.NotFound("没有指定文件");
});


List<string> FindFilesByName(string fileName, string rootPath)
{
    List<string> foundFiles = new List<string>();

    // 遍历目录及其子目录
    foreach (string file in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories))
    {
        try
        {
            // 获取文件名
            string currentFileName = Path.GetFileName(file);

            // 比较文件名
            if (currentFileName == fileName)
            {
                foundFiles.Add(file);
            }
        }
        catch (Exception ex)
        {
            // 忽略文件系统错误（例如权限不足、文件被删除等）
            Console.WriteLine($"Error accessing file {file}: {ex.Message}");
        }
    }

    return foundFiles;
}
app.Run();

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(Option))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
