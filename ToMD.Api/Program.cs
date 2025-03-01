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
    if (token != "guo502") return "�޷���֤";

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

        // �����ļ�����Ӧ
        return Results.File(
            fileBytes,
            contentType: contentType,
            fileDownloadName: fileName
        );
    }

    return Results.NotFound("û��ָ���ļ�");
});


List<string> FindFilesByName(string fileName, string rootPath)
{
    List<string> foundFiles = new List<string>();

    // ����Ŀ¼������Ŀ¼
    foreach (string file in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories))
    {
        try
        {
            // ��ȡ�ļ���
            string currentFileName = Path.GetFileName(file);

            // �Ƚ��ļ���
            if (currentFileName == fileName)
            {
                foundFiles.Add(file);
            }
        }
        catch (Exception ex)
        {
            // �����ļ�ϵͳ��������Ȩ�޲��㡢�ļ���ɾ���ȣ�
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
