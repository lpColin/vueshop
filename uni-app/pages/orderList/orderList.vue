<template>
  <view class="order-list-page">
    <view class="tabs">
      <view
        v-for="tab in tabs"
        :key="tab.key"
        class="tab-item"
        :class="{ active: currentTab === tab.key }"
        @tap="switchTab(tab.key)"
      >
        {{ tab.label }}
      </view>
    </view>

    <scroll-view scroll-y class="list-scroll">
      <view v-if="loading" class="state-text">加载中...</view>
      <view v-else-if="orders.length === 0" class="state-text">暂无订单</view>

      <view v-else>
        <view class="order-card" v-for="order in orders" :key="order.id" @tap="goDetail(order.id)">
          <view class="row head">
            <text class="no">订单号：{{ order.orderNo || order.id }}</text>
            <text class="status" :class="`s-${order.status}`">{{ getStatusText(order.status) }}</text>
          </view>

          <view class="items">
            <view class="item" v-for="it in (order.items || [])" :key="it.id">
              <image class="img" :src="it.image" mode="aspectFill" />
              <view class="meta">
                <text class="name">{{ it.name }}</text>
                <text class="price">¥{{ it.price }} × {{ it.quantity }}</text>
              </view>
            </view>
          </view>

          <view class="row foot">
            <text class="time">{{ formatTime(order.createTime) }}</text>
            <text class="pay">实付：¥{{ Number(order.payAmount || order.totalPrice || 0).toFixed(2) }}</text>
          </view>
        </view>
      </view>
    </scroll-view>
  </view>
</template>

<script>
import request from '@/utils/http'
import { getToken } from '@/utils/auth'

const STATUS_MAP = {
  pending: 1,
  shipping: 2,
  done: 3,
  all: null
}

export default {
  data() {
    return {
      loading: false,
      currentTab: 'all',
      tabs: [
        { key: 'all', label: '全部订单' },
        { key: 'pending', label: '待发货' },
        { key: 'shipping', label: '待收货' },
        { key: 'done', label: '已完成' }
      ],
      orders: []
    }
  },
  onLoad(options) {
    this.currentTab = options?.status || 'all'
    this.loadOrders()
  },
  onShow() {
    this.loadOrders()
  },
  methods: {
    async loadOrders() {
      if (!getToken()) {
        uni.showToast({ title: '请先登录', icon: 'none' })
        setTimeout(() => {
          uni.navigateTo({ url: '/pages/login/login' })
        }, 250)
        return
      }

      this.loading = true
      try {
        const queryStatus = STATUS_MAP[this.currentTab]
        const url = queryStatus == null ? '/api/order?page=1&pageSize=50' : `/api/order?page=1&pageSize=50&status=${queryStatus}`
        const res = await request({ url })
        const list = Array.isArray(res?.data?.list) ? res.data.list : []
        this.orders = list.map((o) => ({
          id: o.id,
          orderNo: o.orderNo,
          status: Number(o.status),
          payAmount: o.payAmount,
          totalPrice: o.payAmount,
          createTime: o.createTime,
          items: (o.items || []).map((it) => ({
            id: it.id,
            name: it.productName,
            price: Number(it.productPrice || 0),
            image: it.productImage || '/static/images/product1.png',
            quantity: Number(it.quantity || 0)
          }))
        }))
      } catch (error) {
        uni.showToast({ title: error.message || '加载订单失败', icon: 'none' })
      } finally {
        this.loading = false
      }
    },
    switchTab(key) {
      if (this.currentTab === key) return
      this.currentTab = key
      this.loadOrders()
    },
    getStatusText(status) {
      const s = Number(status)
      if (s === 0) return '待支付'
      if (s === 1) return '待发货'
      if (s === 2) return '待收货'
      if (s === 3) return '已完成'
      if (s === 4) return '已取消'
      return '未知'
    },
    formatTime(v) {
      if (!v) return ''
      return String(v).replace('T', ' ').slice(0, 19)
    },
    goDetail(id) {
      uni.navigateTo({ url: `/pages/orderDetail/orderDetail?id=${id}` })
    }
  }
}
</script>

<style lang="scss" scoped>
.order-list-page { min-height: 100vh; background: #f5f5f5; }
.tabs { height: 86rpx; background: #fff; display: flex; border-bottom: 1rpx solid #f0f0f0; }
.tab-item { flex: 1; text-align: center; line-height: 86rpx; color: #666; font-size: 28rpx; }
.tab-item.active { color: #e64340; font-weight: 700; border-bottom: 4rpx solid #e64340; }
.list-scroll { height: calc(100vh - 86rpx); padding: 18rpx; }
.state-text { text-align: center; color: #999; margin-top: 120rpx; font-size: 28rpx; }
.order-card { background: #fff; border-radius: 12rpx; margin-bottom: 16rpx; padding: 18rpx; }
.row { display: flex; align-items: center; justify-content: space-between; }
.head { margin-bottom: 10rpx; }
.no { font-size: 22rpx; color: #999; }
.status { font-size: 24rpx; }
.s-0, .s-4 { color: #999; }
.s-1, .s-2 { color: #e64340; }
.s-3 { color: #1aad19; }
.item { display: flex; margin-top: 10rpx; }
.img { width: 120rpx; height: 120rpx; border-radius: 8rpx; margin-right: 12rpx; }
.meta { flex: 1; display: flex; flex-direction: column; justify-content: center; }
.name { font-size: 26rpx; color: #333; }
.price { margin-top: 8rpx; color: #666; font-size: 24rpx; }
.foot { margin-top: 12rpx; border-top: 1rpx solid #f4f4f4; padding-top: 10rpx; }
.time { color: #999; font-size: 22rpx; }
.pay { color: #333; font-size: 24rpx; }
</style>
