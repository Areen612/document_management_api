using Microsoft.AspNetCore.Mvc;
using DocumentManagementAPI.Models;

namespace DocumentManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public DocumentsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<ActionResult<Document>> UploadDocument(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

            if (file.Length > 10 * 1024 * 1024) // Max 10MB
                return BadRequest("File size exceeds the limit.");

            var supportedFileTypes = new[] { "pdf", "docx", "txt" };
            var fileExtension = Path.GetExtension(file.FileName).TrimStart('.');
            if (!supportedFileTypes.Contains(fileExtension))
                return BadRequest("Unsupported file type.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var document = new Document
            {
                Name = file.FileName,
                Content = memoryStream.ToArray(),
                FileType = file.ContentType,
                CreationDate = DateTime.UtcNow
            };

            _dbContext.Documents.Add(document);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(DownloadDocument), new { id = document.ID }, document);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FileContentResult>> DownloadDocument(int id)
        {
            var document = await _dbContext.Documents.FindAsync(id);

            if (document == null)
                return NotFound();

            return File(document.Content, document.FileType, document.Name);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(int id, Document documentToUpdate)
        {
            if (id != documentToUpdate.ID)
                return BadRequest("ID in URL does not match ID in request body.");

            var existingDocument = await _dbContext.Documents.FindAsync(id);

            if (existingDocument == null)
                return NotFound();

            existingDocument.Name = documentToUpdate.Name;
            existingDocument.Content = documentToUpdate.Content;
            existingDocument.FileType = documentToUpdate.FileType;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var document = await _dbContext.Documents.FindAsync(id);

            if (document == null)
                return NotFound();

            _dbContext.Documents.Remove(document);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }

}
