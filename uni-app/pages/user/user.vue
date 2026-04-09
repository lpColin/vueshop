<template>
  <view class="user-page">
    <view class="fixed-hero">
      <view class="top-panel">
        <view class="top-title-row">
          <view class="top-title">个人中心</view>
          <view class="capsule">
            <text class="capsule-dot">•••</text>
            <text class="capsule-sep">|</text>
            <text class="capsule-dot">◦</text>
          </view>
        </view>

        <view class="user-card" @tap="handleLogin">
          <image class="avatar" :src="userInfo.avatar || '/static/images/avatar.png'" mode="aspectFill" />
          <view class="user-meta">
            <text class="nick">{{ userInfo.nickname || '微信用户' }}</text>
            <text class="member">{{ hasLogin ? '鲜果多会员' : '点击登录后查看更多权益' }}</text>
          </view>
          <view class="user-card-tag">{{ hasLogin ? '已登录' : '未登录' }}</view>
        </view>

        <view class="user-stats">
          <view class="user-stat-item">
            <text class="user-stat-value">{{ hasLogin ? '12' : '--' }}</text>
            <text class="user-stat-label">收藏</text>
          </view>
          <view class="user-stat-item accent">
            <text class="user-stat-value">{{ hasLogin ? '3' : '--' }}</text>
            <text class="user-stat-label">优惠券</text>
          </view>
          <view class="user-stat-item">
            <text class="user-stat-value">{{ hasLogin ? '8' : '--' }}</text>
            <text class="user-stat-label">积分</text>
          </view>
        </view>
      </view>

      <view class="section order-section">
        <view class="section-title">我的订单</view>
        <view class="order-grid">
          <view class="order-item" @tap="goOrders('pending')">
            <view class="order-icon box"></view>
            <text class="order-text">待发货</text>
          </view>
          <view class="order-item" @tap="goOrders('shipping')">
            <view class="order-icon truck"></view>
            <text class="order-text">待收货</text>
          </view>
          <view class="order-item" @tap="goOrders('done')">
            <view class="order-icon star"></view>
            <text class="order-text">已完成</text>
          </view>
          <view class="order-item" @tap="goOrders('all')">
            <view class="order-icon list"></view>
            <text class="order-text">全部订单</text>
          </view>
        </view>
      </view>

    </view>

    <scroll-view class="page-body with-fixed-hero menu-scroll" :style="pageBodyStyle" :scroll-y="isMenuScrollable" show-scrollbar="false">
      <view class="menu-scroll-inner">
        <view class="menu-transition">
          <view class="menu-transition-line"></view>
        </view>

        <view class="section menu-section">
          <view class="menu-row" v-if="showAdminEntry" @tap="goAdmin">
            <view class="left"><view class="menu-icon setting"></view><text class="label">后台管理</text></view>
            <text class="arrow">›</text>
          </view>
          <view class="menu-row" @tap="goAddress">
            <view class="left"><view class="menu-icon pin"></view><text class="label">收货地址</text></view>
            <text class="arrow">›</text>
          </view>
          <view class="menu-row" @tap="goCoupon">
            <view class="left"><view class="menu-icon coupon"></view><text class="label">优惠券</text></view>
            <text class="arrow">›</text>
          </view>
          <view class="menu-row" @tap="goFavorites">
            <view class="left"><view class="menu-icon heart"></view><text class="label">我的收藏</text></view>
            <text class="arrow">›</text>
          </view>
          <view class="menu-row" @tap="contactService">
            <view class="left"><view class="menu-icon headset"></view><text class="label">联系客服</text></view>
            <text class="arrow">›</text>
          </view>
          <view class="menu-row" @tap="goSettings">
            <view class="left"><view class="menu-icon setting"></view><text class="label">设置</text></view>
            <text class="arrow">›</text>
          </view>
          <view class="menu-row" v-if="hasLogin" @tap="handleLogout">
            <view class="left"><view class="menu-icon heart"></view><text class="label">退出登录</text></view>
            <text class="arrow">›</text>
          </view>
        </view>
      </view>
    </scroll-view>
  </view>
</template>

<script>
import request from '@/utils/http'
import { getToken, getUserInfo, setUserInfo, logout, normalizeUserInfo, isAdminUser } from '@/utils/auth'

export default {
  data() {
    return {
      userInfo: {
        id: '',
        nickname: '',
        avatar: '',
        role: 'user'
      },
      hasLogin: false,
      isMenuScrollable: false,
      pageBodyTop: 250
    }
  },
  computed: {
    showAdminEntry() {
      return this.hasLogin && isAdminUser(this.userInfo)
    },
    pageBodyStyle() {
      return {
        top: `${this.pageBodyTop}px`
      }
    }
  },
  onShow() {
    if (typeof this.getTabBar === 'function' && this.getTabBar()) {
      this.getTabBar().setSelectedByPath('/pages/user/user')
    }
    this.loadUserInfo()
    this.fetchProfile()
    this.scheduleScrollMeasure()
  },
  methods: {
    scheduleScrollMeasure() {
      setTimeout(() => {
        this.updateLayoutMetrics()
      }, 80)
    },
    updateLayoutMetrics() {
      const query = uni.createSelectorQuery().in(this)
      query.select('.order-section').boundingClientRect()
      query.select('.menu-scroll').boundingClientRect()
      query.select('.menu-scroll-inner').boundingClientRect()
      query.exec((res) => {
        const orderRect = res && res[0]
        const wrapRect = res && res[1]
        const contentRect = res && res[2]

        if (orderRect && typeof orderRect.bottom === 'number') {
          this.pageBodyTop = Math.max(orderRect.bottom + 4, 220)
        }

        this.$nextTick(() => {
          const nextQuery = uni.createSelectorQuery().in(this)
          nextQuery.select('.menu-scroll').boundingClientRect()
          nextQuery.select('.menu-scroll-inner').boundingClientRect()
          nextQuery.exec((nextRes) => {
            const nextWrapRect = nextRes && nextRes[0]
            const nextContentRect = nextRes && nextRes[1]
            const finalWrapRect = nextWrapRect || wrapRect
            const finalContentRect = nextContentRect || contentRect
            if (!finalWrapRect || !finalContentRect) return
            this.isMenuScrollable = finalContentRect.height > finalWrapRect.height + 2
          })
        })
      })
    },
    loadUserInfo() {
      const data = getUserInfo()
      if (data) {
        this.userInfo = data
        this.hasLogin = true
      } else {
        this.hasLogin = false
        this.userInfo = {
          id: '',
          nickname: '',
          avatar: '',
          role: 'user'
        }
      }
    },
    async fetchProfile() {
      if (!getToken()) return
      try {
        const res = await request({ url: '/api/auth/me' })
        if (res?.data) {
          this.userInfo = normalizeUserInfo(res.data)
          this.hasLogin = true
          setUserInfo(this.userInfo)
          this.scheduleScrollMeasure()
        }
      } catch (error) {
        // 由请求层处理异常
      }
    },
    handleLogin() {
      if (!this.hasLogin) {
        uni.navigateTo({ url: '/pages/login/login' })
        return
      }
      uni.showToast({ title: '已登录', icon: 'none' })
    },
    goOrders(status) {
      if (!this.hasLogin) {
        uni.navigateTo({ url: '/pages/login/login' })
        return
      }
      uni.navigateTo({ url: `/pages/orderList/orderList?status=${status || 'all'}` })
    },
    goAddress() {
      if (!this.hasLogin) {
        uni.navigateTo({ url: '/pages/login/login' })
        return
      }
      uni.navigateTo({ url: '/pages/address/address' })
    },
    goCoupon() {
      uni.showToast({ title: '功能开发中', icon: 'none' })
    },
    goFavorites() {
      uni.showToast({ title: '功能开发中', icon: 'none' })
    },
    contactService() {
      uni.makePhoneCall({ phoneNumber: '13888888888' })
    },
    goSettings() {
      uni.showToast({ title: '功能开发中', icon: 'none' })
    },
    goAdmin() {
      uni.navigateTo({ url: '/pages/admin/index' })
    },
    handleLogout() {
      logout()
      this.loadUserInfo()
      uni.showToast({ title: '已退出登录', icon: 'none' })
    }
  }
}
</script>

<style lang="scss" scoped>
.user-page {
  height: 100vh;
  background: linear-gradient(180deg, #fff3f1 0, #efefef 240rpx);
  overflow: hidden;
}

.fixed-hero {
  position: fixed;
  top: 20rpx;
  left: 20rpx;
  right: 20rpx;
  z-index: 20;
  background: linear-gradient(180deg, #fff3f1 0, #fff3f1 86%, rgba(255, 243, 241, 0));
  padding-bottom: 12rpx;
}

.page-body.with-fixed-hero {
  position: fixed;
  left: 0;
  right: 0;
  top: 500rpx;
  bottom: 140rpx;
  padding: 0 20rpx;
  box-sizing: border-box;
}

.menu-scroll {
  width: 100%;
  height: 100%;
}

.menu-scroll-inner {
  padding-bottom: 24rpx;
}

.menu-transition {
  display: flex;
  justify-content: center;
  padding: 2rpx 0 4rpx;
}

.menu-transition-line {
  width: 96rpx;
  height: 10rpx;
  border-radius: 999rpx;
  background: linear-gradient(90deg, rgba(230, 67, 64, 0.04), rgba(230, 67, 64, 0.16), rgba(230, 67, 64, 0.04));
  box-shadow: 0 8rpx 20rpx rgba(230, 67, 64, 0.08);
}

.top-panel {
  background: linear-gradient(135deg, #ff7d6e, #e64340);
  padding: 20rpx 20rpx 24rpx;
  border-radius: 28rpx;
  box-shadow: 0 18rpx 34rpx rgba(230, 67, 64, 0.16);
  position: relative;
}

.top-title-row {
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 18rpx;
  position: relative;
}

.top-title {
  color: #fff;
  font-size: 30rpx;
  font-weight: 700;
}

.capsule {
  position: absolute;
  right: 0;
  height: 44rpx;
  border-radius: 24rpx;
  padding: 0 12rpx;
  background: rgba(255, 255, 255, 0.16);
  border: 1rpx solid rgba(255, 255, 255, 0.35);
  display: flex;
  align-items: center;
  color: #fff;
}

.capsule-dot { font-size: 18rpx; line-height: 1; }
.capsule-sep { margin: 0 8rpx; opacity: 0.7; font-size: 20rpx; }

.user-card {
  display: flex;
  align-items: center;
  background: rgba(255, 255, 255, 0.12);
  border: 1rpx solid rgba(255, 255, 255, 0.16);
  border-radius: 22rpx;
  padding: 20rpx;
}

.avatar {
  width: 96rpx;
  height: 96rpx;
  border-radius: 50%;
  border: 4rpx solid rgba(255, 255, 255, 0.28);
  margin-right: 16rpx;
}

.user-meta { display: flex; flex-direction: column; flex: 1; min-width: 0; }
.nick { color: #fff; font-size: 33rpx; font-weight: 700; }
.member { margin-top: 8rpx; color: rgba(255, 255, 255, 0.85); font-size: 21rpx; }

.user-card-tag {
  padding: 8rpx 16rpx;
  border-radius: 999rpx;
  font-size: 22rpx;
  color: #fff;
  background: rgba(255, 255, 255, 0.16);
}

.user-stats {
  display: flex;
  gap: 14rpx;
  margin-top: 18rpx;
}

.user-stat-item {
  flex: 1;
  background: rgba(255, 255, 255, 0.88);
  border-radius: 18rpx;
  padding: 16rpx 18rpx;
}

.user-stat-item.accent {
  background: linear-gradient(135deg, #fff1ef, #ffe5e0);
}

.user-stat-value {
  display: block;
  font-size: 34rpx;
  font-weight: 700;
  color: #2f2f2f;
}

.user-stat-label {
  display: block;
  margin-top: 6rpx;
  color: #999;
  font-size: 22rpx;
}

.section {
  margin: 18rpx 20rpx 0;
  background: #fff;
  border-radius: 22rpx;
  overflow: hidden;
  box-shadow: 0 10rpx 24rpx rgba(0, 0, 0, 0.04);
}

.order-section {
  margin: 18rpx 0 0;
  border-radius: 22rpx;
  box-shadow: 0 10rpx 24rpx rgba(0, 0, 0, 0.04);
  border: 1rpx solid rgba(230, 67, 64, 0.05);
}

.menu-section {
  margin: 8rpx 0 0;
}

.section-title {
  height: 72rpx;
  line-height: 72rpx;
  padding: 0 24rpx;
  color: #333;
  font-size: 27rpx;
  font-weight: 600;
  border-bottom: 1rpx solid #f2f2f2;
}

.order-grid {
  display: flex;
  padding: 22rpx 8rpx 16rpx;
}

.order-item {
  width: 25%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 10rpx 0;
}

.order-item:active {
  opacity: 0.85;
}

.order-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40rpx;
  height: 40rpx;
  border-radius: 10rpx;
  position: relative;
  box-shadow: 0 8rpx 16rpx rgba(0, 0, 0, 0.06);
}

.order-icon.box {
  background: #f3ba63;
  border: 2rpx solid #d0923f;
  box-sizing: border-box;
}
.order-icon.box::before {
  content: '';
  position: absolute;
  left: -2rpx;
  right: -2rpx;
  top: -8rpx;
  height: 12rpx;
  border-radius: 8rpx 8rpx 0 0;
  background: #f7cc8a;
  border: 2rpx solid #d0923f;
  border-bottom: none;
}
.order-icon.box::after {
  content: '';
  position: absolute;
  left: 18rpx;
  top: 0;
  width: 4rpx;
  height: 40rpx;
  background: rgba(255, 255, 255, 0.85);
}

.order-icon.truck {
  width: 42rpx;
  height: 28rpx;
  margin-top: 6rpx;
  background: #ffd977;
  border-radius: 6rpx;
  border: 2rpx solid #d4a646;
  box-sizing: border-box;
}
.order-icon.truck::before {
  content: '';
  position: absolute;
  right: -10rpx;
  bottom: 2rpx;
  width: 12rpx;
  height: 16rpx;
  border-radius: 4rpx;
  background: #ffca56;
  border: 2rpx solid #d4a646;
  box-sizing: border-box;
}
.order-icon.truck::after {
  content: '';
  position: absolute;
  left: 6rpx;
  bottom: -9rpx;
  width: 10rpx;
  height: 10rpx;
  border-radius: 50%;
  background: #cd8f35;
  box-shadow: 22rpx 0 0 #cd8f35;
}

.order-icon.star {
  background: #ffcd4f;
  border-radius: 50%;
}
.order-icon.star::before {
  content: '';
  position: absolute;
  left: 11rpx;
  top: 18rpx;
  width: 18rpx;
  height: 8rpx;
  border: 4rpx solid #fff;
  border-top: 0;
  border-right: 0;
  transform: rotate(-45deg);
}

.order-icon.list {
  background: #f2c16a;
  border-radius: 6rpx;
  border: 2rpx solid #d89f46;
  box-sizing: border-box;
}
.order-icon.list::before {
  content: '';
  position: absolute;
  left: 7rpx;
  top: 8rpx;
  width: 4rpx;
  height: 4rpx;
  border-radius: 50%;
  background: #fff;
  box-shadow: 0 9rpx 0 #fff, 0 18rpx 0 #fff;
}
.order-icon.list::after {
  content: '';
  position: absolute;
  left: 14rpx;
  right: 6rpx;
  top: 8rpx;
  height: 2rpx;
  background: #fff;
  box-shadow: 0 9rpx 0 #fff, 0 18rpx 0 #fff;
}

.order-text { margin-top: 12rpx; color: #666; font-size: 22rpx; }

.menu-row {
  height: 92rpx;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24rpx;
  border-bottom: 1rpx solid #f3f3f3;
  background: linear-gradient(180deg, #fff, #fffdfd);
}

.menu-row:last-child { border-bottom: none; }

.left { display: flex; align-items: center; }
.menu-icon {
  width: 32rpx;
  height: 32rpx;
  border-radius: 10rpx;
  margin-right: 14rpx;
  position: relative;
}

.menu-icon.pin { background: #ff5aa9; border-radius: 50%; }
.menu-icon.pin::before {
  content: '';
  position: absolute;
  left: 12rpx;
  top: 22rpx;
  width: 0;
  height: 0;
  border-left: 4rpx solid transparent;
  border-right: 4rpx solid transparent;
  border-top: 8rpx solid #ff5aa9;
}
.menu-icon.pin::after {
  content: '';
  position: absolute;
  left: 10rpx;
  top: 9rpx;
  width: 12rpx;
  height: 12rpx;
  border-radius: 50%;
  background: #fff;
  box-shadow: inset 0 0 0 3rpx #ff8dc4;
}

.menu-icon.coupon {
  background: #f5ca56;
  border-radius: 8rpx;
  box-shadow: inset 0 -2rpx 0 rgba(255, 255, 255, 0.45);
  background-image: linear-gradient(to right, rgba(255, 255, 255, 0.9) 0, rgba(255, 255, 255, 0.9) 10rpx, transparent 10rpx, transparent 12rpx, rgba(255, 255, 255, 0.9) 12rpx, rgba(255, 255, 255, 0.9) 24rpx);
  background-repeat: no-repeat;
  background-size: 24rpx 2rpx;
  background-position: center;
}
.menu-icon.coupon::before,
.menu-icon.coupon::after {
  content: '';
  position: absolute;
  top: 14rpx;
  width: 6rpx;
  height: 6rpx;
  border-radius: 50%;
  background: #fff;
}
.menu-icon.coupon::before { left: -3rpx; }
.menu-icon.coupon::after { right: -3rpx; }

.menu-icon.heart {
  background: #ff7f4f;
  border-radius: 8rpx;
}
.menu-icon.heart::before {
  content: '★';
  position: absolute;
  left: 5rpx;
  top: 2rpx;
  color: #fff;
  font-size: 24rpx;
  line-height: 1;
}

.menu-icon.headset {
  background: #b9b4c7;
  border-radius: 12rpx;
}
.menu-icon.headset::before {
  content: '';
  position: absolute;
  left: 7rpx;
  top: 6rpx;
  width: 18rpx;
  height: 12rpx;
  border: 3rpx solid #fff;
  border-bottom: 0;
  border-radius: 10rpx 10rpx 0 0;
}
.menu-icon.headset::after {
  content: '';
  position: absolute;
  left: 8rpx;
  right: 8rpx;
  bottom: 6rpx;
  height: 2rpx;
  background: #fff;
}

.menu-icon.setting {
  background: #a9a2b8;
  border-radius: 50%;
}
.menu-icon.setting::before {
  content: '';
  position: absolute;
  left: 13rpx;
  top: 2rpx;
  width: 6rpx;
  height: 28rpx;
  background: #fff;
  box-shadow: -11rpx 11rpx 0 -2rpx #fff, 11rpx 11rpx 0 -2rpx #fff, -11rpx -11rpx 0 -2rpx #fff, 11rpx -11rpx 0 -2rpx #fff;
  transform: rotate(45deg);
  opacity: 0.75;
}
.menu-icon.setting::after {
  content: '';
  position: absolute;
  left: 8rpx;
  top: 8rpx;
  width: 16rpx;
  height: 16rpx;
  border: 3rpx solid #fff;
  border-radius: 50%;
}

.label { color: #333; font-size: 27rpx; font-weight: 500; }
.arrow { color: #c9c9c9; font-size: 32rpx; }
</style>
