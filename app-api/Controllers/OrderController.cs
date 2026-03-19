using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using app_api.Models;
using app_api.Data;
using app_api.Services;
using Microsoft.EntityFrameworkCore;

namespace app_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrderController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取订单列表（分页）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? status = null)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var query = _context.Orders.Where(o => o.UserId == userId);

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var total = await query.CountAsync();
        var orders = await query
            .OrderByDescending(o => o.CreateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var orderIds = orders.Select(o => o.Id).ToList();
        var orderItems = await _context.OrderItems
            .Where(oi => orderIds.Contains(oi.OrderId))
            .ToListAsync();

        var result = orders.Select(o => new
        {
            o.Id,
            o.OrderNo,
            o.UserId,
            o.ShopId,
            o.TotalAmount,
            o.DeliveryFee,
            o.DiscountAmount,
            o.PayAmount,
            o.Status,
            o.ReceiverName,
            o.ReceiverPhone,
            o.ReceiverAddress,
            o.DeliveryMethod,
            o.Remark,
            o.CreateTime,
            o.PayTime,
            o.ShipTime,
            o.CompleteTime,
            Items = orderItems.Where(oi => oi.OrderId == o.Id).Select(oi => new
            {
                oi.Id,
                oi.ProductId,
                oi.ProductName,
                oi.ProductPrice,
                oi.ProductImage,
                oi.Quantity,
                oi.Amount
            })
        });

        return Ok(new
        {
            success = true,
            data = new
            {
                total,
                page,
                pageSize,
                list = result
            }
        });
    }

    /// <summary>
    /// 获取订单详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound(new { success = false, message = "订单不存在" });
        }

        var items = await _context.OrderItems
            .Where(oi => oi.OrderId == id)
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = new
            {
                order.Id,
                order.OrderNo,
                order.UserId,
                order.ShopId,
                order.TotalAmount,
                order.DeliveryFee,
                order.DiscountAmount,
                order.PayAmount,
                order.Status,
                order.ReceiverName,
                order.ReceiverPhone,
                order.ReceiverAddress,
                order.DeliveryMethod,
                order.Remark,
                order.CreateTime,
                order.PayTime,
                order.ShipTime,
                order.CompleteTime,
                Items = items
            }
        });
    }

    /// <summary>
    /// 创建订单
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        if (request.ItemIds == null || request.ItemIds.Count == 0)
        {
            return BadRequest(new { success = false, message = "请选择商品" });
        }

        // 获取购物车选中的商品
        var cartItems = await _context.CartItems
            .Where(c => c.UserId == userId && request.ItemIds.Contains(c.Id))
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            return BadRequest(new { success = false, message = "请选择要购买的商品" });
        }

        // 验证商品库存和价格
        var productIds = cartItems.Select(c => c.ProductId).ToList();
        var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        var totalAmount = 0m;
        var orderItems = new List<OrderItem>();

        foreach (var cartItem in cartItems)
        {
            var product = products.FirstOrDefault(p => p.Id == cartItem.ProductId);
            if (product == null)
            {
                return BadRequest(new { success = false, message = $"商品 {cartItem.ProductName} 不存在" });
            }

            if (product.Stock < cartItem.Quantity)
            {
                return BadRequest(new { success = false, message = $"商品 {product.Name} 库存不足" });
            }

            var amount = product.Price * cartItem.Quantity;
            totalAmount += amount;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductPrice = product.Price,
                ProductImage = product.Image,
                Quantity = cartItem.Quantity,
                Amount = amount
            });

            // 扣减库存
            product.Stock -= cartItem.Quantity;
            product.Sales += cartItem.Quantity;
        }

        // 生成订单号
        var orderNo = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(1000, 9999);

        var firstProduct = products.FirstOrDefault();
        var order = new Order
        {
            OrderNo = orderNo,
            UserId = userId.Value,
            ShopId = firstProduct?.ShopId ?? 0,
            TotalAmount = totalAmount,
            DeliveryFee = request.DeliveryFee ?? 0,
            DiscountAmount = request.DiscountAmount ?? 0,
            PayAmount = totalAmount + (request.DeliveryFee ?? 0) - (request.DiscountAmount ?? 0),
            Status = 0, // 待付款
            ReceiverName = request.ReceiverName,
            ReceiverPhone = request.ReceiverPhone,
            ReceiverAddress = request.ReceiverAddress,
            DeliveryMethod = request.DeliveryMethod ?? "快递",
            Remark = request.Remark,
            CreateTime = DateTime.Now
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // 保存订单商品
        foreach (var item in orderItems)
        {
            item.OrderId = order.Id;
            _context.OrderItems.Add(item);
        }

        // 删除已购买的购物车商品
        _context.CartItems.RemoveRange(cartItems);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "订单创建成功",
            data = new
            {
                order.Id,
                order.OrderNo,
                order.PayAmount
            }
        });
    }

    /// <summary>
    /// 取消订单
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound(new { success = false, message = "订单不存在" });
        }

        if (order.Status != 0)
        {
            return BadRequest(new { success = false, message = "订单状态不允许取消" });
        }

        order.Status = 4; // 已取消
        order.UpdateTime = DateTime.Now;

        // 恢复库存
        var orderItems = await _context.OrderItems.Where(oi => oi.OrderId == id).ToListAsync();
        foreach (var item in orderItems)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                product.Stock += item.Quantity;
                product.Sales -= item.Quantity;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "订单已取消"
        });
    }

    /// <summary>
    /// 确认收货
    /// </summary>
    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> ConfirmOrder(int id)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound(new { success = false, message = "订单不存在" });
        }

        if (order.Status != 2)
        {
            return BadRequest(new { success = false, message = "订单状态不允许确认收货" });
        }

        order.Status = 3; // 已完成
        order.CompleteTime = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "已确认收货"
        });
    }

    /// <summary>
    /// 删除订单
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound(new { success = false, message = "订单不存在" });
        }

        if (order.Status is not 3 and not 4)
        {
            return BadRequest(new { success = false, message = "只能删除已完成或已取消的订单" });
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "订单已删除"
        });
    }
}

// DTOs
public class CreateOrderRequest
{
    public List<int> ItemIds { get; set; } = new();
    public List<CartItem>? Items { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string ReceiverAddress { get; set; } = string.Empty;
    public string? DeliveryMethod { get; set; }
    public string? Remark { get; set; }
    public decimal? DeliveryFee { get; set; }
    public decimal? DiscountAmount { get; set; }
}