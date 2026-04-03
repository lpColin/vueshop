<template>
  <view class="cart-page">
    <view class="header-shell fixed-hero">
      <view class="header">
        <view class="back-btn" @tap="goBack">
          <text class="back-icon"><</text>
        </view>
        <view class="header-center">
          <text class="header-title">购物车</text>
          <text class="header-subtitle">已选 {{ selectedCount }} 件商品</text>
        </view>
        <view class="header-badge">
          <text class="header-badge-text">{{ cartItems.length }}</text>
        </view>
      </view>
    </view>

    <view v-if="cartItems.length === 0" class="empty-wrap">
      <view class="empty-illustration">
        <text class="empty-icon">🛒</text>
        <view class="empty-bubble bubble-one"></view>
        <view class="empty-bubble bubble-two"></view>
      </view>
      <text class="empty-title">购物车还是空的</text>
      <text class="empty-desc">把喜欢的商品先加入购物车，结算会更方便</text>
      <button class="go-btn" @tap="goIndex">去逛逛</button>
    </view>

    <view v-else class="content with-fixed-hero">
      <view v-for="item in cartItems" :key="item.id" class="item">
        <view class="item-checkbox" @tap="toggleSelect(item.id)">
          <view class="checkbox" :class="{ checked: item.selected }">
            <text v-if="item.selected" class="checkmark">√</text>
          </view>
        </view>
        <image :src="item.image" mode="aspectFill" class="thumb" />
        <view class="meta">
          <text class="name">{{ item.name }}</text>
          <text class="desc">默认规格</text>
          <view class="price-row">
            <text class="price">¥{{ item.price }}</text>
            <text class="original-price" v-if="item.originalPrice">¥{{ item.originalPrice }}</text>
          </view>
        </view>
        <view class="quantity-wrapper">
          <view class="ops">
            <view class="op decrease" @tap="decrease(item.id)">-</view>
            <text class="qty">{{ item.quantity }}</text>
            <view class="op increase" @tap="increase(item.id)">+</view>
          </view>
        </view>
        <text class="del" @tap="remove(item.id)">删除</text>
      </view>
    </view>

    <view class="footer">
      <view class="footer-left" @tap="toggleSelectAll">
        <view class="checkbox" :class="{ checked: isAllSelected }">
          <text v-if="isAllSelected" class="checkmark">√</text>
        </view>
        <text class="select-text">全选</text>
      </view>
      <view class="footer-right">
        <view class="total-wrapper">
          <text class="total-label">合计：</text>
          <text class="total-symbol">¥</text>
          <text class="total">{{ totalPrice }}</text>
        </view>
        <view class="discount-info" v-if="discountAmount > 0">
          <text>共减 ¥{{ discountAmount }}</text>
        </view>
        <button class="checkout" @tap="checkout">去结算({{ selectedCount }})</button>
      </view>
    </view>
  </view>
</template>

<script>
import request from '@/utils/http'
import { getToken } from '@/utils/auth'

export default {
  data() {
    return {
      cartItems: [],
      discountAmount: 0
    }
  },
  computed: {
    isAllSelected() {
      if (this.cartItems.length === 0) return false
      return this.cartItems.every((item) => item.selected)
    },
    selectedCount() {
      return this.cartItems
        .filter((item) => item.selected)
        .reduce((sum, item) => sum + Number(item.quantity || 0), 0)
    },
    totalPrice() {
      return this.cartItems
        .filter((item) => item.selected)
        .reduce((sum, item) => sum + Number(item.price) * Number(item.quantity), 0)
        .toFixed(2)
    },
    selectedItems() {
      return this.cartItems.filter((item) => item.selected)
    }
  },
  onShow() {
    if (typeof this.getTabBar === 'function' && this.getTabBar()) {
      this.getTabBar().setSelectedByPath('/pages/cart/cart')
    }
    this.loadCart()
    this.updateCartBadge()
  },
  methods: {
    getFullImageUrl(path) {
      if (!path) return '/static/images/product1.png'
      if (path.startsWith('http://') || path.startsWith('https://')) return path
      if (path.startsWith('/')) {
        return 'http://localhost:5162' + path
      }
      return 'http://localhost:5162/' + path
    },
    async loadCart() {
      if (getToken()) {
        try {
          const res = await request({ url: '/api/cart' })
          const list = Array.isArray(res?.data?.list) ? res.data.list : []
          this.cartItems = list.map((item) => ({
            id: item.productId,
            cartItemId: item.id,
            name: item.productName,
            price: Number(item.productPrice || 0),
            image: this.getFullImageUrl(item.productImage),
            quantity: Number(item.quantity || 0),
            selected: item.selected !== false,
            stock: Number(item.stock || 0)
          }))
          return
        } catch (error) {
          console.warn('[购物车] 获取后端购物车失败，回退本地', error)
        }
      }

      const localItems = uni.getStorageSync('cartItems') || []
      this.cartItems = localItems.map((item) => ({
        ...item,
        selected: item.selected !== false
      }))
    },
    saveCart() {
      uni.setStorageSync('cartItems', this.cartItems)
    },
    goBack() {
      uni.switchTab({ url: '/pages/index/index' })
    },
    toggleSelectAll() {
      const newStatus = !this.isAllSelected
      this.cartItems.forEach((item) => {
        item.selected = newStatus
      })
      this.saveCart()
    },
    toggleSelect(id) {
      const item = this.cartItems.find((x) => x.id === id)
      if (item) {
        item.selected = !item.selected
        this.saveCart()
      }
    },
    async increase(id) {
      const item = this.cartItems.find((x) => x.id === id)
      if (!item) return
      if (item.stock && item.quantity >= item.stock) {
        uni.showToast({ title: '已达库存上限', icon: 'none' })
        return
      }

      if (getToken() && item.cartItemId) {
        try {
          await request({
            url: `/api/cart/${item.cartItemId}`,
            method: 'PUT',
            data: { quantity: item.quantity + 1 }
          })
          await this.loadCart()
          this.updateCartBadge()
          return
        } catch (error) {
          uni.showToast({ title: error.message || '操作失败', icon: 'none' })
          return
        }
      }

      item.quantity += 1
      this.saveCart()
      this.updateCartBadge()
    },
    async decrease(id) {
      const item = this.cartItems.find((x) => x.id === id)
      if (!item) return
      if (item.quantity <= 1) {
        await this.remove(id)
        return
      }

      if (getToken() && item.cartItemId) {
        try {
          await request({
            url: `/api/cart/${item.cartItemId}`,
            method: 'PUT',
            data: { quantity: item.quantity - 1 }
          })
          await this.loadCart()
          this.updateCartBadge()
          return
        } catch (error) {
          uni.showToast({ title: error.message || '操作失败', icon: 'none' })
          return
        }
      }

      item.quantity -= 1
      this.saveCart()
      this.updateCartBadge()
    },
    async remove(id) {
      const item = this.cartItems.find((x) => x.id === id)
      if (getToken() && item?.cartItemId) {
        try {
          await request({
            url: `/api/cart/${item.cartItemId}`,
            method: 'DELETE'
          })
          await this.loadCart()
          this.updateCartBadge()
          return
        } catch (error) {
          uni.showToast({ title: error.message || '删除失败', icon: 'none' })
          return
        }
      }

      this.cartItems = this.cartItems.filter((x) => x.id !== id)
      this.saveCart()
      this.updateCartBadge()
    },
    checkout() {
      if (!this.cartItems.length) {
        uni.showToast({ title: '购物车为空', icon: 'none' })
        return
      }

      const selectedItems = this.selectedItems
      if (selectedItems.length === 0) {
        uni.showToast({ title: '请选择要结算的商品', icon: 'none' })
        return
      }

      const checkoutItems = selectedItems.map((item) => ({
        id: item.id,
        cartItemId: item.cartItemId,
        name: item.name,
        price: item.price,
        image: item.image,
        quantity: item.quantity,
        spec: item.spec || '默认规格'
      }))

      uni.setStorageSync('checkoutItems', checkoutItems)

      if (getToken()) {
        uni.setStorageSync('checkoutCartItemIds', selectedItems.map((x) => x.cartItemId).filter(Boolean))
      }

      uni.navigateTo({ url: '/pages/order/order' })
    },
    goIndex() {
      uni.switchTab({ url: '/pages/index/index' })
    },
    updateCartBadge() {
      const count = this.cartItems.reduce((sum, item) => sum + Number(item.quantity || 0), 0)
      if (count > 0) {
        if (typeof this.getTabBar === 'function' && this.getTabBar()) {
          this.getTabBar().setCartBadge(String(count))
        }
        uni.setTabBarBadge({
          index: 1,
          text: String(count)
        })
      } else {
        if (typeof this.getTabBar === 'function' && this.getTabBar()) {
          this.getTabBar().setCartBadge('')
        }
        uni.removeTabBarBadge({ index: 1 })
      }
    }
  }
}
</script>

<style lang="scss" scoped>

.cart-page {
  min-height: 100vh;
  background: linear-gradient(180deg, #fff3f1 0, #f5f5f5 220rpx);
  padding-bottom: 220rpx;
}

.fixed-hero {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 20;
  background: linear-gradient(180deg, #fff3f1 0, #fff3f1 84%, rgba(255, 243, 241, 0));
}

.header-shell {
  padding: 24rpx 20rpx 16rpx;
}

.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  min-height: 112rpx;
  padding: 0 24rpx;
  background: linear-gradient(135deg, #ff7d6e, #e64340);
  border-radius: 24rpx;
  box-shadow: 0 16rpx 32rpx rgba(230, 67, 64, 0.18);
}

.back-btn {
  width: 64rpx;
  height: 64rpx;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.18);
}

.back-icon {
  font-size: 40rpx;
  color: #fff;
  font-weight: 400;
  line-height: 1;
}

.header-center {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 18rpx 20rpx;
}

.header-title {
  font-size: 34rpx;
  font-weight: 700;
  color: #fff;
}

.header-subtitle {
  margin-top: 6rpx;
  font-size: 22rpx;
  color: rgba(255, 255, 255, 0.82);
}

.header-badge {
  min-width: 64rpx;
  height: 64rpx;
  padding: 0 16rpx;
  border-radius: 999rpx;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.16);
  border: 1rpx solid rgba(255, 255, 255, 0.22);
}

.header-badge-text {
  font-size: 26rpx;
  color: #fff;
  font-weight: 700;
}

.empty-wrap {
  padding: 140rpx 40rpx;
  text-align: center;
}

.empty-illustration {
  position: relative;
  width: 220rpx;
  height: 220rpx;
  margin: 0 auto 28rpx;
  border-radius: 50%;
  background: radial-gradient(circle at 30% 30%, #fff1ef, #ffe3de);
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 18rpx 40rpx rgba(230, 67, 64, 0.08);
}

.empty-icon {
  font-size: 94rpx;
}

.empty-bubble {
  position: absolute;
  border-radius: 50%;
  background: rgba(255, 122, 107, 0.18);
}

.bubble-one {
  width: 28rpx;
  height: 28rpx;
  top: 30rpx;
  right: 20rpx;
}

.bubble-two {
  width: 18rpx;
  height: 18rpx;
  left: 24rpx;
  bottom: 34rpx;
}

.empty-title {
  color: #444;
  font-size: 34rpx;
  font-weight: 700;
}

.empty-desc {
  display: block;
  margin-top: 14rpx;
  color: #999;
  font-size: 24rpx;
  line-height: 1.6;
}

.go-btn {
  margin-top: 32rpx;
  background: linear-gradient(135deg, #ff7463, #e64340);
  color: #fff;
  border-radius: 999rpx;
  width: 280rpx;
  box-shadow: 0 12rpx 24rpx rgba(230, 67, 64, 0.18);
}

.content {
  padding: 8rpx 20rpx 140rpx;
  padding-bottom: 140rpx;
}

.content.with-fixed-hero {
  padding-top: 164rpx;
  padding-bottom: 170rpx;
}

.item {
  display: flex;
  align-items: center;
  background: linear-gradient(180deg, #ffffff, #fff7f6);
  padding: 22rpx 24rpx;
  margin-bottom: 20rpx;
  border-radius: 22rpx;
  box-shadow: 0 10rpx 26rpx rgba(230, 67, 64, 0.06);
  border: 1rpx solid rgba(230, 67, 64, 0.06);
}

.item-checkbox {
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 16rpx;
}

.thumb {
  width: 160rpx;
  height: 160rpx;
  border-radius: 18rpx;
  margin-right: 20rpx;
  flex-shrink: 0;
  background: #f8f8f8;
  box-shadow: 0 8rpx 18rpx rgba(0, 0, 0, 0.06);
}

.meta {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  min-width: 0;
}

.name {
  font-size: 28rpx;
  color: #2b2b2b;
  font-weight: 600;
  line-height: 1.4;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  margin-bottom: 8rpx;
}

.desc {
  font-size: 22rpx;
  color: #ff7a6b;
  padding: 6rpx 14rpx;
  background: #fff1ef;
  border-radius: 999rpx;
  align-self: flex-start;
  margin-bottom: 8rpx;
}

.price-row {
  display: flex;
  align-items: baseline;
}

.price {
  font-size: 38rpx;
  color: #ff4d4f;
  font-weight: bold;
}

.original-price {
  font-size: 24rpx;
  color: #999;
  text-decoration: line-through;
  margin-left: 12rpx;
}

.quantity-wrapper {
  display: flex;
  align-items: center;
  margin-right: 16rpx;
}

.ops {
  display: flex;
  align-items: center;
  border: 1rpx solid #f0d6d2;
  border-radius: 999rpx;
  overflow: hidden;
  background: #fff;
}

.op {
  width: 56rpx;
  height: 56rpx;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 32rpx;
  color: #e64340;
  background: #fff8f7;
}

.qty {
  min-width: 60rpx;
  text-align: center;
  font-size: 26rpx;
  color: #333;
  font-weight: 600;
}

.del {
  color: #b0a6a4;
  font-size: 24rpx;
  padding: 8rpx 0 8rpx 12rpx;
}

.footer {
  position: fixed;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.96);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 22rpx 24rpx calc(22rpx + env(safe-area-inset-bottom));
  box-shadow: 0 -10rpx 30rpx rgba(0, 0, 0, 0.07);
  backdrop-filter: blur(14rpx);
}

.footer-left {
  display: flex;
  align-items: center;
}

.checkbox {
  width: 40rpx;
  height: 40rpx;
  border-radius: 50%;
  border: 2rpx solid #ddd;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #fff;
}

.checkbox.checked {
  background: #e64340;
  border-color: #e64340;
}

.checkmark {
  color: #fff;
  font-size: 24rpx;
}

.select-text {
  margin-left: 12rpx;
  font-size: 26rpx;
  color: #333;
}

.footer-right {
  display: flex;
  align-items: center;
}

.total-wrapper {
  margin-right: 20rpx;
  display: flex;
  align-items: baseline;
}

.total-label,
.total-symbol,
.total {
  color: #ff4d4f;
}

.total {
  font-size: 34rpx;
  font-weight: bold;
}

.discount-info {
  font-size: 22rpx;
  color: #ff7a6b;
  margin-right: 16rpx;
}

.checkout {
  background: linear-gradient(135deg, #ff6b6b, #e64340);
  color: #fff;
  border-radius: 999rpx;
  padding: 0 42rpx;
  font-size: 26rpx;
  font-weight: 700;
  box-shadow: 0 10rpx 20rpx rgba(230, 67, 64, 0.22);
}
</style>
