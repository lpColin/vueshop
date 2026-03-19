<template>
  <view class="order-detail-page">
    <scroll-view scroll-y class="detail-scroll">
      <!-- 订单状态 -->
      <view class="status-section" :class="statusClass">
        <view class="status-icon">{{ statusIcon }}</view>
        <view class="status-text">{{ orderStatus }}</view>
        <view class="status-subtext">{{ statusSubtext }}</view>
        <view v-if="order.status === 'pending'" class="pay-btn" @tap="payOrder">
          <text class="pay-text">立即支付</text>
        </view>
      </view>

      <!-- 收货信息 -->
      <view class="address-section" v-if="order.address">
        <view class="address-header">
          <text class="receiver-name">{{ order.address.name }}</text>
          <text class="receiver-phone">{{ order.address.phone }}</text>
        </view>
        <view class="address-detail">
          <text class="detail-text">{{ order.address.detail }}</text>
        </view>
      </view>

      <!-- 商品信息 -->
      <view class="product-section">
        <view class="product-list">
          <view v-for="item in order.items" :key="item.id" class="product-item">
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
        <view class="order-info">
          <view class="info-row">
            <text class="info-label">订单编号：</text>
            <text class="info-value">{{ order.id }}</text>
          </view>
          <view class="info-row">
            <text class="info-label">下单时间：</text>
            <text class="info-value">{{ order.createTime }}</text>
          </view>
          <view class="info-row">
            <text class="info-label">配送方式：</text>
            <text class="info-value">{{ order.deliveryMethod || '快递配送' }}</text>
          </view>
          <view v-if="order.remark" class="info-row">
            <text class="info-label">订单备注：</text>
            <text class="info-value">{{ order.remark }}</text>
          </view>
        </view>
      </view>

      <!-- 价格明细 -->
      <view class="price-section">
        <view class="price-item">
          <text class="price-label">商品总额</text>
          <text class="price-value">¥{{ originalPrice }}</text>
        </view>
        <view v-if="order.deliveryFee > 0" class="price-item">
          <text class="price-label">运费</text>
          <text class="price-value">¥{{ order.deliveryFee }}</text>
        </view>
        <view v-if="order.couponDiscount > 0" class="price-item discount">
          <text class="price-label">优惠</text>
          <text class="price-value">-¥{{ order.couponDiscount }}</text>
        </view>
        <view class="price-divider"></view>
        <view class="price-item total">
          <text class="price-label">实付款</text>
          <text class="price-value">¥{{ order.totalPrice }}</text>
        </view>
      </view>

      <!-- 订单操作 -->
      <view class="action-section" v-if="showActions">
        <view v-if="order.status === 'pending'" class="action-btn cancel" @tap="cancelOrder">
          <text class="action-text">取消订单</text>
        </view>
        <view v-if="order.status === 'pending'" class="action-btn primary" @tap="payOrder">
          <text class="action-text">立即支付</text>
        </view>
        <view v-if="order.status === 'shipped'" class="action-btn primary" @tap="confirmReceipt">
          <text class="action-text">确认收货</text>
        </view>
        <view v-if="order.status === 'completed'" class="action-btn primary" @tap="reviewOrder">
          <text class="action-text">评价订单</text>
        </view>
        <view v-if="['pending', 'shipped'].includes(order.status)" class="action-btn" @tap="applyRefund">
          <text class="action-text">申请售后</text>
        </view>
      </view>

      <!-- 联系客服 -->
      <view class="contact-section" @tap="contactService">
        <text class="contact-icon">🎧</text>
        <text class="contact-text">联系客服</text>
      </view>
    </scroll-view>

    <!-- 底部操作栏 -->
    <view v-if="!showActions" class="detail-footer">
      <view class="footer-btn" @tap="goToCart">
        <text class="btn-icon">🛒</text>
        <text class="btn-text">购物车</text>
      </view>
      <view class="footer-btn" @tap="goToHome">
        <text class="btn-icon">🏠</text>
        <text class="btn-text">首页</text>
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
      orderId: '',
      order: {
        id: '',
        items: [],
        address: null,
        deliveryMethod: '快递配送',
        deliveryFee: 0,
        remark: '',
        couponDiscount: 0,
        totalPrice: '0.00',
        status: 'pending',
        createTime: ''
      }
    }
  },
  computed: {
    statusClass() {
      const statusMap = {
        'pending': 'status-pending',
        'paid': 'status-paid',
        'shipped': 'status-shipped',
        'completed': 'status-completed',
        'cancelled': 'status-cancelled'
      }
      return statusMap[this.order.status] || ''
    },
    statusIcon() {
      const iconMap = {
        'pending': '💰',
        'paid': '📦',
        'shipped': '🚚',
        'completed': '✅',
        'cancelled': '❌'
      }
      return iconMap[this.order.status] || '📋'
    },
    orderStatus() {
      const statusMap = {
        'pending': '待支付',
        'paid': '已支付',
        'shipped': '已发货',
        'completed': '已完成',
        'cancelled': '已取消'
      }
      return statusMap[this.order.status] || '未知状态'
    },
    statusSubtext() {
      const subtextMap = {
        'pending': '请尽快完成支付',
        'paid': '商家正在准备商品',
        'shipped': '商品已发出，请注意查收',
        'completed': '交易已完成',
        'cancelled': '订单已取消'
      }
      return subtextMap[this.order.status] || ''
    },
    originalPrice() {
      return (parseFloat(this.order.totalPrice) + this.order.couponDiscount - this.order.deliveryFee).toFixed(2)
    },
    showActions() {
      return ['pending', 'shipped', 'completed'].includes(this.order.status)
    }
  },
  onLoad(options) {
    if (options.id) {
      this.orderId = options.id
      this.loadOrderDetail(options.id)
    }
  },
  methods: {
    mapStatus(statusCode) {
      const s = Number(statusCode)
      if (s === 0) return 'pending'
      if (s === 1) return 'paid'
      if (s === 2) return 'shipped'
      if (s === 3) return 'completed'
      if (s === 4) return 'cancelled'
      return 'pending'
    },
    async loadOrderDetail(id) {
      if (getToken()) {
        try {
          const res = await request({ url: `/api/order/${id}` })
          const d = res?.data
          const items = Array.isArray(d?.items) ? d.items : []
          this.order = {
            id: String(d.id),
            items: items.map((it) => ({
              id: it.id,
              name: it.productName,
              price: Number(it.productPrice || 0),
              image: it.productImage || '/static/images/product1.png',
              quantity: Number(it.quantity || 0),
              spec: '默认规格'
            })),
            address: {
              name: d.receiverName,
              phone: d.receiverPhone,
              detail: d.receiverAddress
            },
            deliveryMethod: d.deliveryMethod || '快递配送',
            deliveryFee: Number(d.deliveryFee || 0),
            remark: d.remark || '',
            couponDiscount: Number(d.discountAmount || 0),
            totalPrice: Number(d.payAmount || 0).toFixed(2),
            status: this.mapStatus(d.status),
            createTime: d.createTime
          }
          return
        } catch (error) {
          console.warn('[订单详情] 读取后端订单失败，回退本地', error)
        }
      }

      const orders = uni.getStorageSync('orders') || []
      const order = orders.find(o => o.id === id)
      if (order) {
        this.order = order
      } else {
        // 模拟订单数据
        this.order = {
          id: id,
          items: [
            { id: 1, name: '红富士苹果 5 斤装', price: 39.00, image: '/static/images/product1.png', quantity: 2, spec: '默认规格' },
            { id: 2, name: '进口香蕉 1 把', price: 15.80, image: '/static/images/product2.png', quantity: 1, spec: '默认规格' }
          ],
          address: {
            name: '张三',
            phone: '138****8888',
            detail: '北京市朝阳区 XX 街道 XX 小区 XX 号楼 XX 室'
          },
          deliveryMethod: '快递配送',
          deliveryFee: 0,
          remark: '',
          couponDiscount: 0,
          totalPrice: '93.80',
          status: 'pending',
          createTime: new Date().toLocaleString()
        }
      }
    },
    payOrder() {
      uni.showToast({ title: '支付功能待接入', icon: 'none' })
    },
    cancelOrder() {
      uni.showModal({
        title: '提示',
        content: '确定要取消订单吗？',
        success: async (res) => {
          if (res.confirm) {
            if (getToken()) {
              try {
                await request({ url: `/api/order/${this.orderId}/cancel`, method: 'POST' })
                await this.loadOrderDetail(this.orderId)
                uni.showToast({ title: '订单已取消', icon: 'success' })
                return
              } catch (error) {
                uni.showToast({ title: error.message || '取消失败', icon: 'none' })
                return
              }
            }
            this.order.status = 'cancelled'
            this.updateOrder()
            uni.showToast({
              title: '订单已取消',
              icon: 'success'
            })
          }
        }
      })
    },
    confirmReceipt() {
      uni.showModal({
        title: '提示',
        content: '确认已收到商品吗？',
        success: async (res) => {
          if (res.confirm) {
            if (getToken()) {
              try {
                await request({ url: `/api/order/${this.orderId}/confirm`, method: 'POST' })
                await this.loadOrderDetail(this.orderId)
                uni.showToast({ title: '已确认收货', icon: 'success' })
                return
              } catch (error) {
                uni.showToast({ title: error.message || '操作失败', icon: 'none' })
                return
              }
            }
            this.order.status = 'completed'
            this.updateOrder()
            uni.showToast({
              title: '已确认收货',
              icon: 'success'
            })
          }
        }
      })
    },
    reviewOrder() {
      uni.showToast({
        title: '功能开发中',
        icon: 'none'
      })
    },
    applyRefund() {
      uni.showToast({
        title: '功能开发中',
        icon: 'none'
      })
    },
    contactService() {
      uni.makePhoneCall({
        phoneNumber: '13888888888'
      })
    },
    updateOrder() {
      const orders = uni.getStorageSync('orders') || []
      const index = orders.findIndex(o => o.id === this.orderId)
      if (index > -1) {
        orders[index] = this.order
        uni.setStorageSync('orders', orders)
      }
    },
    goToCart() {
      uni.switchTab({
        url: '/pages/cart/cart'
      })
    },
    goToHome() {
      uni.switchTab({
        url: '/pages/index/index'
      })
    }
  }
}
</script>

<style lang="scss" scoped>
.order-detail-page {
  min-height: 100vh;
  background-color: #f5f5f5;
}

.detail-scroll {
  height: calc(100vh - 100rpx);
}

.status-section {
  padding: 48rpx 24rpx;
  text-align: center;
  color: #ffffff;
}

.status-pending {
  background: linear-gradient(180deg, #ff9933 0%, #ff6b6b 100%);
}

.status-paid, .status-shipped {
  background: linear-gradient(180deg, #4CAF50 0%, #8BC34A 100%);
}

.status-completed {
  background: linear-gradient(180deg, #2196F3 0%, #03A9F4 100%);
}

.status-cancelled {
  background: linear-gradient(180deg, #999999 0%, #666666 100%);
}

.status-icon {
  font-size: 80rpx;
  margin-bottom: 16rpx;
}

.status-text {
  font-size: 40rpx;
  font-weight: bold;
  margin-bottom: 8rpx;
}

.status-subtext {
  font-size: 26rpx;
  opacity: 0.9;
}

.pay-btn {
  margin-top: 32rpx;
  height: 72rpx;
  padding: 0 48rpx;
  border-radius: 36rpx;
  background-color: #ffffff;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.pay-text {
  color: #ff6b6b;
  font-size: 28rpx;
  font-weight: 500;
}

.address-section {
  background-color: #ffffff;
  padding: 32rpx 24rpx;
  margin-bottom: 20rpx;
}

.address-header {
  margin-bottom: 16rpx;
}

.receiver-name {
  font-size: 32rpx;
  font-weight: bold;
  color: #333333;
  margin-right: 24rpx;
}

.receiver-phone {
  font-size: 28rpx;
  color: #666666;
}

.address-detail {
  display: flex;
  align-items: flex-start;
}

.detail-text {
  font-size: 26rpx;
  color: #666666;
  line-height: 1.6;
}

.product-section {
  background-color: #ffffff;
  margin-bottom: 20rpx;
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

.order-info {
  padding: 24rpx;
  background-color: #fafafa;
}

.info-row {
  display: flex;
  margin-bottom: 16rpx;
}

.info-row:last-child {
  margin-bottom: 0;
}

.info-label {
  font-size: 24rpx;
  color: #999999;
  width: 160rpx;
  flex-shrink: 0;
}

.info-value {
  font-size: 24rpx;
  color: #666666;
  flex: 1;
}

.price-section {
  background-color: #ffffff;
  padding: 24rpx;
  margin-bottom: 20rpx;
}

.price-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16rpx;
}

.price-item:last-child {
  margin-bottom: 0;
}

.price-label {
  font-size: 26rpx;
  color: #666666;
}

.price-value {
  font-size: 28rpx;
  color: #333333;
}

.price-item.discount .price-value {
  color: #33cc33;
}

.price-divider {
  height: 1rpx;
  background-color: #f0f0f0;
  margin: 16rpx 0;
}

.price-item.total .price-label {
  font-size: 28rpx;
  font-weight: bold;
  color: #333333;
}

.price-item.total .price-value {
  font-size: 36rpx;
  font-weight: bold;
  color: #ff4400;
}

.action-section {
  display: flex;
  justify-content: flex-end;
  padding: 24rpx;
  background-color: #ffffff;
  margin-bottom: 20rpx;
  gap: 20rpx;
}

.action-btn {
  height: 64rpx;
  padding: 0 32rpx;
  border-radius: 32rpx;
  border: 2rpx solid #e0e0e0;
  display: flex;
  align-items: center;
  justify-content: center;
}

.action-btn.primary {
  background: linear-gradient(90deg, #ff6b6b, #e64340);
  border-color: transparent;
}

.action-text {
  font-size: 26rpx;
  color: #666666;
}

.action-btn.primary .action-text {
  color: #ffffff;
}

.contact-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 32rpx;
  background-color: #ffffff;
}

.contact-icon {
  font-size: 48rpx;
  margin-bottom: 8rpx;
}

.contact-text {
  font-size: 26rpx;
  color: #666666;
}

.detail-footer {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  height: 100rpx;
  background-color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 -2rpx 10rpx rgba(0, 0, 0, 0.05);
  z-index: 100;
}

.footer-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 0 48rpx;
}

.btn-icon {
  font-size: 36rpx;
  margin-bottom: 4rpx;
}

.btn-text {
  font-size: 22rpx;
  color: #666666;
}
</style>