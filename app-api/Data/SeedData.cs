using app_api.Models;
using app_api.Services;

namespace app_api.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        // 添加管理员用户
        context.Users.Add(new User
        {
            Username = "admin",
            Password = AuthService.HashPassword("123456"),
            Nickname = "系统管理员",
            Role = "admin",
            Status = 1,
            CreateTime = DateTime.Now
        });

        // 添加商家用户
        context.Users.Add(new User
        {
            Username = "shop1",
            Password = AuthService.HashPassword("123456"),
            Nickname = "测试商家",
            Role = "merchant",
            ShopId = 1,
            Status = 1,
            CreateTime = DateTime.Now
        });

        // 添加普通用户
        context.Users.Add(new User
        {
            Username = "user1",
            Password = AuthService.HashPassword("123456"),
            Nickname = "测试用户",
            Avatar = "/static/images/avatar.png",
            Role = "user",
            Status = 1,
            CreateTime = DateTime.Now
        });

        // 添加商家
        context.Shops.Add(new Shop
        {
            Name = "官方旗舰店",
            Description = "品质保证，正品直营",
            Logo = "/static/images/shop1.png",
            Status = 1,
            CreateTime = DateTime.Now
        });

        // 添加分类（7 个指定分类）
        var categories = new[]
        {
            new Category { Name = "热销", Sort = 1, Status = 1 },
            new Category { Name = "碳酸饮料", Sort = 2, Status = 1 },
            new Category { Name = "啤酒", Sort = 3, Status = 1 },
            new Category { Name = "矿泉水", Sort = 4, Status = 1 },
            new Category { Name = "功能性饮料", Sort = 5, Status = 1 },
            new Category { Name = "咖啡类", Sort = 6, Status = 1 },
            new Category { Name = "零食", Sort = 7, Status = 1 }
        };
        
        // 为每个分类设置 2 张默认图片
        var categoryImages = new Dictionary<string, List<string>>
        {
            { "热销", new List<string> { "/static/images/cat-hot1.png", "/static/images/cat-hot2.png" } },
            { "碳酸饮料", new List<string> { "/static/images/cat-soda1.png", "/static/images/cat-soda2.png" } },
            { "啤酒", new List<string> { "/static/images/cat-beer1.png", "/static/images/cat-beer2.png" } },
            { "矿泉水", new List<string> { "/static/images/cat-water1.png", "/static/images/cat-water2.png" } },
            { "功能性饮料", new List<string> { "/static/images/cat-energy1.png", "/static/images/cat-energy2.png" } },
            { "咖啡类", new List<string> { "/static/images/cat-coffee1.png", "/static/images/cat-coffee2.png" } },
            { "零食", new List<string> { "/static/images/cat-snack1.png", "/static/images/cat-snack2.png" } }
        };
        
        foreach (var cat in categories)
        {
            if (categoryImages.ContainsKey(cat.Name))
            {
                cat.SetImageList(categoryImages[cat.Name]);
            }
        }
        
        context.Categories.AddRange(categories);

        context.SaveChanges();

        // 为每个分类添加 3 个商品（共 7 个分类 × 3 个商品 = 21 个商品）
        var products = new List<Product>();
        
        // 分类 1: 热销
        products.AddRange(new[]
        {
            new Product { ShopId = 1, CategoryId = 1, Name = "热销爆款组合装", Description = "精选热销商品组合，超值优惠", Price = 199m, OriginalPrice = 299m, Stock = 500, Sales = 2800, Image = "/static/images/prod1.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod1.png", "/static/images/prod2.png" }), Status = 1, CreateTime = DateTime.Now },
            new Product { ShopId = 1, CategoryId = 1, Name = "店长推荐套装", Description = "店长精心挑选，品质保证", Price = 158m, OriginalPrice = 228m, Stock = 380, Sales = 1950, Image = "/static/images/prod2.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod2.png", "/static/images/prod3.png" }), Status = 1, CreateTime = DateTime.Now },
            new Product { ShopId = 1, CategoryId = 1, Name = "限时特惠大礼包", Description = "限时优惠，买到就是赚到", Price = 288m, OriginalPrice = 399m, Stock = 200, Sales = 1580, Image = "/static/images/prod3.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod3.png", "/static/images/prod1.png" }), Status = 1, CreateTime = DateTime.Now }
        });
        
        // 分类 2: 碳酸饮料
        products.AddRange(new[]
        {
            new Product { ShopId = 1, CategoryId = 2, Name = "可口可乐 330ml*24 罐", Description = "经典口味，冰爽畅饮", Price = 58m, OriginalPrice = 72m, Stock = 1000, Sales = 5200, Image = "/static/images/prod4.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod4.png", "/static/images/prod5.png" }), Status = 1, CreateTime = DateTime.Now },
            new Product { ShopId = 1, CategoryId = 2, Name = "百事可乐 500ml*12 瓶", Description = "渴望无限，畅爽体验", Price = 36m, OriginalPrice = 48m, Stock = 800, Sales = 4100, Image = "/static/images/prod5.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod5.png", "/static/images/prod6.png" }), Status = 1, CreateTime = DateTime.Now },
            new Product { ShopId = 1, CategoryId = 2, Name = "雪碧柠檬味 330ml*24 罐", Description = "透心凉，心飞扬", Price = 55m, OriginalPrice = 70m, Stock = 600, Sales = 3800, Image = "/static/images/prod6.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod6.png", "/static/images/prod4.png" }), Status = 1, CreateTime = DateTime.Now }
        });
        
        // 分类 3: 啤酒
        products.AddRange(new[]
        {
            new Product { ShopId = 1, CategoryId = 3, Name = "青岛啤酒经典 500ml*12 罐", Description = "百年青啤，经典醇香", Price = 89m, OriginalPrice = 108m, Stock = 500, Sales = 2900, Image = "/static/images/prod7.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod7.png", "/static/images/prod8.png" }), Status = 1, CreateTime = DateTime.Now },
            new Product { ShopId = 1, CategoryId = 3, Name = "百威啤酒 330ml*24 瓶", Description = "王者风范，真我呈现", Price = 128m, OriginalPrice = 158m, Stock = 400, Sales = 2200, Image = "/static/images/prod8.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod8.png", "/static/images/prod9.png" }), Status = 1, CreateTime = DateTime.Now },
            new Product { ShopId = 1, CategoryId = 3, Name = "哈尔滨啤酒冰纯 500ml*12 罐", Description = "冰纯口感，清爽怡人", Price = 68m, OriginalPrice = 88m, Stock = 600, Sales = 1800, Image = "/static/images/prod9.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod9.png", "/static/images/prod7.png" }), Status = 1, CreateTime = DateTime.Now }
        });
        
        // 分类 4: 矿泉水
        products.AddRange(new[]
        {
            new Product { ShopId = 1, CategoryId = 4, Name = "农夫山泉 550ml*24 瓶", Description = "天然弱碱性矿泉水", Price = 48m, OriginalPrice = 60m, Stock = 1200, Sales = 6800, Image = "/static/images/prod10.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod10.png", "/static/images/prod11.png" }), Status = 1, CreateTime = DateTime.Now },
            new Product { ShopId = 1, CategoryId = 4, Name = "怡宝纯净水 350ml*24 瓶", Description = "纯净好水，健康相伴", Price = 36m, OriginalPrice = 48m, Stock = 1000, Sales = 5500, Image = "/static/images/prod11.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod11.png", "/static/images/prod12.png" }), Status = 1, CreateTime = DateTime.Now },
            new Product { ShopId = 1, CategoryId = 4, Name = "百岁山矿泉水 570ml*24 瓶", Description = "水中贵族，天然矿泉水", Price = 72m, OriginalPrice = 96m, Stock = 800, Sales = 4200, Image = "/static/images/prod12.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod12.png", "/static/images/prod10.png" }), Status = 1, CreateTime = DateTime.Now }
        });
        
        // 分类 5: 功能性饮料
        products.AddRange(new[]
        {
            new Product { ShopId = 1, CategoryId = 5, Name = "红牛维生素饮料 250ml*24 罐", Description = "补充能量，提神醒脑", Price = 168m, OriginalPrice = 192m, Stock = 600, Sales = 3500, Image = "/static/images/prod13.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod13.png", "/static/images/prod14.png" }), Status = 1, CreateTime = DateTime.Now },
            new Product { ShopId = 1, CategoryId = 5, Name = "东鹏特饮 500ml*15 瓶", Description = "累了困了喝东鹏特饮", Price = 78m, OriginalPrice = 90m, Stock = 700, Sales = 2900, Image = "/static/images/prod14.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod14.png", "/static/images/prod15.png" }), Status = 1, CreateTime = DateTime.Now },
            new Product { ShopId = 1, CategoryId = 5, Name = "乐虎功能饮料 250ml*24 罐", Description = "提神醒脑，补充能量", Price = 98m, OriginalPrice = 120m, Stock = 500, Sales = 2100, Image = "/static/images/prod15.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod15.png", "/static/images/prod13.png" }), Status = 1, CreateTime = DateTime.Now }
        });
        
        // 分类 6: 咖啡类
        products.AddRange(new[]
        {
            new Product { ShopId = 1, CategoryId = 6, Name = "雀巢速溶咖啡 100 条装", Description = "经典原味，醇香浓郁", Price = 89m, OriginalPrice = 108m, Stock = 400, Sales = 2800, Image = "/static/images/prod16.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod16.png", "/static/images/prod17.png" }), Status = 1, CreateTime = DateTime.Now },
            new Product { ShopId = 1, CategoryId = 6, Name = "星巴克星选拿铁 270ml*15 瓶", Description = "星巴克风味，即饮享受", Price = 128m, OriginalPrice = 150m, Stock = 300, Sales = 1900, Image = "/static/images/prod17.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod17.png", "/static/images/prod18.png" }), Status = 1, CreateTime = DateTime.Now },
            new Product { ShopId = 1, CategoryId = 6, Name = "Costa 即饮咖啡 300ml*12 瓶", Description = "英式咖啡，醇正口感", Price = 98m, OriginalPrice = 120m, Stock = 350, Sales = 1600, Image = "/static/images/prod18.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod18.png", "/static/images/prod16.png" }), Status = 1, CreateTime = DateTime.Now }
        });
        
        // 分类 7: 零食
        products.AddRange(new[]
        {
            new Product { ShopId = 1, CategoryId = 7, Name = "三只松鼠坚果大礼包", Description = "混合装每日坚果，1500g", Price = 128m, OriginalPrice = 168m, Stock = 500, Sales = 4200, Image = "/static/images/prod19.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod19.png", "/static/images/prod20.png" }), Status = 1, CreateTime = DateTime.Now },
            new Product { ShopId = 1, CategoryId = 7, Name = "良品铺子零食大礼包", Description = "网红零食组合，1680g", Price = 108m, OriginalPrice = 148m, Stock = 600, Sales = 3800, Image = "/static/images/prod20.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod20.png", "/static/images/prod21.png" }), Status = 1, CreateTime = DateTime.Now },
            new Product { ShopId = 1, CategoryId = 7, Name = "百草味坚果零食组合", Description = "营养美味，1200g", Price = 98m, OriginalPrice = 138m, Stock = 450, Sales = 3200, Image = "/static/images/prod21.png", Images = System.Text.Json.JsonSerializer.Serialize(new List<string> { "/static/images/prod21.png", "/static/images/prod19.png" }), Status = 1, CreateTime = DateTime.Now }
        });

        context.Products.AddRange(products);

        context.SaveChanges();
    }
}
