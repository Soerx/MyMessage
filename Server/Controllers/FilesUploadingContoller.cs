using Microsoft.AspNetCore.Mvc;
using Server.Entities;
using Server.Tools;
using System.Text;

namespace Server.Controllers;

public class FilesUploadingContoller : Controller
{
    private const string INVALID_ARGUMENTS_ERROR_MESSAGE = "Один или несколько аргументов имеют неверный формат или отсутствуют.";
    private const string SALT = "$*MqCc0r843$yOSsP8k";

    private ApplicationContext _context;
    private IWebHostEnvironment _appEnvironment;

    public FilesUploadingContoller(ApplicationContext context, IWebHostEnvironment appEnvironment)
    {
        _context = context;
        _appEnvironment = appEnvironment;
    }

    [HttpPost("UploadImage")]
    public async Task<IActionResult> UploadImage(IFormFile uploadedFile)
    {
        if (uploadedFile is null)
            return BadRequest(INVALID_ARGUMENTS_ERROR_MESSAGE);

        var encodedFileName = Convert.ToBase64String(Encoding.UTF8.GetBytes(uploadedFile.FileName + SALT + DateTime.Now));
        var encodedAndEscapedFilename = encodedFileName.Replace('/', '-');
        string path = "/Images/" + encodedAndEscapedFilename + ".png";
        using FileStream fileStream = new(_appEnvironment.WebRootPath + path, FileMode.Create);
        await uploadedFile.CopyToAsync(fileStream);
        ImageEntity imageEntity = new() { Name = uploadedFile.FileName, Path = path };
        _context.Images.Add(imageEntity);
        _context.SaveChanges();
        return Ok(imageEntity.ConvertToModel());
    }
}
