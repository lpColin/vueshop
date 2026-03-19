using System.Text.Json;

namespace app_api.Models;

/// <summary>
/// 商品表
/// </summary>
public class Product
{
    /// <summary>
    /// 商品 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 关联商家 ID
    /// </summary>
    public int ShopId { get; set; }

    /// <summary>
    /// 关联分类 ID
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 商品描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 价格
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// 原价
    /// </summary>
    public decimal OriginalPrice { get; set; }

    /// <summary>
    /// 库存
    /// </summary>
    public int Stock { get; set; }

    /// <summary>
    /// 销量
    /// </summary>
    public int Sales { get; set; }

    /// <summary>
    /// 主图 URL
    /// </summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// 多图 URL（JSON 数组）
    /// </summary>
    public string? Images { get; set; }

    /// <summary>
    /// 状态：1 上架 0 下架
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