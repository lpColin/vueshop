using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using app_api.Data;
using app_api.Models;
using app_api.Services;

namespace app_api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly FileUploadService _fileUploadService;

    public AdminController(AppDbContext context, FileUploadService fileUploadService)
    {
        _context = context;
        _fileUploadService = fileUploadService;
    }

    private async Task<bool> IsAdminAsync()
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
            return false;

        var user = await _context.Users.FindAsync(userId.Value);
        return user != null && user.Role == "admin";
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (!await IsAdminAsync()) return Forbid();

        var query = _context.Users.AsNoTracking().OrderByDescending(u => u.CreateTime);
        var total = await query.CountAsync();
        var list = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Nickname,
                u.Avatar,
                u.Phone,
                u.Role,
                u.Status,
                u.ShopId,
                u.CreateTime
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = new { total, page, pageSize, list }
        });
    }

    [HttpPut("users/{id}/status")]
    public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusRequest request)
    {
        if (!await IsAdminAsync()) return Forbid();

        if (request.Status is not (0 or 1))
            return BadRequest(new { success = false, message = "状态值不合法" });

        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { success = false, message = "用户不存在" });

        user.Status = request.Status;
        user.UpdateTime = DateTime.Now;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "状态更新成功" });
    }

    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
    {
        if (!await IsAdminAsync()) return Forbid();

        var role = (request.Role ?? string.Empty).Trim().ToLowerInvariant();
        if (role != "admin" && role != "merchant" && role != "user")
            return BadRequest(new { success = false, message = "角色值不合法" });

        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { success = false, message = "用户不存在" });

        user.Role = role;
        user.UpdateTime = DateTime.Now;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "角色更新成功" });
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null,
        [FromQuery] int? status = null)
    {
        if (!await IsAdminAsync()) return Forbid();

        var query = _context.Categories.AsNoTracking().AsQueryable();

        // 根据名称筛选
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(c => c.Name.Contains(keyword));
        }

        // 根据状态筛选
        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        var total = await query.CountAsync();
        var categories = await query
            .OrderBy(c => c.Sort)
            .ThenBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 转换为包含 images 数组的格式
        var list = categories.Select(c => new
        {
            c.Id,
            c.ShopId,
            c.Name,
            Images = c.GetImageList(),
            c.Sort,
            c.Status,
            c.CreateTime
        }).ToList();

        return Ok(new
        {
            success = true,
            data = new { total, page, pageSize, list }
        });
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (!await IsAdminAsync()) return Forbid();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { success = false, message = "分类名称不能为空" });

        var category = new Category
        {
            ShopId = request.ShopId > 0 ? request.ShopId : 1,
            Name = request.Name.Trim(),
            Sort = request.Sort < 1 ? 1 : request.Sort,
            Status = request.Status is 0 or 1 ? request.Status : 1,
            CreateTime = DateTime.Now
        };

        // 设置图片列表
        if (request.Images != null && request.Images.Count > 0)
        {
            category.SetImageList(request.Images);
        }

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "新增分类成功", data = category });
    }

    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        if (!await IsAdminAsync()) return Forbid();

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound(new { success = false, message = "分类不存在" });

        if (!string.IsNullOrWhiteSpace(request.Name))
            category.Name = request.Name.Trim();

        // 更新图片列表
        if (request.Images != null)
        {
            category.SetImageList(request.Images);
        }

        if (request.Sort > 0)
            category.Sort = request.Sort;

        if (request.Status is 0 or 1)
            category.Status = request.Status;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "分类更新成功" });
    }

    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        if (!await IsAdminAsync()) return Forbid();

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound(new { success = false, message = "分类不存在" });

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "分类删除成功" });
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? keyword = null)
    {
        if (!await IsAdminAsync()) return Forbid();

        var query = _context.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p => p.Name.Contains(keyword));
        }

        var total = await query.CountAsync();
        var products = await query
            .OrderByDescending(p => p.CreateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 转换为包含 images 数组的格式
        var list = products.Select(p => new
        {
            p.Id,
            p.ShopId,
            p.CategoryId,
            p.Name,
            p.Price,
            p.Stock,
            p.Status,
            p.Image,
            Images = p.GetImageList(),
            p.CreateTime
        }).ToList();

        return Ok(new
        {
            success = true,
            data = new { total, page, pageSize, list }
        });
    }

    [HttpPut("products/{id}/status")]
    public async Task<IActionResult> UpdateProductStatus(int id, [FromBody] UpdateProductStatusRequest request)
    {
        if (!await IsAdminAsync()) return Forbid();

        if (request.Status is not (0 or 1))
            return BadRequest(new { success = false, message = "状态值不合法" });

        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound(new { success = false, message = "商品不存在" });

        product.Status = request.Status;
        product.UpdateTime = DateTime.Now;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "商品状态更新成功" });
    }

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (!await IsAdminAsync()) return Forbid();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { success = false, message = "商品名称不能为空" });

        if (request.CategoryId <= 0)
            return BadRequest(new { success = false, message = "请选择分类" });

        if (request.Price <= 0)
            return BadRequest(new { success = false, message = "价格必须大于 0" });

        if (request.Stock < 0)
            return BadRequest(new { success = false, message = "库存不能为负数" });

        var product = new Product
        {
            ShopId = 1,
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Description = request.Description ?? "",
            Price = request.Price,
            OriginalPrice = request.OriginalPrice > 0 ? request.OriginalPrice : request.Price,
            Stock = request.Stock,
            Sales = 0,
            Image = request.Image ?? (request.Images != null && request.Images.Count > 0 ? request.Images[0] : ""),
            Images = request.Images != null && request.Images.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(request.Images) : null,
            Status = request.Status,
            CreateTime = DateTime.Now
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "新增商品成功", data = product });
    }

    [HttpPut("products/{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        if (!await IsAdminAsync()) return Forbid();

        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound(new { success = false, message = "商品不存在" });

        if (!string.IsNullOrWhiteSpace(request.Name))
            product.Name = request.Name.Trim();

        if (request.Description != null)
            product.Description = request.Description;

        if (request.Price > 0)
        {
            product.Price = request.Price;
            if (request.OriginalPrice > 0)
                product.OriginalPrice = request.OriginalPrice;
        }

        if (request.Stock >= 0)
            product.Stock = request.Stock;

        if (request.Image != null)
            product.Image = request.Image;

        if (request.Images != null)
        {
            product.Images = request.Images.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(request.Images) : null;
        }

        if (request.Status is 0 or 1)
            product.Status = request.Status;

        product.UpdateTime = DateTime.Now;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "商品更新成功" });
    }

    [HttpDelete("products/{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        if (!await IsAdminAsync()) return Forbid();

        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound(new { success = false, message = "商品不存在" });

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "商品删除成功" });
    }

    [HttpGet("shops")]
    public async Task<IActionResult> GetShops([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? keyword = null, [FromQuery] int? status = null)
    {
        if (!await IsAdminAsync()) return Forbid();

        var query = _context.Shops.AsNoTracking().AsQueryable();

        // 根据名称筛选
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(s => s.Name.Contains(keyword));
        }

        // 根据状态筛选
        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        var total = await query.CountAsync();
        var shops = await query
            .OrderByDescending(s => s.CreateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var list = shops.Select(s => new
        {
            s.Id,
            s.OwnerId,
            s.Name,
            s.Description,
            s.Avatar,
            s.Logo,
            s.Rating,
            s.ReviewCount,
            s.Phone,
            s.BusinessHours,
            s.Address,
            s.Status,
            s.CreateTime
        }).ToList();

        return Ok(new
        {
            success = true,
            data = new { total, page, pageSize, list }
        });
    }

    [HttpPost("shops")]
    public async Task<IActionResult> CreateShop([FromBody] CreateShopRequest request)
    {
        if (!await IsAdminAsync()) return Forbid();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { success = false, message = "店铺名称不能为空" });

        // 新增店铺时店主 ID 可选（自增字段）
        var shop = new Shop
        {
            OwnerId = (request.OwnerId > 0 ? request.OwnerId : 1) ?? 1, // 默认值为 1
            Name = request.Name.Trim(),
            Description = request.Description ?? "",
            Avatar = request.Avatar ?? "",
            Logo = request.Logo,
            Rating = request.Rating > 0 ? request.Rating : 5.0m,
            ReviewCount = request.ReviewCount >= 0 ? request.ReviewCount : 0,
            Phone = request.Phone ?? "",
            BusinessHours = request.BusinessHours ?? "09:00-22:00",
            Address = request.Address ?? "",
            Status = request.Status is 0 or 1 ? request.Status : 1,
            CreateTime = DateTime.Now
        };

        _context.Shops.Add(shop);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "新增店铺成功", data = shop });
    }

    [HttpPut("shops/{id}")]
    public async Task<IActionResult> UpdateShop(int id, [FromBody] UpdateShopRequest request)
    {
        if (!await IsAdminAsync()) return Forbid();

        var shop = await _context.Shops.FindAsync(id);
        if (shop == null)
            return NotFound(new { success = false, message = "店铺不存在" });

        if (!string.IsNullOrWhiteSpace(request.Name))
            shop.Name = request.Name.Trim();

        if (request.OwnerId > 0)
            shop.OwnerId = request.OwnerId ?? shop.OwnerId;

        if (request.Description != null)
            shop.Description = request.Description;

        if (request.Avatar != null)
            shop.Avatar = request.Avatar;

        if (request.Logo != null)
            shop.Logo = request.Logo;

        if (request.Rating > 0)
            shop.Rating = request.Rating;

        if (request.ReviewCount >= 0)
            shop.ReviewCount = request.ReviewCount;

        if (request.Phone != null)
            shop.Phone = request.Phone;

        if (request.BusinessHours != null)
            shop.BusinessHours = request.BusinessHours;

        if (request.Address != null)
            shop.Address = request.Address;

        if (request.Status is 0 or 1)
            shop.Status = request.Status;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "店铺信息更新成功" });
    }

    [HttpDelete("shops/{id}")]
    public async Task<IActionResult> DeleteShop(int id)
    {
        if (!await IsAdminAsync()) return Forbid();

        var shop = await _context.Shops.FindAsync(id);
        if (shop == null)
            return NotFound(new { success = false, message = "店铺不存在" });

        _context.Shops.Remove(shop);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "店铺删除成功" });
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? status = null,
        [FromQuery] string? keyword = null)
    {
        if (!await IsAdminAsync()) return Forbid();

        var query = _context.Orders.AsNoTracking().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(o => o.OrderNo.Contains(keyword) || o.ReceiverName.Contains(keyword) || o.ReceiverPhone.Contains(keyword));
        }

        var total = await query.CountAsync();
        var orders = await query
            .OrderByDescending(o => o.CreateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var orderIds = orders.Select(o => o.Id).ToList();
        var userIds = orders.Select(o => o.UserId).Distinct().ToList();

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => new { u.Username, u.Nickname });

        var orderItems = await _context.OrderItems
            .Where(oi => orderIds.Contains(oi.OrderId))
            .OrderBy(oi => oi.Id)
            .ToListAsync();

        var list = orders.Select(o => new
        {
            o.Id,
            o.OrderNo,
            o.UserId,
            CustomerName = users.ContainsKey(o.UserId)
                ? (!string.IsNullOrWhiteSpace(users[o.UserId].Nickname) ? users[o.UserId].Nickname : users[o.UserId].Username)
                : $"用户{o.UserId}",
            o.PayAmount,
            o.TotalAmount,
            o.DeliveryFee,
            o.DiscountAmount,
            o.Status,
            o.ReceiverName,
            o.ReceiverPhone,
            o.ReceiverAddress,
            o.DeliveryMethod,
            o.CreateTime,
            o.PayTime,
            o.ShipTime,
            o.CompleteTime,
            Items = orderItems.Where(oi => oi.OrderId == o.Id).Select(oi => new
            {
                oi.Id,
                oi.ProductId,
                oi.ProductName,
                oi.ProductImage,
                oi.ProductPrice,
                oi.Quantity,
                oi.Amount
            })
        });

        return Ok(new
        {
            success = true,
            data = new { total, page, pageSize, list }
        });
    }

    [HttpPost("orders/{id}/ship")]
    public async Task<IActionResult> ShipOrder(int id)
    {
        if (!await IsAdminAsync()) return Forbid();

        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return NotFound(new { success = false, message = "订单不存在" });

        if (order.Status != 1)
            return BadRequest(new { success = false, message = "只有待发货订单可发货" });

        order.Status = 2;
        order.ShipTime = DateTime.Now;
        order.UpdateTime = DateTime.Now;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "订单已发货" });
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        if (!await IsAdminAsync()) return Forbid();

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var todayOrders = await _context.Orders
            .Where(o => o.CreateTime >= today && o.CreateTime < tomorrow)
            .ToListAsync();

        var todaySales = todayOrders.Sum(o => o.PayAmount);
        var todayOrderCount = todayOrders.Count;
        var totalSold = await _context.OrderItems.SumAsync(i => (int?)i.Quantity) ?? 0;
        var totalUsers = await _context.Users.CountAsync();

        var pending = new
        {
            unpaid = await _context.Orders.CountAsync(o => o.Status == 0),
            toShip = await _context.Orders.CountAsync(o => o.Status == 1),
            toReceive = await _context.Orders.CountAsync(o => o.Status == 2),
            resolving = await _context.Orders.CountAsync(o => o.Status == 4),
            refund = await _context.Orders.CountAsync(o => o.Status == 4)
        };

        var topProducts = await _context.OrderItems
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new
            {
                ProductId = g.Key.ProductId,
                Name = g.Key.ProductName,
                Sales = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.Sales)
            .Take(5)
            .ToListAsync();

        var productIds = topProducts.Select(x => x.ProductId).ToList();
        var productMap = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.CategoryId);

        var categoryIds = productMap.Values.Distinct().ToList();
        var categoryMap = await _context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name);

        var hotProducts = topProducts.Select(x => new
        {
            x.ProductId,
            x.Name,
            Category = productMap.ContainsKey(x.ProductId) && categoryMap.ContainsKey(productMap[x.ProductId])
                ? categoryMap[productMap[x.ProductId]]
                : "未分类",
            x.Sales
        });

        var trend = new List<object>();
        for (int i = 6; i >= 0; i--)
        {
            var dayStart = today.AddDays(-i);
            var dayEnd = dayStart.AddDays(1);
            var amount = await _context.Orders
                .Where(o => o.CreateTime >= dayStart && o.CreateTime < dayEnd)
                .SumAsync(o => (decimal?)o.PayAmount) ?? 0m;

            trend.Add(new
            {
                day = dayStart.ToString("MM/dd"),
                amount = Math.Round(amount, 2)
            });
        }

        return Ok(new
        {
            success = true,
            data = new
            {
                stats = new
                {
                    todaySales = Math.Round(todaySales, 2),
                    todayOrderCount,
                    totalSold,
                    totalUsers
                },
                pending,
                hotProducts,
                trend
            }
        });
    }

    /// <summary>
    /// 上传分类图片
    /// </summary>
    [HttpPost("categories/upload")]
    public async Task<IActionResult> UploadCategoryImages(List<IFormFile> files)
    {
        if (!await IsAdminAsync()) return Forbid();

        if (files == null || files.Count == 0)
            return BadRequest(new { success = false, message = "请选择要上传的文件" });

        try
        {
            var uploadedPaths = await _fileUploadService.UploadFilesAsync(files, "categories");
            return Ok(new
            {
                success = true,
                message = "上传成功",
                data = uploadedPaths
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = $"上传失败：{ex.Message}"
            });
        }
    }

    /// <summary>
    /// 删除分类图片
    /// </summary>
    [HttpPost("categories/delete-image")]
    public async Task<IActionResult> DeleteCategoryImage([FromBody] DeleteImageRequest request)
    {
        if (!await IsAdminAsync()) return Forbid();

        if (string.IsNullOrWhiteSpace(request.ImagePath))
            return BadRequest(new { success = false, message = "图片路径不能为空" });

        try
        {
            _fileUploadService.DeleteFile(request.ImagePath);
            return Ok(new
            {
                success = true,
                message = "删除成功"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = $"删除失败：{ex.Message}"
            });
        }
    }

    /// <summary>
    /// 更新分类排序
    /// </summary>
    [HttpPut("categories/{id}/sort")]
    public async Task<IActionResult> UpdateCategorySort(int id, [FromBody] UpdateCategorySortRequest request)
    {
        if (!await IsAdminAsync()) return Forbid();

        if (request.Sort < 1)
            return BadRequest(new { success = false, message = "排序值必须为正整数" });

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound(new { success = false, message = "分类不存在" });

        category.Sort = request.Sort;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "排序更新成功" });
    }
}

public class UpdateUserStatusRequest
{
    public int Status { get; set; }
}

public class UpdateUserRoleRequest
{
    public string Role { get; set; } = string.Empty;
}

public class CreateCategoryRequest
{
    public int ShopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string>? Images { get; set; }
    public int Sort { get; set; } = 1;
    public int Status { get; set; } = 1;
}

public class UpdateCategoryRequest
{
    public string? Name { get; set; }
    public List<string>? Images { get; set; }
    public int Sort { get; set; }
    public int Status { get; set; }
}

public class UpdateProductStatusRequest
{
    public int Status { get; set; }
}

public class CreateShopRequest
{
    public int? OwnerId { get; set; }  // 可选字段（自增）
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Avatar { get; set; }
    public string? Logo { get; set; }
    public decimal Rating { get; set; } = 5.0m;
    public int ReviewCount { get; set; }
    public string? Phone { get; set; }
    public string? BusinessHours { get; set; }
    public string? Address { get; set; }
    public int Status { get; set; } = 1;
}

public class UpdateShopRequest
{
    public int? OwnerId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Avatar { get; set; }
    public string? Logo { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string? Phone { get; set; }
    public string? BusinessHours { get; set; }
    public string? Address { get; set; }
    public int Status { get; set; }
}

public class DeleteImageRequest
{
    public string ImagePath { get; set; } = string.Empty;
}

public class UpdateCategorySortRequest
{
    public int Sort { get; set; }
}
