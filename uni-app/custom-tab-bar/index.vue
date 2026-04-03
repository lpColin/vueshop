<template>
  <view class="tabbar-shell">
    <view class="tabbar">
      <view
        v-for="(item, index) in list"
        :key="item.pagePath"
        class="tabbar-item"
        :class="{ active: selected === index }"
        @tap="switchTab(index)"
      >
        <view class="icon-wrap" :class="{ active: selected === index }">
          <image class="tabbar-icon" :src="selected === index ? item.selectedIconPath : item.iconPath" mode="aspectFit" />
          <view v-if="item.badge" class="badge">{{ item.badge }}</view>
        </view>
        <text class="tabbar-text">{{ item.text }}</text>
      </view>
    </view>
  </view>
</template>

<script>
export default {
  data() {
    return {
      selected: 0,
      list: [
        {
          pagePath: '/pages/index/index',
          iconPath: '/static/tabbar/home.png',
          selectedIconPath: '/static/tabbar/home-active.png',
          text: '首页'
        },
        {
          pagePath: '/pages/cart/cart',
          iconPath: '/static/tabbar/cart.png',
          selectedIconPath: '/static/tabbar/cart-active.png',
          text: '购物车',
          badge: ''
        },
        {
          pagePath: '/pages/user/user',
          iconPath: '/static/tabbar/user.png',
          selectedIconPath: '/static/tabbar/user-active.png',
          text: '我的'
        }
      ]
    }
  },
  methods: {
    switchTab(index) {
      const item = this.list[index]
      if (!item) return
      this.selected = index
      uni.switchTab({ url: item.pagePath })
    },
    setSelectedByPath(path) {
      const index = this.list.findIndex((item) => item.pagePath === path)
      if (index > -1) this.selected = index
    },
    setCartBadge(text) {
      this.list[1].badge = text || ''
    }
  }
}
</script>

<style scoped>
.tabbar-shell {
  position: fixed;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 999;
  padding: 0 18rpx calc(env(safe-area-inset-bottom) + 12rpx);
  background: transparent;
}

.tabbar {
  height: 104rpx;
  border-radius: 28rpx;
  background: rgba(255, 255, 255, 0.94);
  backdrop-filter: blur(20rpx);
  box-shadow: 0 -4rpx 24rpx rgba(0, 0, 0, 0.04), 0 12rpx 30rpx rgba(230, 67, 64, 0.08);
  display: flex;
  align-items: center;
  justify-content: space-around;
}

.tabbar-item {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.icon-wrap {
  position: relative;
  width: 54rpx;
  height: 54rpx;
  border-radius: 18rpx;
  display: flex;
  align-items: center;
  justify-content: center;
}

.icon-wrap.active {
  background: linear-gradient(135deg, #fff1ef, #ffe3de);
}

.tabbar-icon {
  width: 42rpx;
  height: 42rpx;
}

.tabbar-text {
  margin-top: 6rpx;
  font-size: 20rpx;
  color: #8d8d8d;
}

.tabbar-item.active .tabbar-text {
  color: #e64340;
  font-weight: 600;
}

.badge {
  position: absolute;
  top: -6rpx;
  right: -14rpx;
  min-width: 28rpx;
  height: 28rpx;
  padding: 0 8rpx;
  border-radius: 999rpx;
  background: #ff4d4f;
  color: #fff;
  font-size: 18rpx;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
