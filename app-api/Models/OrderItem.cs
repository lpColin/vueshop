namespace app_api.Models;

/// <summary>
/// 订单商品表
/// </summary>
public class OrderItem
{
    /// <summary>
    /// 订单商品 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 订单 ID
    /// </summary>
    public int OrderId { get; set; }

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
    /// 小计金额
    /// </summary>
    public decimal Amount { get; set; }
}