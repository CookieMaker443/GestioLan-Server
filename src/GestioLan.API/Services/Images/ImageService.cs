using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using GestioLan.API.Models;
using GestioLan.API.Utils.Helpers; // Per la classe StringHelper e CurrentUserService

namespace GestioLan.API.Services.Images;

public class ImageService : IImageService
{
    private readonly GestioLanContext _context;
    private readonly ILogger<ImageService> _logger;

    private readonly string _itemsFolder;

    private const long MaxPluginImageBytes = 10 * 1024 * 1024; // 10 MB

    // Firme (magic bytes) dei formati ammessi
    private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] JpegSignature = { 0xFF, 0xD8, 0xFF };

    public ImageService(GestioLanContext context, IConfiguration config, 
        ILogger<ImageService> logger)
    {
        _context = context;
        _logger = logger;   // serve per loggare

        _itemsFolder = config["Storage:ItemsPath"] ?? "/app/data/uploads/items";
    }





    // Restituisce la lista di tutte le immagini con le info base (IdImage, FileName, ItemsCount, LastModified)
    public async Task<IEnumerable<object>> GetAllImagesInfoAsync()
    {
        _logger.LogInformation("Retrieving list of all images");
        var images = await _context.Images
            .Select(img => new {
                img.IdImage,
                img.FileName,
                img.ItemsCount,
                img.LastModified
            })
            .ToListAsync();

        _logger.LogInformation("Returned {Count} images", images.Count);
        return images;
    }




    // Restituisce i byte dell'immagine cercandola per nome file
    // Lancia FileNotFoundException se il file non esiste su disco
    public async Task<byte[]> GetImageByNameAsync(string itemImageName)
    {
        // qui si costruisce il percorso interno al container
        // Questo cercherà PRIMA nei User Secrets, poi nelle variabili d'ambiente, poi nel JSON
        // _itemsFolder
        _logger.LogInformation("Image requested by name: {ItemImageName}", itemImageName);
        var filePath = ResolveSafePath(itemImageName);

        // Controlla se il file esiste davvero
        if (!System.IO.File.Exists(filePath))
        {
            _logger.LogWarning("Image file not found on disk: {ItemImageName}", itemImageName);
            throw new FileNotFoundException($"Not Found!");
        }

        // Legge il file e restituisce i byte
        var imageBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        _logger.LogInformation("Image {ItemImageName} served successfully", itemImageName);
        return imageBytes;        
    }




    // Restituisce i byte dell'immagine cercandola per ID nel database
    // Lancia KeyNotFoundException se il record non esiste
    // Lancia FileNotFoundException se il file non esiste su disco
    public async Task<byte[]> GetImageByIdAsync(int idImage)
    {
        // cerca l'id nella tabella e recupera il nome dell'immagine
        _logger.LogInformation("Image requested by ID: {IdImage}", idImage);
        var image = await _context.Images.FindAsync(idImage);

        // se esiste continua, altrimenti errore notfound
        if (image == null)
        {
            _logger.LogWarning("Image record with ID {IdImage} not found in DB", idImage);
            throw new KeyNotFoundException("image search with ID not found");
        }
        string itemImageName = image.FileName;

        var filePath = ResolveSafePath(image.FileName);

        // Controlla se il file esiste davvero
        if (!System.IO.File.Exists(filePath))
        {
            _logger.LogWarning("Image file not found on disk for ID {IdImage}: {FileName}", idImage, itemImageName);
            throw new FileNotFoundException($"Not Found!");
        }

        // Legge il file e restituisce i byte
        var imageBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        _logger.LogInformation("Image with ID {IdImage} served successfully", idImage);
        return imageBytes;
    }




    // Restituisce la lista di immagini con ItemsCount <= qty
    public async Task<IEnumerable<object>> GetImagesByItemsCountAsync(int qty)
    {
        var images = await _context.Images
            .Select(img => new {
                img.IdImage,
                img.FileName,
                img.ItemsCount,
                img.LastModified
            })
            .Where(img => img.ItemsCount <= qty)
            .ToListAsync();

        _logger.LogInformation("Returned {Count} images with ItemsCount <= {Qty}", images.Count, qty);

        return images;
    }




    // Carica una nuova immagine su disco e crea il record nel database
    // Restituisce il messaggio di conferma con il percorso
    // Lancia ArgumentException se il file è null o vuoto
    public async Task<string> CreateImageAsync(string? itemName, IFormFile file)
    {
        _logger.LogInformation("Creating image for item: {ItemName}", itemName ?? "unknown");

        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("File is null or empty");
            throw new ArgumentException("No file selected");
        }

        // prendo il percorso di base dalla configurazione (User Secrets o Environment)
        // Assicuriamo che la cartella esista
        if (!Directory.Exists(_itemsFolder))
        {
            Directory.CreateDirectory(_itemsFolder);
            _logger.LogInformation("Created missing target directory: {ItemsFolder}", _itemsFolder);
        }

        // Creazione del nome del file e il percorso completo
        string fileName = await UploadImage(file, itemName ?? "unknown");
        _logger.LogInformation("Image saved to disk: {FileName}", fileName);

        // Creazione del record nel database
        var newImage = new Image
        {
            FileName = fileName
        };
        _context.Images.Add(newImage);
        await _context.SaveChangesAsync();

        _logger.LogInformation("DB record created with ID: {IdImage}", newImage.IdImage);
        return $"DONE: {fileName}";
    }




    // Sostituisce il file immagine su disco e aggiorna il record e gli Item collegati
    // Restituisce il messaggio di conferma
    // Lancia ArgumentException se il file è null o vuoto
    // Lancia KeyNotFoundException se il record non esiste
    public async Task<string> UpdateImageAsync(int id, string? itemName, IFormFile file)
    {
        // Controlli generici:
        _logger.LogInformation("Updating image with ID: {Id}", id);

        // Vede se l'utente ha inserito qualcosa
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("File is null or empty for update of ID: {Id}", id);
            throw new ArgumentException("No file selected.");
        }

        // mi riassicuro che la cartella esista
        if (!Directory.Exists(_itemsFolder))
        {
            Directory.CreateDirectory(_itemsFolder);
            _logger.LogInformation("Created missing target directory: {directory}", _itemsFolder);
        }

        // vede se il record esiste
        var imageRecord = await _context.Images.FindAsync(id);
        if (imageRecord == null)
        {
            _logger.LogWarning("Image record with ID {Id} not found in DB", id);
            throw new KeyNotFoundException($"Cannot update image. Record with ID {id} does not exist.");
        }

        // PRIMA salvo e valido il nuovo file: se è invalido lancia e il vecchio resta intatto
        string newFileName = await UploadImage(file, itemName ?? "unknown");
        _logger.LogInformation("New image saved to disk: {NewFileName}", newFileName);

        string itemImageName = imageRecord.FileName;
        var oldfilePath = ResolveSafePath(itemImageName);

        // cerca la vecchia immagine, controlla se il file esiste davvero, e nel caso lo elimina
        if (System.IO.File.Exists(oldfilePath))
        {
            System.IO.File.Delete(oldfilePath);
            _logger.LogInformation("Deleted old image file: {filePath}", oldfilePath);
        }
        else
        {
            _logger.LogWarning("Old image file not found on disk at: {filePath}", oldfilePath);
        }

        // salva il record con il nuovo nome dell'immagine
        imageRecord.FileName = newFileName;
        // _context.Images.Update(imageRecord);
        // ridondante perche ef lo traccia visto che è async, ed è tracciato dal "ChangeTracker" di ef

        // Cercare ogni item con idImage = a questo e aggiornare il nome immagine
        var itemsWithThisImage = await _context.Items.Where(item => item.IdImage == id).ToListAsync();
        _logger.LogInformation("Updating ImageName on {Count} item(s) linked to image ID: {Id}", itemsWithThisImage.Count, id);
        foreach (var item in itemsWithThisImage)
        {
            // item.IdImage = id; // in realtà non cambia nulla
            item.ImageName = newFileName; // aggiorna il nome dell'immagine negli item che usano questa immagine
            // _context.Items.Update(item); // sempre ridondante perche la ricerca è asyncrona
        }
        await _context.SaveChangesAsync();

        _logger.LogInformation("Image with ID {Id} updated successfully", id);
        return "DONE";
    }




    // Rinomina il file immagine su disco e aggiorna il record e gli Item collegati
    // Restituisce il messaggio di conferma
    // Lancia KeyNotFoundException se il record non esiste
    // Lancia FileNotFoundException se il file da rinominare non esiste su disco
    public async Task<string> RenameImageAsync(int id, string? itemName)
    {
        // Legge il percorso di base dalla configurazione (User Secrets o Environment)
        _logger.LogInformation("Renaming image with ID: {Id} to item name: {ItemName}", id, itemName ?? "unknown");

        var imageRecord = await _context.Images.FindAsync(id);
        if (imageRecord == null)
        {
            _logger.LogWarning("Image record with ID {Id} not found in DB", id);
            throw new KeyNotFoundException("you cant update a record that does not exist!");
        }

        string oldFileName = imageRecord.FileName;
        var oldFilePath = ResolveSafePath(oldFileName);

        // mantengo l'estensione reale del file, invece di forzare .jpg
        var extension = Path.GetExtension(imageRecord.FileName);


        string newFileName = GenerateUniqueFilename(itemName ?? "unknown", extension); // genera un nuovo nome basato sull'itemName o "unknown" se itemName è null o vuoto
        var newFilePath = ResolveSafePath(newFileName);

        if (!System.IO.File.Exists(oldFilePath))
        {
            _logger.LogWarning("File to rename not found on disk: {OldFilePath}", oldFilePath);
            throw new FileNotFoundException("file da rinominare non trovato");
        }

        System.IO.File.Move(oldFilePath, newFilePath);  // lancia IOException se la destinazione esiste
        imageRecord.FileName = newFileName;
        //_context.Images.Update(imageRecord);
       

        // OPZIONALE: cercare ogni item con idImage = a questo e aggiornare il nome immagine
        var itemsWithThisImage = await _context.Items.Where(item => item.IdImage == id).ToListAsync();
        _logger.LogInformation("Updating ImageName on {Count} item(s) linked to image ID: {Id}", itemsWithThisImage.Count, id);

        foreach (var item in itemsWithThisImage)
        {
            // item.IdImage = id; // in realtà non cambia nulla
            item.ImageName = newFileName; // aggiorna il nome dell'immagine negli item che usano questa immagine
            // _context.Items.Update(item); // ridondanza perche asyncrona
        }
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Image with ID {Id} renamed successfully", id);
        return "DONE";
    }




    // Elimina il file immagine su disco, rimuove il record e azzera i riferimenti negli Item
    // Restituisce il messaggio di conferma
    // Lancia KeyNotFoundException se il record non esiste
    public async Task<string> DeleteImageAsync(int id)
    {
        // Controlla se il record esiste
        _logger.LogInformation("Deleting image with ID: {Id}", id);

        var imageRecord = await _context.Images.FindAsync(id);
        if (imageRecord == null)
        {
            _logger.LogWarning("Image record with ID {Id} not found in DB", id);
            throw new KeyNotFoundException("you cant delete a record that does not exist!");
        }

        string itemImageName = imageRecord.FileName;
        var filePath = ResolveSafePath(itemImageName);  // viene eliminato dopo la rimozione dal database
        

        // elimina ogni riferimento a questa immagine negli item (id_image = null)
        var itemsWithThisImage = await _context.Items.Where(item => item.IdImage == id).ToListAsync();
        _logger.LogInformation("Clearing image reference on {Count} item(s) linked to image ID: {Id}", itemsWithThisImage.Count, id);

        foreach (var item in itemsWithThisImage)
        {
            item.IdImage = null;
            item.ImageName = null;
            //_context.Items.Update(item);
        }

        // elimina il record
        _context.Images.Remove(imageRecord);
        await _context.SaveChangesAsync();

        // elimina il file fisico, al assimo rimane un file orfano
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
            _logger.LogInformation("Deleted image file: {FilePath}", filePath);

        }
        else
        {
            _logger.LogWarning("Image file not found on disk, skipping file deletion: {FilePath}", filePath);
        }

        _logger.LogInformation("Image with ID {Id} deleted successfully", id);
        return "DONE";
    }




    // Salva un'immagine da uno Stream (usato dai plugin del MetadataService, non dal controller)
    // Restituisce l'IdImage del record creato nel DB
    public async Task<int> SaveImageFromStreamAsync(Stream imageStream, string suggestedExtension, string? itemName )
    {
        _logger.LogInformation("Saving image from stream for item: {ItemName}", itemName ?? "unknown");

        if (!Directory.Exists(_itemsFolder))
        {
            Directory.CreateDirectory(_itemsFolder);
            _logger.LogInformation("Created missing target directory: {ItemsFolder}", _itemsFolder);
        }

        // Copio in memoria con un limite di dimensione (gli stream HTTP non sono seekable)
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int n;
        while ((n = await imageStream.ReadAsync(chunk)) > 0)
        {
            if (buffer.Length + n > MaxPluginImageBytes)
                throw new ArgumentException("Image from plugin is too large");
            buffer.Write(chunk, 0, n);
        }
        buffer.Position = 0;

        // L'estensione suggerita dal plugin NON si usa: conta solo quella rilevata dai magic bytes
        var extension = await DetectImageExtensionAsync(buffer);
        if (extension == null)
        {
            _logger.LogWarning("Rejected plugin image for item {ItemName}: invalid magic bytes", itemName ?? "unknown");
            throw new ArgumentException("Invalid image from plugin");
        }

        // Riusa GenerateUniqueFilename per coerenza — se itemName è null usa "unknown"
        string fileName = GenerateUniqueFilename(itemName ?? "unknown", extension);           // es. "aB3xKq1z_coca_cola.jpg"

        var filePath = ResolveSafePath(fileName);

        await using (var fileStream = new FileStream(filePath, FileMode.CreateNew))
        {
            await buffer.CopyToAsync(fileStream);
        }
        _logger.LogInformation("Plugin image saved to disk");

        var newImage = new Image { FileName = fileName };
        _context.Images.Add(newImage);
        await _context.SaveChangesAsync();

        _logger.LogInformation("DB record created with ID: {IdImage} for plugin image: {FileName}", newImage.IdImage, fileName);
        return newImage.IdImage;
    }




    // --- Metodi privati di supporto ---

    private async Task<string> UploadImage(IFormFile file, string itemName = "unknown")
    {
        await using var input = file.OpenReadStream();

        // verifica che il file sia effettivamente un immagine coi magic bytes
        var extension = await DetectImageExtensionAsync(input);
        if (extension == null)
        {
            _logger.LogWarning("Rejected file '{OriginalName}': invalid magic bytes", file.FileName);
            throw new ArgumentException("Invalid file: only real .png and .jpg images are allowed");
        }

        if (file.Length > MaxPluginImageBytes)
        {
            _logger.LogWarning("Rejected file '{OriginalName}': File too large", file.FileName);
            throw new ArgumentException("File too large (max 10 MB)");
        }

        var fileName = GenerateUniqueFilename(itemName, extension); // Genera un nome unico basato sull'itemName
        var newFilePath = ResolveSafePath(fileName);

        await using (var stream = new FileStream(newFilePath, FileMode.CreateNew)) // CreateNew crea un immagine e se trova una collisione genera un errore e non sovrascirve
        {
            await input.CopyToAsync(stream);
           _logger.LogInformation("File uploaded to: {FileName}", fileName);
        }

        return fileName;
    }




    private string GenerateUniqueFilename(string itemName = "unknown", string extension = ".jpg")
    {
        var safe = string.Concat(itemName.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_' or ' '))
            .Replace(" ", "_");     // Sostituisce gli spazi con underscore

        if (safe.Length > 50) safe = safe[..50];    
        if (safe.Length == 0) safe = "unknown";

        // Genera una stringa casuale per evitare conflitti di nome
        var fileName = $"{StringHelper.GenerateRandomString(8)}_{safe}{extension}";
        _logger.LogDebug("Generated unique filename: {FileName}", fileName);
        return fileName;
    }





    // Legge i primi byte dello stream e restituisce l'estensione reale (".png" / ".jpg"),
    // oppure null se non è un'immagine supportata. Riporta lo stream all'inizio.
    private static async Task<string?> DetectImageExtensionAsync(Stream stream)
    {
        var header = new byte[8];
        // legge i primi 8 byte (ReadAtLeastAsync esiste da .NET 7+)
        int read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false);
        stream.Position = 0; // fondamentale: altrimenti CopyToAsync copia il file "senza testa"

        if (read >= PngSignature.Length && header.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature))
            return ".png";

        if (read >= JpegSignature.Length && header.AsSpan(0, JpegSignature.Length).SequenceEqual(JpegSignature))
            return ".jpg";

        return null;
    }



    // helper per pulire path anomali (per evitare Path traversal)
    private string ResolveSafePath(string fileName)
    {
        var safeName = Path.GetFileName(fileName);
        var fullPath = Path.GetFullPath(Path.Combine(_itemsFolder, safeName));
        var root = Path.GetFullPath(_itemsFolder) + Path.DirectorySeparatorChar;

        // difesa in profondità: il risultato deve stare dentro la cartella
        if (!fullPath.StartsWith(root, StringComparison.Ordinal))
            throw new FileNotFoundException("Not Found!");

        return fullPath;
    }
}