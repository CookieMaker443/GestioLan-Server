using Microsoft.AspNetCore.Mvc;
using GestioLan.API.Services.Images;
using Microsoft.AspNetCore.Authorization; // Per l'attributo [Authorize]
using Microsoft.AspNetCore.StaticFiles;


namespace GestioLan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IImageService _imageService;
    private static readonly FileExtensionContentTypeProvider ContentTypes = new();
 
    public ImagesController(IImageService imageService)
    {
        _imageService = imageService;
    }

    // Per l'endpoint per nome: il nome file lo conosci già
    private static string GetContentType(string fileName)
        => ContentTypes.TryGetContentType(fileName, out var ct) ? ct : "application/octet-stream";

    // Per l'endpoint per ID: deduce il tipo dai magic bytes
    private static string DetectContentType(byte[] bytes)
    {
        byte[] png = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        if (bytes.Length >= png.Length && bytes.AsSpan(0, png.Length).SequenceEqual(png))
            return "image/png";
        return "image/jpeg";
    }

    [Authorize] // Protegge questo endpoint, richiede un token JWT valido per accedervi
    [HttpGet("AllImagesInfo")]
    public async Task<IActionResult> GetAllImagesInfo()
    {
        var images = await _imageService.GetAllImagesInfoAsync();
        return Ok(images);
    }


    // NOTA: una chiamata per immagine di item, il client sarà responsabile del caching
    [Authorize]
    [HttpGet("ImageName/{itemImageName}")]
    public async Task<IActionResult> GetImageByName(string itemImageName)
    {
        try
        {
            var imageBytes = await _imageService.GetImageByNameAsync(itemImageName);
            return File(imageBytes, GetContentType(itemImageName)); // Il browser/client vedrà un file immagine
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }


    [Authorize]
    [HttpGet("IdImage/{idImage}")]
    public async Task<IActionResult> GetImageById(int idImage)
    {
        try
        {
            var imageBytes = await _imageService.GetImageByIdAsync(idImage);
            return File(imageBytes, DetectContentType(imageBytes)); // Il browser/client vedrà un file immagine
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }


    [Authorize]
    [HttpGet("ItemsCount/{qty}")]
    public async Task<IActionResult> GetImagesByItemsCount(int qty)
    {
        var images = await _imageService.GetImagesByItemsCountAsync(qty);
        return Ok(images);
    }


    [Authorize] 
    [HttpPost("CreateImage")]
    [RequestSizeLimit(10 * 1024 * 1024)]          // 10 MB sull'intera richiesta
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    public async Task<IActionResult> CreateIImage(string? itemName, IFormFile file)
    {
        try
        {
            var message = await _imageService.CreateImageAsync(itemName, file);
            return Ok(new { message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }


    // Modifica un immagine
    [Authorize] 
    [HttpPut("UpdateImage/{id}")]
    [RequestSizeLimit(10 * 1024 * 1024)]          // 10 MB sull'intera richiesta
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    public async Task<IActionResult> UpdateImage(int id, string? itemName, IFormFile file)
    {
        try
        {
            var message = await _imageService.UpdateImageAsync(id, itemName, file);
            return Ok(new { message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [Authorize]
    [HttpPut("RenameImage/{id}")]
    public async Task<IActionResult> RenameImage(int id, string? itemName)
    {
        try
        {
            var message = await _imageService.RenameImageAsync(id, itemName);
            return Ok(new { message });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }


    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("DeleteImage/{id}")]
    public async Task<IActionResult> DeleteImage(int id)
    {
        try
        {
            var message = await _imageService.DeleteImageAsync(id);
            return Ok(new { message });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}