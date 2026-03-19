namespace app_api.Models;

/// <summary>
/// 购物车商品表
/// </summary>
public class CartItem
{
    /// <summary>
    /// 购物车 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户 ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 商品 ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 商品名称（快照）
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 商品价格（快照）
    /// </summary>
    public decimal ProductPrice { get; set; }

    /// <summary>
    /// 商品图片（快照）
    /// </summary>
    public string ProductImage { get; set; } = string.Empty;

    /// <summary>
    /// 购买数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 是否选中
    /// </summary>
    public bool Selected { get; set; } = true;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; } = DateTime.Now;
}