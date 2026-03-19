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
          placeholder="请输入分类名称"
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
    
    <!-- 分类列表 -->
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
        <view 
          v-for="(item, index) in list" 
          :key="item.id" 
          class="table-row"
          :class="{ 'dragging': draggingIndex === index }"
          @longpress="startDrag(index)"
          @touchmove="onDragMove"
          @touchend="endDrag"
        >
          <view class="table-cell">{{ (page - 1) * pageSize + index + 1 }}</view>
          <view class="table-cell">{{ item.name }}</view>
          <view class="table-cell">
            <image 
              v-if="item.images && item.images.length > 0" 
              :src="getFullImageUrl(item.images[0])" 
              mode="aspectFill"
              class="category-icon"
              @tap="previewImages(index)"
            />
          </view>
          <view class="table-cell">
            <view class="sort-buttons">
              <button size="mini" class="sort-btn" @tap="moveUp(index)" :disabled="index === 0">↑</button>
              <text class="sort-value">{{ item.sort }}</text>
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
          <!-- 分类名称 -->
          <view class="form-item form-item-compact">
            <text class="form-label">分类名称 <text class="required">*</text></text>
            <input 
              class="form-input form-input-name" 
              v-model="formData.name" 
              placeholder="请输入分类名称"
              maxlength="15"
            />
          </view>

          <!-- 图片上传 -->
          <view class="form-item form-item-compact">
            <text class="form-label">分类图标</text>
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

          <!-- 排序 -->
          <view class="form-item">
            <text class="form-label">排序 <text class="required">*</text></text>
            <input 
              class="form-input form-input-number" 
              type="digit"
              v-model.number="formData.sort" 
              placeholder="请输入正整数"
              maxlength="5"
            />
            <text class="form-tip">数值越小越靠前，必须为正整数</text>
          </view>

          <!-- 状态 -->
          <view class="form-item">
            <text class="form-label">状态 <text class="required">*</text></text>
            <view class="radio-group">
              <label class="radio-item">
                <radio :checked="formData.status === 1" color="#007aff" @tap="formData.status = 1" />
                <text>启用</text>
              </label>
              <label class="radio-item">
                <radio :checked="formData.status === 0" color="#007aff" @tap="formData.status = 0" />
                <text>禁用</text>
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
      modalTitle: '新增分类',
      formData: {
        id: null,
        name: '',
        images: [],
        sort: 1,
        status: 1
      },
      draggingIndex: -1,
      apiBaseUrl: 'http://192.168.1.21:5162',
      // 筛选相关
      searchKeyword: '',
      statusOptions: ['全部', '启用', '禁用'],
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
    this.loadList()
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
      if (path.startsWith('http')) return path
      return this.apiBaseUrl + path
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
        
        const res = await request({ url: '/api/admin/categories', data: params })
        this.list = res?.data?.list || []
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
      // 0=全部，1=启用，2=禁用
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
    
    // 图片上传
    chooseImage() {
      uni.chooseImage({
        count: 9 - this.formData.images.length,
        sizeType: ['compressed'],
        sourceType: ['album', 'camera'],
        success: async (res) => {
          const tempFiles = res.tempFiles
          
          // 上传到后端
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
      // 图片加载失败时的处理
      console.log('图片加载失败:', index)
      // 可以显示一个默认占位图或者提示
      uni.showToast({ 
        title: '图片加载失败，可能是预设路径', 
        icon: 'none',
        duration: 1500
      })
    },
    previewEditImages(index) {
      const images = this.formData.images.map(img => this.getFullImageUrl(img))
      uni.previewImage({
        urls: images,
        current: index
      })
    },
    
    // 列表图片预览
    previewImages(index) {
      const item = this.list[index]
      if (!item.images || item.images.length === 0) return
      
      const images = item.images.map(img => this.getFullImageUrl(img))
      uni.previewImage({
        urls: images,
        current: 0
      })
    },
    
    // 拖拽排序
    startDrag(index) {
      this.draggingIndex = index
      uni.vibrateShort()
    },
    onDragMove(e) {
      if (this.draggingIndex < 0) return
      
      // 简单的上下移动检测
      const touch = e.touches[0]
      // 这里可以根据 touch 的 Y 坐标变化来判断移动方向
      // 简化处理，实际可以使用更复杂的逻辑
    },
    endDrag(e) {
      this.draggingIndex = -1
    },
    async moveUp(index) {
      if (index <= 0) return
      
      const currentItem = this.list[index]
      const prevItem = this.list[index - 1]
      
      // 交换排序值
      const tempSort = currentItem.sort
      currentItem.sort = prevItem.sort
      prevItem.sort = tempSort
      
      // 更新到后端
      await this.updateSort(prevItem.id, prevItem.sort)
      await this.updateSort(currentItem.id, currentItem.sort)
      
      // 重新排序列表
      this.list.sort((a, b) => a.sort - b.sort)
      
      uni.showToast({ title: '已上移', icon: 'success' })
    },
    async moveDown(index) {
      if (index >= this.list.length - 1) return
      
      const currentItem = this.list[index]
      const nextItem = this.list[index + 1]
      
      // 交换排序值
      const tempSort = currentItem.sort
      currentItem.sort = nextItem.sort
      nextItem.sort = tempSort
      
      // 更新到后端
      await this.updateSort(nextItem.id, nextItem.sort)
      await this.updateSort(currentItem.id, currentItem.sort)
      
      // 重新排序列表
      this.list.sort((a, b) => a.sort - b.sort)
      
      uni.showToast({ title: '已下移', icon: 'success' })
    },
    async updateSort(id, sort) {
      try {
        await request({ 
          url: `/api/admin/categories/${id}/sort`, 
          method: 'PUT',
          data: { sort }
        })
      } catch (error) {
        uni.showToast({ title: '排序更新失败', icon: 'none' })
      }
    },
    
    // 增删改
    handleAdd() {
      this.modalTitle = '新增分类'
      this.formData = {
        id: null,
        name: '',
        images: [],
        sort: 1,
        status: 1
      }
      this.modalShow = true
    },
    handleEdit(row) {
      this.modalTitle = '编辑分类'
      this.formData = {
        id: row.id,
        name: row.name,
        images: row.images || [],
        sort: row.sort || 1,
        status: row.status !== undefined ? row.status : 1
      }
      this.modalShow = true
    },
    handleDelete(row) {
      uni.showModal({
        title: '提示',
        content: '确定要删除该分类吗？',
        success: async (res) => {
          if (res.confirm) {
            try {
              await request({ url: `/api/admin/categories/${row.id}`, method: 'DELETE' })
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
        uni.showToast({ title: '请输入分类名称', icon: 'none' })
        return
      }
      
      if (!this.formData.sort || this.formData.sort < 1) {
        uni.showToast({ title: '排序必须为正整数', icon: 'none' })
        return
      }
      
      try {
        const data = {
          name: this.formData.name.trim(),
          images: this.formData.images,
          sort: this.formData.sort,
          status: this.formData.status
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
  
  &.dragging {
    background: #f0f9ff;
    opacity: 0.8;
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
  
  // 分类名称列 - 25%
  &:nth-child(2) {
    width: 25%;
    min-width: 150rpx;
    text-align: left;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
  
  // 图标列 - 12%
  &:nth-child(3) {
    width: 12%;
    min-width: 80rpx;
  }
  
  // 排序列 - 18%
  &:nth-child(4) {
    width: 18%;
    min-width: 120rpx;
  }
  
  // 状态列 - 12%
  &:nth-child(5) {
    width: 12%;
    min-width: 100rpx;
  }
  
  // 操作列 - 25%
  &:nth-child(6) {
    width: 25%;
    min-width: 150rpx;
  }
}

.category-icon {
  width: 60rpx;
  height: 60rpx;
  border-radius: 6rpx;
}

.sort-buttons {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8rpx;
}

.sort-btn {
  width: 44rpx;
  height: 44rpx;
  font-size: 22rpx;
  padding: 0;
  line-height: 44rpx;
  background: #f5f5f5;
  border: 1px solid #ddd;
}

.sort-value {
  min-width: 50rpx;
  text-align: center;
  font-weight: 500;
}

.status-tag {
  display: inline-block;
  padding: 6rpx 14rpx;
  border-radius: 6rpx;
  font-size: 24rpx;
  min-width: 80rpx;
  text-align: center;
  
  &.status-enabled {
    background: #e8f5e9;
    color: #2e7d32;
    font-weight: 500;
  }
  
  &.status-disabled {
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
    font-size: 28rpx;
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
    max-width: 300rpx;
  }
  
  // 数字输入框（排序）
  .form-input-number {
    max-width: 180rpx;
  }
  
  .form-tip {
    display: block;
    font-size: 22rpx;
    color: #999;
    margin-top: 6rpx;
  }
}

// 紧凑表单项
.form-item-compact {
  margin-bottom: 20rpx;
}

// 上传区域
.upload-area {
  display: flex;
  flex-wrap: wrap;
  gap: 12rpx;
  min-height: auto; // 动态高度
  
  // 紧凑版本
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
  flex-shrink: 0; // 不收缩
  
  // 小尺寸版本
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
  
  // 小尺寸版本
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
  font-size: 28rpx;
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
