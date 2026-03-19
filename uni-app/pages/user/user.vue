<template>
  <view class="user-page">
    <view class="top-panel">
      <view class="top-title">我的</view>
      <view class="capsule">
        <text class="capsule-dot">•••</text>
        <text class="capsule-sep">|</text>
        <text class="capsule-dot">◦</text>
      </view>

      <view class="user-card" @tap="handleLogin">
        <image class="avatar" :src="userInfo.avatar || '/static/images/avatar.png'" mode="aspectFill" />
        <view class="user-meta">
          <text class="nick">{{ userInfo.nickname || '微信用户' }}</text>
          <text class="member">普通会员</text>
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

    <view class="section menu-section">
      <view class="menu-row" v-if="hasLogin && userInfo.role === 'admin'" @tap="goAdmin">
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
</template>

<script>
import request from '@/utils/http'
import { getToken, getUserInfo, setUserInfo, logout } from '@/utils/auth'

export default {
  data() {
    return {
      userInfo: {
        id: '',
        nickname: '',
        avatar: '',
        role: 'user'
      },
      hasLogin: false
    }
  },
  onShow() {
    this.loadUserInfo()
    this.fetchProfile()
  },
  methods: {
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
          this.userInfo = res.data
          this.hasLogin = true
          setUserInfo(res.data)
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
  min-height: 100vh;
  background: #efefef;
}

.top-panel {
  background: #f4514b;
  padding: 18rpx 20rpx 20rpx;
  position: relative;
}

.top-title {
  text-align: center;
  color: #fff;
  font-size: 28rpx;
  font-weight: 600;
  margin-bottom: 16rpx;
}

.capsule {
  position: absolute;
  right: 20rpx;
  top: 14rpx;
  height: 44rpx;
  border-radius: 24rpx;
  padding: 0 12rpx;
  background: rgba(0, 0, 0, 0.22);
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
}

.avatar {
  width: 88rpx;
  height: 88rpx;
  border-radius: 50%;
  border: 3rpx solid rgba(255, 255, 255, 0.3);
  margin-right: 14rpx;
}

.user-meta { display: flex; flex-direction: column; }
.nick { color: #fff; font-size: 33rpx; font-weight: 700; }
.member { margin-top: 6rpx; color: rgba(255, 255, 255, 0.85); font-size: 21rpx; }

.section {
  margin: 14rpx 14rpx 0;
  background: #fff;
  border-radius: 12rpx;
  overflow: hidden;
}

.section-title {
  height: 72rpx;
  line-height: 72rpx;
  padding: 0 20rpx;
  color: #333;
  font-size: 27rpx;
  font-weight: 600;
  border-bottom: 1rpx solid #f2f2f2;
}

.order-grid {
  display: flex;
  padding: 18rpx 0 12rpx;
}

.order-item {
  width: 25%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.order-icon {
  width: 40rpx;
  height: 40rpx;
  border-radius: 10rpx;
  position: relative;
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
  padding: 0 20rpx;
  border-bottom: 1rpx solid #f3f3f3;
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

.label { color: #333; font-size: 27rpx; }
.arrow { color: #c9c9c9; font-size: 32rpx; }
</style>
