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
          placeholder="请输入商品名称"
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
    
    <!-- 商品列表 -->
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
        <view 
          v-for="(item, index) in list" 
          :key="item.id" 
          class="table-row"
        >
          <view class="table-cell">{{ (page - 1) * pageSize + index + 1 }}</view>
          <view class="table-cell cell-text-ellipsis">{{ item.name }}</view>
          <view class="table-cell">
            <!-- 调试显示 -->
            <text v-if="!item.images" style="font-size: 20rpx; color: #999;">images: null</text>
            <text v-else-if="!Array.isArray(item.images)" style="font-size: 20rpx; color: #999;">images: {{typeof item.images}}</text>
            <text v-else-if="item.images.length === 0" style="font-size: 20rpx; color: #999;">images: []</text>
            
            <image 
              v-if="item.images && Array.isArray(item.images) && item.images.length > 0" 
              :src="getFullImageUrl(item.images[0])" 
              mode="aspectFill"
              class="product-image"
              @tap="previewImages(index)"
            />
            <text v-else class="no-image">无图</text>
          </view>
          <view class="table-cell">¥{{ Number(item.price || 0).toFixed(2) }}</view>
          <view class="table-cell">{{ item.stock }}</view>
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

    <!-- 分页 -->
    <view class="pagination">
      <button size="mini" @tap="prevPage" :disabled="page <= 1">上一页</button>
      <text class="page-info">第 {{ page }} 页 / 共 {{ Math.ceil(total / pageSize) }} 页</text>
      <button size="mini" @tap="nextPage" :disabled="page >= Math.ceil(total / pageSize)">下一页</button>
    </view>

    <!-- 编辑弹窗 -->
    <view class="modal-mask" v-if="modalShow" @tap="closeModal">
      <view class="modal-content" @tap.stop>
        <view class="modal-header">
          <text class="modal-title">{{ modalTitle }}</text>
          <text class="modal-close" @tap="closeModal">×</text>
        </view>
        
        <view class="modal-body">
          <!-- 商品名称 -->
          <view class="form-item form-item-compact">
            <text class="form-label">商品名称 <text class="required">*</text></text>
            <input 
              class="form-input form-input-name" 
              v-model="formData.name" 
              placeholder="请输入商品名称"
              maxlength="30"
            />
          </view>

          <!-- 商品描述 -->
          <view class="form-item form-item-compact">
            <text class="form-label">商品描述</text>
            <textarea 
              class="form-textarea" 
              v-model="formData.description" 
              placeholder="请输入商品描述"
              maxlength="200"
            />
          </view>

          <!-- 分类选择 -->
          <view class="form-item form-item-compact">
            <text class="form-label">所属分类 <text class="required">*</text></text>
            <picker 
              :range="categoryList" 
              :value="categoryIndex"
              range-key="name"
              @change="onCategoryChange"
            >
              <view class="form-picker">
                <text>{{ categoryList[categoryIndex]?.name || '请选择分类' }}</text>
              </view>
            </picker>
          </view>

          <!-- 价格 -->
          <view class="form-item form-item-compact">
            <text class="form-label">价格 <text class="required">*</text></text>
            <input 
              class="form-input form-input-number" 
              type="digit"
              v-model.number="formData.price" 
              placeholder="请输入价格"
              maxlength="10"
            />
          </view>

          <!-- 库存 -->
          <view class="form-item form-item-compact">
            <text class="form-label">库存 <text class="required">*</text></text>
            <input 
              class="form-input form-input-number" 
              type="number"
              v-model.number="formData.stock" 
              placeholder="请输入库存"
              maxlength="6"
            />
          </view>

          <!-- 图片上传 -->
          <view class="form-item form-item-compact">
            <text class="form-label">商品图片</text>
            <view class="upload-area upload-area-compact">
              <view 
                v-for="(img, idx) in formData.images" 
                :key="idx" 
                class="image-item image-item-small"
              >
                <image 
                  :src="getFullImageUrl(img)" 
                  mode="aspectFill" 
                  class="uploaded-image" 
                  @tap="previewEditImages(idx)"
                  @error="handleImageError(idx)"
                />
                <text class="image-delete" @tap="removeImage(idx)">×</text>
              </view>
              <view class="upload-btn upload-btn-small" @tap="chooseImage">
                <text class="upload-icon">+</text>
                <text class="upload-text">上传</text>
              </view>
            </view>
            <text class="form-tip">支持多张图片</text>
          </view>

          <!-- 状态 -->
          <view class="form-item form-item-compact">
            <text class="form-label">状态 <text class="required">*</text></text>
            <view class="radio-group">
              <label class="radio-item">
                <radio :checked="formData.status === 1" color="#007aff" @tap="formData.status = 1" />
                <text>上架</text>
              </label>
              <label class="radio-item">
                <radio :checked="formData.status === 0" color="#007aff" @tap="formData.status = 0" />
                <text>下架</text>
              </label>
            </view>
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
import { getUserInfo } from '@/utils/auth'

export default {
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
        description: '',
        categoryId: null,
        price: 0,
        stock: 0,
        images: [],
        status: 1
      },
      apiBaseUrl: 'http://192.168.1.21:5162',
      categoryList: [],
      categoryIndex: 0,
      // 筛选相关
      searchKeyword: '',
      statusOptions: ['全部', '上架', '下架'],
      statusIndex: 0,
      searchStatus: null
    }
  },
  computed: {
    Math() {
      return Math
    }
  },
  onShow() {
    if (!this.ensureAdminAccess()) return
    this.loadCategories().then(() => {
      this.loadList()
    })
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
      // 使用相对路径，让浏览器自动处理
      if (path.startsWith('/')) {
        return 'http://localhost:5162' + path
      }
      return 'http://localhost:5162/' + path
    },
    async loadCategories() {
      try {
        const res = await request({ url: '/api/admin/categories?status=1' })
        this.categoryList = res?.data?.list || []
        if (this.categoryList.length > 0) {
          this.categoryIndex = 0
        }
      } catch (error) {
        this.categoryList = []
      }
    },
    async loadList() {
      try {
        const params = {
          page: this.page,
          pageSize: this.pageSize
        }
        
        // 添加筛选参数
        if (this.searchKeyword) {
          params.keyword = this.searchKeyword
        }
        if (this.searchStatus !== null && this.searchStatus !== undefined) {
          params.status = this.searchStatus
        }
        
        const res = await request({ url: '/api/admin/products', data: params })
        const rawList = res?.data?.list || []
        
        // 映射分类名称
        const categoryMap = this.categoryList.reduce((map, item) => {
          map[item.id] = item.name
          return map
        }, {})
        
        this.list = rawList.map(item => ({
          ...item,
          categoryName: categoryMap[item.categoryId] || '未分类'
        }))
        this.total = res?.data?.total || 0
      } catch (error) {
        uni.showToast({ title: error.message || '加载失败', icon: 'none' })
      }
    },
    
    // 筛选方法
    handleSearch() {
      this.page = 1
      this.loadList()
    },
    onStatusChange(e) {
      this.statusIndex = e.detail.value
      if (this.statusIndex === 0) {
        this.searchStatus = null
      } else if (this.statusIndex === 1) {
        this.searchStatus = 1
      } else {
        this.searchStatus = 0
      }
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
    
    // 分类选择
    onCategoryChange(e) {
      this.categoryIndex = e.detail.value
      if (this.formData.id) {
        // 编辑模式
        this.formData.categoryId = this.categoryList[this.categoryIndex]?.id
      }
    },
    
    // 图片上传
    chooseImage() {
      uni.chooseImage({
        count: 9 - this.formData.images.length,
        sizeType: ['compressed'],
        sourceType: ['album', 'camera'],
        success: async (res) => {
          const tempFiles = res.tempFiles
          for (let i = 0; i < tempFiles.length; i++) {
            const filePath = tempFiles[i].path
            await this.uploadFile(filePath)
          }
          uni.showToast({ title: '上传成功', icon: 'success' })
        }
      })
    },
    async uploadFile(filePath) {
      return new Promise((resolve, reject) => {
        uni.uploadFile({
          url: this.apiBaseUrl + '/api/admin/categories/upload',
          filePath: filePath,
          name: 'files',
          header: {
            'Authorization': 'Bearer ' + uni.getStorageSync('token')
          },
          success: (res) => {
            const data = JSON.parse(res.data)
            if (data.success && data.data && data.data.length > 0) {
              this.formData.images.push(...data.data)
              resolve()
            } else {
              uni.showToast({ title: data.message || '上传失败', icon: 'none' })
              reject()
            }
          },
          fail: (err) => {
            uni.showToast({ title: '上传失败', icon: 'none' })
            reject()
          }
        })
      })
    },
    removeImage(index) {
      this.formData.images.splice(index, 1)
    },
    handleImageError(index) {
      console.log('图片加载失败:', index)
    },
    previewEditImages(index) {
      const images = this.formData.images.map(img => this.getFullImageUrl(img))
      if (images.length > 0) {
        uni.previewImage({ urls: images, current: index })
      }
    },
    previewImages(index) {
      const item = this.list[index]
      if (!item.images || item.images.length === 0) return
      const images = item.images.map(img => this.getFullImageUrl(img))
      uni.previewImage({ urls: images, current: 0 })
    },
    
    // 增删改
    handleAdd() {
      this.modalTitle = '新增商品'
      this.formData = {
        id: null,
        name: '',
        description: '',
        categoryId: this.categoryList[0]?.id || null,
        price: 0,
        stock: 0,
        images: [],
        status: 1
      }
      this.categoryIndex = 0
      this.modalShow = true
    },
    handleEdit(row) {
      this.modalTitle = '编辑商品'
      this.formData = {
        id: row.id,
        name: row.name,
        description: row.description || '',
        categoryId: row.categoryId,
        price: row.price,
        stock: row.stock,
        images: row.images || [],
        status: row.status !== undefined ? row.status : 1
      }
      // 设置分类选择器索引
      this.categoryIndex = this.categoryList.findIndex(c => c.id === row.categoryId)
      if (this.categoryIndex < 0) this.categoryIndex = 0
      this.modalShow = true
    },
    handleDelete(row) {
      uni.showModal({
        title: '提示',
        content: '确定要删除该商品吗？',
        success: async (res) => {
          if (res.confirm) {
            try {
              await request({ url: `/api/admin/products/${row.id}`, method: 'DELETE' })
              uni.showToast({ title: '删除成功', icon: 'success' })
              this.loadList()
            } catch (error) {
              uni.showToast({ title: error.message || '删除失败', icon: 'none' })
            }
          }
        }
      })
    },
    closeModal() {
      this.modalShow = false
    },
    async handleSubmit() {
      // 验证
      if (!this.formData.name || !this.formData.name.trim()) {
        uni.showToast({ title: '请输入商品名称', icon: 'none' })
        return
      }
      
      if (!this.formData.categoryId) {
        uni.showToast({ title: '请选择分类', icon: 'none' })
        return
      }
      
      if (!this.formData.price || this.formData.price <= 0) {
        uni.showToast({ title: '价格必须大于 0', icon: 'none' })
        return
      }
      
      if (!this.formData.stock || this.formData.stock < 0) {
        uni.showToast({ title: '库存不能为负数', icon: 'none' })
        return
      }
      
      try {
        const data = {
          name: this.formData.name.trim(),
          description: this.formData.description,
          categoryId: this.formData.categoryId,
          price: this.formData.price,
          stock: this.formData.stock,
          images: this.formData.images,
          status: this.formData.status
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
      if (this.page > 1) {
        this.page--
        this.loadList()
      }
    },
    nextPage() {
      const maxPage = Math.ceil(this.total / this.pageSize)
      if (this.page < maxPage) {
        this.page++
        this.loadList()
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

// 表格样式
.table-container {
  background: #fff;
  border-radius: 8rpx;
  overflow: hidden;
  margin-bottom: 20rpx;
}

.table-header {
  background: #f5f7fa;
}

.table-row {
  display: flex;
  align-items: center;
  border-bottom: 1px solid #eee;
  
  &.header-row {
    font-weight: bold;
    color: #666;
  }
}

.table-cell {
  padding: 16rpx;
  text-align: center;
  font-size: 26rpx;
  
  // 序号列 - 8%
  &:nth-child(1) {
    width: 8%;
    min-width: 60rpx;
  }
  
  // 商品名称列 - 25%
  &:nth-child(2) {
    width: 25%;
    min-width: 150rpx;
    text-align: left;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
  
  // 图片列 - 12%
  &:nth-child(3) {
    width: 12%;
    min-width: 80rpx;
  }
  
  // 价格列 - 12%
  &:nth-child(4) {
    width: 12%;
    min-width: 100rpx;
  }
  
  // 库存列 - 10%
  &:nth-child(5) {
    width: 10%;
    min-width: 80rpx;
  }
  
  // 状态列 - 12%
  &:nth-child(6) {
    width: 12%;
    min-width: 100rpx;
  }
  
  // 操作列 - 25%
  &:nth-child(7) {
    width: 25%;
    min-width: 150rpx;
  }
}

.cell-text-ellipsis {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.product-image {
  width: 60rpx;
  height: 60rpx;
  border-radius: 6rpx;
  background: #f5f5f5;
}

.no-image {
  font-size: 22rpx;
  color: #999;
}

.status-tag {
  display: inline-block;
  padding: 6rpx 14rpx;
  border-radius: 6rpx;
  font-size: 24rpx;
  min-width: 80rpx;
  text-align: center;
  
  &.status-onshelf {
    background: #e8f5e9;
    color: #2e7d32;
    font-weight: 500;
  }
  
  &.status-offshelf {
    background: #ffebee;
    color: #c62828;
    font-weight: 500;
  }
}

// 操作按钮容器
.action-buttons {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: center;
  gap: 12rpx;
  width: 100%;
}

.btn-edit, .btn-delete {
  font-size: 22rpx;
  padding: 0 12rpx;
  height: 48rpx;
  line-height: 48rpx;
  margin: 0;
  border-radius: 6rpx;
  flex: 0 0 auto;
}

.btn-edit {
  background: #2196f3;
  color: #fff;
}

.btn-delete {
  background: #f44336;
  color: #fff;
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
  max-width: 800rpx;
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
    color: #333;
    margin-bottom: 12rpx;
    
    .required {
      color: #f44336;
    }
  }
  
  .form-input {
    width: 100%;
    height: 64rpx;
    border: 1px solid #ddd;
    border-radius: 6rpx;
    padding: 0 12rpx;
    font-size: 26rpx;
  }
  
  // 名称输入框
  .form-input-name {
    max-width: 400rpx;
  }
  
  // 数字输入框
  .form-input-number {
    max-width: 200rpx;
  }
  
  .form-textarea {
    width: 100%;
    min-height: 120rpx;
    border: 1px solid #ddd;
    border-radius: 6rpx;
    padding: 12rpx;
    font-size: 26rpx;
    line-height: 1.6;
  }
  
  .form-picker {
    height: 64rpx;
    line-height: 64rpx;
    background: #f5f5f5;
    border-radius: 6rpx;
    padding: 0 16rpx;
    font-size: 26rpx;
    border: 1px solid #ddd;
  }
  
  .form-tip {
    display: block;
    font-size: 22rpx;
    color: #999;
    margin-top: 6rpx;
  }
}

.form-item-compact {
  margin-bottom: 20rpx;
}

// 上传区域
.upload-area {
  display: flex;
  flex-wrap: wrap;
  gap: 12rpx;
  min-height: auto;
  
  &.upload-area-compact {
    gap: 10rpx;
  }
}

.image-item {
  position: relative;
  width: 120rpx;
  height: 120rpx;
  background: #f5f5f5;
  border-radius: 6rpx;
  overflow: hidden;
  flex-shrink: 0;
  
  &.image-item-small {
    width: 100rpx;
    height: 100rpx;
  }
  
  .uploaded-image {
    width: 100%;
    height: 100%;
    border-radius: 6rpx;
    background: #e0e0e0;
  }
  
  .image-delete {
    position: absolute;
    top: -8rpx;
    right: -8rpx;
    width: 36rpx;
    height: 36rpx;
    background: rgba(244, 67, 54, 0.9);
    color: #fff;
    border-radius: 50%;
    text-align: center;
    line-height: 32rpx;
    font-size: 24rpx;
    z-index: 10;
  }
}

.upload-btn {
  width: 120rpx;
  height: 120rpx;
  border: 2rpx dashed #ddd;
  border-radius: 6rpx;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: #fafafa;
  flex-shrink: 0;
  
  &.upload-btn-small {
    width: 100rpx;
    height: 100rpx;
    
    .upload-icon {
      font-size: 40rpx;
    }
    
    .upload-text {
      font-size: 20rpx;
    }
  }
  
  .upload-icon {
    font-size: 44rpx;
    color: #999;
    line-height: 1;
  }
  
  .upload-text {
    font-size: 22rpx;
    color: #999;
    margin-top: 6rpx;
  }
}

// 单选框
.radio-group {
  display: flex;
  gap: 40rpx;
}

.radio-item {
  display: flex;
  align-items: center;
  gap: 10rpx;
  font-size: 26rpx;
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
    background: #007aff;
    color: #fff;
  }
}
</style>
