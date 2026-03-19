namespace app_api.Models;

/// <summary>
/// 用户表 - 支持三种角色：管理员、商家、普通用户
/// </summary>
public class User
{
    /// <summary>
    /// 用户 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码（加密存储）
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 微信 OpenId（预留）
    /// </summary>
    public string? OpenId { get; set; }

    /// <summary>
    /// 微信 UnionId（预留）
    /// </summary>
    public string? UnionId { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// 头像 URL
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 角色：admin(管理员) / merchant(商家) / user(普通用户)
    /// </summary>
    public string Role { get; set; } = "user";

    /// <summary>
    /// 关联商家 ID（商家角色时有值）
    /// </summary>
    public int? ShopId { get; set; }

    /// <summary>
    /// 状态：1 正常 0 禁用
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
}