# 鲜果多商城 - uni-app 版本

这是一个使用 uni-app 框架开发的生鲜电商小程序，支持编译到微信小程序、H5 等多个平台。

## 项目结构

```
uni-app/
├── pages/              # 页面目录
│   ├── index/         # 首页
│   ├── cart/          # 购物车
│   ├── user/          # 个人中心
│   ├── product/       # 商品详情
│   ├── order/         # 订单确认
│   └── orderDetail/   # 订单详情
├── static/            # 静态资源
│   ├── images/        # 图片资源
│   └── tabbar/        # 底部导航图标
├── App.vue            # 应用入口
├── main.js            # 入口文件
├── pages.json         # 页面配置
├── manifest.json      # 应用配置
├── vite.config.js     # Vite 配置
└── index.html         # HTML 模板
```

## 功能特性

- 🏠 **首页**: 店铺信息展示、商品分类、商品列表、搜索功能
- 🛒 **购物车**: 商品选择、数量调整、全选/取消全选、结算
- 👤 **个人中心**: 用户信息、订单入口、功能菜单
- 📦 **商品详情**: 商品图片轮播、规格选择、加入购物车、立即购买
- 📝 **订单确认**: 收货地址、配送方式、优惠券、订单备注
- 📋 **订单详情**: 订单状态、商品信息、价格明细、订单操作

## 快速开始

### 1. 安装依赖

```bash
cd uni-app
npm install
```

### 2. 运行项目

#### H5 开发模式
```bash
cd uni-app
npm run dev:h5
```
访问 http://localhost:5173 查看应用

#### 微信小程序
```bash
npm run dev:mp-weixin
```
然后在微信开发者工具中打开构建输出目录（当前项目配置为 `uni-app/dist`）

### 3. 打包项目

#### H5 打包
```bash
npm run build:h5
```

#### 微信小程序
```bash
npm run build:mp-weixin
```
构建完成后在微信开发者工具中打开 `uni-app/dist` 目录。

## 技术栈

- **框架**: uni-app (Vue 3)
- **构建工具**: Vite 5
- **样式**: SCSS
- **平台**: 微信小程序、H5

## 开发说明

### 页面开发
每个页面包含三个部分：
- `<template>`: 页面结构
- `<script>`: 页面逻辑
- `<style lang="scss" scoped>`: 页面样式

### 数据持久化
使用 `uni.setStorageSync` 和 `uni.getStorageSync` 进行本地数据存储。

### 页面跳转
```javascript
// 普通跳转
uni.navigateTo({ url: '/pages/product/product?id=1' })

// Tab 切换
uni.switchTab({ url: '/pages/cart/cart' })

// 返回
uni.navigateBack()
```

## 注意事项

1. 图片资源路径使用 `/static/` 开头
2. 样式单位使用 rpx 适配不同屏幕
3. 使用 uni-app 提供的 API 进行系统操作

## 开发者

鲜果多商城团队

## 许可证

MIT License