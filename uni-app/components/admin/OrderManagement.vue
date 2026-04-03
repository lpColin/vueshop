<template>
  <view class="order-management">
    <view class="page-bar">
      <view class="search-bar">
        <view class="search-input">
          <input class="input" v-model="searchKeyword" placeholder="璇疯緭鍏ヨ鍗曞彿/鏀惰揣浜? confirm-type="search" @confirm="handleSearch" />
          <text class="search-icon" @tap="handleSearch">馃攳</text>
        </view>
        <view class="search-select">
          <picker :range="statusOptions" :value="statusIndex" @change="onStatusChange">
            <view class="picker"><text>{{ statusOptions[statusIndex] }}</text></view>
          </picker>
        </view>
        <button size="mini" class="btn-reset" @tap="resetSearch">閲嶇疆</button>
      </view>
    </view>

    <view class="order-list">
      <view v-for="item in orderList" :key="item.id" class="order-card">
        <view class="order-header">
          <text class="order-no">璁㈠崟鍙凤細{{ item.orderNo }}</text>
          <text :class="['order-status', getStatusClass(item.status)]">{{ getStatusText(item.status) }}</text>
        </view>
        <view class="order-items">
          <view v-for="it in item.items" :key="it.id" class="order-item">
            <image :src="getFullImageUrl(it.productImage)" mode="aspectFill" class="item-image" />
            <view class="item-info">
              <text class="item-name">{{ it.productName }}</text>
              <view class="item-meta">
                <text class="item-price">楼{{ Number(it.productPrice).toFixed(2) }}</text>
                <text class="item-quantity">x{{ it.quantity }}</text>
              </view>
            </view>
          </view>
        </view>
        <view class="order-info">
          <view class="info-row">
            <text class="info-label">鏀惰揣浜猴細</text>
            <text class="info-value">{{ item.receiverName }} {{ item.receiverPhone }}</text>
          </view>
          <view class="info-row">
            <text class="info-label">鍦板潃锛?/text>
            <text class="info-value">{{ item.receiverAddress }}</text>
          </view>
          <view class="info-row">
            <text class="info-label">閰嶉€侊細</text>
            <text class="info-value">{{ item.deliveryMethod }}</text>
          </view>
        </view>
        <view class="order-footer">
          <view class="order-total">
            <text class="total-label">瀹炰粯娆撅細</text>
            <text class="total-amount">楼{{ Number(item.payAmount).toFixed(2) }}</text>
          </view>
          <view class="order-actions">
            <text class="order-time">{{ formatTime(item.createTime) }}</text>
            <button v-if="item.status === 1" size="mini" class="btn-ship" @tap="shipOrder(item)">鍙戣揣</button>
            <button size="mini" class="btn-detail" @tap="viewDetail(item)">璇︽儏</button>
          </view>
        </view>
      </view>
      <view v-if="orderList.length === 0" class="empty-state">
        <text class="empty-text">鏆傛棤璁㈠崟</text>
      </view>
    </view>

    <view class="pagination">
      <button size="mini" @tap="prevPage" :disabled="page <= 1">涓婁竴椤?/button>
      <text class="page-info">绗?{{ page }} 椤?/ 鍏?{{ Math.ceil(total / pageSize) }} 椤?/text>
      <button size="mini" @tap="nextPage" :disabled="page >= Math.ceil(total / pageSize)">涓嬩竴椤?/button>
    </view>

    <!-- 鍙戣揣寮圭獥 -->
    <view class="modal-mask" v-if="shipModalShow" @tap="shipModalShow = false">
      <view class="modal-content" @tap.stop>
        <view class="modal-header">
          <text class="modal-title">璁㈠崟鍙戣揣</text>
          <text class="modal-close" @tap="shipModalShow = false">脳</text>
        </view>
        <view class="modal-body">
          <view class="form-item">
            <text class="form-label">鐗╂祦鍏徃</text>
            <input class="form-input" v-model="shipForm.deliveryCompany" placeholder="璇疯緭鍏ョ墿娴佸叕鍙稿悕绉? />
          </view>
          <view class="form-item">
            <text class="form-label">鐗╂祦鍗曞彿</text>
            <input class="form-input" v-model="shipForm.trackingNo" placeholder="璇疯緭鍏ョ墿娴佸崟鍙? />
          </view>
        </view>
        <view class="modal-footer">
          <button class="btn-cancel" @tap="shipModalShow = false">鍙栨秷</button>
          <button class="btn-confirm" @tap="confirmShip">纭鍙戣揣</button>
        </view>
      </view>
    </view>
  </view>
</template>

<script>
import request from '@/utils/http'

export default {
  name: 'OrderManagement',
  data() {
    return {
      orderList: [], total: 0, page: 1, pageSize: 10,
      searchKeyword: '', statusOptions: ['鍏ㄩ儴', '寰呬粯娆?, '寰呭彂璐?, '寰呮敹璐?, '宸插畬鎴?, '鍞悗'],
      statusIndex: 0, searchStatus: null,
      shipModalShow: false, shipForm: { orderId: null, deliveryCompany: '', trackingNo: '' },
      apiBaseUrl: 'http://localhost:5162'
    }
  },
  mounted() { this.loadOrders() },
  methods: {
    getFullImageUrl(path) {
      if (!path) return ''
      if (path.startsWith('http://') || path.startsWith('https://')) return path
      return 'http://localhost:5162' + (path.startsWith('/') ? path : '/' + path)
    },
    getStatusClass(status) {
      return { 0: 'status-pending', 1: 'status-to-ship', 2: 'status-shipped', 3: 'status-completed', 4: 'status-refund' }[status] || ''
    },
    getStatusText(status) {
      return { 0: '寰呬粯娆?, 1: '寰呭彂璐?, 2: '寰呮敹璐?, 3: '宸插畬鎴?, 4: '鍞悗' }[status] || '鏈煡'
    },
    formatTime(time) {
      if (!time) return ''
      return String(time).substring(0, 19).replace('T', ' ')
    },
    async loadOrders() {
      try {
        const params = { page: this.page, pageSize: this.pageSize }
        if (this.searchKeyword) params.keyword = this.searchKeyword
        if (this.searchStatus !== null && this.searchStatus !== undefined) params.status = this.searchStatus
        const res = await request({ url: '/api/admin/orders', data: params })
        this.orderList = res?.data?.list || []
        this.total = res?.data?.total || 0
      } catch (error) { uni.showToast({ title: error.message || '鍔犺浇澶辫触', icon: 'none' }) }
    },
    handleSearch() { this.page = 1; this.loadOrders() },
    onStatusChange(e) {
      this.statusIndex = e.detail.value
      this.searchStatus = this.statusIndex === 0 ? null : (this.statusIndex - 1)
      this.page = 1; this.loadOrders()
    },
    resetSearch() { this.searchKeyword = ''; this.statusIndex = 0; this.searchStatus = null; this.page = 1; this.loadOrders() },
    shipOrder(item) {
      this.shipForm = { orderId: item.id, deliveryCompany: '', trackingNo: '' }
      this.shipModalShow = true
    },
    async confirmShip() {
      if (!this.shipForm.deliveryCompany?.trim()) { uni.showToast({ title: '璇疯緭鍏ョ墿娴佸叕鍙?, icon: 'none' }); return }
      if (!this.shipForm.trackingNo?.trim()) { uni.showToast({ title: '璇疯緭鍏ョ墿娴佸崟鍙?, icon: 'none' }); return }
      try {
        await request({
          url: `/api/admin/orders/${this.shipForm.orderId}/ship`,
          method: 'POST',
          data: { deliveryCompany: this.shipForm.deliveryCompany.trim(), trackingNo: this.shipForm.trackingNo.trim() }
        })
        uni.showToast({ title: '鍙戣揣鎴愬姛', icon: 'success' })
        this.shipModalShow = false; this.loadOrders()
      } catch (error) { uni.showToast({ title: error.message || '鍙戣揣澶辫触', icon: 'none' }) }
    },
    viewDetail(item) {
      uni.showModal({
        title: '璁㈠崟璇︽儏',
        content: `璁㈠崟鍙凤細${item.orderNo}\n瀹炰粯娆撅細楼${Number(item.payAmount).toFixed(2)}\n鏀惰揣浜猴細${item.receiverName}\n鍦板潃锛?{item.receiverAddress}`,
        showCancel: false
      })
    },
    prevPage() { if (this.page > 1) { this.page--; this.loadOrders() } },
    nextPage() { if (this.page < Math.ceil(this.total / this.pageSize)) { this.page++; this.loadOrders() } }
  }
}
</script>

<style lang="scss" scoped>
.order-management { padding: 20px; }
.page-bar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.search-bar { display: flex; gap: 16px; padding: 16px; background: #fff; border-radius: 8px; align-items: center; flex: 1; }
.search-input { display: flex; align-items: center; border: 1px solid #ddd; border-radius: 8px; padding: 0 16px; background: #f5f5f5; flex: 1; .input { height: 40px; font-size: 14px; flex: 1; } .search-icon { font-size: 18px; padding-left: 10px; cursor: pointer; } }
.search-select { width: 120px; .picker { height: 40px; line-height: 40px; text-align: center; background: #f5f5f5; border-radius: 8px; font-size: 14px; border: 1px solid #ddd; } }
.btn-reset { font-size: 14px; }
.order-list { display: flex; flex-direction: column; gap: 16px; }
.order-card { background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.06); }
.order-header { display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; border-bottom: 1px solid #f0f0f0; }
.order-no { font-size: 14px; color: #666; }
.order-status { font-size: 13px; padding: 4px 12px; border-radius: 4px; &.status-pending { background: #f5f5f5; color: #999; } &.status-to-ship { background: #fff3e0; color: #e65100; } &.status-shipped { background: #e3f2fd; color: #1976d2; } &.status-completed { background: #e8f5e9; color: #2e7d32; } &.status-refund { background: #ffebee; color: #c62828; } }
.order-items { padding: 16px 20px; }
.order-item { display: flex; gap: 12px; margin-bottom: 12px; &:last-child { margin-bottom: 0; } }
.item-image { width: 80px; height: 80px; border-radius: 8px; background: #f5f5f5; flex-shrink: 0; }
.item-info { flex: 1; display: flex; flex-direction: column; justify-content: space-between; }
.item-name { font-size: 14px; color: #333; overflow: hidden; text-overflow: ellipsis; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; }
.item-meta { display: flex; justify-content: space-between; align-items: center; }
.item-price { font-size: 15px; color: #f44336; font-weight: 600; }
.item-quantity { font-size: 13px; color: #999; }
.order-info { padding: 16px 20px; background: #fafafa; }
.info-row { display: flex; margin-bottom: 8px; font-size: 13px; &:last-child { margin-bottom: 0; } }
.info-label { color: #999; width: 70px; flex-shrink: 0; }
.info-value { color: #333; flex: 1; }
.order-footer { display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; border-top: 1px solid #f0f0f0; }
.order-total { display: flex; align-items: center; }
.total-label { font-size: 14px; color: #666; }
.total-amount { font-size: 18px; color: #f44336; font-weight: 700; margin-left: 8px; }
.order-actions { display: flex; align-items: center; gap: 12px; }
.order-time { font-size: 13px; color: #999; }
.btn-ship { background: #2196f3; color: #fff; font-size: 13px; padding: 0 16px; height: 32px; line-height: 32px; }
.btn-detail { background: #f5f5f5; color: #666; font-size: 13px; padding: 0 16px; height: 32px; line-height: 32px; }
.empty-state { text-align: center; padding: 60px 20px; .empty-text { font-size: 16px; color: #999; } }
.pagination { display: flex; align-items: center; justify-content: center; gap: 20px; padding: 20px; background: #fff; border-radius: 8px; margin-top: 20px; .page-info { font-size: 14px; color: #666; } }
.modal-mask { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; }
.modal-content { background: #fff; border-radius: 12px; width: 90%; max-width: 400px; }
.modal-header { display: flex; align-items: center; justify-content: space-between; padding: 20px; border-bottom: 1px solid #eee; .modal-title { font-size: 18px; font-weight: bold; } .modal-close { font-size: 24px; color: #999; cursor: pointer; } }
.modal-body { padding: 20px; }
.form-item { margin-bottom: 20px; .form-label { display: block; font-size: 14px; color: #333; margin-bottom: 8px; } .form-input { width: 100%; height: 40px; border: 1px solid #ddd; border-radius: 6px; padding: 0 12px; font-size: 14px; } }
.modal-footer { display: flex; gap: 12px; padding: 20px; border-top: 1px solid #eee; button { flex: 1; font-size: 16px; } .btn-cancel { background: #f5f5f5; color: #666; } .btn-confirm { background: #007aff; color: #fff; } }
</style>

