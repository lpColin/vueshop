using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using app_api.Data;

namespace app_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取分类列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories([FromQuery] int? shopId = null, [FromQuery] int status = 1)
    {
        var query = _context.Categories.AsNoTracking().AsQueryable();

        if (shopId.HasValue)
        {
            query = query.Where(c => c.ShopId == shopId.Value);
        }

        if (status is 0 or 1)
        {
            query = query.Where(c => c.Status == status);
        }

        var list = await query
            .OrderBy(c => c.Sort)
            .ThenBy(c => c.Id)
            .ToListAsync();

        // 转换为包含 images 数组的格式
        var resultList = list.Select(c => new
        {
            c.Id,
            c.ShopId,
            c.Name,
            Images = c.GetImageList(),
            c.Sort,
            c.Status
        }).ToList();

        return Ok(new
        {
            success = true,
            data = resultList
        });
    }
}
