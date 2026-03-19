using Microsoft.AspNetCore.Http;

namespace app_api.Services;

public class FileUploadService
{
    private readonly IWebHostEnvironment _environment;

    public FileUploadService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// 上传文件到 wwwroot/uploads/{folder}
    /// </summary>
    public async Task<List<string>> UploadFilesAsync(List<IFormFile> files, string folder = "categories")
    {
        var uploadedPaths = new List<string>();
        
        if (files == null || files.Count == 0)
            return uploadedPaths;

        var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", folder);
        
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                // 生成唯一文件名
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 返回相对路径
                var relativePath = $"/uploads/{folder}/{fileName}";
                uploadedPaths.Add(relativePath);
            }
        }

        return uploadedPaths;
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    public void DeleteFile(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        var fullPath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
        
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    /// <summary>
    /// 删除多个文件
    /// </summary>
    public void DeleteFiles(List<string> relativePaths)
    {
        if (relativePaths == null)
            return;

        foreach (var path in relativePaths)
        {
            DeleteFile(path);
        }
    }
}
