<template>
  <view class="product-management">
    <view class="page-bar">
      <view class="search-bar">
        <view class="search-input">
          <input
            class="input"
            v-model="searchKeyword"
            placeholder="请输入商品名称"
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
      <button class="btn-add" @tap="handleAdd">+ 新增商品</button>
    </view>

    <view class="table-container">
      <view class="table-header">
        <view class="table-row header-row">
          <view class="table-cell">序号</view>
          <view class="table-cell">商品名称</view>
          <view class="table-cell">图片</view>
          <view class="table-cell">价格</view>
          <view class="table-cell">库存</view>
          <view class="table-cell">状态</view>
          <view class="table-cell">操作</view>
        </view>
      </view>
      <view class="table-body">
        <view v-for="(item, index) in list" :key="item.id" class="table-row">
          <view class="table-cell">{{ (page - 1) * pageSize + index + 1 }}</view>
          <view class="table-cell cell-ellipsis">{{ item.name || '-' }}</view>
          <view class="table-cell">
            <image
              v-if="firstImage(item)"
              :src="getFullImageUrl(firstImage(item))"
              mode="aspectFill"
              class="product-image"
              @tap="previewImages(index)"
            />
            <text v-else class="no-image">无图</text>
          </view>
          <view class="table-cell">¥{{ Number(item.price || 0).toFixed(2) }}</view>
          <view class="table-cell">{{ item.stock ?? 0 }}</view>
          <view class="table-cell">
            <text :class="['status-tag', item.status === 1 ? 'status-onshelf' : 'status-offshelf']">
              {{ item.status === 1 ? '上架' : '下架' }}
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
            <text class="form-label">商品名称 <text class="required">*</text></text>
            <input class="form-input" v-model="formData.name" placeholder="请输入商品名称" maxlength="50" />
          </view>
          <view class="form-item">
            <text class="form-label">价格 <text class="required">*</text></text>
            <input class="form-input" type="number" v-model.number="formData.price" placeholder="请输入商品价格" />
          </view>
          <view class="form-item">
            <text class="form-label">库存 <text class="required">*</text></text>
            <input class="form-input" type="number" v-model.number="formData.stock" placeholder="请输入商品库存" />
          </view>
          <view class="form-item">
            <text class="form-label">状态</text>
            <view class="radio-group">
              <label class="radio-item" @tap="formData.status = 1">
                <radio :checked="formData.status === 1" color="#007aff" />
                <text>上架</text>
              </label>
              <label class="radio-item" @tap="formData.status = 0">
                <radio :checked="formData.status === 0" color="#007aff" />
                <text>下架</text>
              </label>
            </view>
          </view>
          <view class="form-item">
            <text class="form-label">商品描述</text>
            <textarea class="form-textarea" v-model="formData.description" placeholder="请输入商品描述" maxlength="200" />
          </view>
        </view>
        <view class="modal-footer">
          <button class="btn-cancel" @tap="closeModal">取消</button>
          <button class="btn-confirm" @tap="handleSubmit">确定</button>
        </view>
      </view>
    </view>
  </view>
</template>

<script>
import request from '@/utils/http'

export default {
  name: 'ProductManagement',
  data() {
    return {
      list: [],
      total: 0,
      page: 1,
      pageSize: 10,
      modalShow: false,
      modalTitle: '新增商品',
      formData: {
        id: null,
        name: '',
        price: 0,
        stock: 0,
        status: 1,
        description: '',
        images: []
      },
      apiBaseUrl: 'http://localhost:5162',
      searchKeyword: '',
      statusOptions: ['全部', '上架', '下架'],
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
    firstImage(item) {
      return Array.isArray(item?.images) && item.images.length > 0 ? item.images[0] : ''
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
        const res = await request({ url: '/api/admin/products', data: params })
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
    previewImages(index) {
      const item = this.list[index]
      if (!Array.isArray(item?.images) || item.images.length === 0) return
      uni.previewImage({
        urls: item.images.map((img) => this.getFullImageUrl(img)),
        current: 0
      })
    },
    handleAdd() {
      this.modalTitle = '新增商品'
      this.formData = { id: null, name: '', price: 0, stock: 0, status: 1, description: '', images: [] }
      this.modalShow = true
    },
    handleEdit(row) {
      this.modalTitle = '编辑商品'
      this.formData = {
        id: row.id,
        name: row.name || '',
        price: row.price || 0,
        stock: row.stock || 0,
        status: row.status ?? 1,
        description: row.description || '',
        images: row.images || []
      }
      this.modalShow = true
    },
    handleDelete(row) {
      uni.showModal({
        title: '提示',
        content: '确定要删除该商品吗？',
        success: async (res) => {
          if (!res.confirm) return
          try {
            await request({ url: `/api/admin/products/${row.id}`, method: 'DELETE' })
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
        uni.showToast({ title: '请输入商品名称', icon: 'none' })
        return
      }
      if (!this.formData.price || this.formData.price <= 0) {
        uni.showToast({ title: '请输入正确价格', icon: 'none' })
        return
      }
      try {
        const data = {
          name: this.formData.name.trim(),
          price: Number(this.formData.price),
          stock: Number(this.formData.stock || 0),
          status: this.formData.status,
          description: this.formData.description,
          images: this.formData.images
        }
        if (this.formData.id) {
          await request({ url: `/api/admin/products/${this.formData.id}`, method: 'PUT', data })
        } else {
          await request({ url: '/api/admin/products', method: 'POST', data })
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
.product-management { padding: 20px; }
.page-bar { display: flex; justify-content: space-between; align-items: center; gap: 16px; margin-bottom: 20px; }
.search-bar { display: flex; gap: 16px; padding: 16px; background: #fff; border-radius: 8px; align-items: center; flex: 1; }
.search-input { display: flex; align-items: center; border: 1px solid #ddd; border-radius: 8px; padding: 0 16px; background: #f5f5f5; flex: 1; }
.search-input .input { height: 40px; font-size: 14px; flex: 1; }
.search-input .search-icon { font-size: 18px; padding-left: 10px; cursor: pointer; color: #666; }
.search-select { width: 120px; }
.search-select .picker { height: 40px; line-height: 40px; text-align: center; background: #f5f5f5; border-radius: 8px; font-size: 14px; border: 1px solid #ddd; }
.btn-reset, .btn-add { font-size: 14px; }
.btn-add { background: #4f46e5; color: #fff; border: none; padding: 0 20px; height: 40px; }
.table-container { background: #fff; border-radius: 8px; overflow: hidden; margin-bottom: 20px; }
.table-header { background: #f5f7fa; }
.table-row { display: flex; align-items: center; border-bottom: 1px solid #eee; }
.table-row.header-row { font-weight: bold; color: #666; }
.table-cell { padding: 16px; text-align: center; font-size: 14px; box-sizing: border-box; }
.table-cell:nth-child(1) { width: 80px; }
.table-cell:nth-child(2) { flex: 1; text-align: left; }
.table-cell:nth-child(3) { width: 100px; }
.table-cell:nth-child(4) { width: 100px; }
.table-cell:nth-child(5) { width: 80px; }
.table-cell:nth-child(6) { width: 80px; }
.table-cell:nth-child(7) { width: 150px; }
.cell-ellipsis { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.product-image { width: 60px; height: 60px; border-radius: 8px; background: #f5f5f5; cursor: pointer; }
.no-image { font-size: 12px; color: #999; }
.status-tag { display: inline-block; padding: 4px 12px; border-radius: 4px; font-size: 12px; }
.status-onshelf { background: #e8f5e9; color: #2e7d32; }
.status-offshelf { background: #fff3e0; color: #e65100; }
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
.form-textarea { width: 100%; min-height: 80px; border: 1px solid #ddd; border-radius: 6px; padding: 12px; font-size: 14px; box-sizing: border-box; }
.radio-group { display: flex; gap: 30px; }
.radio-item { display: flex; align-items: center; gap: 8px; font-size: 14px; }
.modal-footer { display: flex; gap: 12px; padding: 20px; border-top: 1px solid #eee; }
.modal-footer button { flex: 1; font-size: 16px; }
.btn-cancel { background: #f5f5f5; color: #666; }
.btn-confirm { background: #007aff; color: #fff; }
</style>

