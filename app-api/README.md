# VueShop API

基于 .NET 10 + SQLite 的商城后端 API 服务。

## 技术栈

- **.NET 10** - 最新版本的.NET 框架
- **Entity Framework Core 10** - ORM 框架
- **SQLite** - 轻量级关系型数据库
- **JWT** - 用户认证
- **ASP.NET Core Web API** - RESTful API 服务

## 项目结构

```
app-api/
├── Controllers/          # API 控制器
│   ├── AuthController.cs      # 认证相关接口
│   ├── ProductsController.cs  # 商品相关接口
│   ├── CartController.cs      # 购物车相关接口
│   ├── OrderController.cs     # 订单相关接口
│   ├── AddressController.cs   # 地址相关接口
│   └── AdminController.cs     # 管理后台接口
├── Models/             # 数据模型
│   ├── User.cs         # 用户模型
│   ├── Shop.cs         # 商家模型
│   ├── Category.cs     # 分类模型
│   ├── Product.cs      # 商品模型
│   ├── Order.cs        # 订单模型
│   ├── OrderItem.cs    # 订单项模型
│   ├── CartItem.cs     # 购物车项模型
│   └── Address.cs      # 地址模型
├── Data/               # 数据访问层
│   ├── AppDbContext.cs      # 数据库上下文
│   └── SeedData.cs          # 初始数据
├── Services/           # 业务服务层
│   └── AuthService.cs       # 认证服务
├── Program.cs          # 程序入口
├── appsettings.json    # 配置文件
└── app-api.csproj      # 项目文件
```

## 快速开始

### 环境要求

- .NET 10 SDK 或更高版本
- SQLite（已包含在 EF Core 中）

### Swagger 接口文档

启动项目后，访问以下地址查看 Swagger 接口文档：

- **Swagger UI**: http://localhost:5162/swagger
- **Swagger JSON**: http://localhost:5162/swagger/v1/swagger.json

在 Swagger UI 中，您可以：
- 查看所有 API 接口列表
- 测试 API 接口（需要 JWT 认证的接口请先登录获取 token）
- 查看请求/响应模型

### 运行项目

```bash
# 进入项目目录
cd app-api

# 还原依赖
dotnet restore

# 运行项目
dotnet run
```

项目默认运行在 `http://localhost:5162`

### 数据库

数据库文件 `app.db` 会在首次运行时自动创建在 `app-api` 目录下。

如需重置数据库，删除 `app.db` 文件后重新启动项目即可。

## 默认账号

系统初始化时会创建以下测试账号：

| 用户名 | 密码 | 角色 | 说明 |
|--------|------|------|------|
| admin | 123456 | admin | 系统管理员 |
| shop1 | 123456 | merchant | 商家用户 |
| user1 | 123456 | user | 普通用户 |

## API 接口文档

### 认证接口 `/api/auth`

#### 用户登录
```
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "123456"
}
```

响应：
```json
{
  "success": true,
  "message": "登录成功",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": 1,
      "username": "admin",
      "nickname": "系统管理员",
      "role": "admin"
    }
  }
}
```

#### 用户注册
```
POST /api/auth/register
Content-Type: application/json

{
  "username": "newuser",
  "password": "password123",
  "nickname": "新用户"
}
```

### 商品接口 `/api/products`

#### 获取商品列表
```
GET /api/products?page=1&pageSize=10&categoryId=1&keyword=手机
```

#### 获取商品详情
```
GET /api/products/{id}
```

### 购物车接口 `/api/cart`

需要 JWT 认证。

#### 获取购物车
```
GET /api/cart
Authorization: Bearer {token}
```

#### 添加商品到购物车
```
POST /api/cart
Authorization: Bearer {token}
Content-Type: application/json

{
  "productId": 1,
  "quantity": 2
}
```

#### 更新购物车商品数量
```
PUT /api/cart/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "quantity": 3
}
```

#### 从购物车删除商品
```
DELETE /api/cart/{id}
Authorization: Bearer {token}
```

#### 清空购物车
```
DELETE /api/cart
Authorization: Bearer {token}
```

### 订单接口 `/api/order`

需要 JWT 认证。

#### 创建订单
```
POST /api/order
Authorization: Bearer {token}
Content-Type: application/json

{
  "shopId": 1,
  "items": [
    { "productId": 1, "quantity": 2 }
  ],
  "receiverName": "张三",
  "receiverPhone": "13800138000",
  "receiverAddress": "北京市朝阳区 xxx",
  "deliveryMethod": "express",
  "remark": "请尽快发货"
}
```

#### 获取订单列表
```
GET /api/order?status=1&page=1&pageSize=10
Authorization: Bearer {token}
```

#### 获取订单详情
```
GET /api/order/{id}
Authorization: Bearer {token}
```

#### 取消订单
```
POST /api/order/{id}/cancel
Authorization: Bearer {token}
```

### 地址接口 `/api/address`

需要 JWT 认证。

#### 获取地址列表
```
GET /api/address
Authorization: Bearer {token}
```

#### 添加地址
```
POST /api/address
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "张三",
  "phone": "13800138000",
  "province": "北京市",
  "city": "北京市",
  "district": "朝阳区",
  "detail": "xxx 街道 xxx 号",
  "isDefault": true
}
```

#### 更新地址
```
PUT /api/address/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "张三",
  "phone": "13800138000",
  "province": "北京市",
  "city": "北京市",
  "district": "朝阳区",
  "detail": "xxx 街道 xxx 号",
  "isDefault": true
}
```

#### 删除地址
```
DELETE /api/address/{id}
Authorization: Bearer {token}
```

#### 设置默认地址
```
POST /api/address/{id}/default
Authorization: Bearer {token}
```

### 管理员接口 `/api/admin`

需要 JWT 认证，且当前用户 `role=admin`。

#### 用户管理
```
GET /api/admin/users?page=1&pageSize=20
PUT /api/admin/users/{id}/status
PUT /api/admin/users/{id}/role
```

#### 分类管理
```
GET /api/admin/categories?page=1&pageSize=20
POST /api/admin/categories
PUT /api/admin/categories/{id}
DELETE /api/admin/categories/{id}
```

#### 商品管理
```
GET /api/admin/products?page=1&pageSize=20
PUT /api/admin/products/{id}/status
```

## 数据模型

### User (用户)
| 字段 | 类型 | 说明 |
|------|------|------|
| Id | int | 主键 |
| Username | string | 用户名 |
| Password | string | 密码（加密） |
| OpenId | string | 微信 OpenId |
| UnionId | string | 微信 UnionId |
| Nickname | string | 昵称 |
| Avatar | string | 头像 |
| Phone | string | 手机号 |
| Role | string | 角色 (admin/merchant/user) |
| ShopId | int? | 关联商家 ID |
| Status | int | 状态 (0:禁用 1:正常) |

### Product (商品)
| 字段 | 类型 | 说明 |
|------|------|------|
| Id | int | 主键 |
| ShopId | int | 商家 ID |
| CategoryId | int | 分类 ID |
| Name | string | 商品名称 |
| Description | string | 商品描述 |
| Price | decimal | 售价 |
| OriginalPrice | decimal | 原价 |
| Stock | int | 库存 |
| Sales | int | 销量 |
| Image | string | 主图 |
| Images | string | 图片列表 (JSON) |
| Status | int | 状态 |

### Order (订单)
| 字段 | 类型 | 说明 |
|------|------|------|
| Id | int | 主键 |
| OrderNo | string | 订单号 |
| UserId | int | 用户 ID |
| ShopId | int | 商家 ID |
| TotalAmount | decimal | 商品总金额 |
| DeliveryFee | decimal | 运费 |
| DiscountAmount | decimal | 优惠金额 |
| PayAmount | decimal | 实付金额 |
| Status | int | 订单状态 |
| ReceiverName | string | 收货人姓名 |
| ReceiverPhone | string | 收货人电话 |
| ReceiverAddress | string | 收货人地址 |
| DeliveryMethod | string | 配送方式 |
| Remark | string | 备注 |

## 订单状态

| 状态码 | 说明 |
|--------|------|
| 0 | 待付款 |
| 1 | 待发货 |
| 2 | 待收货 |
| 3 | 已完成 |
| 4 | 已取消 |

## 配置说明

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  },
  "JwtSettings": {
    "SecretKey": "VueShopSecretKey2026YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "app-api",
    "Audience": "app-client",
    "ExpirationMinutes": 10080
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## 开发说明

### 添加新的 API 接口

1. 在 `Controllers/` 目录下创建新的 Controller 类
2. 继承 `ControllerBase`
3. 使用 `[ApiController]` 和 `[Route("api/[controller]")]` 特性
4. 添加 Action 方法处理请求

### 添加新的数据模型

1. 在 `Models/` 目录下创建新的 Model 类
2. 在 `AppDbContext.cs` 中添加 `DbSet<T>` 属性
3. 运行项目时 EF Core 会自动创建对应的表

## 常见问题

### 数据库文件位置

数据库文件 `app.db` 位于 `app-api` 项目目录下。

### 如何重置数据库

删除 `app.db` 文件，然后重新启动项目。

### JWT Token 过期

默认 Token 有效期为 10080 分钟（7 天），可在 `appsettings.json` 中修改。

## License

MIT