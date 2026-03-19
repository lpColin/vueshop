<template>
  <view class="index-page">
    <view class="shop-header" @tap="goShopInfo">
      <view class="shop-avatar">
        <image :src="shop.avatar" mode="aspectFill" class="avatar-img" />
      </view>
      <view class="shop-info">
        <view class="shop-name">{{ shop.name }}</view>
        <view class="shop-desc">{{ shop.description }}</view>
        <view class="shop-rating">
          <text class="stars">⭐⭐⭐⭐⭐</text>
          <text class="rating-text">{{ shop.rating }}分</text>
          <text class="review-count">{{ shop.reviewCount }}+ 人关注</text>
        </view>
      </view>
      <view class="arrow-right">
        <text class="arrow">›</text>
      </view>
    </view>

    <view class="search-section">
      <view class="search-bar">
        <text class="search-icon">🔍</text>
        <input
          class="search-input"
          placeholder="搜索商品"
          v-model="searchKeyword"
          @confirm="handleSearch"
          confirm-type="search"
        />
        <text v-if="searchKeyword" class="clear-icon" @tap.stop="clearSearch">✕</text>
      </view>
    </view>

    <view class="content-section">
      <view class="category-sidebar">
        <scroll-view scroll-y class="category-scroll">
          <view
            v-for="item in categories"
            :key="item.id"
            class="category-item"
            :class="{ active: currentCategoryId === item.id }"
            @tap="selectCategory(item.id)"
          >
            <text class="category-name">{{ item.name }}</text>
          </view>
        </scroll-view>
      </view>

      <view class="product-sidebar">
        <scroll-view scroll-y class="product-scroll">
          <view class="product-list">
            <view
              v-for="item in filteredProducts"
              :key="item.id"
              class="product-card"
              @tap="goToProduct(item.id)"
            >
              <view class="product-image-wrapper">
                <image :src="item.image" mode="aspectFill" class="product-image" />
              </view>
              <view class="product-info">
                <view class="product-name">{{ item.name }}</view>
                <view class="product-meta">
                  <text class="product-price">¥{{ item.price }}</text>
                  <text class="product-stock">库存 {{ item.stock }}</text>
                </view>
                <view class="product-action">
                  <view class="add-cart-btn" @tap.stop="addToCart(item.id)">
                    <text class="plus-icon">+</text>
                  </view>
                </view>
              </view>
            </view>
          </view>
          <view v-if="filteredProducts.length === 0" class="empty-state">
            <text class="empty-text">暂无商品</text>
          </view>
        </scroll-view>
      </view>
    </view>

  </view>
</template>

<script>
import request from '@/utils/http'

export default {
  data() {
    return {
      apiBaseUrl: 'http://192.168.1.21:5162',
      shop: {
        id: 1,
        name: '鲜果多商城',
        description: '新鲜水果，产地直供，品质保证',
        avatar: '/static/images/avatar.png',
        rating: 4.9,
        reviewCount: 1000,
        phone: '138-8888-8888',
        businessHours: '08:00 - 22:00',
        address: 'XX 市 XX 区 XX 路 XX 号',
        status: '营业中'
      },
      categories: [
        { id: 0, name: '全部分类' },
        { id: 1, name: '水果蔬菜' },
        { id: 2, name: '肉禽蛋品' },
        { id: 3, name: '海鲜水产' },
        { id: 4, name: '粮油调味' },
        { id: 5, name: '休闲零食' },
        { id: 6, name: '酒水饮料' }
      ],
      products: [
        { id: 1, categoryId: 1, name: '红富士苹果 5 斤装', price: 39.0, stock: 99, image: '/static/images/product1.png' },
        { id: 2, categoryId: 1, name: '进口香蕉 1 把', price: 15.8, stock: 150, image: '/static/images/product2.png' },
        { id: 3, categoryId: 1, name: '新鲜草莓 500g', price: 28.0, stock: 80, image: '/static/images/product3.png' },
        { id: 4, categoryId: 2, name: '土鸡蛋 30 枚', price: 45.0, stock: 100, image: '/static/images/product7.png' },
        { id: 5, categoryId: 3, name: '鲜活基围虾 500g', price: 58.0, stock: 30, image: '/static/images/product9.png' },
        { id: 6, categoryId: 4, name: '东北大米 5kg', price: 42.0, stock: 120, image: '/static/images/product12.png' },
        { id: 7, categoryId: 5, name: '每日坚果 30 包', price: 99.0, stock: 80, image: '/static/images/product15.png' },
        { id: 8, categoryId: 6, name: '可口可乐 12 罐', price: 35.0, stock: 200, image: '/static/images/product18.png' }
      ],
      currentCategoryId: 0,
      searchKeyword: '',
      isSearching: false,
      cartItems: []
    }
  },
  computed: {
    filteredProducts() {
      if (this.isSearching && this.searchKeyword) {
        return this.products.filter((p) => p.name.toLowerCase().includes(this.searchKeyword.toLowerCase()))
      }
      if (this.currentCategoryId === 0) return this.products
      return this.products.filter((p) => p.categoryId === this.currentCategoryId)
    }
  },
  onShow() {
    this.cartItems = uni.getStorageSync('cartItems') || []
    this.loadRemoteData()
    this.loadCartCount()
  },
  methods: {
    getFullImageUrl(path) {
      if (!path) return ''
      if (path.startsWith('http://') || path.startsWith('https://')) return path
      if (path.startsWith('/')) {
        return this.apiBaseUrl + path
      }
      return this.apiBaseUrl + '/' + path
    },
    async loadRemoteData() {
      await Promise.all([this.fetchCategories(), this.fetchProducts()])
    },
    async fetchCategories() {
      try {
        const res = await request({ url: '/api/admin/categories?status=1' })
        const list = Array.isArray(res?.data?.list) ? res.data.list : []
        if (list.length) {
          this.categories = [{ id: 0, name: '全部分类' }, ...list.map((item) => ({ id: item.id, name: item.name }))]
        }
      } catch (error) {
        console.warn('[首页] 获取分类失败，使用本地兜底数据', error)
      }
    },
    async fetchProducts() {
      try {
        const res = await request({ url: '/api/admin/products?page=1&pageSize=200' })
        const list = Array.isArray(res?.data?.list) ? res.data.list : []
        console.log('[首页] 获取商品列表:', list.length, '个商品')
        if (list.length) {
          this.products = list.map((item, index) => {
            // 优先使用 images 数组的第一张图，其次使用 image 字段
            let imageUrl = item.image || '/static/images/product1.png'
            if (item.images && Array.isArray(item.images) && item.images.length > 0) {
              imageUrl = item.images[0]
            }
            // 转换为完整 URL
            const fullImageUrl = this.getFullImageUrl(imageUrl)
            if (index < 3) {
              console.log('[首页] 商品图片处理:', item.name, '->', imageUrl, '->', fullImageUrl)
            }
            return {
              id: item.id,
              categoryId: item.categoryId,
              name: item.name,
              price: Number(item.price || 0),
              stock: Number(item.stock || 0),
              image: fullImageUrl
            }
          })
          console.log('[首页] 商品列表处理完成，总数:', this.products.length)
        }
      } catch (error) {
        console.warn('[首页] 获取商品失败，使用本地兜底数据', error)
      }
    },
    selectCategory(id) {
      this.currentCategoryId = id
      this.isSearching = false
      if (this.searchKeyword) this.searchKeyword = ''
    },
    handleSearch() {
      const keyword = this.searchKeyword.trim()
      if (!keyword) {
        this.isSearching = false
        return
      }
      this.isSearching = true
      this.currentCategoryId = 0
    },
    clearSearch() {
      this.searchKeyword = ''
      this.isSearching = false
      this.currentCategoryId = 0
    },
    goShopInfo() {
      uni.navigateTo({ url: '/pages/shopInfo/shopInfo' })
    },
    goToProduct(id) {
      uni.navigateTo({ url: `/pages/product/product?id=${id}` })
    },
    async addToCart(id) {
      const product = this.products.find((p) => p.id === id)
      if (!product) return

      const token = uni.getStorageSync('token')
      
      // 已登录用户使用后端购物车
      if (token) {
        try {
          await request({
            url: '/api/cart',
            method: 'POST',
            data: {
              productId: product.id,
              quantity: 1
            }
          })
          await this.loadCartCount()
          uni.showToast({ title: '已加入购物车', icon: 'success' })
          return
        } catch (error) {
          console.warn('[加入购物车] 后端接口失败，回退本地', error)
        }
      }

      // 未登录用户使用本地存储
      const cartItems = this.cartItems || []
      const index = cartItems.findIndex((item) => item.id === id)

      if (index > -1) {
        cartItems[index].quantity += 1
      } else {
        cartItems.push({
          id: product.id,
          name: product.name,
          price: product.price,
          image: product.image,
          quantity: 1,
          stock: product.stock
        })
      }

      uni.setStorageSync('cartItems', cartItems)
      this.cartItems = cartItems
      await this.loadCartCount()
      uni.showToast({ title: '已加入购物车', icon: 'success' })
    },
    async loadCartCount() {
      try {
        const token = uni.getStorageSync('token')
        let count = 0
        
        if (token) {
          // 已登录用户从后端获取
          const res = await request({ url: '/api/cart' })
          const list = Array.isArray(res?.data?.list) ? res.data.list : []
          count = list.reduce((sum, item) => sum + Number(item.quantity || 0), 0)
        } else {
          // 未登录用户从本地存储获取
          const cartItems = uni.getStorageSync('cartItems') || []
          count = cartItems.reduce((sum, item) => sum + Number(item.quantity || 0), 0)
        }
        
        // 更新购物车红点徽章
        if (count > 0) {
          uni.setTabBarBadge({
            index: 1,
            text: String(count)
          })
        } else {
          uni.removeTabBarBadge({ index: 1 })
        }
      } catch (error) {
        console.warn('[购物车数量] 加载失败', error)
      }
    }
  }
}
</script>

<style lang="scss" scoped>
.index-page { min-height: 100vh; background-color: #f5f5f5; display: flex; flex-direction: column; }
.shop-header { display: flex; align-items: center; padding: 24rpx; background-color: #fff; border-bottom: 1rpx solid #f0f0f0; }
.shop-avatar { width: 100rpx; height: 100rpx; border-radius: 50%; overflow: hidden; margin-right: 20rpx; flex-shrink: 0; }
.avatar-img { width: 100%; height: 100%; }
.shop-info { flex: 1; min-width: 0; }
.shop-name { font-size: 32rpx; font-weight: bold; color: #333; margin-bottom: 8rpx; }
.shop-desc { font-size: 24rpx; color: #999; margin-bottom: 12rpx; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.shop-rating { display: flex; align-items: center; }
.rating-text, .review-count { font-size: 22rpx; color: #999; margin-left: 10rpx; }
.arrow { font-size: 48rpx; color: #ccc; }
.search-section { padding: 20rpx; background-color: #fff; }
.search-bar { display: flex; align-items: center; background-color: #f5f5f5; border-radius: 40rpx; padding: 20rpx 30rpx; }
.search-input { flex: 1; font-size: 28rpx; color: #333; height: 48rpx; line-height: 48rpx; }
.clear-icon { color: #999; font-size: 24rpx; margin-left: 16rpx; }
.content-section { flex: 1; display: flex; overflow: hidden; }
.category-sidebar { width: 180rpx; background: #fff; }
.category-scroll { height: 100%; }
.category-item { padding: 24rpx 12rpx; border-left: 6rpx solid transparent; color: #666; font-size: 24rpx; }
.category-item.active { color: #e64340; border-left-color: #e64340; background: #fff5f5; font-weight: 600; }
.product-sidebar { flex: 1; }
.product-scroll { height: 100%; padding: 16rpx; }
.product-card { display: flex; background: #fff; border-radius: 14rpx; padding: 16rpx; margin-bottom: 14rpx; }
.product-image-wrapper { width: 140rpx; height: 140rpx; margin-right: 16rpx; }
.product-image { width: 100%; height: 100%; border-radius: 10rpx; }
.product-info { flex: 1; min-width: 0; }
.product-name { font-size: 26rpx; color: #333; }
.product-meta { margin-top: 10rpx; display: flex; justify-content: space-between; }
.product-price { font-size: 30rpx; color: #ff4d4f; font-weight: 700; }
.product-stock { font-size: 22rpx; color: #999; }
.product-action { display: flex; justify-content: flex-end; }
.add-cart-btn { margin-top: 10rpx; width: 46rpx; height: 46rpx; border-radius: 50%; background: #e64340; color: #fff; display: flex; align-items: center; justify-content: center; }
.empty-state { text-align: center; color: #999; padding: 120rpx 0; }
</style>
