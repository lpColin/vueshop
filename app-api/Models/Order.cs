namespace app_api.Models;

/// <summary>
/// 订单表
/// </summary>
public class Order
{
    /// <summary>
    /// 订单 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>
    /// 用户 ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 商家 ID
    /// </summary>
    public int ShopId { get; set; }

    /// <summary>
    /// 订单总金额
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 配送费
    /// </summary>
    public decimal DeliveryFee { get; set; }

    /// <summary>
    /// 优惠金额
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// 实付金额
    /// </summary>
    public decimal PayAmount { get; set; }

    /// <summary>
    /// 订单状态：0 待付款 1 待发货 2 待收货 3 已完成 4 已取消
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 收货人姓名
    /// </summary>
    public string ReceiverName { get; set; } = string.Empty;

    /// <summary>
    /// 收货人电话
    /// </summary>
    public string ReceiverPhone { get; set; } = string.Empty;

    /// <summary>
    /// 收货地址
    /// </summary>
    public string ReceiverAddress { get; set; } = string.Empty;

    /// <summary>
    /// 配送方式
    /// </summary>
    public string DeliveryMethod { get; set; } = string.Empty;

    /// <summary>
    /// 订单备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 付款时间
    /// </summary>
    public DateTime? PayTime { get; set; }

    /// <summary>
    /// 发货时间
    /// </summary>
    public DateTime? ShipTime { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? CompleteTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
}
