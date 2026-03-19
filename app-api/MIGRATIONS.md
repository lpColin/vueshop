# EF Core Migrations 使用指南

## 📚 什么是 Migrations

EF Core Migrations 允许你逐步修改数据库架构，而不会丢失数据。每次模型更改都会创建一个新的迁移文件。

## 🚀 快速开始

### 1. 修改数据库前先备份

```powershell
# 执行备份脚本
.\backup-db.ps1

# 或手动备份
Copy-Item app.db app.db.backup
```

### 2. 添加新的迁移

当你修改了模型（如 Category.cs）后：

```bash
cd E:\code\vueShop\app-api
dotnet ef migrations add <迁移名称>
```

例如：
```bash
dotnet ef migrations add AddDescriptionToCategory
```

### 3. 应用迁移到数据库

```bash
dotnet ef database update
```

或在应用启动时自动应用（已在 Program.cs 中配置）

## 📝 常用命令

### 查看所有迁移
```bash
dotnet ef migrations list
```

### 查看待处理的迁移
```bash
dotnet ef migrations list --pending
```

### 撤销最后一个迁移
```bash
dotnet ef migrations remove
```

### 回滚到指定迁移
```bash
dotnet ef database update <迁移名称>
```

### 生成 SQL 脚本
```bash
dotnet ef migrations script --output migration.sql
```

## 🔄 完整工作流程示例

### 场景：给 Category 添加 Description 字段

1. **备份数据库**
   ```powershell
   .\backup-db.ps1
   ```

2. **修改模型** - 在 `Models/Category.cs` 中添加：
   ```csharp
   public string? Description { get; set; }
   ```

3. **创建迁移**
   ```bash
   dotnet ef migrations add AddDescriptionToCategory
   ```

4. **查看生成的迁移文件** - 确认更改正确

5. **应用迁移**
   ```bash
   dotnet ef database update
   ```

6. **验证** - 检查数据库是否包含新字段

## ⚠️ 注意事项

### ✅ 推荐做法
- 每次只做一个小的更改
- 提交迁移文件到 Git
- 在生产环境应用前先在测试环境验证
- 定期备份数据库

### ❌ 避免的做法
- 不要手动修改已提交的迁移文件
- 不要删除数据库文件（会丢失数据）
- 不要在迁移中使用硬编码的路径
- 不要忽略迁移冲突

## 🛠️ 故障排除

### 问题：迁移失败
```bash
# 1. 撤销最后一个迁移
dotnet ef migrations remove

# 2. 修复模型代码

# 3. 重新创建迁移
dotnet ef migrations add <新名称>
```

### 问题：数据库与模型不同步
```bash
# 强制更新数据库
dotnet ef database update
```

### 问题：迁移冲突
```bash
# 1. 删除冲突的迁移文件
# 2. 重新创建迁移
dotnet ef migrations add <新名称>
```

## 📂 项目结构

```
app-api/
├── Migrations/              # 迁移文件目录
│   ├── 20260307074240_InitialCreate.cs
│   ├── 20260307074240_InitialCreate.Designer.cs
│   └── AppDbContextModelSnapshot.cs
├── Models/                  # 数据模型
├── Data/
│   └── AppDbContext.cs     # 数据库上下文
├── backup-db.ps1           # 备份脚本
└── app.db                  # SQLite 数据库文件
```

## 🔗 相关资源

- [EF Core Migrations 官方文档](https://docs.microsoft.com/ef/core/managing-schemas/migrations/)
- [EF Core Tools](https://docs.microsoft.com/ef/core/cli/dotnet)

---

**记住：先备份，再迁移！** 💾
