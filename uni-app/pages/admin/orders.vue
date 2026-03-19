<template>
  <view class="page">
    <view class="page-bar">
      <button size="mini" @tap="goAdminHome">返回看板</button>
      <button size="mini" @tap="goUserCenter">个人中心</button>
    </view>
    
    <!-- 筛选框 -->
    <view class="search-bar">
      <view class="search-input">
        <input 
          class="input" 
          v-model="searchKeyword" 
          placeholder="请输入订单号/收货人"
          confirm-type="search"
          @confirm="handleSearch"
        />
        <text class="search-icon" @tap="handleSearch">🔍</text>
      </view>
      
      <view class="search-select">
        <picker 
          :range="statusOptions" 
          :value="statusIndex"
          @change="onStatusChange"
        >
          <view class="picker">
            <text>{{ statusOptions[statusIndex] }}</text>
          </view>
        </picker>
      </view>
      
      <button size="mini" class="btn-reset" @tap="resetSearch">重置</button>
    </view>
    
    <!-- 订单列表 -->
    <view class="order-list">
      <view 
        v-for="item in orderList" 
        :key="item.id" 
        class="order-card"
      >
        <view class="order-header">
          <text class="order-no">订单号：{{ item.orderNo }}</text>
          <text :class="['order-status', getStatusClass(item.status)]">
            {{ getStatusText(item.status) }}
          </text>
        </view>
        
        <view class="order-items">
          <view v-for="it in item.items" :key="it.id" class="order-item">
            <image :src="getFullImageUrl(it.productImage)" mode="aspectFill" class="item-image" />
            <view class="item-info">
              <text class="item-name">{{ it.productName }}</text>
              <view class="item-meta">
                <text class="item-price">¥{{ Number(it.productPrice).toFixed(2) }}</text>
                <text class="item-quantity">x{{ it.quantity }}</text>
              </view>
            </view>
          </view>
        </view>
        
        <view class="order-info">
          <view class="info-row">
            <text class="info-label">收货人：</text>
            <text class="info-value">{{ item.receiverName }} {{ item.receiverPhone }}</text>
          </view>
          <view class="info-row">
            <text class="info-label">地址：</text>
            <text class="info-value">{{ item.receiverAddress }}</text>
          </view>
          <view class="info-row">
            <text class="info-label">配送：</text>
            <text class="info-value">{{ item.deliveryMethod }}</text>
          </view>
          <view v-if="item.remark" class="info-row">
            <text class="info-label">备注：</text>
            <text class="info-value">{{ item.remark }}</text>
          </view>
        </view>
        
        <view class="order-footer">
          <view class="order-total">
            <text class="total-label">实付款：</text>
            <text class="total-amount">¥{{ Number(item.payAmount).toFixed(2) }}</text>
          </view>
          <view class="order-actions">
            <text class="order-time">{{ formatTime(item.createTime) }}</text>
            <button v-if="item.status === 1" size="mini" class="btn-ship" @tap="shipOrder(item)">发货</button>
            <button size="mini" class="btn-detail" @tap="viewDetail(item)">详情</button>
          </view>
        </view>
      </view>
      
      <view v-if="orderList.length === 0" class="empty-state">
        <text class="empty-text">暂无订单</text>
      </view>
    </view>

    <!-- 分页 -->
    <view class="pagination">
      <button size="mini" @tap="prevPage" :disabled="page <= 1">上一页</button>
      <text class="page-info">第 {{ page }} 页 / 共 {{ Math.ceil(total / pageSize) }} 页</text>
      <button size="mini" @tap="nextPage" :disabled="page >= Math.ceil(total / pageSize)">下一页</button>
    </view>

    <!-- 发货弹窗 -->
    <view class="modal-mask" v-if="showShipModal" @tap="closeShipModal">
      <view class="modal-content" @tap.stop>
        <view class="modal-header">
          <text class="modal-title">订单发货</text>
          <text class="modal-close" @tap="closeShipModal">×</text>
        </view>
        
        <view class="modal-body">
          <view class="form-item">
            <text class="form-label">订单号</text>
            <text class="form-value">{{ currentOrder.orderNo }}</text>
          </view>
          
          <view class="form-item">
            <text class="form-label">收货信息</text>
            <text class="form-value">{{ currentOrder.receiverName }} {{ currentOrder.receiverPhone }}</text>
            <text class="form-value">{{ currentOrder.receiverAddress }}</text>
          </view>
          
          <view class="form-item">
            <text class="form-label">商品列表</text>
            <view v-for="it in currentOrder.items" :key="it.id" class="ship-item">
              <text class="ship-item-name">{{ it.productName }}</text>
              <text class="ship-item-qty">x{{ it.quantity }}</text>
            </view>
          </view>
        </view>

        <view class="modal-footer">
          <button class="btn-cancel" @tap="closeShipModal">取消</button>
          <button class="btn-confirm" @tap="confirmShip">确认发货</button>
        </view>
      </view>
    </view>
  </view>
</template>

<script>
import request from '@/utils/http'
import { getUserInfo } from '@/utils/auth'

export default {
  data() {
    return {
      orderList: [],
      total: 0,
      page: 1,
      pageSize: 10,
      apiBaseUrl: 'http://localhost:5162',
      // 筛选相关
      searchKeyword: '',
      statusOptions: ['全部', '待支付', '待发货', '待收货', '已完成', '已取消'],
      statusIndex: 0,
      searchStatus: null,
      // 发货弹窗
      showShipModal: false,
      currentOrder: {
        id: null,
        orderNo: '',
        receiverName: '',
        receiverPhone: '',
        receiverAddress: '',
        items: []
      }
    }
  },
  computed: {
    Math() {
      return Math
    }
  },
  onShow() {
    if (!this.ensureAdminAccess()) return
    this.loadOrders()
  },
  methods: {
    goAdminHome() {
      uni.navigateTo({ url: '/pages/admin/index' })
    },
    goUserCenter() {
      uni.switchTab({ url: '/pages/user/user' })
    },
    ensureAdminAccess() {
      const user = getUserInfo()
      if (!user || user.role !== 'admin') {
        uni.showToast({ title: '仅管理员可访问', icon: 'none' })
        setTimeout(() => {
          uni.switchTab({ url: '/pages/user/user' })
        }, 300)
        return false
      }
      return true
    },
    getFullImageUrl(path) {
      if (!path) return ''
      if (path.startsWith('http://') || path.startsWith('https://')) return path
      if (path.startsWith('/')) {
        return this.apiBaseUrl + path
      }
      return this.apiBaseUrl + '/' + path
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
    getStatusClass(status) {
      const s = Number(status)
      if (s === 0) return 'status-pending-pay'
      if (s === 1) return 'status-pending-ship'
      if (s === 2) return 'status-pending-receive'
      if (s === 3) return 'status-completed'
      if (s === 4) return 'status-cancelled'
      return ''
    },
    formatTime(timeStr) {
      if (!timeStr) return ''
      return String(timeStr).replace('T', ' ').slice(0, 16)
    },
    async loadOrders() {
      try {
        const params = {
          page: this.page,
          pageSize: this.pageSize
        }
        
        // 添加筛选参数
        if (this.searchStatus !== null && this.searchStatus !== undefined) {
          params.status = this.searchStatus
        }
        
        const res = await request({ url: '/api/admin/orders', data: params })
        this.orderList = res?.data?.list || []
        this.total = res?.data?.total || 0
      } catch (error) {
        uni.showToast({ title: error.message || '加载失败', icon: 'none' })
      }
    },
    
    // 筛选方法
    handleSearch() {
      this.page = 1
      this.loadOrders()
    },
    onStatusChange(e) {
      this.statusIndex = e.detail.value
      // 0=全部，1=待支付 (0), 2=待发货 (1), 3=待收货 (2), 4=已完成 (3), 5=已取消 (4)
      if (this.statusIndex === 0) {
        this.searchStatus = null
      } else {
        this.searchStatus = this.statusIndex - 1
      }
      this.page = 1
      this.loadOrders()
    },
    resetSearch() {
      this.searchKeyword = ''
      this.statusIndex = 0
      this.searchStatus = null
      this.page = 1
      this.loadOrders()
    },
    
    // 发货操作
    shipOrder(order) {
      this.currentOrder = {
        id: order.id,
        orderNo: order.orderNo,
        receiverName: order.receiverName,
        receiverPhone: order.receiverPhone,
        receiverAddress: order.receiverAddress,
        items: order.items || []
      }
      this.showShipModal = true
    },
    closeShipModal() {
      this.showShipModal = false
      this.currentOrder = {
        id: null,
        orderNo: '',
        receiverName: '',
        receiverPhone: '',
        receiverAddress: '',
        items: []
      }
    },
    async confirmShip() {
      try {
        await request({
          url: `/api/admin/orders/${this.currentOrder.id}/ship`,
          method: 'POST'
        })
        uni.showToast({ title: '发货成功', icon: 'success' })
        this.closeShipModal()
        this.loadOrders()
      } catch (error) {
        uni.showToast({ title: error.message || '发货失败', icon: 'none' })
      }
    },
    
    // 查看详情
    viewDetail(order) {
      uni.navigateTo({
        url: `/pages/orderDetail/orderDetail?id=${order.id}`
      })
    },
    
    // 分页
    prevPage() {
      if (this.page > 1) {
        this.page--
        this.loadOrders()
      }
    },
    nextPage() {
      const maxPage = Math.ceil(this.total / this.pageSize)
      if (this.page < maxPage) {
        this.page++
        this.loadOrders()
      }
    }
  }
}
</script>

<style lang="scss" scoped>
.page {
  min-height: 100vh;
  background: #eceef2;
  padding: 20rpx;
}

.page-bar {
  display: flex;
  justify-content: flex-end;
  gap: 12rpx;
  margin-bottom: 16rpx;
}

// 筛选栏
.search-bar {
  display: flex;
  gap: 16rpx;
  margin-bottom: 20rpx;
  padding: 20rpx;
  background: #fff;
  border-radius: 8rpx;
  align-items: center;
  
  .search-input {
    flex: 1;
    display: flex;
    align-items: center;
    border: 1px solid #ddd;
    border-radius: 8rpx;
    padding: 0 16rpx;
    background: #f5f5f5;
    
    .input {
      flex: 1;
      height: 70rpx;
      font-size: 28rpx;
    }
    
    .search-icon {
      font-size: 32rpx;
      padding-left: 10rpx;
    }
  }
  
  .search-select {
    width: 200rpx;
    
    .picker {
      height: 70rpx;
      line-height: 70rpx;
      text-align: center;
      background: #f5f5f5;
      border-radius: 8rpx;
      font-size: 28rpx;
      border: 1px solid #ddd;
    }
  }
  
  .btn-reset {
    font-size: 28rpx;
    padding: 0 24rpx;
  }
}

// 订单列表
.order-list {
  margin-bottom: 20rpx;
}

.order-card {
  background: #fff;
  border-radius: 12rpx;
  padding: 20rpx;
  margin-bottom: 16rpx;
  box-shadow: 0 2rpx 12rpx rgba(0, 0, 0, 0.05);
}

.order-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 16rpx;
  border-bottom: 1rpx solid #f0f0f0;
}

.order-no {
  font-size: 24rpx;
  color: #666;
}

.order-status {
  font-size: 24rpx;
  padding: 4rpx 12rpx;
  border-radius: 4rpx;
  
  &.status-pending-pay {
    background: #fff3e0;
    color: #ff9800;
  }
  
  &.status-pending-ship {
    background: #e3f2fd;
    color: #2196f3;
  }
  
  &.status-pending-receive {
    background: #f3e5f5;
    color: #9c27b0;
  }
  
  &.status-completed {
    background: #e8f5e9;
    color: #4caf50;
  }
  
  &.status-cancelled {
    background: #ffebee;
    color: #f44336;
  }
}

.order-items {
  padding: 16rpx 0;
}

.order-item {
  display: flex;
  margin-bottom: 16rpx;
  
  &:last-child {
    margin-bottom: 0;
  }
}

.item-image {
  width: 120rpx;
  height: 120rpx;
  border-radius: 8rpx;
  margin-right: 16rpx;
  flex-shrink: 0;
}

.item-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.item-name {
  font-size: 26rpx;
  color: #333;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  line-height: 1.4;
}

.item-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.item-price {
  font-size: 28rpx;
  color: #ff4400;
  font-weight: 600;
}

.item-quantity {
  font-size: 24rpx;
  color: #999;
}

.order-info {
  padding: 16rpx 0;
  border-top: 1rpx solid #f0f0f0;
  border-bottom: 1rpx solid #f0f0f0;
}

.info-row {
  display: flex;
  margin-bottom: 8rpx;
  
  &:last-child {
    margin-bottom: 0;
  }
}

.info-label {
  font-size: 24rpx;
  color: #999;
  min-width: 100rpx;
}

.info-value {
  font-size: 24rpx;
  color: #333;
  flex: 1;
}

.order-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 16rpx;
}

.order-total {
  display: flex;
  align-items: baseline;
}

.total-label {
  font-size: 24rpx;
  color: #666;
}

.total-amount {
  font-size: 32rpx;
  color: #ff4400;
  font-weight: 700;
}

.order-actions {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 8rpx;
}

.order-time {
  font-size: 22rpx;
  color: #999;
}

.btn-ship, .btn-detail {
  font-size: 24rpx;
  padding: 0 20rpx;
  height: 56rpx;
  line-height: 56rpx;
  border-radius: 28rpx;
}

.btn-ship {
  background: #2196f3;
  color: #fff;
}

.btn-detail {
  background: #f5f5f5;
  color: #666;
}

// 分页
.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 20rpx;
  padding: 20rpx;
  background: #fff;
  border-radius: 8rpx;
  
  .page-info {
    font-size: 26rpx;
    color: #666;
  }
}

// 空状态
.empty-state {
  text-align: center;
  padding: 120rpx 0;
  
  .empty-text {
    font-size: 28rpx;
    color: #999;
  }
}

// 弹窗
.modal-mask {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: #fff;
  border-radius: 16rpx;
  width: 90%;
  max-width: 700rpx;
  max-height: 80vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 30rpx;
  border-bottom: 1px solid #eee;
  
  .modal-title {
    font-size: 32rpx;
    font-weight: bold;
  }
  
  .modal-close {
    font-size: 48rpx;
    color: #999;
    line-height: 1;
  }
}

.modal-body {
  padding: 30rpx;
}

.form-item {
  margin-bottom: 24rpx;
  
  .form-label {
    display: block;
    font-size: 26rpx;
    color: #666;
    margin-bottom: 12rpx;
  }
  
  .form-value {
    display: block;
    font-size: 26rpx;
    color: #333;
    margin-bottom: 8rpx;
  }
}

.ship-item {
  display: flex;
  justify-content: space-between;
  padding: 8rpx 0;
  font-size: 24rpx;
  border-bottom: 1rpx solid #f5f5f5;
  
  &:last-child {
    border-bottom: none;
  }
  
  .ship-item-name {
    color: #333;
  }
  
  .ship-item-qty {
    color: #999;
  }
}

.modal-footer {
  display: flex;
  gap: 20rpx;
  padding: 30rpx;
  border-top: 1px solid #eee;
  
  button {
    flex: 1;
    font-size: 30rpx;
  }
  
  .btn-cancel {
    background: #f5f5f5;
    color: #666;
  }
  
  .btn-confirm {
    background: #2196f3;
    color: #fff;
  }
}
</style>
