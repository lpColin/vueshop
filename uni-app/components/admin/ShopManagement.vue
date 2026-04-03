<template>
  <view class="shop-management">
    <!-- 统计卡片 -->
    <view class="stats-row">
      <view class="stat-card stat-total">
        <view class="stat-icon">🏪</view>
        <view class="stat-info">
          <text class="stat-value">{{ total }}</text>
          <text class="stat-label">店铺总数</text>
        </view>
      </view>
      <view class="stat-card stat-open">
        <view class="stat-icon">✅</view>
        <view class="stat-info">
          <text class="stat-value">{{ openCount }}</text>
          <text class="stat-label">营业中</text>
        </view>
      </view>
      <view class="stat-card stat-closed">
        <view class="stat-icon">🌙</view>
        <view class="stat-info">
          <text class="stat-value">{{ closedCount }}</text>
          <text class="stat-label">已打烊</text>
        </view>
      </view>
    </view>

    <!-- 筛选工具栏 -->
    <view class="toolbar">
      <view class="toolbar-left">
        <view class="search-box">
          <text class="search-icon">🔍</text>
          <input 
            class="search-input" 
            v-model="searchKeyword" 
            placeholder="搜索店铺名称..."
            confirm-type="search"
            @confirm="handleSearch"
          />
          <text class="search-clear" v-if="searchKeyword" @tap="clearSearch">✕</text>
        </view>
        
        <view class="filter-group">
          <view class="filter-label">状态：</view>
          <view class="filter-options">
            <view 
              :class="['filter-option', statusIndex === 0 ? 'active' : '']"
              @tap="selectStatus(0)"
            >
              全部
            </view>
            <view 
              :class="['filter-option', statusIndex === 1 ? 'active' : '']"
              @tap="selectStatus(1)"
            >
              营业中
            </view>
            <view 
              :class="['filter-option', statusIndex === 2 ? 'active' : '']"
              @tap="selectStatus(2)"
            >
              已打烊
            </view>
          </view>
        </view>
      </view>
      
      <view class="toolbar-right">
        <button class="btn-reset" @tap="resetSearch">
          <text class="btn-icon">🔄</text>
          重置
        </button>
        <button class="btn-add" @tap="handleAdd">
          <text class="btn-icon">+</text>
          新增店铺
        </button>
      </view>
    </view>

    <!-- 数据表格 -->
    <view class="table-wrapper">
      <view class="table-container">
        <view class="table-header">
          <view class="table-row">
            <view class="table-cell cell-checkbox">
              <view class="checkbox" :class="{checked: allSelected}" @tap="toggleSelectAll"></view>
            </view>
            <view class="table-cell cell-id">ID</view>
            <view class="table-cell cell-shop">店铺信息</view>
            <view class="table-cell cell-rating">评分</view>
            <view class="table-cell cell-phone">联系电话</view>
            <view class="table-cell cell-status">状态</view>
            <view class="table-cell cell-actions">操作</view>
          </view>
        </view>
        
        <view class="table-body">
          <view 
            v-for="(item, index) in list" 
            :key="item.id" 
            class="table-row"
            :class="{highlighted: selectedIds.includes(item.id)}"
          >
            <view class="table-cell cell-checkbox">
              <view 
                class="checkbox" 
                :class="{checked: selectedIds.includes(item.id)}"
                @tap="toggleSelect(item.id)"
              ></view>
            </view>
            <view class="table-cell cell-id">#{{ item.id }}</view>
            <view class="table-cell cell-shop">
              <view class="shop-info">
                <image 
                  v-if="item.avatar" 
                  :src="getFullImageUrl(item.avatar)" 
                  mode="aspectFill"
                  class="shop-avatar"
                  @tap="previewAvatar(index)"
                />
                <view class="shop-avatar-placeholder" v-else>
                  <text>店</text>
                </view>
                <view class="shop-details">
                  <text class="shop-name">{{ item.name }}</text>
                  <text class="shop-desc" v-if="item.description">{{ item.description }}</text>
                </view>
              </view>
            </view>
            <view class="table-cell cell-rating">
              <view class="rating-stars">
                <text class="star-icon" v-for="i in 5" :key="i">{{ i <= Math.round(item.rating || 5) ? '⭐' : '☆' }}</text>
                <text class="rating-value">{{ Number(item.rating || 5).toFixed(1) }}</text>
              </view>
            </view>
            <view class="table-cell cell-phone">{{ item.phone || '-' }}</view>
            <view class="table-cell cell-status">
              <view :class="['status-badge', item.status === 1 ? 'status-open' : 'status-closed']">
                <view class="status-dot"></view>
                {{ item.status === 1 ? '营业中' : '已打烊' }}
              </view>
            </view>
            <view class="table-cell cell-actions">
              <view class="action-btns">
                <button class="btn-action btn-view" @tap="handleView(item)" title="查看">
                  <text>👁</text>
                </button>
                <button class="btn-action btn-edit" @tap="handleEdit(item)" title="编辑">
                  <text>✏</text>
                </button>
                <button class="btn-action btn-delete" @tap="handleDelete(item)" title="删除">
                  <text>🗑</text>
                </button>
              </view>
            </view>
          </view>
        </view>
        
        <view v-if="list.length === 0" class="empty-state">
          <view class="empty-icon">📭</view>
          <view class="empty-text">暂无店铺数据</view>
          <button class="btn-add-empty" @tap="handleAdd">立即添加</button>
        </view>
      </view>
      
      <view class="pagination">
        <view class="pagination-info">
          共 {{ total }} 条数据，每页 {{ pageSize }} 条
        </view>
        <view class="pagination-controls">
          <button class="btn-page" :disabled="page <= 1" @tap="prevPage">
            <text>‹</text> 上一页
          </button>
          <view class="page-numbers">
            <view 
              v-for="p in visiblePages" 
              :key="p"
              :class="['page-number', p === page ? 'active' : '']"
              @tap="goToPage(p)"
            >
              {{ p }}
            </view>
          </view>
          <button class="btn-page" :disabled="page >= totalPages" @tap="nextPage">
            下一页 <text>›</text>
          </button>
        </view>
      </view>
    </view>

    <!-- 编辑弹窗 -->
    <view class="modal-mask" v-if="modalShow" @tap="closeModal">
      <view class="modal-dialog" @tap.stop>
        <view class="modal-header">
          <view class="modal-title-wrapper">
            <text class="modal-icon">{{ modalTitle.includes('新增') ? '➕' : '✏️' }}</text>
            <text class="modal-title">{{ modalTitle }}</text>
          </view>
          <button class="modal-close" @tap="closeModal">✕</button>
        </view>
        
        <view class="modal-body">
          <view class="form-grid">
            <view class="form-item">
              <text class="form-label">店铺名称 <text class="required">*</text></text>
              <input class="form-input" v-model="formData.name" placeholder="请输入店铺名称" maxlength="30" />
            </view>
            
            <view class="form-item">
              <text class="form-label">店主 <text class="required">*</text></text>
              <picker :range="userList" :value="userIndex" range-key="nickname" @change="onUserChange">
                <view class="form-picker">
                  <text>{{ userList[userIndex]?.nickname || '请选择店主' }}</text>
                  <text class="picker-arrow">▼</text>
                </view>
              </picker>
            </view>
            
            <view class="form-item">
              <text class="form-label">店铺评分</text>
              <view class="rating-input">
                <view class="rating-stars-input">
                  <text v-for="i in 5" :key="i" class="star" :class="{active: i <= (formData.rating || 5)}" @tap="setRating(i)">⭐</text>
                </view>
                <text class="rating-text">{{ (formData.rating || 5).toFixed(1) }}分</text>
              </view>
            </view>
            
            <view class="form-item">
              <text class="form-label">营业状态</text>
              <view class="toggle-group">
                <view :class="['toggle-option', formData.status === 1 ? 'active' : '']" @tap="formData.status = 1">
                  <text>✅ 营业</text>
                </view>
                <view :class="['toggle-option', formData.status === 0 ? 'active' : '']" @tap="formData.status = 0">
                  <text>🌙 打烊</text>
                </view>
              </view>
            </view>
            
            <view class="form-item">
              <text class="form-label">联系电话</text>
              <input class="form-input" v-model="formData.phone" placeholder="请输入联系电话" maxlength="20" />
            </view>
            
            <view class="form-item">
              <text class="form-label">营业时间</text>
              <input class="form-input" v-model="formData.businessHours" placeholder="例如：09:00-22:00" maxlength="30" />
            </view>
            
            <view class="form-item form-item-full">
              <text class="form-label">店铺地址</text>
              <input class="form-input" v-model="formData.address" placeholder="请输入详细地址" maxlength="100" />
            </view>
            
            <view class="form-item form-item-full">
              <text class="form-label">店铺描述</text>
              <textarea class="form-textarea" v-model="formData.description" placeholder="请输入店铺描述（选填）" maxlength="200" />
            </view>
            
            <view class="form-item form-item-full">
              <text class="form-label">店铺头像</text>
              <view class="upload-area">
                <view v-if="formData.avatar" class="upload-preview">
                  <image :src="getFullImageUrl(formData.avatar)" mode="aspectFill" class="uploaded-image" @tap="previewAvatarEdit" />
                  <button class="image-delete" @tap="removeAvatar">✕</button>
                </view>
                <view class="upload-btn" @tap="chooseAvatar">
                  <text class="upload-icon">📷</text>
                  <text class="upload-text">点击上传头像</text>
                  <text class="upload-tip">支持 JPG/PNG 格式</text>
                </view>
              </view>
            </view>
          </view>
        </view>

        <view class="modal-footer">
          <button class="btn-cancel" @tap="closeModal">取消</button>
          <button class="btn-confirm" @tap="handleSubmit">确认保存</button>
        </view>
      </view>
    </view>

    <!-- 查看弹窗 -->
    <view class="modal-mask" v-if="viewModalShow" @tap="viewModalShow = false">
      <view class="modal-dialog modal-view" @tap.stop>
        <view class="modal-header">
          <text class="modal-title">📋 店铺详情</text>
          <button class="modal-close" @tap="viewModalShow = false">✕</button>
        </view>
        <view class="modal-body view-body">
          <view class="view-grid">
            <view class="view-item">
              <text class="view-label">店铺 ID</text>
              <text class="view-value">#{{ viewData.id }}</text>
            </view>
            <view class="view-item">
              <text class="view-label">店铺名称</text>
              <text class="view-value">{{ viewData.name }}</text>
            </view>
            <view class="view-item">
              <text class="view-label">店主</text>
              <text class="view-value">{{ viewData.ownerName || '-' }}</text>
            </view>
            <view class="view-item">
              <text class="view-label">店铺评分</text>
              <text class="view-value">{{ Number(viewData.rating || 5).toFixed(1) }}分</text>
            </view>
            <view class="view-item">
              <text class="view-label">联系电话</text>
              <text class="view-value">{{ viewData.phone || '-' }}</text>
            </view>
            <view class="view-item">
              <text class="view-label">营业状态</text>
              <view :class="['status-badge', viewData.status === 1 ? 'status-open' : 'status-closed']">
                {{ viewData.status === 1 ? '营业中' : '已打烊' }}
              </view>
            </view>
            <view class="view-item full">
              <text class="view-label">营业时间</text>
              <text class="view-value">{{ viewData.businessHours || '-' }}</text>
            </view>
            <view class="view-item full">
              <text class="view-label">店铺地址</text>
              <text class="view-value">{{ viewData.address || '-' }}</text>
            </view>
            <view class="view-item full">
              <text class="view-label">店铺描述</text>
              <text class="view-value view-desc">{{ viewData.description || '暂无描述' }}</text>
            </view>
            <view class="view-item full" v-if="viewData.avatar">
              <text class="view-label">店铺头像</text>
              <image :src="getFullImageUrl(viewData.avatar)" mode="aspectFill" class="view-avatar" @tap="previewViewAvatar" />
            </view>
          </view>
        </view>
        <view class="modal-footer">
          <button class="btn-cancel" @tap="viewModalShow = false">关闭</button>
          <button class="btn-edit-full" @tap="handleEditFromView">编辑此店铺</button>
        </view>
      </view>
    </view>
  </view>
</template>

<script>
import request from '@/utils/http'

export default {
  name: 'ShopManagement',
  data() {
    return {
      list: [],
      total: 0,
      page: 1,
      pageSize: 10,
      modalShow: false,
      viewModalShow: false,
      modalTitle: '新增店铺',
      formData: {
        id: null, name: '', description: '', ownerId: null, rating: 5,
        phone: '', businessHours: '', address: '', avatar: '', logo: '',
        reviewCount: 0, status: 1
      },
      viewData: {},
      apiBaseUrl: 'http://localhost:5162',
      userList: [],
      userIndex: 0,
      searchKeyword: '',
      statusIndex: 0,
      searchStatus: null,
      selectedIds: [],
      allSelected: false
    }
  },
  mounted() {
    this.loadUsers().then(() => this.loadList())
  },
  computed: {
    Math() { return Math },
    totalPages() { return Math.ceil(this.total / this.pageSize) },
    openCount() { return this.list.filter(item => item.status === 1).length },
    closedCount() { return this.list.filter(item => item.status === 0).length },
    visiblePages() {
      const pages = []
      const total = this.totalPages
      let start = Math.max(1, this.page - 2)
      let end = Math.min(total, this.page + 2)
      for (let i = start; i <= end; i++) pages.push(i)
      return pages
    }
  },
  methods: {
    getFullImageUrl(path) {
      if (!path) return ''
      if (path.startsWith('http://') || path.startsWith('https://')) return path
      return 'http://localhost:5162' + (path.startsWith('/') ? path : '/' + path)
    },
    async loadUsers() {
      try {
        const res = await request({ url: '/api/admin/users?page=1&pageSize=100' })
        const allUsers = res?.data?.list || []
        this.userList = allUsers.filter(u => u.role === 'merchant')
        if (this.userList.length === 0) this.userList = allUsers
        if (this.userList.length > 0) this.userIndex = 0
      } catch (error) { this.userList = [] }
    },
    async loadList() {
      try {
        const params = { page: this.page, pageSize: this.pageSize }
        if (this.searchKeyword) params.keyword = this.searchKeyword
        if (this.searchStatus !== null && this.searchStatus !== undefined) params.status = this.searchStatus
        const res = await request({ url: '/api/admin/shops', data: params })
        this.list = res?.data?.list || []
        this.total = res?.data?.total || 0
        this.selectedIds = []
        this.allSelected = false
      } catch (error) {
        uni.showToast({ title: error.message || '加载失败', icon: 'none' })
      }
    },
    handleSearch() { this.page = 1; this.loadList() },
    clearSearch() { this.searchKeyword = ''; this.handleSearch() },
    selectStatus(index) {
      this.statusIndex = index
      this.searchStatus = index === 0 ? null : (index === 1 ? 1 : 0)
      this.page = 1; this.loadList()
    },
    resetSearch() {
      this.searchKeyword = ''; this.statusIndex = 0; this.searchStatus = null; this.page = 1; this.loadList()
    },
    onUserChange(e) {
      this.userIndex = e.detail.value
      if (this.formData.id) this.formData.ownerId = this.userList[this.userIndex]?.id
    },
    setRating(rating) { this.formData.rating = rating },
    toggleSelect(id) {
      const index = this.selectedIds.indexOf(id)
      if (index > -1) this.selectedIds.splice(index, 1)
      else this.selectedIds.push(id)
      this.updateSelectAll()
    },
    toggleSelectAll() {
      if (this.allSelected) this.selectedIds = []
      else this.selectedIds = this.list.map(item => item.id)
      this.allSelected = !this.allSelected
    },
    updateSelectAll() {
      this.allSelected = this.selectedIds.length === this.list.length && this.list.length > 0
    },
    previewAvatar(index) {
      const item = this.list[index]
      if (!item.avatar) return
      uni.previewImage({ urls: [this.getFullImageUrl(item.avatar)], current: 0 })
    },
    previewAvatarEdit() {
      if (!this.formData.avatar) return
      uni.previewImage({ urls: [this.getFullImageUrl(this.formData.avatar)], current: 0 })
    },
    previewViewAvatar() {
      if (!this.viewData.avatar) return
      uni.previewImage({ urls: [this.getFullImageUrl(this.viewData.avatar)], current: 0 })
    },
    async uploadAvatar(filePath) {
      return new Promise((resolve, reject) => {
        const uploadUrl = this.apiBaseUrl + '/api/admin/categories/upload'
        uni.uploadFile({
          url: uploadUrl, filePath, name: 'files',
          fileName: 'upload_' + Date.now() + '.jpg',
          header: { 'Authorization': 'Bearer ' + uni.getStorageSync('token') },
          formData: { folder: 'categories' },
          success: (res) => {
            uni.hideLoading()
            if (res.statusCode !== 200) {
              uni.showToast({ title: '上传失败：HTTP ' + res.statusCode, icon: 'none' })
              reject(new Error('HTTP ' + res.statusCode)); return
            }
            let data
            try { data = typeof res.data === 'string' ? JSON.parse(res.data) : res.data }
            catch (e) { uni.showToast({ title: '响应解析失败', icon: 'none' }); reject(e); return }
            if (data.success && data.data && Array.isArray(data.data) && data.data.length > 0) {
              this.formData.avatar = data.data[0]
              uni.showToast({ title: '上传成功', icon: 'success' }); resolve()
            } else {
              const msg = data.message || data.msg || '上传失败'
              uni.showToast({ title: msg, icon: 'none' }); reject(new Error(msg))
            }
          },
          fail: (err) => { uni.hideLoading(); uni.showToast({ title: '上传失败', icon: 'none' }); reject(err) }
        })
      })
    },
    chooseAvatar() {
      uni.showLoading({ title: '选择图片...' })
      uni.chooseImage({
        count: 1, sizeType: ['compressed'], sourceType: ['album', 'camera'],
        success: async (res) => {
          uni.hideLoading()
          if (!res.tempFiles || res.tempFiles.length === 0) {
            uni.showToast({ title: '没有选择文件', icon: 'none' }); return
          }
          const filePath = res.tempFiles[0].path
          uni.showLoading({ title: '上传中...' })
          try { await this.uploadAvatar(filePath) }
          catch (error) { console.error('[上传异常]', error) }
          finally { uni.hideLoading() }
        },
        fail: (err) => {
          uni.hideLoading()
          if (!err.errMsg || !err.errMsg.includes('cancel')) {
            uni.showToast({ title: '选择失败', icon: 'none' })
          }
        }
      })
    },
    removeAvatar() { this.formData.avatar = '' },
    handleAdd() {
      this.modalTitle = '新增店铺'
      this.formData = {
        id: null, name: '', description: '', ownerId: this.userList[0]?.id || null, rating: 5,
        phone: '', businessHours: '09:00-22:00', address: '', avatar: '', logo: '',
        reviewCount: 0, status: 1
      }
      this.userIndex = 0; this.modalShow = true
    },
    handleEdit(row) {
      this.modalTitle = '编辑店铺'
      this.formData = {
        id: row.id, name: row.name, description: row.description || '', ownerId: row.ownerId,
        rating: row.rating || 5, phone: row.phone || '', businessHours: row.businessHours || '',
        address: row.address || '', avatar: row.avatar || '', logo: row.logo || '',
        reviewCount: row.reviewCount || 0, status: row.status !== undefined ? row.status : 1
      }
      const ownerIdNum = Number(row.ownerId)
      this.userIndex = this.userList.findIndex(u => Number(u.id) === ownerIdNum)
      if (this.userIndex < 0) this.userIndex = 0
      this.modalShow = true
    },
    handleView(row) {
      this.viewData = { ...row }
      const owner = this.userList.find(u => Number(u.id) === Number(row.ownerId))
      this.viewData.ownerName = owner?.nickname || '-'
      this.viewModalShow = true
    },
    handleEditFromView() {
      this.viewModalShow = false
      setTimeout(() => { this.handleEdit(this.viewData) }, 300)
    },
    handleDelete(row) {
      uni.showModal({
        title: '确认删除',
        content: '确定要删除店铺「' + row.name + '」吗？此操作不可恢复。',
        success: async (res) => {
          if (res.confirm) {
            try {
              await request({ url: `/api/admin/shops/${row.id}`, method: 'DELETE' })
              uni.showToast({ title: '删除成功', icon: 'success' }); this.loadList()
            } catch (error) {
              uni.showToast({ title: error.message || '删除失败', icon: 'none' })
            }
          }
        }
      })
    },
    closeModal() { this.modalShow = false },
    async handleSubmit() {
      if (!this.formData.name || !this.formData.name.trim()) {
        uni.showToast({ title: '请输入店铺名称', icon: 'none' }); return
      }
      if (this.userList[this.userIndex]?.id) {
        this.formData.ownerId = this.userList[this.userIndex].id
      }
      try {
        const data = {
          name: this.formData.name.trim(), description: this.formData.description,
          rating: this.formData.rating, phone: this.formData.phone,
          businessHours: this.formData.businessHours, address: this.formData.address,
          avatar: this.formData.avatar, logo: this.formData.logo,
          reviewCount: this.formData.reviewCount, status: this.formData.status
        }
        if (this.formData.ownerId) data.ownerId = this.formData.ownerId
        if (this.formData.id) {
          await request({ url: `/api/admin/shops/${this.formData.id}`, method: 'PUT', data })
        } else {
          await request({ url: '/api/admin/shops', method: 'POST', data })
        }
        uni.showToast({ title: '保存成功', icon: 'success' }); this.closeModal(); this.loadList()
      } catch (error) {
        uni.showToast({ title: error.message || '保存失败', icon: 'none' })
      }
    },
    prevPage() { if (this.page > 1) { this.page--; this.loadList() } },
    nextPage() { if (this.page < this.totalPages) { this.page++; this.loadList() } },
    goToPage(page) {
      if (page !== this.page && page >= 1 && page <= this.totalPages) {
        this.page = page; this.loadList()
      }
    }
  }
}
</script>

<style lang="scss" scoped>
$primary-color: #4f46e5; $primary-hover: #4338ca; $success-color: #10b981;
$danger-color: #ef4444; $warning-color: #f59e0b; $info-color: #3b82f6;
$text-primary: #1f2937; $text-secondary: #6b7280; $text-light: #9ca3af;
$border-color: #e5e7eb; $bg-light: #f9fafb; $bg-white: #ffffff;
$shadow: 0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06);
$radius: 8px; $radius-lg: 12px; $radius-xl: 16px;

.shop-management { padding: 20px; }

.stats-row { display: flex; gap: 20px; margin-bottom: 24px; }
.stat-card {
  flex: 1; display: flex; align-items: center; gap: 16px; padding: 20px 24px;
  background: $bg-white; border-radius: $radius-lg; box-shadow: $shadow;
  border-left: 4px solid transparent;
  &.stat-total { border-left-color: $primary-color; }
  &.stat-open { border-left-color: $success-color; }
  &.stat-closed { border-left-color: $warning-color; }
  .stat-icon { font-size: 32px; line-height: 1; }
  .stat-info { display: flex; flex-direction: column; }
  .stat-value { font-size: 28px; font-weight: 700; color: $text-primary; }
  .stat-label { font-size: 13px; color: $text-secondary; margin-top: 2px; }
}

.toolbar {
  display: flex; justify-content: space-between; align-items: center;
  padding: 16px 20px; background: $bg-white; border-radius: $radius-lg;
  box-shadow: $shadow; margin-bottom: 20px;
  .toolbar-left { display: flex; align-items: center; gap: 24px; flex: 1; }
  .search-box {
    position: relative; width: 280px; display: flex; align-items: center;
    .search-icon { position: absolute; left: 12px; font-size: 16px; color: $text-light; z-index: 1; }
    .search-input {
      width: 100%; height: 40px; padding: 0 36px; background: $bg-light;
      border: 1px solid $border-color; border-radius: $radius; font-size: 14px;
      &:focus { background: $bg-white; border-color: $primary-color; box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1); }
    }
    .search-clear { position: absolute; right: 12px; font-size: 14px; color: $text-light; cursor: pointer; z-index: 1; }
  }
  .filter-group {
    display: flex; align-items: center; gap: 12px;
    .filter-label { font-size: 14px; color: $text-secondary; }
    .filter-options { display: flex; gap: 8px; }
    .filter-option {
      padding: 8px 16px; background: $bg-light; border: 1px solid $border-color;
      border-radius: $radius; font-size: 14px; color: $text-secondary; cursor: pointer;
      &:hover { background: #f3f4f6; }
      &.active { background: $primary-color; border-color: $primary-color; color: #fff; }
    }
  }
  .toolbar-right { display: flex; gap: 12px; }
  .btn-reset, .btn-add {
    display: flex; align-items: center; gap: 6px; padding: 10px 16px;
    border: none; border-radius: $radius; font-size: 14px; cursor: pointer;
  }
  .btn-reset { background: $bg-light; color: $text-secondary; &:hover { background: #f3f4f6; } }
  .btn-add { background: $primary-color; color: #fff; &:hover { background: $primary-hover; } }
}

.table-wrapper {
  background: $bg-white; border-radius: $radius-lg; box-shadow: $shadow; overflow: hidden;
}
.table-container { overflow-x: auto; }
.table-header { background: #f9fafb; border-bottom: 1px solid $border-color; }
.table-row {
  display: flex; align-items: center; border-bottom: 1px solid $border-color;
  &:last-child { border-bottom: none; }
  &.highlighted { background: rgba(79, 70, 229, 0.04); }
}
.table-cell {
  padding: 16px 20px; display: flex; align-items: center;
  &.cell-checkbox { width: 60px; justify-content: center; }
  &.cell-id { width: 80px; font-size: 13px; color: $text-light; font-family: monospace; }
  &.cell-shop { flex: 1; min-width: 250px; }
  &.cell-rating { width: 180px; }
  &.cell-phone { width: 150px; color: $text-secondary; }
  &.cell-status { width: 120px; }
  &.cell-actions { width: 140px; justify-content: center; }
}
.checkbox {
  width: 18px; height: 18px; border: 2px solid $border-color; border-radius: 4px;
  cursor: pointer; position: relative;
  &.checked {
    background: $primary-color; border-color: $primary-color;
    &::after {
      content: '✓'; position: absolute; top: 50%; left: 50%;
      transform: translate(-50%, -50%); color: #fff; font-size: 12px; font-weight: bold;
    }
  }
}
.shop-info { display: flex; align-items: center; gap: 12px; }
.shop-avatar, .shop-avatar-placeholder {
  width: 48px; height: 48px; border-radius: $radius; flex-shrink: 0;
}
.shop-avatar {
  object-fit: cover; cursor: pointer; &:hover { transform: scale(1.05); }
}
.shop-avatar-placeholder {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex; align-items: center; justify-content: center;
  text { color: #fff; font-size: 16px; font-weight: bold; }
}
.shop-details { display: flex; flex-direction: column; gap: 4px; min-width: 0; }
.shop-name {
  font-size: 15px; font-weight: 600; color: $text-primary;
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
}
.shop-desc {
  font-size: 12px; color: $text-light;
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap; max-width: 300px;
}
.rating-stars { display: flex; align-items: center; gap: 4px; }
.status-badge {
  display: inline-flex; align-items: center; gap: 6px; padding: 6px 12px;
  border-radius: 20px; font-size: 13px; font-weight: 500;
  &.status-open { background: rgba(16, 185, 129, 0.1); color: $success-color; }
  &.status-closed { background: rgba(245, 158, 11, 0.1); color: $warning-color; }
  .status-dot { width: 6px; height: 6px; border-radius: 50%; background: currentColor; }
}
.action-btns { display: flex; gap: 8px; }
.btn-action {
  width: 36px; height: 36px; display: flex; align-items: center; justify-content: center;
  border: none; border-radius: $radius; cursor: pointer; font-size: 16px; padding: 0;
  &.btn-view { background: rgba(59, 130, 246, 0.1); color: $info-color; &:hover { background: $info-color; color: #fff; } }
  &.btn-edit { background: rgba(245, 158, 11, 0.1); color: $warning-color; &:hover { background: $warning-color; color: #fff; } }
  &.btn-delete { background: rgba(239, 68, 68, 0.1); color: $danger-color; &:hover { background: $danger-color; color: #fff; } }
}
.empty-state {
  display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 60px 20px;
  .empty-icon { font-size: 64px; margin-bottom: 16px; }
  .empty-text { font-size: 16px; color: $text-secondary; margin-bottom: 24px; }
  .btn-add-empty {
    padding: 12px 32px; background: $primary-color; color: #fff; border: none;
    border-radius: $radius; font-size: 14px; cursor: pointer; &:hover { background: $primary-hover; }
  }
}
.pagination {
  display: flex; justify-content: space-between; align-items: center;
  padding: 16px 24px; border-top: 1px solid $border-color;
  .pagination-info { font-size: 14px; color: $text-secondary; }
  .pagination-controls { display: flex; align-items: center; gap: 12px; }
  .btn-page {
    display: flex; align-items: center; gap: 6px; padding: 8px 16px;
    background: $bg-light; border: 1px solid $border-color; border-radius: $radius;
    font-size: 14px; color: $text-secondary; cursor: pointer;
    &:hover:not(:disabled) { background: #f3f4f6; border-color: $primary-color; color: $primary-color; }
    &:disabled { opacity: 0.5; cursor: not-allowed; }
  }
  .page-numbers { display: flex; gap: 4px; }
  .page-number {
    min-width: 36px; height: 36px; display: flex; align-items: center; justify-content: center;
    padding: 0 8px; background: $bg-light; border: 1px solid $border-color;
    border-radius: $radius; font-size: 14px; color: $text-secondary; cursor: pointer;
    &:hover { border-color: $primary-color; color: $primary-color; }
    &.active { background: $primary-color; border-color: $primary-color; color: #fff; }
  }
}
.modal-mask {
  position: fixed; top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(0, 0, 0, 0.5); display: flex; align-items: center;
  justify-content: center; z-index: 1000; backdrop-filter: blur(4px);
}
.modal-dialog {
  background: $bg-white; border-radius: $radius-xl; width: 90%;
  max-width: 700px; max-height: 85vh; display: flex; flex-direction: column;
  box-shadow: 0 10px 15px rgba(0, 0, 0, 0.1);
}
.modal-header {
  display: flex; justify-content: space-between; align-items: center;
  padding: 20px 24px; border-bottom: 1px solid $border-color;
  .modal-title-wrapper { display: flex; align-items: center; gap: 10px; }
  .modal-icon { font-size: 20px; }
  .modal-title { font-size: 18px; font-weight: 600; color: $text-primary; }
  .modal-close {
    width: 32px; height: 32px; display: flex; align-items: center; justify-content: center;
    background: $bg-light; border: none; border-radius: $radius;
    font-size: 18px; color: $text-secondary; cursor: pointer;
    &:hover { background: $danger-color; color: #fff; }
  }
}
.modal-body { padding: 24px; overflow-y: auto; flex: 1; }
.form-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 20px; }
.form-item {
  display: flex; flex-direction: column; gap: 8px;
  &.form-item-full { grid-column: 1 / -1; }
  .form-label { font-size: 14px; font-weight: 500; color: $text-primary; .required { color: $danger-color; } }
  .form-input, .form-textarea {
    padding: 10px 14px; background: $bg-light; border: 1px solid $border-color;
    border-radius: $radius; font-size: 14px;
    &:focus { background: $bg-white; border-color: $primary-color; box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1); outline: none; }
  }
  .form-input { height: 42px; }
  .form-textarea { min-height: 80px; resize: vertical; }
  .form-picker {
    display: flex; justify-content: space-between; align-items: center;
    padding: 10px 14px; background: $bg-light; border: 1px solid $border-color;
    border-radius: $radius; font-size: 14px; color: $text-secondary; cursor: pointer;
    .picker-arrow { font-size: 12px; }
  }
}
.rating-input { display: flex; align-items: center; gap: 12px; }
.rating-stars-input { display: flex; gap: 4px; .star { font-size: 20px; opacity: 0.3; cursor: pointer; &.active { opacity: 1; } } }
.rating-text { font-size: 14px; color: $text-secondary; }
.toggle-group { display: flex; gap: 8px; .toggle-option { flex: 1; padding: 10px; background: $bg-light; border: 2px solid $border-color; border-radius: $radius; text-align: center; cursor: pointer; &.active { background: $primary-color; border-color: $primary-color; color: #fff; } } }
.upload-area { display: flex; gap: 16px; align-items: flex-start; }
.upload-preview { position: relative; width: 120px; height: 120px; border-radius: $radius; overflow: hidden; .uploaded-image { width: 100%; height: 100%; object-fit: cover; } .image-delete { position: absolute; top: 8px; right: 8px; width: 28px; height: 28px; display: flex; align-items: center; justify-content: center; background: rgba(239, 68, 68, 0.9); color: #fff; border: none; border-radius: 50%; cursor: pointer; } }
.upload-btn {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  gap: 8px; padding: 20px 32px; background: $bg-light; border: 2px dashed $border-color;
  border-radius: $radius; cursor: pointer; &:hover { border-color: $primary-color; background: rgba(79, 70, 229, 0.05); }
  .upload-icon { font-size: 32px; } .upload-text { font-size: 14px; color: $text-primary; font-weight: 500; } .upload-tip { font-size: 12px; color: $text-light; }
}
.modal-footer {
  display: flex; gap: 12px; padding: 20px 24px; border-top: 1px solid $border-color;
  button { flex: 1; padding: 12px 24px; border: none; border-radius: $radius; font-size: 14px; font-weight: 500; cursor: pointer; }
  .btn-cancel { background: $bg-light; color: $text-secondary; &:hover { background: #f3f4f6; } }
  .btn-confirm { background: $primary-color; color: #fff; &:hover { background: $primary-hover; } }
  .btn-edit-full { background: $info-color; color: #fff; &:hover { background: #2563eb; } }
}
.modal-view { max-width: 600px; }
.view-body { padding-top: 0; }
.view-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; }
.view-item {
  display: flex; flex-direction: column; gap: 6px;
  &.full { grid-column: 1 / -1; }
  .view-label { font-size: 13px; color: $text-light; }
  .view-value { font-size: 15px; color: $text-primary; &.view-desc { color: $text-secondary; line-height: 1.6; } }
  .status-badge { align-self: flex-start; }
  .view-avatar { width: 100px; height: 100px; border-radius: $radius; cursor: pointer; }
}
</style>
