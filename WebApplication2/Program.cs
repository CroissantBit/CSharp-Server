using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File($"Logs/{Assembly.GetExecutingAssembly().GetName().Name}.log")
    .CreateLogger();

app.MapGet("/", () => "Hello World!");

app.MapPost("/upload", async (HttpContext context) =>
{
    var uid = Guid.NewGuid();
    var form = await context.Request.ReadFormAsync();
    var file = form.Files.GetFile("file");
    if (file == null)
    {
        Log.Logger.Write(LogEventLevel.Information, uid + " : " + "file was not received by server ");
        context.Response.StatusCode = 418;
        await context.Response.WriteAsync("No file received");
        return;
    }
    
    if (file.ContentType != "video/mp4")
    {
        Log.Logger.Write(LogEventLevel.Information, uid + " : " + "file was not received by server ");
        context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        await context.Response.WriteAsync("Invalid file format. Only video/mp4 is allowed.");
        return;
    }
    
    var filePath = "./" + file.FileName; 
    Log.Logger.Write(LogEventLevel.Debug, uid + " : " + "uploaded file " + file.FileName);

    await using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }
    //try to read file and extract metadata
    //save in database
    context.Response.StatusCode = 200;
    await context.Response.WriteAsync($"Received file: {file.FileName}");
});



app.Run();