using System.Text.Json;

namespace app_api.Models;

/// <summary>
/// 商品分类表
/// </summary>
public class Category
{
    /// <summary>
    /// 分类 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 关联商家 ID
    /// </summary>
    public int ShopId { get; set; }

    /// <summary>
    /// 分类名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 图标（JSON 数组，存储多个图片路径）
    /// </summary>
    public string? Images { get; set; }

    /// <summary>
    /// 排序（正整数）
    /// </summary>
    public int Sort { get; set; } = 1;

    /// <summary>
    /// 状态：1 启用 0 禁用
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 获取图片列表
    /// </summary>
    public List<string> GetImageList()
    {
        if (string.IsNullOrWhiteSpace(Images))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(Images) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// 设置图片列表
    /// </summary>
    public void SetImageList(List<string> images)
    {
        Images = images != null && images.Count > 0 
            ? JsonSerializer.Serialize(images) 
            : null;
    }
}