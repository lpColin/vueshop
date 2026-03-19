<template>
  <view class="cart-page">
    <!-- 顶部导航栏 -->
    <view class="header">
      <view class="back-btn" @tap="goBack">
        <text class="back-icon">‹</text>
      </view>
      <text class="header-title">购物车</text>
      <view class="header-right"></view>
    </view>

    <view v-if="cartItems.length === 0" class="empty-wrap">
      <text class="empty-title">购物车还是空的</text>
      <button class="go-btn" @tap="goIndex">去逛逛</button>
    </view>

    <view v-else class="content">
      <!-- 商品列表 -->
      <view v-for="item in cartItems" :key="item.id" class="item">
        <view class="item-checkbox" @tap="toggleSelect(item.id)">
          <view class="checkbox" :class="{ checked: item.selected }">
            <text v-if="item.selected" class="checkmark">✓</text>
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

    <!-- 底部结算栏 -->
    <view class="footer">
      <view class="footer-left" @tap="toggleSelectAll">
        <view class="checkbox" :class="{ checked: isAllSelected }">
          <text v-if="isAllSelected" class="checkmark">✓</text>
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
          <text>共减¥{{ discountAmount }}</text>
        </view>
        <button class="checkout" @tap="checkout">去结算 ({{ selectedCount }})</button>
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
    // 是否全选
    isAllSelected() {
      if (this.cartItems.length === 0) return false
      return this.cartItems.every(item => item.selected)
    },
    // 已选商品数量
    selectedCount() {
      return this.cartItems
        .filter(item => item.selected)
        .reduce((sum, item) => sum + Number(item.quantity || 0), 0)
    },
    // 选中商品总金额
    totalPrice() {
      return this.cartItems
        .filter(item => item.selected)
        .reduce((sum, item) => sum + Number(item.price) * Number(item.quantity), 0)
        .toFixed(2)
    },
    // 原价总额（用于计算折扣）
    originalTotal() {
      return this.cartItems
        .filter(item => item.selected)
        .reduce((sum, item) => sum + (Number(item.originalPrice) || Number(item.price)) * Number(item.quantity), 0)
        .toFixed(2)
    },
    // 选中商品列表
    selectedItems() {
      return this.cartItems.filter(item => item.selected)
    }
  },
  onShow() {
    this.loadCart()
    this.updateCartBadge()
  },
  methods: {
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
            image: item.productImage || '/static/images/product1.png',
            quantity: Number(item.quantity || 0),
            selected: item.selected !== false // 默认选中
          }))
          return
        } catch (error) {
          console.warn('[购物车] 获取后端购物车失败，回退本地', error)
        }
      }
      const localItems = uni.getStorageSync('cartItems') || []
      // 本地存储的商品添加 selected 属性，默认选中
      this.cartItems = localItems.map(item => ({
        ...item,
        selected: item.selected !== false
      }))
    },
    saveCart() {
      uni.setStorageSync('cartItems', this.cartItems)
    },
    // 返回上一页
    goBack() {
      uni.switchTab({ url: '/pages/index/index' })
    },
    // 切换全选状态
    toggleSelectAll() {
      const newStatus = !this.isAllSelected
      this.cartItems.forEach(item => {
        item.selected = newStatus
      })
      this.saveCart()
    },
    // 切换单个商品选中状态
    toggleSelect(id) {
      const item = this.cartItems.find(x => x.id === id)
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
      
      // 只结算选中的商品
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
      
      // 保存选中的商品到缓存
      uni.setStorageSync('checkoutItems', checkoutItems)
      
      // 保存购物车项 ID（用于后端删除已购买的商品）
      if (getToken()) {
        uni.setStorageSync('checkoutCartItemIds', selectedItems.map(x => x.cartItemId).filter(Boolean))
      }
      
      // 跳转到订单确认页
      uni.navigateTo({ url: '/pages/order/order' })
    },
    goIndex() {
      uni.switchTab({ url: '/pages/index/index' })
    },
    updateCartBadge() {
      const count = this.cartItems.reduce((sum, item) => sum + Number(item.quantity || 0), 0)
      if (count > 0) {
        uni.setTabBarBadge({
          index: 1,
          text: String(count)
        })
      } else {
        uni.removeTabBarBadge({ index: 1 })
      }
    }
  }
}
</script>

<style lang="scss" scoped>
.cart-page {
  min-height: 100vh;
  background: #f5f5f5;
  padding-bottom: 220rpx;
}

// 顶部导航栏
.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 88rpx;
  padding: 0 24rpx;
  background: #fff;
  border-bottom: 1rpx solid #f0f0f0;
  
  .back-btn {
    width: 60rpx;
    height: 60rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    
    .back-icon {
      font-size: 56rpx;
      color: #333;
      font-weight: 300;
      line-height: 1;
    }
  }
  
  .header-title {
    font-size: 34rpx;
    font-weight: 600;
    color: #333;
  }
  
  .header-right {
    width: 60rpx;
  }
}

.empty-wrap { padding: 180rpx 40rpx; text-align: center; }
.empty-title { color: #888; font-size: 30rpx; }
.go-btn { margin-top: 24rpx; background: #e64340; color: #fff; }

.content {
  padding: 20rpx;
  padding-bottom: 140rpx;
}

// 商品卡片
.item {
  display: flex;
  align-items: center;
  background: #fff;
  padding: 20rpx 24rpx;
  margin-bottom: 20rpx;
  border-radius: 16rpx;
  
  .item-checkbox {
    display: flex;
    align-items: center;
    justify-content: center;
    margin-right: 16rpx;
    
    .checkbox {
      width: 40rpx;
      height: 40rpx;
    }
  }
  
  .thumb {
    width: 160rpx;
    height: 160rpx;
    border-radius: 12rpx;
    margin-right: 20rpx;
    flex-shrink: 0;
    background: #f8f8f8;
  }
  
  .meta {
    flex: 1;
    display: flex;
    flex-direction: column;
    justify-content: center;
    min-width: 0;
    
    .name {
      font-size: 28rpx;
      color: #333;
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
      color: #999;
      padding: 4rpx 12rpx;
      background: #f8f8f8;
      border-radius: 4rpx;
      align-self: flex-start;
      margin-bottom: 8rpx;
    }
    
    .price-row {
      display: flex;
      align-items: baseline;
      
      .price {
        font-size: 36rpx;
        color: #ff4d4f;
        font-weight: bold;
      }
      
      .original-price {
        font-size: 24rpx;
        color: #999;
        text-decoration: line-through;
        margin-left: 12rpx;
      }
    }
  }
  
  .quantity-wrapper {
    display: flex;
    align-items: center;
    margin-right: 16rpx;
    
    .ops {
      display: flex;
      align-items: center;
      border: 2rpx solid #f0f0f0;
      border-radius: 32rpx;
      overflow: hidden;
      
      .op {
        width: 48rpx;
        height: 48rpx;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 30rpx;
        color: #333;
        background: #fff;
        
        &.decrease {
          color: #999;
        }
        
        &.increase {
          color: #e64340;
          font-weight: bold;
        }
      }
      
      .qty {
        min-width: 56rpx;
        text-align: center;
        font-size: 28rpx;
        color: #333;
      }
    }
  }
  
  .del {
    font-size: 26rpx;
    color: #999;
    padding: 8rpx 12rpx;
  }
}

// 复选框
.checkbox {
  width: 36rpx;
  height: 36rpx;
  border-radius: 50%;
  border: 2rpx solid #ddd;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background: #fff;
  transition: all 0.2s;
  
  &.checked {
    background: #e64340;
    border-color: #e64340;
    
    .checkmark {
      display: block;
      color: #fff;
      font-size: 24rpx;
      font-weight: bold;
    }
  }
}

.checkmark {
  display: none;
  line-height: 1;
}

// 底部结算栏
.footer {
  position: fixed;
  left: 0;
  right: 0;
  bottom: 98rpx;
  height: 100rpx;
  background: #fff;
  border-top: 1rpx solid #f0f0f0;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24rpx;
  box-shadow: 0 -2rpx 8rpx rgba(0, 0, 0, 0.06);
  z-index: 999;
  
  .footer-left {
    display: flex;
    align-items: center;
    
    .checkbox {
      width: 36rpx;
      height: 36rpx;
      margin-right: 12rpx;
    }
    
    .select-text {
      font-size: 28rpx;
      color: #333;
    }
  }
  
  .footer-right {
    display: flex;
    align-items: center;
    
    .total-wrapper {
      display: flex;
      align-items: baseline;
      margin-right: 24rpx;
      
      .total-label {
        font-size: 28rpx;
        color: #333;
        margin-right: 8rpx;
      }
      
      .total-symbol {
        font-size: 28rpx;
        color: #ff4d4f;
        font-weight: bold;
      }
      
      .total {
        font-size: 40rpx;
        color: #ff4d4f;
        font-weight: bold;
      }
    }
    
    .checkout {
      margin: 0;
      padding: 0 52rpx;
      height: 72rpx;
      line-height: 72rpx;
      background: linear-gradient(135deg, #ff4d4f 0%, #e64340 100%);
      color: #fff;
      font-size: 30rpx;
      font-weight: 600;
      border-radius: 36rpx;
      border: none;
      box-shadow: 0 4rpx 12rpx rgba(230, 67, 64, 0.3);
      
      &:active {
        opacity: 0.9;
      }
    }
  }
}
</style>
