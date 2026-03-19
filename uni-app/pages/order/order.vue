<template>
  <view class="order-page">
    <scroll-view scroll-y class="order-scroll">
      <!-- 收货地址 -->
      <view class="address-section" @tap="selectAddress">
        <view v-if="selectedAddress" class="address-content">
          <view class="address-header">
            <text class="receiver-name">{{ selectedAddress.name }}</text>
            <text class="receiver-phone">{{ selectedAddress.phone }}</text>
          </view>
          <view class="address-detail">
            <text class="detail-text">{{ selectedAddress.detail }}</text>
          </view>
          <view class="address-tag" v-if="selectedAddress.isDefault">默认</view>
        </view>
        <view v-else class="no-address">
          <text class="no-address-text">请选择收货地址</text>
          <text class="address-arrow">›</text>
        </view>
      </view>

      <!-- 商品信息 -->
      <view class="product-section">
        <view class="section-header">
          <text class="section-title">商品信息</text>
          <text class="section-count">共 {{ totalCount }} 件商品</text>
        </view>
        <view class="product-list">
          <view v-for="item in checkoutItems" :key="item.id" class="product-item">
            <image :src="item.image" mode="aspectFill" class="product-image" />
            <view class="product-info">
              <view class="product-name">{{ item.name }}</view>
              <view class="product-spec">{{ item.spec || '默认规格' }}</view>
              <view class="product-bottom">
                <text class="product-price">¥{{ item.price }}</text>
                <text class="product-quantity">x{{ item.quantity }}</text>
              </view>
            </view>
          </view>
        </view>
      </view>

      <!-- 配送方式 -->
      <view class="delivery-section" @tap="selectDelivery">
        <view class="delivery-left">
          <text class="delivery-label">配送方式</text>
          <text class="delivery-value">{{ deliveryMethod }}</text>
        </view>
        <text class="delivery-arrow">›</text>
      </view>

      <!-- 订单备注 -->
      <view class="remark-section">
        <view class="remark-label">订单备注</view>
        <input 
          class="remark-input" 
          placeholder="选填：对本订单的说明" 
          v-model="remark"
          placeholder-class="remark-placeholder"
        />
      </view>

      <!-- 发票信息 -->
      <view class="invoice-section" @tap="selectInvoice">
        <view class="invoice-left">
          <text class="invoice-label">发票</text>
          <text class="invoice-value">{{ invoiceType }}</text>
        </view>
        <text class="invoice-arrow">›</text>
      </view>

      <!-- 优惠券 -->
      <view class="coupon-section" @tap="selectCoupon">
        <view class="coupon-left">
          <text class="coupon-label">优惠券</text>
          <text class="coupon-value" :class="{ 'coupon-selected': selectedCoupon }">
            {{ selectedCoupon ? selectedCoupon.name : '不使用优惠' }}
          </text>
        </view>
        <view class="coupon-right">
          <text v-if="availableCoupons.length > 0" class="coupon-count">{{ availableCoupons.length }}张可用</text>
          <text class="coupon-arrow">›</text>
        </view>
      </view>
    </scroll-view>

    <!-- 底部结算栏 -->
    <view class="order-footer">
      <view class="footer-left">
        <text class="total-label">合计：</text>
        <view class="total-price-wrapper">
          <text class="total-symbol">¥</text>
          <text class="total-price">{{ totalPrice }}</text>
        </view>
        <view v-if="couponDiscount > 0" class="coupon-discount">
          <text class="discount-text">优惠 {{ couponDiscount }}元</text>
        </view>
      </view>
      <view class="submit-btn" @tap="submitOrder">
        <text class="submit-text">提交订单</text>
      </view>
    </view>

    <!-- 配送方式选择器 -->
    <view v-if="showDeliveryPicker" class="picker-mask" @tap="hideDeliveryPicker">
      <view class="picker-content" @tap.stop>
        <view class="picker-header">
          <text class="picker-title">选择配送方式</text>
          <view class="picker-close" @tap="hideDeliveryPicker">
            <text class="close-icon">✕</text>
          </view>
        </view>
        <view class="picker-options">
          <view 
            v-for="method in deliveryMethods" 
            :key="method.value"
            class="picker-option"
            :class="{ active: selectedDelivery === method.value }"
            @tap="confirmDelivery(method.value, method.label)"
          >
            <view class="option-info">
              <text class="option-name">{{ method.label }}</text>
              <text class="option-desc">{{ method.desc }}</text>
            </view>
            <view class="option-price">
              <text v-if="method.price === 0" class="free-text">免运费</text>
              <text v-else class="price-value">¥{{ method.price }}</text>
            </view>
            <view class="option-check" :class="{ checked: selectedDelivery === method.value }">
              <text v-if="selectedDelivery === method.value" class="check-icon">✓</text>
            </view>
          </view>
        </view>
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
      checkoutItems: [],
      selectedAddress: null,
      remark: '',
      invoiceType: '不开发票',
      selectedCoupon: null,
      availableCoupons: [
        { id: 1, name: '满 100 减 10', threshold: 100, discount: 10 },
        { id: 2, name: '满 200 减 30', threshold: 200, discount: 30 }
      ],
      deliveryMethod: '快递配送',
      deliveryFee: 0,
      showDeliveryPicker: false,
      deliveryMethods: [
        { value: 'express', label: '快递配送', desc: '预计 1-3 天送达', price: 0 },
        { value: 'instant', label: '即时配送', desc: '预计 1 小时送达', price: 10 }
      ],
      selectedDelivery: 'express'
    }
  },
  computed: {
    totalCount() {
      return this.checkoutItems.reduce((sum, item) => sum + item.quantity, 0)
    },
    originalPrice() {
      return this.checkoutItems.reduce((sum, item) => sum + item.price * item.quantity, 0)
    },
    couponDiscount() {
      if (!this.selectedCoupon) return 0
      if (this.originalPrice >= this.selectedCoupon.threshold) {
        return this.selectedCoupon.discount
      }
      return 0
    },
    totalPrice() {
      const total = this.originalPrice + this.deliveryFee - this.couponDiscount
      return Math.max(0, total).toFixed(2)
    }
  },
  onLoad() {
    this.loadCheckoutItems()
    this.loadDefaultAddress()
  },
  onShow() {
    const selected = uni.getStorageSync('selectedAddressForOrder')
    if (selected) {
      this.selectedAddress = selected
      uni.removeStorageSync('selectedAddressForOrder')
    }
  },
  methods: {
    loadCheckoutItems() {
      const items = uni.getStorageSync('checkoutItems') || []
      this.checkoutItems = items
    },
    async loadDefaultAddress() {
      if (getToken()) {
        try {
          const res = await request({ url: '/api/address' })
          const list = Array.isArray(res?.data) ? res.data : []
          const def = list.find((x) => x.isDefault) || list[0]
          if (def) {
            this.selectedAddress = {
              id: def.id,
              name: def.name,
              phone: def.phone,
              detail: `${def.province || ''}${def.city || ''}${def.district || ''}${def.detail || ''}`,
              isDefault: def.isDefault
            }
            return
          }
        } catch (error) {
          console.warn('[订单确认] 读取地址失败，回退本地', error)
        }
      }

      const address = uni.getStorageSync('defaultAddress')
      if (address) {
        this.selectedAddress = address
      } else {
        // 模拟默认地址
        this.selectedAddress = {
          name: '张三',
          phone: '138****8888',
          detail: '北京市朝阳区 XX 街道 XX 小区 XX 号楼 XX 室',
          isDefault: true
        }
      }
    },
    selectAddress() {
      uni.navigateTo({ url: '/pages/address/address?select=1' })
    },
    selectDelivery() {
      this.showDeliveryPicker = true
    },
    hideDeliveryPicker() {
      this.showDeliveryPicker = false
    },
    confirmDelivery(value, label) {
      this.selectedDelivery = value
      const method = this.deliveryMethods.find(m => m.value === value)
      this.deliveryMethod = method.label
      this.deliveryFee = method.price
      this.hideDeliveryPicker()
    },
    selectInvoice() {
      uni.showToast({
        title: '功能开发中',
        icon: 'none'
      })
    },
    selectCoupon() {
      uni.showToast({
        title: '功能开发中',
        icon: 'none'
      })
    },
    async submitOrder() {
      if (!this.selectedAddress) {
        uni.showToast({
          title: '请选择收货地址',
          icon: 'none'
        })
        return
      }

      if (getToken()) {
        const checkoutIds = uni.getStorageSync('checkoutCartItemIds') || this.checkoutItems.map((x) => x.cartItemId).filter(Boolean)
        if (!checkoutIds.length) {
          uni.showToast({ title: '请从购物车勾选商品后结算', icon: 'none' })
          return
        }

        try {
          const res = await request({
            url: '/api/order',
            method: 'POST',
            data: {
              itemIds: checkoutIds,
              receiverName: this.selectedAddress.name,
              receiverPhone: this.selectedAddress.phone,
              receiverAddress: this.selectedAddress.detail,
              deliveryMethod: this.deliveryMethod,
              remark: this.remark,
              deliveryFee: this.deliveryFee,
              discountAmount: this.couponDiscount
            }
          })

          uni.removeStorageSync('checkoutItems')
          uni.removeStorageSync('checkoutCartItemIds')
          uni.showToast({ title: '下单成功', icon: 'success' })
          const orderId = res?.data?.id
          setTimeout(() => {
            uni.redirectTo({ url: `/pages/orderDetail/orderDetail?id=${orderId}` })
          }, 200)
          return
        } catch (error) {
          uni.showToast({ title: error.message || '下单失败', icon: 'none' })
          return
        }
      }
      
      const orderData = {
        id: Date.now().toString(),
        items: this.checkoutItems,
        address: this.selectedAddress,
        deliveryMethod: this.deliveryMethod,
        deliveryFee: this.deliveryFee,
        remark: this.remark,
        invoiceType: this.invoiceType,
        coupon: this.selectedCoupon,
        couponDiscount: this.couponDiscount,
        totalPrice: this.totalPrice,
        status: 'pending',
        createTime: new Date().toLocaleString()
      }
      
      // 保存订单
      const orders = uni.getStorageSync('orders') || []
      orders.unshift(orderData)
      uni.setStorageSync('orders', orders)
      
      // 清空购物车中已购买的商品
      const cartItems = uni.getStorageSync('cartItems') || []
      const checkoutIds = this.checkoutItems.map(item => item.id)
      const remainingCart = cartItems.filter(item => !checkoutIds.includes(item.id))
      uni.setStorageSync('cartItems', remainingCart)
      
      // 跳转到订单详情页
      uni.redirectTo({
        url: `/pages/orderDetail/orderDetail?id=${orderData.id}`
      })
    }
  }
}
</script>

<style lang="scss" scoped>
.order-page {
  min-height: 100vh;
  background-color: #f5f5f5;
  padding-bottom: 120rpx;
}

.order-scroll {
  height: calc(100vh - 120rpx);
}

.address-section {
  background: linear-gradient(180deg, #e64340 0%, #ff6b6b 100%);
  padding: 32rpx 24rpx;
  margin-bottom: 20rpx;
  position: relative;
}

.address-content {
  color: #ffffff;
}

.address-header {
  margin-bottom: 16rpx;
}

.receiver-name {
  font-size: 32rpx;
  font-weight: bold;
  margin-right: 24rpx;
}

.receiver-phone {
  font-size: 28rpx;
}

.address-detail {
  margin-bottom: 16rpx;
}

.detail-text {
  font-size: 26rpx;
  line-height: 1.6;
  opacity: 0.9;
}

.address-tag {
  display: inline-block;
  padding: 4rpx 16rpx;
  background-color: rgba(255, 255, 255, 0.2);
  border-radius: 4rpx;
  font-size: 22rpx;
}

.no-address {
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: #ffffff;
}

.no-address-text {
  font-size: 28rpx;
}

.address-arrow {
  font-size: 40rpx;
  opacity: 0.8;
}

.product-section {
  background-color: #ffffff;
  margin-bottom: 20rpx;
}

.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 24rpx;
  border-bottom: 1rpx solid #f5f5f5;
}

.section-title {
  font-size: 28rpx;
  color: #666666;
}

.section-count {
  font-size: 24rpx;
  color: #999999;
}

.product-list {
  padding: 24rpx;
}

.product-item {
  display: flex;
  padding-bottom: 24rpx;
  margin-bottom: 24rpx;
  border-bottom: 1rpx solid #f5f5f5;
}

.product-item:last-child {
  padding-bottom: 0;
  margin-bottom: 0;
  border-bottom: none;
}

.product-image {
  width: 160rpx;
  height: 160rpx;
  border-radius: 12rpx;
  margin-right: 20rpx;
  flex-shrink: 0;
}

.product-info {
  flex: 1;
  min-width: 0;
}

.product-name {
  font-size: 28rpx;
  color: #333333;
  margin-bottom: 8rpx;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  line-height: 1.4;
}

.product-spec {
  font-size: 22rpx;
  color: #999999;
  margin-bottom: 16rpx;
}

.product-bottom {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.product-price {
  font-size: 32rpx;
  color: #ff4400;
  font-weight: bold;
}

.product-quantity {
  font-size: 26rpx;
  color: #999999;
}

.delivery-section, .remark-section, .invoice-section, .coupon-section {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background-color: #ffffff;
  padding: 32rpx 24rpx;
  margin-bottom: 20rpx;
}

.delivery-label, .remark-label, .invoice-label, .coupon-label {
  font-size: 28rpx;
  color: #333333;
}

.delivery-value, .invoice-value {
  font-size: 28rpx;
  color: #666666;
}

.coupon-value {
  color: #999999;
}

.coupon-selected {
  color: #e64340;
}

.coupon-right {
  display: flex;
  align-items: center;
}

.coupon-count {
  font-size: 24rpx;
  color: #999999;
  margin-right: 16rpx;
}

.delivery-arrow, .invoice-arrow, .coupon-arrow {
  font-size: 36rpx;
  color: #cccccc;
}

.remark-input {
  flex: 1;
  text-align: right;
  font-size: 28rpx;
  color: #666666;
}

.remark-placeholder {
  color: #cccccc;
}

.order-footer {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  height: 100rpx;
  background-color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24rpx;
  box-shadow: 0 -2rpx 10rpx rgba(0, 0, 0, 0.05);
  z-index: 100;
}

.footer-left {
  display: flex;
  align-items: center;
}

.total-label {
  font-size: 26rpx;
  color: #666666;
  margin-right: 12rpx;
}

.total-price-wrapper {
  display: flex;
  align-items: baseline;
}

.total-symbol {
  font-size: 24rpx;
  color: #ff4400;
  font-weight: bold;
}

.total-price {
  font-size: 40rpx;
  color: #ff4400;
  font-weight: bold;
}

.coupon-discount {
  margin-left: 16rpx;
}

.discount-text {
  font-size: 22rpx;
  color: #33cc33;
}

.submit-btn {
  height: 72rpx;
  padding: 0 48rpx;
  border-radius: 36rpx;
  background: linear-gradient(90deg, #ff6b6b, #e64340);
  display: flex;
  align-items: center;
  justify-content: center;
}

.submit-text {
  color: #ffffff;
  font-size: 28rpx;
  font-weight: 500;
}

/* 配送方式选择器 */
.picker-mask {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: flex-end;
  justify-content: center;
  z-index: 1000;
}

.picker-content {
  background-color: #ffffff;
  border-radius: 32rpx 32rpx 0 0;
  width: 100%;
  animation: slideUp 0.3s ease;
}

@keyframes slideUp {
  from {
    transform: translateY(100%);
  }
  to {
    transform: translateY(0);
  }
}

.picker-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 32rpx 24rpx;
  border-bottom: 1rpx solid #f5f5f5;
}

.picker-title {
  font-size: 30rpx;
  font-weight: bold;
  color: #333333;
}

.picker-close {
  width: 48rpx;
  height: 48rpx;
  display: flex;
  align-items: center;
  justify-content: center;
}

.close-icon {
  font-size: 32rpx;
  color: #999999;
}

.picker-options {
  padding: 24rpx;
}

.picker-option {
  display: flex;
  align-items: center;
  padding: 24rpx;
  border-radius: 12rpx;
  margin-bottom: 16rpx;
  border: 2rpx solid #f5f5f5;
}

.picker-option.active {
  border-color: #e64340;
  background-color: #fff5f5;
}

.option-info {
  flex: 1;
}

.option-name {
  font-size: 28rpx;
  color: #333333;
  font-weight: 500;
}

.option-desc {
  font-size: 24rpx;
  color: #999999;
  margin-top: 8rpx;
}

.option-price {
  margin-right: 24rpx;
}

.free-text {
  font-size: 26rpx;
  color: #33cc33;
}

.price-value {
  font-size: 28rpx;
  color: #ff4400;
  font-weight: bold;
}

.option-check {
  width: 40rpx;
  height: 40rpx;
  border-radius: 50%;
  border: 2rpx solid #dddddd;
  display: flex;
  align-items: center;
  justify-content: center;
}

.option-check.checked {
  background-color: #e64340;
  border-color: #e64340;
}

.check-icon {
  color: #ffffff;
  font-size: 24rpx;
}
</style>