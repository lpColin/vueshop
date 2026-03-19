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
          placeholder="请输入店铺名称"
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
    
    <!-- 店铺列表 -->
    <view class="table-container">
      <view class="table-header">
        <view class="table-row header-row">
          <view class="table-cell">序号</view>
          <view class="table-cell">店铺名称</view>
          <view class="table-cell">头像</view>
          <view class="table-cell">评分</view>
          <view class="table-cell">联系电话</view>
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
            <image 
              v-if="item.avatar" 
              :src="getFullImageUrl(item.avatar)" 
              mode="aspectFill"
              class="shop-avatar"
              @tap="previewAvatar(index)"
            />
            <text v-else class="no-image">无图</text>
          </view>
          <view class="table-cell">⭐{{ Number(item.rating || 5).toFixed(1) }}</view>
          <view class="table-cell">{{ item.phone || '-' }}</view>
          <view class="table-cell">
            <text :class="['status-tag', item.status === 1 ? 'status-open' : 'status-closed']">
              {{ item.status === 1 ? '营业' : '打烊' }}
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
          <!-- 店铺名称 -->
          <view class="form-item form-item-compact">
            <text class="form-label">店铺名称 <text class="required">*</text></text>
            <input 
              class="form-input form-input-name" 
              v-model="formData.name" 
              placeholder="请输入店铺名称"
              maxlength="30"
            />
          </view>

          <!-- 店铺描述 -->
          <view class="form-item form-item-compact">
            <text class="form-label">店铺描述</text>
            <textarea 
              class="form-textarea" 
              v-model="formData.description" 
              placeholder="请输入店铺描述"
              maxlength="200"
            />
          </view>

          <!-- 店主选择 -->
          <view class="form-item form-item-compact">
            <text class="form-label">店主 <text class="required">*</text></text>
            <picker 
              :range="userList" 
              :value="userIndex"
              range-key="nickname"
              @change="onUserChange"
            >
              <view class="form-picker">
                <text>{{ userList[userIndex]?.nickname || '请选择店主' }}</text>
              </view>
            </picker>
          </view>

          <!-- 评分 -->
          <view class="form-item form-item-compact">
            <text class="form-label">店铺评分</text>
            <input 
              class="form-input form-input-number" 
              type="digit"
              v-model.number="formData.rating" 
              placeholder="1-5 分"
              maxlength="3"
            />
          </view>

          <!-- 联系电话 -->
          <view class="form-item form-item-compact">
            <text class="form-label">联系电话</text>
            <input 
              class="form-input form-input-name" 
              v-model="formData.phone" 
              placeholder="请输入联系电话"
              maxlength="20"
            />
          </view>

          <!-- 营业时间 -->
          <view class="form-item form-item-compact">
            <text class="form-label">营业时间</text>
            <input 
              class="form-input form-input-name" 
              v-model="formData.businessHours" 
              placeholder="例如：09:00-22:00"
              maxlength="30"
            />
          </view>

          <!-- 店铺地址 -->
          <view class="form-item form-item-compact">
            <text class="form-label">店铺地址</text>
            <input 
              class="form-input form-input-name" 
              v-model="formData.address" 
              placeholder="请输入详细地址"
              maxlength="100"
            />
          </view>

          <!-- 头像上传 -->
          <view class="form-item form-item-compact">
            <text class="form-label">店铺头像</text>
            
            <!-- 统一使用 uni.chooseImage（参照分类管理页面） -->
            <view class="upload-area upload-area-compact">
              <view 
                v-if="formData.avatar" 
                class="image-item image-item-small"
              >
                <image 
                  :src="getFullImageUrl(formData.avatar)" 
                  mode="aspectFill" 
                  class="uploaded-image" 
                  @tap="previewAvatarEdit"
                />
                <text class="image-delete" @tap="removeAvatar">×</text>
              </view>
              <view class="upload-btn upload-btn-small" @tap="chooseAvatar">
                <text class="upload-icon">+</text>
                <text class="upload-text">上传</text>
              </view>
            </view>
            
            <text class="form-tip">点击上传头像</text>
          </view>

          <!-- 状态 -->
          <view class="form-item form-item-compact">
            <text class="form-label">状态 <text class="required">*</text></text>
            <view class="radio-group">
              <label class="radio-item">
                <radio :checked="formData.status === 1" color="#007aff" @tap="formData.status = 1" />
                <text>营业</text>
              </label>
              <label class="radio-item">
                <radio :checked="formData.status === 0" color="#007aff" @tap="formData.status = 0" />
                <text>打烊</text>
              </label>
            </view>
          </view>
        </view>

        <view class="modal-footer">
          <button class="btn-cancel" type="button" @tap="closeModal">取消</button>
          <button class="btn-confirm" type="button" @tap="handleSubmit">确定</button>
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
      modalTitle: '新增店铺',
      formData: {
        id: null,
        name: '',
        description: '',
        ownerId: null,
        rating: 5,
        phone: '',
        businessHours: '',
        address: '',
        avatar: '',
        logo: '',
        reviewCount: 0,
        status: 1
      },
      apiBaseUrl: 'http://192.168.1.21:5162',
      userList: [],
      userIndex: 0,
      // 筛选相关
      searchKeyword: '',
      statusOptions: ['全部', '营业', '打烊'],
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
    console.log('[onShow] 页面显示')
    if (!this.ensureAdminAccess()) return
    console.log('[onShow] 开始加载数据')
    this.loadUsers().then(() => {
      console.log('[onShow] 用户列表加载完成，数量:', this.userList.length)
      this.loadList()
    }).catch(err => {
      console.error('[onShow] 加载失败', err)
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
      if (path.startsWith('/')) {
        return 'http://localhost:5162' + path
      }
      return 'http://localhost:5162/' + path
    },
    async loadUsers() {
      try {
        console.log('[loadUsers] 开始加载用户列表')
        const res = await request({ url: '/api/admin/users?page=1&pageSize=100' })
        const allUsers = res?.data?.list || []
        console.log('[loadUsers] 所有用户数量:', allUsers.length)
        
        // 筛选出商家用户
        const merchantUsers = allUsers.filter(u => u.role === 'merchant')
        console.log('[loadUsers] 商家用户数量:', merchantUsers.length)
        
        this.userList = merchantUsers.length > 0 ? merchantUsers : allUsers
        console.log('[loadUsers] 最终用户列表数量:', this.userList.length)
        console.log('[loadUsers] 用户列表:', this.userList)
        
        if (this.userList.length > 0) {
          this.userIndex = 0
        }
      } catch (error) {
        console.error('[loadUsers] 加载失败', error)
        this.userList = []
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
        
        const res = await request({ url: '/api/admin/shops', data: params })
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
    
    // 店主选择
    onUserChange(e) {
      this.userIndex = e.detail.value
      if (this.formData.id) {
        this.formData.ownerId = this.userList[this.userIndex]?.id
      }
    },
    
    // 头像预览
    previewAvatar(index) {
      const item = this.list[index]
      if (!item.avatar) return
      uni.previewImage({
        urls: [this.getFullImageUrl(item.avatar)],
        current: 0
      })
    },
    previewAvatarEdit() {
      if (!this.formData.avatar) return
      uni.previewImage({
        urls: [this.getFullImageUrl(this.formData.avatar)],
        current: 0
      })
    },
    
    // 头像上传（统一使用 uni.chooseImage，参照分类管理页面）
    chooseAvatar() {
      console.log('===== 点击上传按钮 =====')
      console.log('[当前 avatar]', this.formData.avatar)
      
      uni.showLoading({ title: '选择图片...' })
      
      // 使用 uni.chooseImage（所有环境通用）
      uni.chooseImage({
        count: 1,
        sizeType: ['compressed'],
        sourceType: ['album', 'camera'],
        success: async (res) => {
          console.log('[chooseImage 成功]', res)
          uni.hideLoading()
          
          if (!res.tempFiles || res.tempFiles.length === 0) {
            console.error('[没有选择文件]')
            uni.showToast({ title: '没有选择文件', icon: 'none' })
            return
          }
          
          const filePath = res.tempFiles[0].path
          console.log('[文件路径]', filePath)
          console.log('[文件大小]', res.tempFiles[0].size, 'bytes')
          
          uni.showLoading({ title: '上传中...' })
          try {
            await this.uploadAvatar(filePath)
          } catch (error) {
            console.error('[上传异常]', error)
          } finally {
            uni.hideLoading()
          }
        },
        fail: (err) => {
          console.error('[chooseImage 失败]', err)
          uni.hideLoading()
          
          // 判断是否是用户取消
          if (err.errMsg && err.errMsg.includes('cancel')) {
            console.log('[用户取消选择]')
          } else {
            console.error('[选择图片错误]', err)
            uni.showToast({ title: '选择失败：' + (err.errMsg || '未知错误'), icon: 'none', duration: 3000 })
          }
        }
      })
    },
    
    // 头像上传（统一使用 uni.chooseImage，参照分类管理页面）
    chooseAvatar() {
      console.log('===== 点击上传按钮 =====')
      console.log('[当前 avatar]', this.formData.avatar)
      
      uni.showLoading({ title: '选择图片...' })
      
      // 使用 uni.chooseImage
      uni.chooseImage({
        count: 1,
        sizeType: ['compressed'],
        sourceType: ['album', 'camera'],
        success: async (res) => {
          console.log('[chooseImage 成功]', res)
          uni.hideLoading()
          
          if (!res.tempFiles || res.tempFiles.length === 0) {
            console.error('[没有选择文件]')
            uni.showToast({ title: '没有选择文件', icon: 'none' })
            return
          }
          
          const filePath = res.tempFiles[0].path
          console.log('[文件路径]', filePath)
          console.log('[文件大小]', res.tempFiles[0].size, 'bytes')
          
          uni.showLoading({ title: '上传中...' })
          try {
            await this.uploadAvatar(filePath)
          } catch (error) {
            console.error('[上传异常]', error)
          } finally {
            uni.hideLoading()
          }
        },
        fail: (err) => {
          console.error('[chooseImage 失败]', err)
          uni.hideLoading()
          
          // 判断是否是用户取消
          if (err.errMsg && err.errMsg.includes('cancel')) {
            console.log('[用户取消选择]')
          } else {
            console.error('[选择图片错误]', err)
            uni.showToast({ title: '选择失败：' + (err.errMsg || '未知错误'), icon: 'none', duration: 3000 })
          }
        }
      })
    },
    async uploadAvatar(filePath) {
      return new Promise((resolve, reject) => {
        console.log('[开始上传] URL:', this.apiBaseUrl + '/api/admin/categories/upload')
        console.log('[文件路径]', filePath)
        console.log('[Token]', uni.getStorageSync('token'))
        
        // H5 环境可能需要使用完整的 URL
        const uploadUrl = this.apiBaseUrl + '/api/admin/categories/upload'
        
        uni.uploadFile({
          url: uploadUrl,
          filePath: filePath,
          name: 'files',
          fileName: 'upload_' + Date.now() + '.jpg',
          header: {
            'Authorization': 'Bearer ' + uni.getStorageSync('token')
          },
          formData: {
            folder: 'categories'
          },
          success: (res) => {
            console.log('===== 上传成功 =====')
            console.log('[响应状态码]', res.statusCode)
            console.log('[响应原始数据]', res.data)
            console.log('[响应数据类型]', typeof res.data)
            
            uni.hideLoading()
            
            // 检查状态码
            if (res.statusCode !== 200) {
              console.error('[HTTP 错误]', res.statusCode)
              uni.showToast({ title: '上传失败：HTTP ' + res.statusCode, icon: 'none' })
              reject(new Error('HTTP ' + res.statusCode))
              return
            }
            
            // 解析响应数据
            let data
            try {
              // H5 环境下 res.data 可能已经是对象
              if (typeof res.data === 'string') {
                console.log('[解析 JSON 字符串]')
                data = JSON.parse(res.data)
              } else if (typeof res.data === 'object') {
                console.log('[直接使用对象]')
                data = res.data
              } else {
                console.log('[尝试解析]', res.data)
                data = JSON.parse(res.data)
              }
              console.log('[解析后的数据]', data)
            } catch (e) {
              console.error('[JSON 解析失败]', e)
              console.error('[原始数据]', res.data)
              uni.showToast({ title: '响应解析失败：' + e.message, icon: 'none' })
              reject(e)
              return
            }
            
            // 检查响应内容
            if (data.success && data.data && Array.isArray(data.data) && data.data.length > 0) {
              this.formData.avatar = data.data[0]
              console.log('[头像已设置]', this.formData.avatar)
              uni.showToast({ title: '上传成功', icon: 'success' })
              resolve()
            } else {
              const msg = data.message || data.msg || '上传失败：未知错误'
              console.error('[业务错误]', msg, data)
              uni.showToast({ title: msg, icon: 'none', duration: 3000 })
              reject(new Error(msg))
            }
          },
          fail: (err) => {
            console.error('===== 上传失败 =====')
            console.error('[错误信息]', err)
            console.error('[错误码]', err.errCode)
            console.error('[错误消息]', err.errMsg)
            
            uni.hideLoading()
            
            let errorMsg = '上传失败'
            if (err.errMsg) {
              if (err.errMsg.includes('timeout')) {
                errorMsg = '上传超时，请重试'
              } else if (err.errMsg.includes('fail')) {
                errorMsg = '网络错误，请检查连接'
              } else {
                errorMsg = err.errMsg
              }
            }
            
            uni.showToast({ title: errorMsg, icon: 'none', duration: 3000 })
            reject(err)
          }
        })
      })
    },
    removeAvatar() {
      this.formData.avatar = ''
    },
    
    // 增删改
    handleAdd() {
      this.modalTitle = '新增店铺'
      this.formData = {
        id: null,
        name: '',
        description: '',
        ownerId: this.userList[0]?.id || null,
        rating: 5,
        phone: '',
        businessHours: '09:00-22:00',
        address: '',
        avatar: '',
        logo: '',
        reviewCount: 0,
        status: 1
      }
      this.userIndex = 0
      this.modalShow = true
    },
    handleEdit(row) {
      console.log('[handleEdit] 编辑行数据:', row)
      console.log('[handleEdit] row.ownerId:', row.ownerId, '类型:', typeof row.ownerId)
      console.log('[handleEdit] 当前 userList:', this.userList)
      console.log('[handleEdit] userList 长度:', this.userList.length)
      
      this.modalTitle = '编辑店铺'
      this.formData = {
        id: row.id,
        name: row.name,
        description: row.description || '',
        ownerId: row.ownerId,
        rating: row.rating || 5,
        phone: row.phone || '',
        businessHours: row.businessHours || '',
        address: row.address || '',
        avatar: row.avatar || '',
        logo: row.logo || '',
        reviewCount: row.reviewCount || 0,
        status: row.status !== undefined ? row.status : 1
      }
      
      console.log('[handleEdit] formData:', this.formData)
      console.log('[handleEdit] formData.ownerId:', this.formData.ownerId)
      
      // 设置店主选择器索引
      // 注意：需要确保类型一致（number vs string）
      const ownerIdNum = Number(row.ownerId)
      this.userIndex = this.userList.findIndex(u => {
        const userIdNum = Number(u.id)
        const match = userIdNum === ownerIdNum
        console.log('[handleEdit] 比较用户 ID:', u.id, '==', row.ownerId, '?', match)
        return match
      })
      
      console.log('[handleEdit] userIndex:', this.userIndex)
      
      if (this.userIndex < 0) {
        console.warn('[handleEdit] 未找到匹配的店主（ownerId=' + row.ownerId + '），使用第一个')
        console.warn('[handleEdit] userList:', this.userList)
        this.userIndex = 0
      }
      
      console.log('[handleEdit] 最终 userIndex:', this.userIndex)
      console.log('[handleEdit] 选中的店主:', this.userList[this.userIndex])
      
      this.modalShow = true
    },
    handleDelete(row) {
      uni.showModal({
        title: '提示',
        content: '确定要删除该店铺吗？',
        success: async (res) => {
          if (res.confirm) {
            try {
              await request({ url: `/api/admin/shops/${row.id}`, method: 'DELETE' })
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
      console.log('===== 点击确定按钮 =====')
      console.log('[当前 formData]', this.formData)
      console.log('[是否是编辑]', !!this.formData.id)
      
      // 验证店铺名称（必填）
      if (!this.formData.name || !this.formData.name.trim()) {
        console.warn('[验证失败] 店铺名称为空')
        uni.showToast({ title: '请输入店铺名称', icon: 'none' })
        return
      }
      
      // 编辑模式下验证店主 ID（新增店铺时店主 ID 可选，为自增字段）
      // 注意：ownerId 为 0 也表示没有设置店主
      if (this.formData.id && (!this.formData.ownerId || this.formData.ownerId <= 0)) {
        console.warn('[验证失败] 编辑模式下店主 ID 不能为空')
        console.warn('[当前 ownerId]', this.formData.ownerId)
        uni.showToast({ title: '请选择店主', icon: 'none' })
        return
      }
      
      console.log('[验证通过]')
      
      try {
        // 构建提交数据
        const data = {
          name: this.formData.name.trim(),
          description: this.formData.description,
          rating: this.formData.rating,
          phone: this.formData.phone,
          businessHours: this.formData.businessHours,
          address: this.formData.address,
          avatar: this.formData.avatar,
          logo: this.formData.logo,
          reviewCount: this.formData.reviewCount,
          status: this.formData.status
        }
        
        // 编辑模式下必须传递 ownerId，新增模式下可选
        if (this.formData.ownerId) {
          data.ownerId = this.formData.ownerId
        }
        
        console.log('[提交数据]', data)
        
        if (this.formData.id) {
          console.log('[更新店铺] ID:', this.formData.id)
          await request({ url: `/api/admin/shops/${this.formData.id}`, method: 'PUT', data })
        } else {
          console.log('[新增店铺]')
          await request({ url: '/api/admin/shops', method: 'POST', data })
        }
        
        console.log('[保存成功]')
        uni.showToast({ title: '保存成功', icon: 'success' })
        this.closeModal()
        this.loadList()
      } catch (error) {
        console.error('[保存失败]', error)
        console.error('[错误信息]', error.message)
        console.error('[错误响应]', error.response)
        uni.showToast({ title: error.message || '保存失败', icon: 'none', duration: 3000 })
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
  
  // 店铺名称列 - 25%
  &:nth-child(2) {
    width: 25%;
    min-width: 150rpx;
    text-align: left;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
  
  // 头像列 - 12%
  &:nth-child(3) {
    width: 12%;
    min-width: 100rpx;
  }
  
  // 评分列 - 10%
  &:nth-child(4) {
    width: 10%;
    min-width: 100rpx;
  }
  
  // 联系电话列 - 15%
  &:nth-child(5) {
    width: 15%;
    min-width: 150rpx;
  }
  
  // 状态列 - 10%
  &:nth-child(6) {
    width: 10%;
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

.shop-avatar {
  width: 60rpx;
  height: 60rpx;
  border-radius: 50%;
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
  min-width: 70rpx;
  text-align: center;
  
  &.status-open {
    background: #e8f5e9;
    color: #2e7d32;
    font-weight: 500;
  }
  
  &.status-closed {
    background: #fff3e0;
    color: #e65100;
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
