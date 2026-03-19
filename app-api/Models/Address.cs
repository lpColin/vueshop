namespace app_api.Models;

/// <summary>
/// 用户收货地址表
/// </summary>
public class Address
{
    /// <summary>
    /// 地址 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户 ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 收货人姓名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 收货人电话
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// 省
    /// </summary>
    public string Province { get; set; } = string.Empty;

    /// <summary>
    /// 市
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// 区
    /// </summary>
    public string District { get; set; } = string.Empty;

    /// <summary>
    /// 详细地址
    /// </summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>
    /// 是否默认地址
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; } = DateTime.Now;
}