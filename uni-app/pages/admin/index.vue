<template>
  <view class="admin-page">
    <!-- 顶部栏 -->
    <view class="topbar">
      <view class="brand">
        <text class="brand-logo">VS</text>
        <text class="brand-title">电商后台管理系统</text>
      </view>
      <view class="topbar-user">
        <text class="user-name">管理员</text>
        <text class="dot">●</text>
        <text class="topbar-link" @tap="goUserCenter">返回个人中心</text>
      </view>
    </view>

    <view class="layout">
      <!-- 左侧菜单 -->
      <view v-if="!isMobile" class="sidebar">
        <view
          v-for="item in menuList"
          :key="item.key"
          class="menu-item"
          :class="{ active: activeMenu === item.key }"
          @tap="onMenuClick(item)"
        >
          <text class="menu-icon">{{ item.icon }}</text>
          <text class="menu-text">{{ item.name }}</text>
        </view>
      </view>

      <!-- 右侧内容区 -->
      <view class="content">
        <!-- 移动端顶部菜单 -->
        <scroll-view v-if="isMobile" class="mobile-menu" scroll-x>
          <view class="mobile-menu-inner">
            <view
              v-for="item in menuList"
              :key="item.key"
              class="mobile-menu-item"
              :class="{ active: activeMenu === item.key }"
              @tap="onMenuClick(item)"
            >
              {{ item.name }}
            </view>
          </view>
        </scroll-view>

        <!-- 首页看板 -->
        <view v-if="activeMenu === 'home'" class="page-content">
          <view class="stats-grid">
            <view v-for="card in statsCards" :key="card.key" class="stat-card" :class="card.key">
              <view class="stat-icon">{{ card.icon }}</view>
              <view class="stat-info">
                <text class="stat-value">{{ card.value }}</text>
                <text class="stat-title">{{ card.title }}</text>
              </view>
            </view>
          </view>

          <view class="module-grid">
            <view class="card module">
              <view class="module-head">
                <text class="module-title">待处理</text>
              </view>
              <view class="todo-row clickable" v-for="todo in pendingList" :key="todo.key" @tap="onPendingClick(todo)">
                <text class="todo-name">{{ todo.name }}</text>
                <view class="todo-tail">
                  <text class="todo-count">{{ todo.count }}</text>
                  <text class="todo-arrow">›</text>
                </view>
              </view>
            </view>

            <view class="card module">
              <view class="module-head">
                <text class="module-title">热销商品</text>
                <text class="module-more">月</text>
              </view>
              <view class="hot-head">
                <text class="col name">商品名称</text>
                <text class="col cate">分类</text>
                <text class="col sales">销量</text>
              </view>
              <view class="hot-row" v-for="item in hotProducts" :key="item.key">
                <text class="col name">{{ item.name }}</text>
                <text class="col cate">{{ item.category }}</text>
                <text class="col sales">{{ item.sales }}</text>
              </view>
            </view>

            <view class="card module full-width">
              <view class="module-head">
                <text class="module-title">常用菜单</text>
              </view>
              <view class="quick-grid">
                <view class="quick-item" v-for="item in quickMenus" :key="item.key" @tap="onMenuClick(item)">
                  <text class="quick-icon">{{ item.icon }}</text>
                  <text class="quick-text">{{ item.name }}</text>
                </view>
              </view>
            </view>

            <view class="card module full-width">
              <view class="module-head">
                <text class="module-title">近7日销售趋势</text>
              </view>
              <view class="trend-row" v-for="item in salesTrend" :key="item.day">
                <text class="trend-day">{{ item.day }}</text>
                <view class="trend-bar-wrap">
                  <view class="trend-bar" :style="{ width: barWidth(item.amount) }"></view>
                </view>
                <text class="trend-amount">{{ item.amount }}</text>
              </view>
            </view>
          </view>
        </view>

      </view>
    </view>
  </view>
</template>

<script>
import { getUserInfo, isAdminUser } from '@/utils/auth'
import request from '@/utils/http'

export default {
  data() {
    return {
      isMobile: false,
      activeMenu: 'home',
      menuList: [
        { key: 'home', name: '首页', icon: '⌂' },
        { key: 'shop', name: '商家管理', icon: '▦' },
        { key: 'merchant', name: '商品分类', icon: '✦' },
        { key: 'products', name: '商品管理', icon: '◫' },
        { key: 'orders', name: '订单管理', icon: '☰' },
        { key: 'users', name: '用户管理', icon: '◉' }
      ],
      statsCards: [
        { key: 's1', title: '今日销售额', value: '¥ 0.00', icon: '¥' },
        { key: 's2', title: '今日订单数', value: '0', icon: '▤' },
        { key: 's3', title: '总销售商品数', value: '0', icon: '▣' },
        { key: 's4', title: '总会员数', value: '0', icon: '◉' }
      ],
      pendingList: [
        { key: 'p1', name: '待付款', count: 0 },
        { key: 'p2', name: '待发货', count: 0 },
        { key: 'p3', name: '待收货', count: 0 },
        { key: 'p4', name: '待解决', count: 0 },
        { key: 'p5', name: '退换货', count: 0 }
      ],
      hotProducts: [],
      quickMenus: [
        { key: 'products', name: '商品管理', icon: '⊕' },
        { key: 'orders', name: '订单管理', icon: '✓' },
        { key: 'shop', name: '商家管理', icon: '▦' },
        { key: 'merchant', name: '商品分类', icon: '✦' },
        { key: 'users', name: '用户管理', icon: '◌' }
      ],
      salesTrend: []
    }
  },
  computed: {
    trendMax() {
      const values = this.salesTrend.map(i => i.amount)
      const max = Math.max.apply(null, values)
      return max > 0 ? max : 1
    }
  },
  onLoad() {
    this.detectDevice()
  },
  onShow() {
    const user = getUserInfo()
    if (!user || !isAdminUser(user)) {
      uni.showToast({ title: '仅管理员可访问', icon: 'none' })
      setTimeout(() => {
        uni.switchTab({ url: '/pages/user/user' })
      }, 300)
      return
    }
    this.detectDevice()
    this.loadDashboard()
  },
  methods: {
    async loadDashboard() {
      try {
        const res = await request({ url: '/api/admin/dashboard' })
        const data = res?.data || {}

        const stats = data.stats || {}
        this.statsCards = [
          { key: 's1', title: '今日销售额', value: `¥ ${Number(stats.todaySales || 0).toFixed(2)}`, icon: '¥' },
          { key: 's2', title: '今日订单数', value: String(stats.todayOrderCount || 0), icon: '▤' },
          { key: 's3', title: '总销售商品数', value: String(stats.totalSold || 0), icon: '▣' },
          { key: 's4', title: '总会员数', value: String(stats.totalUsers || 0), icon: '◉' }
        ]

        const pending = data.pending || {}
        this.pendingList = [
          { key: 'p1', name: '待付款', count: pending.unpaid || 0 },
          { key: 'p2', name: '待发货', count: pending.toShip || 0 },
          { key: 'p3', name: '待收货', count: pending.toReceive || 0 },
          { key: 'p4', name: '待解决', count: pending.resolving || 0 },
          { key: 'p5', name: '退换货', count: pending.refund || 0 }
        ]

        const hot = Array.isArray(data.hotProducts) ? data.hotProducts : []
        this.hotProducts = hot.map((item, idx) => ({
          key: idx + 1,
          name: item.name,
          category: item.category,
          sales: item.sales
        }))

        const trend = Array.isArray(data.trend) ? data.trend : []
        if (trend.length) {
          this.salesTrend = trend.map(item => ({
            day: item.day,
            amount: Number(item.amount || 0)
          }))
        }
      } catch (error) {
        console.warn('[后台看板] 读取接口失败', error)
      }
    },
    detectDevice() {
      const info = uni.getSystemInfoSync()
      this.isMobile = (info.windowWidth || 0) < 768
    },
    barWidth(amount) {
      const percent = Math.round((amount / this.trendMax) * 100)
      return `${percent}%`
    },
    goUserCenter() {
      uni.switchTab({ url: '/pages/user/user' })
    },
    onPendingClick(todo) {
      uni.navigateTo({ url: '/pages/admin/orders' })
    },
    onMenuClick(item) {
      if (item.key === 'home') {
        this.activeMenu = 'home'
        return
      }

      const routeMap = {
        shop: '/pages/admin/shop',
        merchant: '/pages/admin/categories',
        products: '/pages/admin/products',
        orders: '/pages/admin/orders',
        users: '/pages/admin/users'
      }

      const url = routeMap[item.key]
      if (url) {
        uni.navigateTo({ url })
      }
    }
  }
}
</script>

<style lang="scss" scoped>
.admin-page { min-height: 100vh; background: #eceef2; color: #1f2329; }

.topbar { 
  height: 88rpx; 
  background: #3b3d43; 
  color: #fff; 
  padding: 0 28rpx; 
  display: flex; 
  align-items: center; 
  justify-content: space-between; 
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 100;
}

.brand { display: flex; align-items: center; }
.brand-logo { width: 42rpx; height: 42rpx; border-radius: 50%; background: #2ea7e0; text-align: center; line-height: 42rpx; font-size: 22rpx; margin-right: 12rpx; }
.brand-title { font-size: 26rpx; font-weight: 600; }
.topbar-user { display: flex; align-items: center; font-size: 22rpx; color: #dbe0ea; }
.dot { margin-left: 10rpx; color: #ff5b5b; }
.topbar-link { margin-left: 18rpx; padding: 8rpx 16rpx; border-radius: 999rpx; background: rgba(255,255,255,.12); color: #fff; cursor: pointer; }

.layout { 
  display: flex; 
  min-height: 100vh; 
  padding-top: 88rpx;
}

.sidebar { 
  width: 220rpx; 
  background: #fff; 
  border-right: 1rpx solid #e7e8ea; 
  position: fixed;
  top: 88rpx;
  left: 0;
  bottom: 0;
  overflow-y: auto;
}

.menu-item { 
  height: 78rpx; 
  display: flex; 
  align-items: center; 
  padding: 0 18rpx; 
  color: #666; 
  font-size: 24rpx; 
  cursor: pointer;
  transition: all 0.2s;
}

.menu-item:hover { background: #f5f7fa; }
.menu-item.active { background: #2ea7e0; color: #fff; }
.menu-icon { width: 34rpx; margin-right: 10rpx; }

.content { 
  flex: 1; 
  margin-left: 220rpx;
  min-height: calc(100vh - 88rpx);
}

.mobile-menu { margin-bottom: 16rpx; white-space: nowrap; }
.mobile-menu-inner { display: inline-flex; }
.mobile-menu-item { padding: 10rpx 20rpx; margin-right: 10rpx; border-radius: 999rpx; background: #fff; border: 1rpx solid #dfe3ea; color: #666; font-size: 22rpx; cursor: pointer; }
.mobile-menu-item.active { background: #2ea7e0; color: #fff; border-color: #2ea7e0; }

.page-content { 
  padding: 22rpx; 
  min-height: calc(100vh - 88rpx);
  background: #eceef2;
}

.stats-grid { display: flex; flex-wrap: wrap; gap: 14rpx; }
.stat-card { flex: 1; min-width: 260rpx; border-radius: 14rpx; padding: 18rpx 20rpx; color: #fff; display: flex; align-items: center; }
.stat-card.s1 { background: linear-gradient(120deg, #6a5af9, #4f88ff); }
.stat-card.s2 { background: linear-gradient(120deg, #ff7aa5, #ff8f6a); }
.stat-card.s3 { background: linear-gradient(120deg, #f2a65d, #f58658); }
.stat-card.s4 { background: linear-gradient(120deg, #5ac8e1, #4d9cff); }
.stat-icon { width: 56rpx; height: 56rpx; border-radius: 12rpx; background: rgba(255,255,255,.22); text-align: center; line-height: 56rpx; font-size: 30rpx; margin-right: 14rpx; }
.stat-value { display: block; font-size: 34rpx; font-weight: 700; }
.stat-title { display: block; margin-top: 6rpx; font-size: 22rpx; opacity: .95; }

.module-grid { display: flex; flex-wrap: wrap; gap: 14rpx; margin-top: 14rpx; }
.card { background: #fff; border-radius: 14rpx; padding: 16rpx; box-shadow: 0 4rpx 16rpx rgba(0,0,0,.04); }
.module { width: calc(50% - 7rpx); }
.full-width { width: 100%; }
.module-head { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10rpx; }
.module-title { font-size: 26rpx; font-weight: 600; color: #333; }
.module-more { font-size: 22rpx; color: #98a2b3; }

.todo-row { height: 62rpx; display: flex; align-items: center; justify-content: space-between; border-bottom: 1rpx solid #f0f2f5; }
.todo-row:last-child { border-bottom: 0; }
.todo-row.clickable { cursor: pointer; }
.todo-name { color: #555; font-size: 24rpx; }
.todo-tail { display: flex; align-items: center; }
.todo-count { color: #111; font-weight: 600; font-size: 24rpx; }
.todo-arrow { margin-left: 10rpx; color: #b4bdcb; font-size: 28rpx; }

.hot-head, .hot-row { display: flex; align-items: center; border-bottom: 1rpx solid #f0f2f5; min-height: 56rpx; }
.hot-head { color: #8a94a6; font-size: 22rpx; }
.hot-row { color: #475467; font-size: 23rpx; }
.col.name { flex: 1; padding-right: 10rpx; }
.col.cate { width: 90rpx; text-align: center; }
.col.sales { width: 90rpx; text-align: right; }

.quick-grid { display: flex; flex-wrap: wrap; gap: 12rpx; }
.quick-item { width: calc(20% - 10rpx); min-width: 120rpx; height: 94rpx; border: 1rpx solid #e8ecf3; border-radius: 10rpx; display: flex; align-items: center; justify-content: center; flex-direction: column; cursor: pointer; transition: all 0.2s; }
.quick-item:hover { border-color: #2ea7e0; background: rgba(46, 167, 224, 0.05); }
.quick-icon { font-size: 28rpx; }
.quick-text { margin-top: 6rpx; font-size: 22rpx; color: #556070; }

.trend-row { display: flex; align-items: center; margin-bottom: 10rpx; }
.trend-day { width: 92rpx; color: #8a94a6; font-size: 22rpx; }
.trend-bar-wrap { flex: 1; height: 18rpx; background: #edf1f7; border-radius: 999rpx; overflow: hidden; }
.trend-bar { height: 18rpx; background: linear-gradient(90deg, #4f46e5, #6d28d9); border-radius: 999rpx; }
.trend-amount { width: 110rpx; text-align: right; color: #4b5565; font-size: 22rpx; }

@media (max-width: 768px) {
  .content { margin-left: 0; }
  .page-content { padding: 16rpx; }
  .topbar-user { display: flex; }
  .user-name, .dot { display: none; }
  .topbar-link { margin-left: 0; font-size: 20rpx; }
  .stats-grid { gap: 10rpx; }
  .stat-card { min-width: calc(50% - 6rpx); padding: 14rpx; }
  .module { width: 100%; }
  .quick-item { width: calc(33.333% - 8rpx); min-width: 0; }
}
</style>