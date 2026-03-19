namespace app_api.Models;

/// <summary>
/// 商家店铺表
/// </summary>
public class Shop
{
    /// <summary>
    /// 店铺 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 关联用户 ID（店主）
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// 店铺名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 店铺描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 店铺头像
    /// </summary>
    public string Avatar { get; set; } = string.Empty;

    /// <summary>
    /// 店铺 Logo
    /// </summary>
    public string? Logo { get; set; }

    /// <summary>
    /// 评分
    /// </summary>
    public decimal Rating { get; set; } = 5.0m;

    /// <summary>
    /// 关注人数/评价数
    /// </summary>
    public int ReviewCount { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// 营业时间
    /// </summary>
    public string BusinessHours { get; set; } = string.Empty;

    /// <summary>
    /// 店铺地址
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// 状态：1 营业 0 打烊
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; } = DateTime.Now;
}