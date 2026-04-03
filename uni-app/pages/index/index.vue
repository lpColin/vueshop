<template>
  <view class="index-page">
    <view class="hero-shell fixed-hero">
      <view class="shop-header" @tap="goShopInfo">
        <view class="shop-avatar">
          <image :src="shop.avatar" mode="aspectFill" class="avatar-img" />
        </view>
        <view class="shop-info">
          <view class="shop-tag-row">
            <text class="shop-tag">鲜果优选</text>
            <text class="shop-status">{{ shop.status }}</text>
          </view>
          <view class="shop-name">{{ shop.name }}</view>
          <view class="shop-desc">{{ shop.description }}</view>
          <view class="shop-rating">
            <text class="stars">⭐⭐⭐⭐⭐</text>
            <text class="rating-text">{{ shop.rating }}分</text>
            <text class="review-count">{{ shop.reviewCount }}+ 人关注</text>
          </view>
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
          <text v-if="searchKeyword" class="clear-icon" @tap.stop="clearSearch">×</text>
        </view>
      </view>

    </view>

    <view class="content-section page-content with-fixed-hero">
      <view class="category-sidebar">
        <scroll-view :scroll-y="isCategoryScrollable" class="category-scroll" show-scrollbar="false">
          <view class="category-scroll-inner">
            <view
              v-for="item in categories"
              :key="item.id"
              class="category-item"
              :class="{ active: currentCategoryId === item.id }"
              @tap="selectCategory(item.id)"
            >
              <text class="category-name">{{ item.name }}</text>
            </view>
          </view>
        </scroll-view>
      </view>

      <view class="product-sidebar">
        <scroll-view :scroll-y="isProductScrollable" class="product-scroll" show-scrollbar="false">
          <view class="product-scroll-inner">
            <view class="product-list">
            <view
              v-for="item in filteredProducts"
              :key="item.id"
              class="product-card"
              @tap="goToProduct(item.id)"
            >
              <view class="product-image-wrapper">
                <image :src="item.image" mode="aspectFill" class="product-image" />
                <view class="product-badge">热卖</view>
              </view>
              <view class="product-info">
                <view class="product-name">{{ item.name }}</view>
                <view class="product-desc">产地直供 · 新鲜到家</view>
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
      apiBaseUrl: 'http://localhost:5162',
      shop: {
        id: 1,
        name: '鲜果多商城',
        description: '新鲜水果，产地直供，品质保证',
        avatar: '/static/images/avatar.png',
        rating: 4.9,
        reviewCount: 1000,
        phone: '138-8888-8888',
        businessHours: '08:00 - 22:00',
        address: 'XX市XX区XX路XX号',
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
        { id: 1, categoryId: 1, name: '红富士苹果 5斤装', price: 39.0, stock: 99, image: '/static/images/product1.png' },
        { id: 2, categoryId: 1, name: '进口香蕉 1把', price: 15.8, stock: 150, image: '/static/images/product2.png' },
        { id: 3, categoryId: 1, name: '新鲜草莓 500g', price: 28.0, stock: 80, image: '/static/images/product3.png' },
        { id: 4, categoryId: 2, name: '土鸡蛋 30枚', price: 45.0, stock: 100, image: '/static/images/product7.png' },
        { id: 5, categoryId: 3, name: '鲜活基围虾 500g', price: 58.0, stock: 30, image: '/static/images/product9.png' },
        { id: 6, categoryId: 4, name: '东北大米 5kg', price: 42.0, stock: 120, image: '/static/images/product12.png' },
        { id: 7, categoryId: 5, name: '每日坚果 30包', price: 99.0, stock: 80, image: '/static/images/product15.png' },
        { id: 8, categoryId: 6, name: '可口可乐 12听', price: 35.0, stock: 200, image: '/static/images/product18.png' }
      ],
      currentCategoryId: 0,
      searchKeyword: '',
      isSearching: false,
      cartItems: [],
      isCategoryScrollable: false,
      isProductScrollable: false
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
    if (typeof this.getTabBar === 'function' && this.getTabBar()) {
      this.getTabBar().setSelectedByPath('/pages/index/index')
      const count = (uni.getStorageSync('cartItems') || []).reduce((sum, item) => sum + Number(item.quantity || 0), 0)
      this.getTabBar().setCartBadge(count > 0 ? String(count) : '')
    }
    this.cartItems = uni.getStorageSync('cartItems') || []
    this.loadRemoteData()
    this.loadCartCount()
    this.scheduleScrollMeasure()
  },
  methods: {
    scheduleScrollMeasure() {
      setTimeout(() => {
        this.updateScrollState()
      }, 80)
    },
    updateScrollState() {
      const query = uni.createSelectorQuery().in(this)
      query.select('.category-scroll').boundingClientRect()
      query.select('.category-scroll-inner').boundingClientRect()
      query.select('.product-scroll').boundingClientRect()
      query.select('.product-scroll-inner').boundingClientRect()
      query.exec((res) => {
        const categoryWrap = res && res[0]
        const categoryInner = res && res[1]
        const productWrap = res && res[2]
        const productInner = res && res[3]
        if (categoryWrap && categoryInner) {
          this.isCategoryScrollable = categoryInner.height > categoryWrap.height + 2
        }
        if (productWrap && productInner) {
          this.isProductScrollable = productInner.height > productWrap.height + 2
        }
      })
    },
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
      this.scheduleScrollMeasure()
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
        if (list.length) {
          this.products = list.map((item) => {
            let imageUrl = item.image || '/static/images/product1.png'
            if (item.images && Array.isArray(item.images) && item.images.length > 0) {
              imageUrl = item.images[0]
            }
            return {
              id: item.id,
              categoryId: item.categoryId,
              name: item.name,
              price: Number(item.price || 0),
              stock: Number(item.stock || 0),
              image: this.getFullImageUrl(imageUrl)
            }
          })
        }
      } catch (error) {
        console.warn('[首页] 获取商品失败，使用本地兜底数据', error)
      }
    },
    selectCategory(id) {
      this.currentCategoryId = id
      this.isSearching = false
      if (this.searchKeyword) this.searchKeyword = ''
      this.scheduleScrollMeasure()
    },
    handleSearch() {
      const keyword = this.searchKeyword.trim()
      if (!keyword) {
        this.isSearching = false
        return
      }
      this.isSearching = true
      this.currentCategoryId = 0
      this.scheduleScrollMeasure()
    },
    clearSearch() {
      this.searchKeyword = ''
      this.isSearching = false
      this.currentCategoryId = 0
      this.scheduleScrollMeasure()
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
          const res = await request({ url: '/api/cart' })
          const list = Array.isArray(res?.data?.list) ? res.data.list : []
          count = list.reduce((sum, item) => sum + Number(item.quantity || 0), 0)
        } else {
          const cartItems = uni.getStorageSync('cartItems') || []
          count = cartItems.reduce((sum, item) => sum + Number(item.quantity || 0), 0)
        }

        if (count > 0) {
          if (typeof this.getTabBar === 'function' && this.getTabBar()) {
            this.getTabBar().setCartBadge(String(count))
          }
          uni.setTabBarBadge({
            index: 1,
            text: String(count)
          })
        } else {
          if (typeof this.getTabBar === 'function' && this.getTabBar()) {
            this.getTabBar().setCartBadge('')
          }
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
.index-page {
  height: 100vh;
  background: linear-gradient(180deg, #fff3f1 0, #f5f5f5 260rpx);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.fixed-hero {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 20;
  background: linear-gradient(180deg, #fff3f1 0, #fff3f1 84%, rgba(255, 243, 241, 0));
}

.hero-shell {
  padding: 20rpx 20rpx 14rpx;
}

.shop-header {
  display: flex;
  align-items: center;
  padding: 26rpx;
  background: linear-gradient(135deg, #ff7d6e, #e64340);
  border-radius: 28rpx;
  box-shadow: 0 18rpx 34rpx rgba(230, 67, 64, 0.16);
}

.shop-avatar {
  width: 108rpx;
  height: 108rpx;
  border-radius: 50%;
  overflow: hidden;
  margin-right: 20rpx;
  flex-shrink: 0;
  border: 4rpx solid rgba(255, 255, 255, 0.26);
}

.avatar-img { width: 100%; height: 100%; }

.shop-info { flex: 1; min-width: 0; }

.shop-tag-row {
  display: flex;
  align-items: center;
  gap: 12rpx;
  margin-bottom: 10rpx;
}

.shop-tag,
.shop-status {
  padding: 6rpx 14rpx;
  border-radius: 999rpx;
  font-size: 20rpx;
  color: #fff;
  background: rgba(255, 255, 255, 0.16);
}

.shop-name {
  font-size: 34rpx;
  font-weight: 700;
  color: #fff;
  margin-bottom: 8rpx;
}

.shop-desc {
  font-size: 24rpx;
  color: rgba(255, 255, 255, 0.82);
  margin-bottom: 12rpx;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.shop-rating { display: flex; align-items: center; }
.stars { font-size: 22rpx; }
.rating-text, .review-count { font-size: 22rpx; color: rgba(255, 255, 255, 0.86); margin-left: 10rpx; }

.search-section {
  padding: 18rpx 0 0;
}

.search-bar {
  display: flex;
  align-items: center;
  background: rgba(255, 255, 255, 0.9);
  border-radius: 999rpx;
  padding: 20rpx 30rpx;
  box-shadow: 0 10rpx 24rpx rgba(0, 0, 0, 0.05);
}

.search-icon { margin-right: 12rpx; }
.search-input { flex: 1; font-size: 28rpx; color: #333; height: 48rpx; line-height: 48rpx; }
.clear-icon { color: #999; font-size: 24rpx; margin-left: 16rpx; }

.content-section {
  flex: 1;
  display: flex;
  overflow: hidden;
  padding: 0 20rpx 8rpx;
}

.page-content.with-fixed-hero {
  margin-top: 372rpx;
  padding-bottom: 122rpx;
}

.product-scroll {
  height: calc(100vh - 372rpx - 122rpx);
  padding-top: 10rpx;
}

.category-sidebar {
  width: 184rpx;
  background: rgba(255, 255, 255, 0.78);
  border-radius: 24rpx;
  overflow: hidden;
  box-shadow: 0 10rpx 24rpx rgba(0, 0, 0, 0.04);
}

.category-scroll { height: calc(100vh - 372rpx - 122rpx); }

.category-scroll-inner,
.product-scroll-inner {
  min-height: 100%;
}

.category-item {
  padding: 24rpx 16rpx;
  margin: 8rpx 8rpx 0;
  border-radius: 18rpx;
  color: #666;
  font-size: 24rpx;
  text-align: center;
}

.category-item.active {
  color: #e64340;
  background: #fff1ef;
  font-weight: 600;
}

.product-sidebar { flex: 1; margin-left: 16rpx; }

.product-card {
  display: flex;
  background: linear-gradient(180deg, #ffffff, #fff8f7);
  border-radius: 22rpx;
  padding: 18rpx;
  margin-bottom: 16rpx;
  box-shadow: 0 10rpx 26rpx rgba(230, 67, 64, 0.06);
  border: 1rpx solid rgba(230, 67, 64, 0.05);
}

.product-image-wrapper {
  position: relative;
  width: 152rpx;
  height: 152rpx;
  margin-right: 18rpx;
}

.product-image {
  width: 100%;
  height: 100%;
  border-radius: 18rpx;
  box-shadow: 0 8rpx 18rpx rgba(0, 0, 0, 0.06);
}

.product-badge {
  position: absolute;
  left: 10rpx;
  top: 10rpx;
  padding: 4rpx 10rpx;
  border-radius: 999rpx;
  background: rgba(230, 67, 64, 0.9);
  color: #fff;
  font-size: 18rpx;
}

.product-info { flex: 1; min-width: 0; display: flex; flex-direction: column; }

.product-name {
  font-size: 28rpx;
  color: #2f2f2f;
  font-weight: 600;
  line-height: 1.4;
}

.product-desc {
  margin-top: 8rpx;
  font-size: 22rpx;
  color: #999;
}

.product-meta {
  margin-top: 14rpx;
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
}

.product-price { font-size: 34rpx; color: #ff4d4f; font-weight: 700; }
.product-stock { font-size: 22rpx; color: #999; }

.product-action {
  display: flex;
  justify-content: flex-end;
  margin-top: auto;
}

.add-cart-btn {
  margin-top: 14rpx;
  width: 54rpx;
  height: 54rpx;
  border-radius: 50%;
  background: linear-gradient(135deg, #ff7463, #e64340);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 10rpx 18rpx rgba(230, 67, 64, 0.2);
}

.plus-icon {
  font-size: 34rpx;
  line-height: 1;
}

.empty-state {
  text-align: center;
  color: #999;
  padding: 120rpx 0;
}
</style>
