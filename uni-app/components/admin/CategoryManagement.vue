<template>
  <view class="category-management">
    <view class="page-bar">
      <view class="search-bar">
        <view class="search-input">
          <input class="input" v-model="searchKeyword" placeholder="璇疯緭鍏ュ垎绫诲悕绉? confirm-type="search" @confirm="handleSearch" />
          <text class="search-icon" @tap="handleSearch">馃攳</text>
        </view>
        <view class="search-select">
          <picker :range="statusOptions" :value="statusIndex" @change="onStatusChange">
            <view class="picker"><text>{{ statusOptions[statusIndex] }}</text></view>
          </picker>
        </view>
        <button size="mini" class="btn-reset" @tap="resetSearch">閲嶇疆</button>
      </view>
      <button class="btn-add" @tap="handleAdd">+ 鏂板鍒嗙被</button>
    </view>

    <view class="table-container">
      <view class="table-header">
        <view class="table-row header-row">
          <view class="table-cell">搴忓彿</view>
          <view class="table-cell">鍒嗙被鍚嶇О</view>
          <view class="table-cell">鍥炬爣</view>
          <view class="table-cell">鎺掑簭</view>
          <view class="table-cell">鐘舵€?/view>
          <view class="table-cell">鎿嶄綔</view>
        </view>
      </view>
      <view class="table-body">
        <view v-for="(item, index) in list" :key="item.id" class="table-row">
          <view class="table-cell">{{ (page - 1) * pageSize + index + 1 }}</view>
          <view class="table-cell">{{ item.name }}</view>
          <view class="table-cell">
            <image v-if="item.images && item.images.length > 0" :src="getFullImageUrl(item.images[0])" mode="aspectFill" class="category-icon" @tap="previewImages(index)" />
          </view>
          <view class="table-cell">
            <view class="sort-buttons">
              <button size="mini" class="sort-btn" @tap="moveUp(index)" :disabled="index === 0">鈫?/button>
              <text class="sort-value">{{ item.sort }}</text>
              <button size="mini" class="sort-btn" @tap="moveDown(index)" :disabled="index === list.length - 1">鈫?/button>
            </view>
          </view>
          <view class="table-cell">
            <text :class="['status-tag', item.status === 1 ? 'status-enabled' : 'status-disabled']">
              {{ item.status === 1 ? '鍚敤' : '绂佺敤' }}
            </text>
          </view>
          <view class="table-cell">
            <view class="action-buttons">
              <button size="mini" class="btn-edit" @tap="handleEdit(item)">缂栬緫</button>
              <button size="mini" class="btn-delete" @tap="handleDelete(item)">鍒犻櫎</button>
            </view>
          </view>
        </view>
      </view>
    </view>

    <view class="pagination">
      <button size="mini" @tap="prevPage" :disabled="page <= 1">涓婁竴椤?/button>
      <text class="page-info">绗?{{ page }} 椤?/ 鍏?{{ Math.ceil(total / pageSize) }} 椤?/text>
      <button size="mini" @tap="nextPage" :disabled="page >= Math.ceil(total / pageSize)">涓嬩竴椤?/button>
    </view>

    <view class="modal-mask" v-if="modalShow" @tap="closeModal">
      <view class="modal-content" @tap.stop>
        <view class="modal-header">
          <text class="modal-title">{{ modalTitle }}</text>
          <text class="modal-close" @tap="closeModal">脳</text>
        </view>
        <view class="modal-body">
          <view class="form-item">
            <text class="form-label">鍒嗙被鍚嶇О <text class="required">*</text></text>
            <input class="form-input" v-model="formData.name" placeholder="璇疯緭鍏ュ垎绫诲悕绉? maxlength="20" />
          </view>
          <view class="form-item">
            <text class="form-label">鎺掑簭</text>
            <input class="form-input" type="number" v-model.number="formData.sort" placeholder="鏁板€艰秺灏忚秺闈犲墠" />
          </view>
          <view class="form-item">
            <text class="form-label">鐘舵€?/text>
            <view class="radio-group">
              <label class="radio-item"><radio :checked="formData.status === 1" color="#007aff" @tap="formData.status = 1" /><text>鍚敤</text></label>
              <label class="radio-item"><radio :checked="formData.status === 0" color="#007aff" @tap="formData.status = 0" /><text>绂佺敤</text></label>
            </view>
          </view>
          <view class="form-item">
            <text class="form-label">鍒嗙被鍥炬爣</text>
            <view class="upload-area">
              <view v-if="formData.icon" class="image-item">
                <image :src="getFullImageUrl(formData.icon)" mode="aspectFill" class="uploaded-image" @tap="previewIcon" />
                <text class="image-delete" @tap="removeIcon">脳</text>
              </view>
              <view class="upload-btn" @tap="chooseIcon">
                <text class="upload-icon">+</text>
                <text class="upload-text">涓婁紶</text>
              </view>
            </view>
          </view>
        </view>
        <view class="modal-footer">
          <button class="btn-cancel" type="button" @tap="closeModal">鍙栨秷</button>
          <button class="btn-confirm" type="button" @tap="handleSubmit">纭畾</button>
        </view>
      </view>
    </view>
  </view>
</template>

<script>
import request from '@/utils/http'

export default {
  name: 'CategoryManagement',
  data() {
    return {
      list: [], total: 0, page: 1, pageSize: 10, modalShow: false, modalTitle: '鏂板鍒嗙被',
      formData: { id: null, name: '', sort: 0, status: 1, icon: '', images: [] },
      apiBaseUrl: 'http://localhost:5162',
      searchKeyword: '', statusOptions: ['鍏ㄩ儴', '鍚敤', '绂佺敤'], statusIndex: 0, searchStatus: null
    }
  },
  mounted() { this.loadList() },
  methods: {
    getFullImageUrl(path) {
      if (!path) return ''
      if (path.startsWith('http://') || path.startsWith('https://')) return path
      return 'http://localhost:5162' + (path.startsWith('/') ? path : '/' + path)
    },
    async loadList() {
      try {
        const params = { page: this.page, pageSize: this.pageSize }
        if (this.searchKeyword) params.keyword = this.searchKeyword
        if (this.searchStatus !== null && this.searchStatus !== undefined) params.status = this.searchStatus
        const res = await request({ url: '/api/admin/categories', data: params })
        this.list = res?.data?.list || []
        this.total = res?.data?.total || 0
      } catch (error) { uni.showToast({ title: error.message || '鍔犺浇澶辫触', icon: 'none' }) }
    },
    handleSearch() { this.page = 1; this.loadList() },
    onStatusChange(e) {
      this.statusIndex = e.detail.value
      this.searchStatus = this.statusIndex === 0 ? null : (this.statusIndex === 1 ? 1 : 0)
      this.page = 1; this.loadList()
    },
    resetSearch() { this.searchKeyword = ''; this.statusIndex = 0; this.searchStatus = null; this.page = 1; this.loadList() },
    moveUp(index) {
      if (index > 0) {
        const temp = this.list[index]
        this.list[index] = this.list[index - 1]
        this.list[index - 1] = temp
        this.updateSort(this.list[index - 1], index - 1)
        this.updateSort(this.list[index], index)
      }
    },
    moveDown(index) {
      if (index < this.list.length - 1) {
        const temp = this.list[index]
        this.list[index] = this.list[index + 1]
        this.list[index + 1] = temp
        this.updateSort(this.list[index + 1], index + 1)
        this.updateSort(this.list[index], index)
      }
    },
    async updateSort(item, index) {
      try {
        await request({ url: `/api/admin/categories/${item.id}`, method: 'PUT', data: { sort: index + 1 } })
      } catch (error) { console.error('鏇存柊鎺掑簭澶辫触', error) }
    },
    previewImages(index) {
      const item = this.list[index]
      if (!item.images || item.images.length === 0) return
      uni.previewImage({ urls: item.images.map(img => this.getFullImageUrl(img)), current: 0 })
    },
    previewIcon() {
      if (!this.formData.icon) return
      uni.previewImage({ urls: [this.getFullImageUrl(this.formData.icon)], current: 0 })
    },
    async uploadIcon(filePath) {
      return new Promise((resolve, reject) => {
        uni.uploadFile({
          url: this.apiBaseUrl + '/api/admin/categories/upload', filePath, name: 'files',
          header: { 'Authorization': 'Bearer ' + uni.getStorageSync('token') },
          formData: { folder: 'categories' },
          success: (res) => {
            uni.hideLoading()
            if (res.statusCode === 200) {
              let data = typeof res.data === 'string' ? JSON.parse(res.data) : res.data
              if (data.success && data.data?.[0]) { this.formData.icon = data.data[0]; uni.showToast({ title: '涓婁紶鎴愬姛', icon: 'success' }); resolve() }
              else { uni.showToast({ title: data.message || '涓婁紶澶辫触', icon: 'none' }); reject(new Error(data.message)) }
            } else { uni.showToast({ title: '涓婁紶澶辫触', icon: 'none' }); reject(new Error('HTTP ' + res.statusCode)) }
          },
          fail: (err) => { uni.hideLoading(); uni.showToast({ title: '涓婁紶澶辫触', icon: 'none' }); reject(err) }
        })
      })
    },
    chooseIcon() {
      uni.showLoading({ title: '閫夋嫨鍥剧墖...' })
      uni.chooseImage({
        count: 1, sizeType: ['compressed'], sourceType: ['album', 'camera'],
        success: async (res) => {
          uni.hideLoading()
          if (res.tempFiles?.[0]) {
            uni.showLoading({ title: '涓婁紶涓?..' })
            try { await this.uploadIcon(res.tempFiles[0].path) } catch (e) { console.error(e) }
            finally { uni.hideLoading() }
          }
        },
        fail: (err) => { uni.hideLoading() }
      })
    },
    removeIcon() { this.formData.icon = '' },
    handleAdd() {
      this.modalTitle = '鏂板鍒嗙被'
      this.formData = { id: null, name: '', sort: 0, status: 1, icon: '', images: [] }
      this.modalShow = true
    },
    handleEdit(row) {
      this.modalTitle = '缂栬緫鍒嗙被'
      this.formData = {
        id: row.id, name: row.name, sort: row.sort || 0, status: row.status ?? 1,
        icon: row.icon || '', images: row.images || []
      }
      this.modalShow = true
    },
    handleDelete(row) {
      uni.showModal({
        title: '鎻愮ず', content: '纭畾瑕佸垹闄よ鍒嗙被鍚楋紵',
        success: async (res) => {
          if (res.confirm) {
            try {
              await request({ url: `/api/admin/categories/${row.id}`, method: 'DELETE' })
              uni.showToast({ title: '鍒犻櫎鎴愬姛', icon: 'success' }); this.loadList()
            } catch (error) { uni.showToast({ title: error.message || '鍒犻櫎澶辫触', icon: 'none' }) }
          }
        }
      })
    },
    closeModal() { this.modalShow = false },
    async handleSubmit() {
      if (!this.formData.name?.trim()) { uni.showToast({ title: '璇疯緭鍏ュ垎绫诲悕绉?, icon: 'none' }); return }
      try {
        const data = { name: this.formData.name.trim(), sort: this.formData.sort, status: this.formData.status, icon: this.formData.icon }
        if (this.formData.id) await request({ url: `/api/admin/categories/${this.formData.id}`, method: 'PUT', data })
        else await request({ url: '/api/admin/categories', method: 'POST', data })
        uni.showToast({ title: '淇濆瓨鎴愬姛', icon: 'success' }); this.closeModal(); this.loadList()
      } catch (error) { uni.showToast({ title: error.message || '淇濆瓨澶辫触', icon: 'none' }) }
    },
    prevPage() { if (this.page > 1) { this.page--; this.loadList() } },
    nextPage() { if (this.page < Math.ceil(this.total / this.pageSize)) { this.page++; this.loadList() } }
  }
}
</script>

<style lang="scss" scoped>
.category-management { padding: 20px; }
.page-bar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.search-bar { display: flex; gap: 16px; padding: 16px; background: #fff; border-radius: 8px; align-items: center; }
.search-input { display: flex; align-items: center; border: 1px solid #ddd; border-radius: 8px; padding: 0 16px; background: #f5f5f5; .input { height: 40px; font-size: 14px; } .search-icon { font-size: 18px; padding-left: 10px; cursor: pointer; } }
.search-select { width: 150px; .picker { height: 40px; line-height: 40px; text-align: center; background: #f5f5f5; border-radius: 8px; font-size: 14px; border: 1px solid #ddd; } }
.btn-reset, .btn-add { font-size: 14px; padding: 0 20px; }
.btn-add { background: #4f46e5; color: #fff; border: none; }
.table-container { background: #fff; border-radius: 8px; overflow: hidden; margin-bottom: 20px; }
.table-header { background: #f5f7fa; }
.table-row { display: flex; align-items: center; border-bottom: 1px solid #eee; &.header-row { font-weight: bold; color: #666; } }
.table-cell { padding: 16px; text-align: center; font-size: 14px; &:nth-child(1) { width: 80px; } &:nth-child(2) { flex: 1; text-align: left; } &:nth-child(3) { width: 100px; } &:nth-child(4) { width: 120px; } &:nth-child(5) { width: 100px; } &:nth-child(6) { width: 150px; } }
.category-icon { width: 50px; height: 50px; border-radius: 8px; background: #f5f5f5; cursor: pointer; }
.sort-buttons { display: flex; align-items: center; justify-content: center; gap: 8px; .sort-btn { width: 32px; height: 32px; padding: 0; font-size: 16px; } .sort-value { font-size: 14px; min-width: 30px; text-align: center; } }
.status-tag { display: inline-block; padding: 4px 12px; border-radius: 4px; font-size: 12px; &.status-enabled { background: #e8f5e9; color: #2e7d32; } &.status-disabled { background: #fff3e0; color: #e65100; } }
.action-buttons { display: flex; gap: 8px; justify-content: center; .btn-edit, .btn-delete { font-size: 12px; padding: 0 12px; height: 32px; line-height: 32px; } .btn-edit { background: #2196f3; color: #fff; } .btn-delete { background: #f44336; color: #fff; } }
.pagination { display: flex; align-items: center; justify-content: center; gap: 20px; padding: 20px; background: #fff; border-radius: 8px; .page-info { font-size: 14px; color: #666; } }
.modal-mask { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; }
.modal-content { background: #fff; border-radius: 12px; width: 90%; max-width: 500px; }
.modal-header { display: flex; align-items: center; justify-content: space-between; padding: 20px; border-bottom: 1px solid #eee; .modal-title { font-size: 18px; font-weight: bold; } .modal-close { font-size: 24px; color: #999; cursor: pointer; } }
.modal-body { padding: 20px; }
.form-item { margin-bottom: 20px; .form-label { display: block; font-size: 14px; color: #333; margin-bottom: 8px; .required { color: #f44336; } } .form-input { width: 100%; height: 40px; border: 1px solid #ddd; border-radius: 6px; padding: 0 12px; font-size: 14px; } .radio-group { display: flex; gap: 30px; .radio-item { display: flex; align-items: center; gap: 8px; font-size: 14px; } } }
.upload-area { display: flex; gap: 12px; }
.image-item { position: relative; width: 100px; height: 100px; background: #f5f5f5; border-radius: 6px; overflow: hidden; .uploaded-image { width: 100%; height: 100%; } .image-delete { position: absolute; top: 0; right: 0; width: 24px; height: 24px; background: rgba(244,67,54,0.9); color: #fff; border-radius: 50%; text-align: center; line-height: 24px; font-size: 14px; cursor: pointer; } }
.upload-btn { width: 100px; height: 100px; border: 2px dashed #ddd; border-radius: 6px; display: flex; flex-direction: column; align-items: center; justify-content: center; background: #fafafa; cursor: pointer; .upload-icon { font-size: 32px; color: #999; } .upload-text { font-size: 12px; color: #999; margin-top: 4px; } }
.modal-footer { display: flex; gap: 12px; padding: 20px; border-top: 1px solid #eee; button { flex: 1; font-size: 16px; } .btn-cancel { background: #f5f5f5; color: #666; } .btn-confirm { background: #007aff; color: #fff; } }
</style>

