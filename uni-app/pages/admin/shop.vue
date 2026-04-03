<template>
  <view class="page">
    <!-- 椤甸潰澶撮儴 -->
    <view class="page-header">
      <view class="header-left">
        <text class="page-title">搴楅摵绠＄悊</text>
        <text class="page-subtitle">绠＄悊鎵€鏈夊叆椹诲晢瀹跺簵閾轰俊鎭?/text>
      </view>
      <view class="header-right">
        <button class="btn-back" @tap="goAdminHome">
          <text class="btn-icon">鈫?/text>
          杩斿洖鐪嬫澘
        </button>
        <button class="btn-user" @tap="goUserCenter">
          <text class="btn-icon">馃懁</text>
          涓汉涓績
        </button>
      </view>
    </view>

    <!-- 缁熻鍗＄墖 -->
    <view class="stats-row">
      <view class="stat-card stat-total">
        <view class="stat-icon">馃彧</view>
        <view class="stat-info">
          <text class="stat-value">{{ total }}</text>
          <text class="stat-label">搴楅摵鎬绘暟</text>
        </view>
      </view>
      <view class="stat-card stat-open">
        <view class="stat-icon">鉁?/view>
        <view class="stat-info">
          <text class="stat-value">{{ openCount }}</text>
          <text class="stat-label">钀ヤ笟涓?/text>
        </view>
      </view>
      <view class="stat-card stat-closed">
        <view class="stat-icon">馃寵</view>
        <view class="stat-info">
          <text class="stat-value">{{ closedCount }}</text>
          <text class="stat-label">宸叉墦鐑?/text>
        </view>
      </view>
    </view>

    <!-- 绛涢€夊伐鍏锋爮 -->
    <view class="toolbar">
      <view class="toolbar-left">
        <view class="search-box">
          <text class="search-icon">馃攳</text>
          <input 
            class="search-input" 
            v-model="searchKeyword" 
            placeholder="鎼滅储搴楅摵鍚嶇О..."
            confirm-type="search"
            @confirm="handleSearch"
          />
          <text class="search-clear" v-if="searchKeyword" @tap="clearSearch">鉁?/text>
        </view>
        
        <view class="filter-group">
          <view class="filter-label">鐘舵€侊細</view>
          <view class="filter-options">
            <view 
              :class="['filter-option', statusIndex === 0 ? 'active' : '']"
              @tap="selectStatus(0)"
            >
              鍏ㄩ儴
            </view>
            <view 
              :class="['filter-option', statusIndex === 1 ? 'active' : '']"
              @tap="selectStatus(1)"
            >
              钀ヤ笟涓?            </view>
            <view 
              :class="['filter-option', statusIndex === 2 ? 'active' : '']"
              @tap="selectStatus(2)"
            >
              宸叉墦鐑?            </view>
          </view>
        </view>
      </view>
      
      <view class="toolbar-right">
        <button class="btn-reset" @tap="resetSearch">
          <text class="btn-icon">馃攧</text>
          閲嶇疆
        </button>
        <button class="btn-add" @tap="handleAdd">
          <text class="btn-icon">+</text>
          鏂板搴楅摵
        </button>
      </view>
    </view>

    <!-- 鏁版嵁琛ㄦ牸 -->
    <view class="table-wrapper">
      <view class="table-container">
        <view class="table-header">
          <view class="table-row">
            <view class="table-cell cell-checkbox">
              <view class="checkbox" :class="{checked: allSelected}" @tap="toggleSelectAll"></view>
            </view>
            <view class="table-cell cell-id">ID</view>
            <view class="table-cell cell-shop">搴楅摵淇℃伅</view>
            <view class="table-cell cell-rating">璇勫垎</view>
            <view class="table-cell cell-phone">鑱旂郴鐢佃瘽</view>
            <view class="table-cell cell-status">鐘舵€?/view>
            <view class="table-cell cell-actions">鎿嶄綔</view>
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
                  <text>搴?/text>
                </view>
                <view class="shop-details">
                  <text class="shop-name">{{ item.name }}</text>
                  <text class="shop-desc" v-if="item.description">{{ item.description }}</text>
                </view>
              </view>
            </view>
            <view class="table-cell cell-rating">
              <view class="rating-stars">
                <text class="star-icon" v-for="i in 5" :key="i">{{ i <= Math.round(item.rating || 5) ? '猸? : '鈽? }}</text>
                <text class="rating-value">{{ Number(item.rating || 5).toFixed(1) }}</text>
              </view>
            </view>
            <view class="table-cell cell-phone">{{ item.phone || '-' }}</view>
            <view class="table-cell cell-status">
              <view :class="['status-badge', item.status === 1 ? 'status-open' : 'status-closed']">
                <view class="status-dot"></view>
                {{ item.status === 1 ? '钀ヤ笟涓? : '宸叉墦鐑? }}
              </view>
            </view>
            <view class="table-cell cell-actions">
              <view class="action-btns">
                <button class="btn-action btn-view" @tap="handleView(item)" title="鏌ョ湅">
                  <text>馃憗</text>
                </button>
                <button class="btn-action btn-edit" @tap="handleEdit(item)" title="缂栬緫">
                  <text>鉁?/text>
                </button>
                <button class="btn-action btn-delete" @tap="handleDelete(item)" title="鍒犻櫎">
                  <text>馃棏</text>
                </button>
              </view>
            </view>
          </view>
        </view>
        
        <!-- 绌虹姸鎬?-->
        <view v-if="list.length === 0" class="empty-state">
          <view class="empty-icon">馃摥</view>
          <view class="empty-text">鏆傛棤搴楅摵鏁版嵁</view>
          <button class="btn-add-empty" @tap="handleAdd">绔嬪嵆娣诲姞</button>
        </view>
      </view>
      
      <!-- 鍒嗛〉 -->
      <view class="pagination">
        <view class="pagination-info">
          鍏?{{ total }} 鏉℃暟鎹紝姣忛〉 {{ pageSize }} 鏉?        </view>
        <view class="pagination-controls">
          <button 
            class="btn-page" 
            :disabled="page <= 1" 
            @tap="prevPage"
          >
            <text>鈥?/text> 涓婁竴椤?          </button>
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
          <button 
            class="btn-page" 
            :disabled="page >= totalPages" 
            @tap="nextPage"
          >
            涓嬩竴椤?<text>鈥?/text>
          </button>
        </view>
      </view>
    </view>

    <!-- 缂栬緫寮圭獥 -->
    <view class="modal-mask" v-if="modalShow" @tap="closeModal">
      <view class="modal-dialog" @tap.stop>
        <view class="modal-header">
          <view class="modal-title-wrapper">
            <text class="modal-icon">{{ modalTitle.includes('鏂板') ? '鉃? : '鉁忥笍' }}</text>
            <text class="modal-title">{{ modalTitle }}</text>
          </view>
          <button class="modal-close" @tap="closeModal">鉁?/button>
        </view>
        
        <view class="modal-body">
          <view class="form-grid">
            <!-- 绗竴琛岋細搴楅摵鍚嶇О + 搴椾富 -->
            <view class="form-item">
              <text class="form-label">搴楅摵鍚嶇О <text class="required">*</text></text>
              <input 
                class="form-input" 
                v-model="formData.name" 
                placeholder="璇疯緭鍏ュ簵閾哄悕绉?
                maxlength="30"
              />
            </view>
            
            <view class="form-item">
              <text class="form-label">搴椾富 <text class="required">*</text></text>
              <picker 
                :range="userList" 
                :value="userIndex"
                range-key="nickname"
                @change="onUserChange"
              >
                <view class="form-picker">
                  <text>{{ userList[userIndex]?.nickname || '璇烽€夋嫨搴椾富' }}</text>
                  <text class="picker-arrow">鈻?/text>
                </view>
              </picker>
            </view>
            
            <!-- 绗簩琛岋細璇勫垎 + 鐘舵€?-->
            <view class="form-item">
              <text class="form-label">搴楅摵璇勫垎</text>
              <view class="rating-input">
                <view class="rating-stars-input">
                  <text 
                    v-for="i in 5" 
                    :key="i"
                    class="star"
                    :class="{active: i <= (formData.rating || 5)}"
                    @tap="setRating(i)"
                  >猸?/text>
                </view>
                <text class="rating-text">{{ (formData.rating || 5).toFixed(1) }}鍒?/text>
              </view>
            </view>
            
            <view class="form-item">
              <text class="form-label">钀ヤ笟鐘舵€?/text>
              <view class="toggle-group">
                <view 
                  :class="['toggle-option', formData.status === 1 ? 'active' : '']"
                  @tap="formData.status = 1"
                >
                  <text>鉁?钀ヤ笟</text>
                </view>
                <view 
                  :class="['toggle-option', formData.status === 0 ? 'active' : '']"
                  @tap="formData.status = 0"
                >
                  <text>馃寵 鎵撶儕</text>
                </view>
              </view>
            </view>
            
            <!-- 绗笁琛岋細鑱旂郴鐢佃瘽 + 钀ヤ笟鏃堕棿 -->
            <view class="form-item">
              <text class="form-label">鑱旂郴鐢佃瘽</text>
              <input 
                class="form-input" 
                v-model="formData.phone" 
                placeholder="璇疯緭鍏ヨ仈绯荤數璇?
                maxlength="20"
              />
            </view>
            
            <view class="form-item">
              <text class="form-label">钀ヤ笟鏃堕棿</text>
              <input 
                class="form-input" 
                v-model="formData.businessHours" 
                placeholder="渚嬪锛?9:00-22:00"
                maxlength="30"
              />
            </view>
            
            <!-- 绗洓琛岋細搴楅摵鍦板潃 -->
            <view class="form-item form-item-full">
              <text class="form-label">搴楅摵鍦板潃</text>
              <input 
                class="form-input" 
                v-model="formData.address" 
                placeholder="璇疯緭鍏ヨ缁嗗湴鍧€"
                maxlength="100"
              />
            </view>
            
            <!-- 绗簲琛岋細搴楅摵鎻忚堪 -->
            <view class="form-item form-item-full">
              <text class="form-label">搴楅摵鎻忚堪</text>
              <textarea 
                class="form-textarea" 
                v-model="formData.description" 
                placeholder="璇疯緭鍏ュ簵閾烘弿杩帮紙閫夊～锛?
                maxlength="200"
              />
            </view>
            
            <!-- 绗叚琛岋細澶村儚涓婁紶 -->
            <view class="form-item form-item-full">
              <text class="form-label">搴楅摵澶村儚</text>
              <view class="upload-area">
                <view 
                  v-if="formData.avatar" 
                  class="upload-preview"
                >
                  <image 
                    :src="getFullImageUrl(formData.avatar)" 
                    mode="aspectFill" 
                    class="uploaded-image" 
                    @tap="previewAvatarEdit"
                  />
                  <button class="image-delete" @tap="removeAvatar">鉁?/button>
                </view>
                <view class="upload-btn" @tap="chooseAvatar">
                  <text class="upload-icon">馃摲</text>
                  <text class="upload-text">鐐瑰嚮涓婁紶澶村儚</text>
                  <text class="upload-tip">鏀寔 JPG/PNG 鏍煎紡</text>
                </view>
              </view>
            </view>
          </view>
        </view>

        <view class="modal-footer">
          <button class="btn-cancel" @tap="closeModal">鍙栨秷</button>
          <button class="btn-confirm" @tap="handleSubmit">纭淇濆瓨</button>
        </view>
      </view>
    </view>

    <!-- 鏌ョ湅寮圭獥 -->
    <view class="modal-mask" v-if="viewModalShow" @tap="viewModalShow = false">
      <view class="modal-dialog modal-view" @tap.stop>
        <view class="modal-header">
          <text class="modal-title">馃搵 搴楅摵璇︽儏</text>
          <button class="modal-close" @tap="viewModalShow = false">鉁?/button>
        </view>
        <view class="modal-body view-body">
          <view class="view-grid">
            <view class="view-item">
              <text class="view-label">搴楅摵 ID</text>
              <text class="view-value">#{{ viewData.id }}</text>
            </view>
            <view class="view-item">
              <text class="view-label">搴楅摵鍚嶇О</text>
              <text class="view-value">{{ viewData.name }}</text>
            </view>
            <view class="view-item">
              <text class="view-label">搴椾富</text>
              <text class="view-value">{{ viewData.ownerName || '-' }}</text>
            </view>
            <view class="view-item">
              <text class="view-label">搴楅摵璇勫垎</text>
              <text class="view-value">{{ Number(viewData.rating || 5).toFixed(1) }}鍒?/text>
            </view>
            <view class="view-item">
              <text class="view-label">鑱旂郴鐢佃瘽</text>
              <text class="view-value">{{ viewData.phone || '-' }}</text>
            </view>
            <view class="view-item">
              <text class="view-label">钀ヤ笟鐘舵€?/text>
              <view :class="['status-badge', viewData.status === 1 ? 'status-open' : 'status-closed']">
                {{ viewData.status === 1 ? '钀ヤ笟涓? : '宸叉墦鐑? }}
              </view>
            </view>
            <view class="view-item full">
              <text class="view-label">钀ヤ笟鏃堕棿</text>
              <text class="view-value">{{ viewData.businessHours || '-' }}</text>
            </view>
            <view class="view-item full">
              <text class="view-label">搴楅摵鍦板潃</text>
              <text class="view-value">{{ viewData.address || '-' }}</text>
            </view>
            <view class="view-item full">
              <text class="view-label">搴楅摵鎻忚堪</text>
              <text class="view-value view-desc">{{ viewData.description || '鏆傛棤鎻忚堪' }}</text>
            </view>
            <view class="view-item full" v-if="viewData.avatar">
              <text class="view-label">搴楅摵澶村儚</text>
              <image 
                :src="getFullImageUrl(viewData.avatar)" 
                mode="aspectFill" 
                class="view-avatar"
                @tap="previewViewAvatar"
              />
            </view>
          </view>
        </view>
        <view class="modal-footer">
          <button class="btn-cancel" @tap="viewModalShow = false">鍏抽棴</button>
          <button class="btn-edit-full" @tap="handleEditFromView">缂栬緫姝ゅ簵閾?/button>
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
      viewModalShow: false,
      modalTitle: '鏂板搴楅摵',
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
  computed: {
    Math() {
      return Math
    },
    totalPages() {
      return Math.ceil(this.total / this.pageSize)
    },
    openCount() {
      return this.list.filter(item => item.status === 1).length
    },
    closedCount() {
      return this.list.filter(item => item.status === 0).length
    },
    visiblePages() {
      const pages = []
      const total = this.totalPages
      let start = Math.max(1, this.page - 2)
      let end = Math.min(total, this.page + 2)
      
      for (let i = start; i <= end; i++) {
        pages.push(i)
      }
      return pages
    }
  },
  onShow() {
    if (!this.ensureAdminAccess()) return
    this.loadUsers().then(() => {
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
        uni.showToast({ title: '浠呯鐞嗗憳鍙闂?, icon: 'none' })
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
        const res = await request({ url: '/api/admin/users?page=1&pageSize=100' })
        const allUsers = res?.data?.list || []
        this.userList = allUsers.filter(u => u.role === 'merchant')
        if (this.userList.length === 0) {
          this.userList = allUsers
        }
        if (this.userList.length > 0) {
          this.userIndex = 0
        }
      } catch (error) {
        this.userList = []
      }
    },
    async loadList() {
      try {
        const params = {
          page: this.page,
          pageSize: this.pageSize
        }
        if (this.searchKeyword) {
          params.keyword = this.searchKeyword
        }
        if (this.searchStatus !== null && this.searchStatus !== undefined) {
          params.status = this.searchStatus
        }
        const res = await request({ url: '/api/admin/shops', data: params })
        this.list = res?.data?.list || []
        this.total = res?.data?.total || 0
        this.selectedIds = []
        this.allSelected = false
      } catch (error) {
        uni.showToast({ title: error.message || '鍔犺浇澶辫触', icon: 'none' })
      }
    },
    handleSearch() {
      this.page = 1
      this.loadList()
    },
    clearSearch() {
      this.searchKeyword = ''
      this.handleSearch()
    },
    selectStatus(index) {
      this.statusIndex = index
      if (index === 0) {
        this.searchStatus = null
      } else if (index === 1) {
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
    onUserChange(e) {
      this.userIndex = e.detail.value
      if (this.formData.id) {
        this.formData.ownerId = this.userList[this.userIndex]?.id
      }
    },
    setRating(rating) {
      this.formData.rating = rating
    },
    toggleSelect(id) {
      const index = this.selectedIds.indexOf(id)
      if (index > -1) {
        this.selectedIds.splice(index, 1)
      } else {
        this.selectedIds.push(id)
      }
      this.updateSelectAll()
    },
    toggleSelectAll() {
      if (this.allSelected) {
        this.selectedIds = []
      } else {
        this.selectedIds = this.list.map(item => item.id)
      }
      this.allSelected = !this.allSelected
    },
    updateSelectAll() {
      this.allSelected = this.selectedIds.length === this.list.length && this.list.length > 0
    },
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
    previewViewAvatar() {
      if (!this.viewData.avatar) return
      uni.previewImage({
        urls: [this.getFullImageUrl(this.viewData.avatar)],
        current: 0
      })
    },
    async uploadAvatar(filePath) {
      return new Promise((resolve, reject) => {
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
            uni.hideLoading()
            if (res.statusCode !== 200) {
              uni.showToast({ title: '涓婁紶澶辫触锛欻TTP ' + res.statusCode, icon: 'none' })
              reject(new Error('HTTP ' + res.statusCode))
              return
            }
            let data
            try {
              if (typeof res.data === 'string') {
                data = JSON.parse(res.data)
              } else {
                data = res.data
              }
            } catch (e) {
              uni.showToast({ title: '鍝嶅簲瑙ｆ瀽澶辫触', icon: 'none' })
              reject(e)
              return
            }
            if (data.success && data.data && Array.isArray(data.data) && data.data.length > 0) {
              this.formData.avatar = data.data[0]
              uni.showToast({ title: '涓婁紶鎴愬姛', icon: 'success' })
              resolve()
            } else {
              const msg = data.message || data.msg || '涓婁紶澶辫触'
              uni.showToast({ title: msg, icon: 'none' })
              reject(new Error(msg))
            }
          },
          fail: (err) => {
            uni.hideLoading()
            uni.showToast({ title: '涓婁紶澶辫触', icon: 'none' })
            reject(err)
          }
        })
      })
    },
    chooseAvatar() {
      uni.showLoading({ title: '閫夋嫨鍥剧墖...' })
      uni.chooseImage({
        count: 1,
        sizeType: ['compressed'],
        sourceType: ['album', 'camera'],
        success: async (res) => {
          uni.hideLoading()
          if (!res.tempFiles || res.tempFiles.length === 0) {
            uni.showToast({ title: '娌℃湁閫夋嫨鏂囦欢', icon: 'none' })
            return
          }
          const filePath = res.tempFiles[0].path
          uni.showLoading({ title: '涓婁紶涓?..' })
          try {
            await this.uploadAvatar(filePath)
          } catch (error) {
            console.error('[涓婁紶寮傚父]', error)
          } finally {
            uni.hideLoading()
          }
        },
        fail: (err) => {
          uni.hideLoading()
          if (!err.errMsg || !err.errMsg.includes('cancel')) {
            uni.showToast({ title: '閫夋嫨澶辫触', icon: 'none' })
          }
        }
      })
    },
    removeAvatar() {
      this.formData.avatar = ''
    },
    handleAdd() {
      this.modalTitle = '鏂板搴楅摵'
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
      this.modalTitle = '缂栬緫搴楅摵'
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
      const ownerIdNum = Number(row.ownerId)
      this.userIndex = this.userList.findIndex(u => Number(u.id) === ownerIdNum)
      if (this.userIndex < 0) {
        this.userIndex = 0
      }
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
      setTimeout(() => {
        this.handleEdit(this.viewData)
      }, 300)
    },
    handleDelete(row) {
      uni.showModal({
        title: '纭鍒犻櫎',
        content: '纭畾瑕佸垹闄ゅ簵閾恒€? + row.name + '銆嶅悧锛熸鎿嶄綔涓嶅彲鎭㈠銆?,
        success: async (res) => {
          if (res.confirm) {
            try {
              await request({ url: `/api/admin/shops/${row.id}`, method: 'DELETE' })
              uni.showToast({ title: '鍒犻櫎鎴愬姛', icon: 'success' })
              this.loadList()
            } catch (error) {
              uni.showToast({ title: error.message || '鍒犻櫎澶辫触', icon: 'none' })
            }
          }
        }
      })
    },
    closeModal() {
      this.modalShow = false
    },
    async handleSubmit() {
      if (!this.formData.name || !this.formData.name.trim()) {
        uni.showToast({ title: '璇疯緭鍏ュ簵閾哄悕绉?, icon: 'none' })
        return
      }
      if (this.formData.id && this.userList[this.userIndex]?.id) {
        this.formData.ownerId = this.userList[this.userIndex].id
      } else if (!this.formData.id && this.userList[this.userIndex]?.id) {
        this.formData.ownerId = this.userList[this.userIndex].id
      }
      try {
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
        if (this.formData.ownerId) {
          data.ownerId = this.formData.ownerId
        }
        if (this.formData.id) {
          await request({ url: `/api/admin/shops/${this.formData.id}`, method: 'PUT', data })
        } else {
          await request({ url: '/api/admin/shops', method: 'POST', data })
        }
        uni.showToast({ title: '淇濆瓨鎴愬姛', icon: 'success' })
        this.closeModal()
        this.loadList()
      } catch (error) {
        uni.showToast({ title: error.message || '淇濆瓨澶辫触', icon: 'none' })
      }
    },
    prevPage() {
      if (this.page > 1) {
        this.page--
        this.loadList()
      }
    },
    nextPage() {
      const maxPage = this.totalPages
      if (this.page < maxPage) {
        this.page++
        this.loadList()
      }
    },
    goToPage(page) {
      if (page !== this.page && page >= 1 && page <= this.totalPages) {
        this.page = page
        this.loadList()
      }
    }
  }
}
</script>

<style lang="scss" scoped>
// 鍙橀噺瀹氫箟
$primary-color: #4f46e5;
$primary-hover: #4338ca;
$success-color: #10b981;
$danger-color: #ef4444;
$warning-color: #f59e0b;
$info-color: #3b82f6;
$text-primary: #1f2937;
$text-secondary: #6b7280;
$text-light: #9ca3af;
$border-color: #e5e7eb;
$bg-light: #f9fafb;
$bg-white: #ffffff;
$shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.05);
$shadow: 0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06);
$shadow-md: 0 4px 6px rgba(0, 0, 0, 0.1), 0 2px 4px rgba(0, 0, 0, 0.06);
$shadow-lg: 0 10px 15px rgba(0, 0, 0, 0.1), 0 4px 6px rgba(0, 0, 0, 0.05);
$radius-sm: 6px;
$radius: 8px;
$radius-lg: 12px;
$radius-xl: 16px;

.page {
  min-height: 100vh;
  background: #f3f4f6;
  padding: 24px;
}

// 椤甸潰澶撮儴
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  padding: 20px 24px;
  background: $bg-white;
  border-radius: $radius-lg;
  box-shadow: $shadow;
  
  .header-left {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }
  
  .page-title {
    font-size: 24px;
    font-weight: 700;
    color: $text-primary;
  }
  
  .page-subtitle {
    font-size: 14px;
    color: $text-secondary;
  }
  
  .header-right {
    display: flex;
    gap: 12px;
  }
  
  .btn-back, .btn-user {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 10px 16px;
    background: $bg-light;
    border: 1px solid $border-color;
    border-radius: $radius;
    font-size: 14px;
    color: $text-secondary;
    transition: all 0.2s;
    
    &:hover {
      background: #f3f4f6;
      color: $text-primary;
    }
    
    .btn-icon {
      font-size: 16px;
    }
  }
}

// 缁熻鍗＄墖
.stats-row {
  display: flex;
  gap: 20px;
  margin-bottom: 24px;
}

.stat-card {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px 24px;
  background: $bg-white;
  border-radius: $radius-lg;
  box-shadow: $shadow;
  border-left: 4px solid transparent;
  
  &.stat-total {
    border-left-color: $primary-color;
  }
  
  &.stat-open {
    border-left-color: $success-color;
  }
  
  &.stat-closed {
    border-left-color: $warning-color;
  }
  
  .stat-icon {
    font-size: 32px;
    line-height: 1;
  }
  
  .stat-info {
    display: flex;
    flex-direction: column;
  }
  
  .stat-value {
    font-size: 28px;
    font-weight: 700;
    color: $text-primary;
  }
  
  .stat-label {
    font-size: 13px;
    color: $text-secondary;
    margin-top: 2px;
  }
}

// 宸ュ叿鏍?.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  background: $bg-white;
  border-radius: $radius-lg;
  box-shadow: $shadow;
  margin-bottom: 20px;
  
  .toolbar-left {
    display: flex;
    align-items: center;
    gap: 24px;
    flex: 1;
  }
  
  .search-box {
    position: relative;
    width: 280px;
    display: flex;
    align-items: center;
    
    .search-icon {
      position: absolute;
      left: 12px;
      font-size: 16px;
      color: $text-light;
      z-index: 1;
    }
    
    .search-input {
      width: 100%;
      height: 40px;
      padding: 0 36px;
      background: $bg-light;
      border: 1px solid $border-color;
      border-radius: $radius;
      font-size: 14px;
      transition: all 0.2s;
      
      &:focus {
        background: $bg-white;
        border-color: $primary-color;
        box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
      }
    }
    
    .search-clear {
      position: absolute;
      right: 12px;
      font-size: 14px;
      color: $text-light;
      cursor: pointer;
      z-index: 1;
      
      &:hover {
        color: $text-secondary;
      }
    }
  }
  
  .filter-group {
    display: flex;
    align-items: center;
    gap: 12px;
    
    .filter-label {
      font-size: 14px;
      color: $text-secondary;
    }
    
    .filter-options {
      display: flex;
      gap: 8px;
    }
    
    .filter-option {
      padding: 8px 16px;
      background: $bg-light;
      border: 1px solid $border-color;
      border-radius: $radius;
      font-size: 14px;
      color: $text-secondary;
      cursor: pointer;
      transition: all 0.2s;
      
      &:hover {
        background: #f3f4f6;
      }
      
      &.active {
        background: $primary-color;
        border-color: $primary-color;
        color: #fff;
      }
    }
  }
  
  .toolbar-right {
    display: flex;
    gap: 12px;
  }
  
  .btn-reset, .btn-add {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 10px 16px;
    border: none;
    border-radius: $radius;
    font-size: 14px;
    cursor: pointer;
    transition: all 0.2s;
  }
  
  .btn-reset {
    background: $bg-light;
    color: $text-secondary;
    
    &:hover {
      background: #f3f4f6;
    }
    
    .btn-icon {
      font-size: 16px;
    }
  }
  
  .btn-add {
    background: $primary-color;
    color: #fff;
    
    &:hover {
      background: $primary-hover;
    }
    
    .btn-icon {
      font-size: 18px;
      font-weight: bold;
    }
  }
}

// 琛ㄦ牸
.table-wrapper {
  background: $bg-white;
  border-radius: $radius-lg;
  box-shadow: $shadow;
  overflow: hidden;
}

.table-container {
  overflow-x: auto;
}

.table-header {
  background: #f9fafb;
  border-bottom: 1px solid $border-color;
}

.table-row {
  display: flex;
  align-items: center;
  border-bottom: 1px solid $border-color;
  transition: background 0.2s;
  
  &:last-child {
    border-bottom: none;
  }
  
  &.highlighted {
    background: rgba(79, 70, 229, 0.04);
  }
}

.table-cell {
  padding: 16px 20px;
  display: flex;
  align-items: center;
  
  &.cell-checkbox {
    width: 60px;
    justify-content: center;
  }
  
  &.cell-id {
    width: 80px;
    font-size: 13px;
    color: $text-light;
    font-family: monospace;
  }
  
  &.cell-shop {
    flex: 1;
    min-width: 250px;
  }
  
  &.cell-rating {
    width: 180px;
  }
  
  &.cell-phone {
    width: 150px;
    color: $text-secondary;
  }
  
  &.cell-status {
    width: 120px;
  }
  
  &.cell-actions {
    width: 140px;
    justify-content: center;
  }
}

.checkbox {
  width: 18px;
  height: 18px;
  border: 2px solid $border-color;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
  
  &.checked {
    background: $primary-color;
    border-color: $primary-color;
    
    &::after {
      content: '鉁?;
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      color: #fff;
      font-size: 12px;
      font-weight: bold;
    }
  }
}

.shop-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.shop-avatar, .shop-avatar-placeholder {
  width: 48px;
  height: 48px;
  border-radius: $radius;
  flex-shrink: 0;
}

.shop-avatar {
  object-fit: cover;
  cursor: pointer;
  transition: transform 0.2s;
  
  &:hover {
    transform: scale(1.05);
  }
}

.shop-avatar-placeholder {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  
  text {
    color: #fff;
    font-size: 16px;
    font-weight: bold;
  }
}

.shop-details {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.shop-name {
  font-size: 15px;
  font-weight: 600;
  color: $text-primary;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.shop-desc {
  font-size: 12px;
  color: $text-light;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 300px;
}

.rating-stars {
  display: flex;
  align-items: center;
  gap: 4px;
  
  .star-icon {
    font-size: 14px;
  }
  
  .rating-value {
    font-size: 13px;
    color: $text-secondary;
    margin-left: 4px;
  }
}

.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 13px;
  font-weight: 500;
  
  &.status-open {
    background: rgba(16, 185, 129, 0.1);
    color: $success-color;
  }
  
  &.status-closed {
    background: rgba(245, 158, 11, 0.1);
    color: $warning-color;
  }
  
  .status-dot {
    width: 6px;
    height: 6px;
    border-radius: 50%;
    background: currentColor;
  }
}

.action-btns {
  display: flex;
  gap: 8px;
}

.btn-action {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  border-radius: $radius;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 16px;
  padding: 0;
  
  &.btn-view {
    background: rgba(59, 130, 246, 0.1);
    color: $info-color;
    
    &:hover {
      background: $info-color;
      color: #fff;
    }
  }
  
  &.btn-edit {
    background: rgba(245, 158, 11, 0.1);
    color: $warning-color;
    
    &:hover {
      background: $warning-color;
      color: #fff;
    }
  }
  
  &.btn-delete {
    background: rgba(239, 68, 68, 0.1);
    color: $danger-color;
    
    &:hover {
      background: $danger-color;
      color: #fff;
    }
  }
}

// 绌虹姸鎬?.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  
  .empty-icon {
    font-size: 64px;
    margin-bottom: 16px;
  }
  
  .empty-text {
    font-size: 16px;
    color: $text-secondary;
    margin-bottom: 24px;
  }
  
  .btn-add-empty {
    padding: 12px 32px;
    background: $primary-color;
    color: #fff;
    border: none;
    border-radius: $radius;
    font-size: 14px;
    cursor: pointer;
    transition: all 0.2s;
    
    &:hover {
      background: $primary-hover;
    }
  }
}

// 鍒嗛〉
.pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-top: 1px solid $border-color;
  
  .pagination-info {
    font-size: 14px;
    color: $text-secondary;
  }
  
  .pagination-controls {
    display: flex;
    align-items: center;
    gap: 12px;
  }
  
  .btn-page {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 8px 16px;
    background: $bg-light;
    border: 1px solid $border-color;
    border-radius: $radius;
    font-size: 14px;
    color: $text-secondary;
    cursor: pointer;
    transition: all 0.2s;
    
    &:hover:not(:disabled) {
      background: #f3f4f6;
      border-color: $primary-color;
      color: $primary-color;
    }
    
    &:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
  }
  
  .page-numbers {
    display: flex;
    gap: 4px;
  }
  
  .page-number {
    min-width: 36px;
    height: 36px;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0 8px;
    background: $bg-light;
    border: 1px solid $border-color;
    border-radius: $radius;
    font-size: 14px;
    color: $text-secondary;
    cursor: pointer;
    transition: all 0.2s;
    
    &:hover {
      border-color: $primary-color;
      color: $primary-color;
    }
    
    &.active {
      background: $primary-color;
      border-color: $primary-color;
      color: #fff;
    }
  }
}

// 寮圭獥
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
  backdrop-filter: blur(4px);
}

.modal-dialog {
  background: $bg-white;
  border-radius: $radius-xl;
  width: 90%;
  max-width: 700px;
  max-height: 85vh;
  display: flex;
  flex-direction: column;
  box-shadow: $shadow-lg;
  animation: modalSlideIn 0.3s ease-out;
}

@keyframes modalSlideIn {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid $border-color;
  
  .modal-title-wrapper {
    display: flex;
    align-items: center;
    gap: 10px;
  }
  
  .modal-icon {
    font-size: 20px;
  }
  
  .modal-title {
    font-size: 18px;
    font-weight: 600;
    color: $text-primary;
  }
  
  .modal-close {
    width: 32px;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: $bg-light;
    border: none;
    border-radius: $radius;
    font-size: 18px;
    color: $text-secondary;
    cursor: pointer;
    transition: all 0.2s;
    
    &:hover {
      background: $danger-color;
      color: #fff;
    }
  }
}

.modal-body {
  padding: 24px;
  overflow-y: auto;
  flex: 1;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 20px;
}

.form-item {
  display: flex;
  flex-direction: column;
  gap: 8px;
  
  &.form-item-full {
    grid-column: 1 / -1;
  }
  
  .form-label {
    font-size: 14px;
    font-weight: 500;
    color: $text-primary;
    
    .required {
      color: $danger-color;
    }
  }
  
  .form-input, .form-textarea {
    padding: 10px 14px;
    background: $bg-light;
    border: 1px solid $border-color;
    border-radius: $radius;
    font-size: 14px;
    transition: all 0.2s;
    
    &:focus {
      background: $bg-white;
      border-color: $primary-color;
      box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
      outline: none;
    }
  }
  
  .form-input {
    height: 42px;
  }
  
  .form-textarea {
    min-height: 80px;
    resize: vertical;
  }
  
  .form-picker {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 10px 14px;
    background: $bg-light;
    border: 1px solid $border-color;
    border-radius: $radius;
    font-size: 14px;
    color: $text-secondary;
    cursor: pointer;
    
    .picker-arrow {
      font-size: 12px;
    }
  }
}

.rating-input {
  display: flex;
  align-items: center;
  gap: 12px;
  
  .rating-stars-input {
    display: flex;
    gap: 4px;
    
    .star {
      font-size: 20px;
      opacity: 0.3;
      cursor: pointer;
      transition: all 0.2s;
      
      &.active {
        opacity: 1;
      }
      
      &:hover {
        transform: scale(1.1);
      }
    }
  }
  
  .rating-text {
    font-size: 14px;
    color: $text-secondary;
  }
}

.toggle-group {
  display: flex;
  gap: 8px;
  
  .toggle-option {
    flex: 1;
    padding: 10px;
    background: $bg-light;
    border: 2px solid $border-color;
    border-radius: $radius;
    text-align: center;
    cursor: pointer;
    transition: all 0.2s;
    
    &:hover {
      border-color: $primary-color;
    }
    
    &.active {
      background: $primary-color;
      border-color: $primary-color;
      color: #fff;
    }
  }
}

.upload-area {
  display: flex;
  gap: 16px;
  align-items: flex-start;
}

.upload-preview {
  position: relative;
  width: 120px;
  height: 120px;
  border-radius: $radius;
  overflow: hidden;
  
  .uploaded-image {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }
  
  .image-delete {
    position: absolute;
    top: 8px;
    right: 8px;
    width: 28px;
    height: 28px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: rgba(239, 68, 68, 0.9);
    color: #fff;
    border: none;
    border-radius: 50%;
    cursor: pointer;
    transition: all 0.2s;
    
    &:hover {
      transform: scale(1.1);
    }
  }
}

.upload-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 20px 32px;
  background: $bg-light;
  border: 2px dashed $border-color;
  border-radius: $radius;
  cursor: pointer;
  transition: all 0.2s;
  
  &:hover {
    border-color: $primary-color;
    background: rgba(79, 70, 229, 0.05);
  }
  
  .upload-icon {
    font-size: 32px;
  }
  
  .upload-text {
    font-size: 14px;
    color: $text-primary;
    font-weight: 500;
  }
  
  .upload-tip {
    font-size: 12px;
    color: $text-light;
  }
}

.modal-footer {
  display: flex;
  gap: 12px;
  padding: 20px 24px;
  border-top: 1px solid $border-color;
  
  button {
    flex: 1;
    padding: 12px 24px;
    border: none;
    border-radius: $radius;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s;
  }
  
  .btn-cancel {
    background: $bg-light;
    color: $text-secondary;
    
    &:hover {
      background: #f3f4f6;
    }
  }
  
  .btn-confirm {
    background: $primary-color;
    color: #fff;
    
    &:hover {
      background: $primary-hover;
    }
  }
  
  .btn-edit-full {
    background: $info-color;
    color: #fff;
    
    &:hover {
      background: #2563eb;
    }
  }
}

// 鏌ョ湅寮圭獥
.modal-view {
  max-width: 600px;
}

.view-body {
  padding-top: 0;
}

.view-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}

.view-item {
  display: flex;
  flex-direction: column;
  gap: 6px;
  
  &.full {
    grid-column: 1 / -1;
  }
  
  .view-label {
    font-size: 13px;
    color: $text-light;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }
  
  .view-value {
    font-size: 15px;
    color: $text-primary;
    
    &.view-desc {
      color: $text-secondary;
      line-height: 1.6;
    }
  }
  
  .status-badge {
    align-self: flex-start;
  }
  
  .view-avatar {
    width: 100px;
    height: 100px;
    border-radius: $radius;
    cursor: pointer;
    transition: transform 0.2s;
    
    &:hover {
      transform: scale(1.05);
    }
  }
}
</style>

