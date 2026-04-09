<template>
  <view class="category-management">
    <view class="page-bar">
      <view class="search-bar">
        <view class="search-input">
          <input
            class="input"
            v-model="searchKeyword"
            placeholder="请输入分类名称"
            confirm-type="search"
            @confirm="handleSearch"
          />
          <text class="search-icon" @tap="handleSearch">⌕</text>
        </view>
        <view class="search-select">
          <picker :range="statusOptions" :value="statusIndex" @change="onStatusChange">
            <view class="picker"><text>{{ statusOptions[statusIndex] }}</text></view>
          </picker>
        </view>
        <button size="mini" class="btn-reset" @tap="resetSearch">重置</button>
      </view>
      <button class="btn-add" @tap="handleAdd">+ 新增分类</button>
    </view>

    <view class="table-container">
      <view class="table-header">
        <view class="table-row header-row">
          <view class="table-cell">序号</view>
          <view class="table-cell">分类名称</view>
          <view class="table-cell">图标</view>
          <view class="table-cell">排序</view>
          <view class="table-cell">状态</view>
          <view class="table-cell">操作</view>
        </view>
      </view>
      <view class="table-body">
        <view v-for="(item, index) in list" :key="item.id" class="table-row">
          <view class="table-cell">{{ (page - 1) * pageSize + index + 1 }}</view>
          <view class="table-cell name-cell">{{ item.name || '-' }}</view>
          <view class="table-cell">
            <image
              v-if="displayIcon(item)"
              :src="getFullImageUrl(displayIcon(item))"
              mode="aspectFill"
              class="category-icon"
              @tap="previewImages(index)"
            />
            <text v-else class="empty-text">-</text>
          </view>
          <view class="table-cell">
            <view class="sort-buttons">
              <button size="mini" class="sort-btn" @tap="moveUp(index)" :disabled="index === 0">↑</button>
              <text class="sort-value">{{ item.sort ?? 0 }}</text>
              <button size="mini" class="sort-btn" @tap="moveDown(index)" :disabled="index === list.length - 1">↓</button>
            </view>
          </view>
          <view class="table-cell">
            <text :class="['status-tag', item.status === 1 ? 'status-enabled' : 'status-disabled']">
              {{ item.status === 1 ? '启用' : '禁用' }}
            </text>
          </view>
          <view class="table-cell">
            <view class="action-buttons">
              <button size="mini" class="btn-edit" @tap="handleEdit(item)">编辑</button>
              <button size="mini" class="btn-delete" @tap="handleDelete(item)">删除</button>
            </view>
          </view>
        </view>
      </view>
    </view>

    <view class="pagination">
      <button size="mini" @tap="prevPage" :disabled="page <= 1">上一页</button>
      <text class="page-info">第 {{ page }} 页 / 共 {{ totalPages }} 页</text>
      <button size="mini" @tap="nextPage" :disabled="page >= totalPages">下一页</button>
    </view>

    <view class="modal-mask" v-if="modalShow" @tap="closeModal">
      <view class="modal-content" @tap.stop>
        <view class="modal-header">
          <text class="modal-title">{{ modalTitle }}</text>
          <text class="modal-close" @tap="closeModal">×</text>
        </view>
        <view class="modal-body">
          <view class="form-item">
            <text class="form-label">分类名称 <text class="required">*</text></text>
            <input class="form-input" v-model="formData.name" placeholder="请输入分类名称" maxlength="20" />
          </view>
          <view class="form-item">
            <text class="form-label">排序</text>
            <input class="form-input" type="number" v-model.number="formData.sort" placeholder="数字越小越靠前" />
          </view>
          <view class="form-item">
            <text class="form-label">状态</text>
            <view class="radio-group">
              <label class="radio-item" @tap="formData.status = 1">
                <radio :checked="formData.status === 1" color="#007aff" />
                <text>启用</text>
              </label>
              <label class="radio-item" @tap="formData.status = 0">
                <radio :checked="formData.status === 0" color="#007aff" />
                <text>禁用</text>
              </label>
            </view>
          </view>
          <view class="form-item">
            <text class="form-label">分类图标</text>
            <view class="upload-area">
              <view v-if="formData.icon" class="image-item">
                <image :src="getFullImageUrl(formData.icon)" mode="aspectFill" class="uploaded-image" @tap="previewIcon" />
                <text class="image-delete" @tap="removeIcon">×</text>
              </view>
              <view class="upload-btn" @tap="chooseIcon">
                <text class="upload-icon">+</text>
                <text class="upload-text">上传</text>
              </view>
            </view>
          </view>
        </view>
        <view class="modal-footer">
          <button class="btn-cancel" type="default" @tap="closeModal">取消</button>
          <button class="btn-confirm" type="primary" @tap="handleSubmit">确定</button>
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
      list: [],
      total: 0,
      page: 1,
      pageSize: 10,
      modalShow: false,
      modalTitle: '新增分类',
      formData: {
        id: null,
        name: '',
        sort: 0,
        status: 1,
        icon: '',
        images: []
      },
      apiBaseUrl: 'http://localhost:5162',
      searchKeyword: '',
      statusOptions: ['全部', '启用', '禁用'],
      statusIndex: 0,
      searchStatus: null
    }
  },
  computed: {
    totalPages() {
      const pages = Math.ceil(this.total / this.pageSize)
      return pages > 0 ? pages : 1
    }
  },
  mounted() {
    this.loadList()
  },
  methods: {
    displayIcon(item) {
      return item?.icon || item?.images?.[0] || ''
    },
    getFullImageUrl(path) {
      if (!path) return ''
      if (path.startsWith('http://') || path.startsWith('https://')) return path
      return this.apiBaseUrl + (path.startsWith('/') ? path : `/${path}`)
    },
    async loadList() {
      try {
        const params = { page: this.page, pageSize: this.pageSize }
        if (this.searchKeyword) params.keyword = this.searchKeyword
        if (this.searchStatus !== null && this.searchStatus !== undefined) params.status = this.searchStatus
        const res = await request({ url: '/api/admin/categories', data: params })
        this.list = res?.data?.list || []
        this.total = res?.data?.total || 0
      } catch (error) {
        uni.showToast({ title: error.message || '加载失败', icon: 'none' })
      }
    },
    handleSearch() {
      this.page = 1
      this.loadList()
    },
    onStatusChange(e) {
      this.statusIndex = Number(e.detail.value)
      this.searchStatus = this.statusIndex === 0 ? null : (this.statusIndex === 1 ? 1 : 0)
      this.page = 1
      this.loadList()
    },
    resetSearch() {
      this.searchKeyword = ''
      this.statusIndex = 0
      this.searchStatus = null
      this.page = 1
      this.loadList()
    },
    moveUp(index) {
      if (index <= 0) return
      const temp = this.list[index]
      this.list.splice(index, 1, this.list[index - 1])
      this.list.splice(index - 1, 1, temp)
      this.updateSort(this.list[index - 1], index - 1)
      this.updateSort(this.list[index], index)
    },
    moveDown(index) {
      if (index >= this.list.length - 1) return
      const temp = this.list[index]
      this.list.splice(index, 1, this.list[index + 1])
      this.list.splice(index + 1, 1, temp)
      this.updateSort(this.list[index], index)
      this.updateSort(this.list[index + 1], index + 1)
    },
    async updateSort(item, index) {
      try {
        await request({
          url: `/api/admin/categories/${item.id}`,
          method: 'PUT',
          data: {
            name: item.name,
            sort: index + 1,
            status: item.status,
            icon: item.icon || ''
          }
        })
      } catch (error) {
        console.error('更新排序失败', error)
      }
    },
    previewImages(index) {
      const item = this.list[index]
      const urls = []
      if (item?.icon) urls.push(this.getFullImageUrl(item.icon))
      if (Array.isArray(item?.images)) {
        urls.push(...item.images.map((img) => this.getFullImageUrl(img)))
      }
      if (!urls.length) return
      uni.previewImage({ urls, current: 0 })
    },
    previewIcon() {
      if (!this.formData.icon) return
      uni.previewImage({ urls: [this.getFullImageUrl(this.formData.icon)], current: 0 })
    },
    async uploadIcon(filePath) {
      return new Promise((resolve, reject) => {
        uni.uploadFile({
          url: `${this.apiBaseUrl}/api/admin/categories/upload`,
          filePath,
          name: 'files',
          header: {
            Authorization: `Bearer ${uni.getStorageSync('token')}`
          },
          formData: { folder: 'categories' },
          success: (res) => {
            if (res.statusCode !== 200) {
              reject(new Error(`HTTP ${res.statusCode}`))
              return
            }
            const data = typeof res.data === 'string' ? JSON.parse(res.data) : res.data
            if (data?.success && data.data?.[0]) {
              this.formData.icon = data.data[0]
              uni.showToast({ title: '上传成功', icon: 'success' })
              resolve(data.data[0])
              return
            }
            reject(new Error(data?.message || '上传失败'))
          },
          fail: reject
        })
      })
    },
    chooseIcon() {
      uni.chooseImage({
        count: 1,
        sizeType: ['compressed'],
        sourceType: ['album', 'camera'],
        success: async (res) => {
          const filePath = res.tempFiles?.[0]?.path
          if (!filePath) return
          uni.showLoading({ title: '上传中...' })
          try {
            await this.uploadIcon(filePath)
          } catch (error) {
            uni.showToast({ title: error.message || '上传失败', icon: 'none' })
          } finally {
            uni.hideLoading()
          }
        }
      })
    },
    removeIcon() {
      this.formData.icon = ''
    },
    handleAdd() {
      this.modalTitle = '新增分类'
      this.formData = { id: null, name: '', sort: 0, status: 1, icon: '', images: [] }
      this.modalShow = true
    },
    handleEdit(row) {
      this.modalTitle = '编辑分类'
      this.formData = {
        id: row.id,
        name: row.name || '',
        sort: row.sort || 0,
        status: row.status ?? 1,
        icon: row.icon || '',
        images: row.images || []
      }
      this.modalShow = true
    },
    handleDelete(row) {
      uni.showModal({
        title: '提示',
        content: '确定要删除该分类吗？',
        success: async (res) => {
          if (!res.confirm) return
          try {
            await request({ url: `/api/admin/categories/${row.id}`, method: 'DELETE' })
            uni.showToast({ title: '删除成功', icon: 'success' })
            this.loadList()
          } catch (error) {
            uni.showToast({ title: error.message || '删除失败', icon: 'none' })
          }
        }
      })
    },
    closeModal() {
      this.modalShow = false
    },
    async handleSubmit() {
      if (!this.formData.name?.trim()) {
        uni.showToast({ title: '请输入分类名称', icon: 'none' })
        return
      }
      try {
        const data = {
          name: this.formData.name.trim(),
          sort: Number(this.formData.sort || 0),
          status: this.formData.status,
          icon: this.formData.icon
        }
        if (this.formData.id) {
          await request({ url: `/api/admin/categories/${this.formData.id}`, method: 'PUT', data })
        } else {
          await request({ url: '/api/admin/categories', method: 'POST', data })
        }
        uni.showToast({ title: '保存成功', icon: 'success' })
        this.closeModal()
        this.loadList()
      } catch (error) {
        uni.showToast({ title: error.message || '保存失败', icon: 'none' })
      }
    },
    prevPage() {
      if (this.page <= 1) return
      this.page -= 1
      this.loadList()
    },
    nextPage() {
      if (this.page >= this.totalPages) return
      this.page += 1
      this.loadList()
    }
  }
}
</script>

<style lang="scss" scoped>
.category-management { padding: 20px; }
.page-bar { display: flex; justify-content: space-between; align-items: center; gap: 16px; margin-bottom: 20px; }
.search-bar { display: flex; flex: 1; gap: 16px; padding: 16px; background: #fff; border-radius: 8px; align-items: center; }
.search-input { display: flex; flex: 1; align-items: center; border: 1px solid #ddd; border-radius: 8px; padding: 0 16px; background: #f5f5f5; }
.search-input .input { flex: 1; height: 40px; font-size: 14px; }
.search-input .search-icon { font-size: 18px; padding-left: 10px; cursor: pointer; color: #666; }
.search-select { width: 150px; }
.search-select .picker { height: 40px; line-height: 40px; text-align: center; background: #f5f5f5; border-radius: 8px; font-size: 14px; border: 1px solid #ddd; }
.btn-reset, .btn-add { font-size: 14px; padding: 0 20px; }
.btn-add { background: #4f46e5; color: #fff; border: none; }
.table-container { background: #fff; border-radius: 8px; overflow: hidden; margin-bottom: 20px; }
.table-header { background: #f5f7fa; }
.table-row { display: flex; align-items: center; border-bottom: 1px solid #eee; }
.table-row.header-row { font-weight: bold; color: #666; }
.table-cell { padding: 16px; text-align: center; font-size: 14px; box-sizing: border-box; }
.table-cell:nth-child(1) { width: 80px; }
.table-cell:nth-child(2) { flex: 1; text-align: left; }
.table-cell:nth-child(3) { width: 100px; }
.table-cell:nth-child(4) { width: 120px; }
.table-cell:nth-child(5) { width: 100px; }
.table-cell:nth-child(6) { width: 160px; }
.name-cell { word-break: break-all; }
.category-icon { width: 50px; height: 50px; border-radius: 8px; background: #f5f5f5; cursor: pointer; }
.empty-text { color: #999; }
.sort-buttons { display: flex; align-items: center; justify-content: center; gap: 8px; }
.sort-btn { width: 32px; height: 32px; padding: 0; font-size: 16px; }
.sort-value { font-size: 14px; min-width: 30px; text-align: center; }
.status-tag { display: inline-block; padding: 4px 12px; border-radius: 4px; font-size: 12px; }
.status-enabled { background: #e8f5e9; color: #2e7d32; }
.status-disabled { background: #fff3e0; color: #e65100; }
.action-buttons { display: flex; gap: 8px; justify-content: center; }
.btn-edit, .btn-delete { font-size: 12px; padding: 0 12px; height: 32px; line-height: 32px; }
.btn-edit { background: #2196f3; color: #fff; }
.btn-delete { background: #f44336; color: #fff; }
.pagination { display: flex; align-items: center; justify-content: center; gap: 20px; padding: 20px; background: #fff; border-radius: 8px; }
.page-info { font-size: 14px; color: #666; }
.modal-mask { position: fixed; inset: 0; background: rgba(0, 0, 0, 0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; }
.modal-content { background: #fff; border-radius: 12px; width: 90%; max-width: 500px; }
.modal-header { display: flex; align-items: center; justify-content: space-between; padding: 20px; border-bottom: 1px solid #eee; }
.modal-title { font-size: 18px; font-weight: bold; }
.modal-close { font-size: 24px; color: #999; cursor: pointer; }
.modal-body { padding: 20px; }
.form-item { margin-bottom: 20px; }
.form-label { display: block; font-size: 14px; color: #333; margin-bottom: 8px; }
.required { color: #f44336; }
.form-input { width: 100%; height: 40px; border: 1px solid #ddd; border-radius: 6px; padding: 0 12px; font-size: 14px; box-sizing: border-box; }
.radio-group { display: flex; gap: 30px; }
.radio-item { display: flex; align-items: center; gap: 8px; font-size: 14px; }
.upload-area { display: flex; gap: 12px; }
.image-item { position: relative; width: 100px; height: 100px; background: #f5f5f5; border-radius: 6px; overflow: hidden; }
.uploaded-image { width: 100%; height: 100%; }
.image-delete { position: absolute; top: 0; right: 0; width: 24px; height: 24px; background: rgba(244, 67, 54, 0.9); color: #fff; border-radius: 50%; text-align: center; line-height: 24px; font-size: 14px; cursor: pointer; }
.upload-btn { width: 100px; height: 100px; border: 2px dashed #ddd; border-radius: 6px; display: flex; flex-direction: column; align-items: center; justify-content: center; background: #fafafa; cursor: pointer; }
.upload-icon { font-size: 32px; color: #999; }
.upload-text { font-size: 12px; color: #999; margin-top: 4px; }
.modal-footer { display: flex; gap: 12px; padding: 20px; border-top: 1px solid #eee; }
.modal-footer button { flex: 1; font-size: 16px; }
.btn-cancel { background: #f5f5f5; color: #666; }
.btn-confirm { background: #007aff; color: #fff; }
</style>

