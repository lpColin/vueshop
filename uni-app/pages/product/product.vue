<template>
  <view class="product-page">
    <scroll-view scroll-y class="product-scroll">
      <!-- 商品图片 -->
      <view class="product-images">
        <swiper class="image-swiper" indicator-dots :autoplay="true" :circular="true">
          <swiper-item v-for="(image, index) in productImages" :key="index">
            <image :src="image" mode="aspectFill" class="swiper-image" />
          </swiper-item>
        </swiper>
        <view class="image-indicator">
          <text class="current">{{ currentImageIndex + 1 }}</text>
          <text class="total">/{{ productImages.length }}</text>
        </view>
      </view>

      <!-- 商品信息 -->
      <view class="product-info-section">
        <view class="product-price">
          <text class="price-symbol">¥</text>
          <text class="price-value">{{ product.price }}</text>
          <text class="original-price">¥{{ product.originalPrice }}</text>
        </view>
        <view class="product-name">{{ product.name }}</view>
        <view class="product-desc">{{ product.description }}</view>
        <view class="product-sales">
          <text class="sales-text">已售 {{ product.sales }}件</text>
          <text class="stock-text">库存 {{ product.stock }}件</text>
        </view>
      </view>

      <!-- 规格选择 -->
      <view class="spec-section" @tap="showSpecModal">
        <view class="spec-left">
          <text class="spec-label">已选</text>
          <text class="spec-value">{{ selectedSpec }}</text>
        </view>
        <text class="spec-arrow">›</text>
      </view>

      <!-- 配送信息 -->
      <view class="delivery-section" @tap="showDeliveryInfo">
        <view class="delivery-left">
          <text class="delivery-label">配送至</text>
          <text class="delivery-value">{{ deliveryAddress }}</text>
        </view>
        <text class="delivery-arrow">›</text>
      </view>

      <!-- 服务保障 -->
      <view class="service-section">
        <view class="service-item">
          <text class="service-icon">✓</text>
          <text class="service-text">正品保证</text>
        </view>
        <view class="service-item">
          <text class="service-icon">✓</text>
          <text class="service-text">极速退款</text>
        </view>
        <view class="service-item">
          <text class="service-icon">✓</text>
          <text class="service-text">七天无理由退换</text>
        </view>
      </view>

      <!-- 商品详情 -->
      <view class="detail-section">
        <view class="detail-header">
          <text class="detail-title">商品详情</text>
        </view>
        <view class="detail-content">
          <view class="detail-item" v-for="(item, index) in detailList" :key="index">
            <image :src="item" mode="widthFix" class="detail-image" />
          </view>
        </view>
      </view>

      <!-- 推荐商品 -->
      <view class="recommend-section">
        <view class="section-header">
          <text class="section-title">推荐商品</text>
        </view>
        <view class="recommend-list">
          <view 
            v-for="item in recommendProducts" 
            :key="item.id"
            class="recommend-item"
            @tap="goToProduct(item.id)"
          >
            <image :src="item.image" mode="aspectFill" class="recommend-image" />
            <view class="recommend-info">
              <view class="recommend-name">{{ item.name }}</view>
              <view class="recommend-price">¥{{ item.price }}</view>
            </view>
          </view>
        </view>
      </view>
    </scroll-view>

    <!-- 底部操作栏 -->
    <view class="product-footer">
      <view class="footer-left">
        <view class="footer-btn" @tap="goToCart">
          <text class="btn-icon">🛒</text>
          <text class="btn-text">购物车</text>
          <view v-if="cartCount > 0" class="cart-badge">{{ cartCount }}</view>
        </view>
        <view class="footer-btn" @tap="goToFavorites">
          <text class="btn-icon">❤️</text>
          <text class="btn-text">收藏</text>
        </view>
      </view>
      <view class="footer-right">
        <view class="action-btn add-cart" @tap="addToCart">
          <text class="action-text">加入购物车</text>
        </view>
        <view class="action-btn buy-now" @tap="buyNow">
          <text class="action-text">立即购买</text>
        </view>
      </view>
    </view>

    <!-- 规格选择弹窗 -->
    <view v-if="showSpec" class="spec-modal" @tap="hideSpecModal">
      <view class="spec-content" @tap.stop>
        <view class="spec-header">
          <image :src="product.image" mode="aspectFill" class="spec-product-image" />
          <view class="spec-price-info">
            <view class="spec-price">¥{{ product.price }}</view>
            <view class="spec-stock">库存：{{ product.stock }}件</view>
          </view>
          <view class="spec-close" @tap="hideSpecModal">
            <text class="close-icon">✕</text>
          </view>
        </view>
        <view class="spec-body">
          <view class="spec-title">规格</view>
          <view class="spec-options">
            <view 
              v-for="spec in specOptions" 
              :key="spec"
              class="spec-option"
              :class="{ active: selectedSpec === spec }"
              @tap="selectSpec(spec)"
            >
              {{ spec }}
            </view>
          </view>
          <view class="spec-quantity">
            <view class="spec-title">数量</view>
            <view class="quantity-control">
              <view class="control-btn" :class="{ disabled: quantity <= 1 }" @tap="decreaseQuantity">
                <text class="btn-icon">-</text>
              </view>
              <view class="control-value">{{ quantity }}</view>
              <view class="control-btn" :class="{ disabled: quantity >= product.stock }" @tap="increaseQuantity">
                <text class="btn-icon">+</text>
              </view>
            </view>
          </view>
        </view>
        <view class="spec-actions">
          <view class="spec-action-btn add-cart" @tap="addToCartFromSpec">
            <text class="action-btn-text">加入购物车</text>
          </view>
          <view class="spec-action-btn buy-now" @tap="buyNowFromSpec">
            <text class="action-btn-text">立即购买</text>
          </view>
        </view>
      </view>
    </view>
  </view>
</template>

<script>
import request from '@/utils/http'
import { getToken } from '@/utils/auth'

export default {
  data() {
    return {
      productId: '',
      product: {
        id: 1,
        name: '红富士苹果 5 斤装',
        price: 39.00,
        originalPrice: 59.00,
        description: '产地直供，新鲜采摘，脆甜多汁',
        sales: 500,
        stock: 99,
        image: '/static/images/product1.png'
      },
      productImages: [
        '/static/images/product1.png',
        '/static/images/product2.png',
        '/static/images/product3.png'
      ],
      currentImageIndex: 0,
      selectedSpec: '默认规格',
      specOptions: ['默认规格', '精品装 (+¥10)', '礼盒装 (+¥20)'],
      deliveryAddress: '北京市朝阳区',
      quantity: 1,
      showSpec: false,
      cartCount: 0,
      detailList: [
        '/static/images/product1.png',
        '/static/images/product2.png',
        '/static/images/product3.png'
      ],
      recommendProducts: [
        { id: 2, name: '进口香蕉 1 把', price: 15.80, image: '/static/images/product2.png' },
        { id: 3, name: '新鲜草莓 500g', price: 28.00, image: '/static/images/product3.png' },
        { id: 4, name: '绿心猕猴桃 12 个', price: 35.00, image: '/static/images/product4.png' },
        { id: 5, name: '有机生菜 2 颗', price: 12.00, image: '/static/images/product5.png' }
      ]
    }
  },
  onLoad(options) {
    if (options.id) {
      this.productId = options.id
      this.loadProduct(options.id)
    }
    this.loadCartCount()
  },
  methods: {
    async loadProduct(id) {
      try {
        const res = await request({ url: `/api/products/${id}` })
        const p = res?.data
        if (!p) return

        this.product = {
          id: p.id,
          name: p.name,
          price: Number(p.price || 0),
          originalPrice: Number(p.originalPrice || p.price || 0),
          description: p.description || '',
          sales: Number(p.sales || 0),
          stock: Number(p.stock || 0),
          image: p.image || '/static/images/product1.png'
        }

        this.productImages = this.normalizeImages(p.images, this.product.image)
        this.detailList = this.productImages
      } catch (error) {
        console.warn('[商品详情] 获取商品失败，使用本地兜底', error)
      }
      this.loadRecommend()
    },
    async loadRecommend() {
      try {
        const res = await request({ url: '/api/products?page=1&pageSize=8' })
        const list = Array.isArray(res?.data?.list) ? res.data.list : []
        this.recommendProducts = list
          .filter((x) => Number(x.id) !== Number(this.product.id))
          .slice(0, 4)
          .map((x) => ({
            id: x.id,
            name: x.name,
            price: Number(x.price || 0),
            image: x.image || '/static/images/product1.png'
          }))
      } catch (error) {
        console.warn('[商品详情] 获取推荐商品失败', error)
      }
    },
    normalizeImages(images, fallback) {
      if (!images) return [fallback]
      try {
        const arr = typeof images === 'string' ? JSON.parse(images) : images
        if (Array.isArray(arr) && arr.length) {
          return arr
        }
      } catch (e) {
        // ignore
      }
      return [fallback]
    },
    async loadCartCount() {
      if (getToken()) {
        try {
          const res = await request({ url: '/api/cart' })
          const list = res?.data?.list || []
          this.cartCount = list.reduce((sum, item) => sum + Number(item.quantity || 0), 0)
          return
        } catch (error) {
          console.warn('[商品详情] 获取购物车数量失败，回退本地', error)
        }
      }
      const cartItems = uni.getStorageSync('cartItems') || []
      this.cartCount = cartItems.reduce((sum, item) => sum + Number(item.quantity || 0), 0)
    },
    showSpecModal() {
      this.showSpec = true
    },
    hideSpecModal() {
      this.showSpec = false
    },
    selectSpec(spec) {
      this.selectedSpec = spec
    },
    increaseQuantity() {
      if (this.quantity < this.product.stock) {
        this.quantity += 1
      }
    },
    decreaseQuantity() {
      if (this.quantity > 1) {
        this.quantity -= 1
      }
    },
    showDeliveryInfo() {
      uni.showToast({
        title: '功能开发中',
        icon: 'none'
      })
    },
    goToProduct(id) {
      uni.navigateTo({
        url: `/pages/product/product?id=${id}`
      })
    },
    goToCart() {
      uni.switchTab({
        url: '/pages/cart/cart'
      })
    },
    goToFavorites() {
      uni.showToast({
        title: '已加入收藏',
        icon: 'success'
      })
    },
    async addToCart() {
      const token = getToken()
      
      // 已登录用户使用后端购物车
      if (token) {
        try {
          await request({
            url: '/api/cart',
            method: 'POST',
            data: {
              productId: this.product.id,
              quantity: this.quantity
            }
          })
          await this.loadCartCount()
          this.quantity = 1
          uni.showToast({ title: '已加入购物车', icon: 'success' })
          return
        } catch (error) {
          console.warn('[加入购物车] 后端接口失败，回退本地', error)
        }
      }

      // 未登录用户使用本地存储
      const cartItems = uni.getStorageSync('cartItems') || []
      const existingIndex = cartItems.findIndex(item => item.id === this.product.id)
      
      if (existingIndex > -1) {
        cartItems[existingIndex].quantity += this.quantity
      } else {
        cartItems.push({
          id: this.product.id,
          name: this.product.name,
          price: this.product.price,
          image: this.product.image,
          quantity: this.quantity,
          stock: this.product.stock,
          spec: this.selectedSpec
        })
      }
      
      uni.setStorageSync('cartItems', cartItems)
      await this.loadCartCount()
      this.quantity = 1
      
      uni.showToast({
        title: '已加入购物车',
        icon: 'success'
      })
    },
    async addToCartFromSpec() {
      await this.addToCart()
      this.hideSpecModal()
    },
    buyNow() {
      const checkoutItems = [{
        id: this.product.id,
        name: this.product.name,
        price: this.product.price,
        image: this.product.image,
        quantity: this.quantity,
        stock: this.product.stock,
        spec: this.selectedSpec
      }]
      uni.setStorageSync('checkoutItems', checkoutItems)
      uni.navigateTo({
        url: '/pages/order/order'
      })
    },
    buyNowFromSpec() {
      this.buyNow()
      this.hideSpecModal()
    }
  }
}
</script>

<style lang="scss" scoped>
.product-page {
  min-height: 100vh;
  background-color: #f5f5f5;
  padding-bottom: 120rpx;
}

.product-scroll {
  height: calc(100vh - 120rpx);
}

.product-images {
  position: relative;
  background-color: #ffffff;
}

.image-swiper {
  width: 100%;
  height: 750rpx;
}

.swiper-image {
  width: 100%;
  height: 100%;
}

.image-indicator {
  position: absolute;
  bottom: 24rpx;
  right: 24rpx;
  background-color: rgba(0, 0, 0, 0.3);
  border-radius: 28rpx;
  padding: 8rpx 20rpx;
  display: flex;
  align-items: center;
}

.current {
  font-size: 28rpx;
  color: #ffffff;
  font-weight: bold;
}

.total {
  font-size: 24rpx;
  color: rgba(255, 255, 255, 0.7);
  margin-left: 4rpx;
}

.product-info-section {
  background-color: #ffffff;
  padding: 32rpx 24rpx;
  margin-bottom: 20rpx;
}

.product-price {
  display: flex;
  align-items: baseline;
  margin-bottom: 16rpx;
}

.price-symbol {
  font-size: 28rpx;
  color: #ff4400;
  font-weight: bold;
}

.price-value {
  font-size: 56rpx;
  color: #ff4400;
  font-weight: bold;
}

.original-price {
  font-size: 26rpx;
  color: #999999;
  text-decoration: line-through;
  margin-left: 16rpx;
}

.product-name {
  font-size: 32rpx;
  font-weight: bold;
  color: #333333;
  margin-bottom: 12rpx;
}

.product-desc {
  font-size: 26rpx;
  color: #666666;
  margin-bottom: 20rpx;
  line-height: 1.6;
}

.product-sales {
  display: flex;
  justify-content: space-between;
}

.sales-text, .stock-text {
  font-size: 24rpx;
  color: #999999;
}

.spec-section {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background-color: #ffffff;
  padding: 32rpx 24rpx;
  margin-bottom: 20rpx;
}

.spec-left {
  display: flex;
  align-items: center;
}

.spec-label {
  font-size: 28rpx;
  color: #666666;
  margin-right: 16rpx;
}

.spec-value {
  font-size: 28rpx;
  color: #333333;
}

.spec-arrow, .delivery-arrow {
  font-size: 36rpx;
  color: #cccccc;
}

.delivery-section {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background-color: #ffffff;
  padding: 32rpx 24rpx;
  margin-bottom: 20rpx;
}

.delivery-label {
  font-size: 28rpx;
  color: #666666;
  margin-right: 16rpx;
}

.delivery-value {
  font-size: 28rpx;
  color: #333333;
}

.service-section {
  display: flex;
  flex-wrap: wrap;
  background-color: #ffffff;
  padding: 24rpx;
  margin-bottom: 20rpx;
}

.service-item {
  display: flex;
  align-items: center;
  width: 33.33%;
  margin-bottom: 16rpx;
}

.service-icon {
  color: #e64340;
  margin-right: 8rpx;
  font-size: 24rpx;
}

.service-text {
  font-size: 24rpx;
  color: #666666;
}

.detail-section {
  background-color: #ffffff;
  margin-bottom: 20rpx;
}

.detail-header {
  padding: 24rpx;
  border-bottom: 1rpx solid #f5f5f5;
}

.detail-title {
  font-size: 30rpx;
  font-weight: bold;
  color: #333333;
}

.detail-content {
  padding: 24rpx;
}

.detail-item {
  margin-bottom: 20rpx;
}

.detail-image {
  width: 100%;
  border-radius: 12rpx;
}

.recommend-section {
  background-color: #ffffff;
  padding: 32rpx 24rpx;
}

.section-title {
  font-size: 30rpx;
  font-weight: bold;
  color: #333333;
}

.recommend-list {
  display: flex;
  flex-wrap: wrap;
  margin-top: 24rpx;
}

.recommend-item {
  width: 48%;
  margin-right: 4%;
  margin-bottom: 24rpx;
}

.recommend-item:nth-child(2n) {
  margin-right: 0;
}

.recommend-image {
  width: 100%;
  height: 300rpx;
  border-radius: 12rpx;
  background-color: #f5f5f5;
}

.recommend-info {
  padding: 16rpx 0;
}

.recommend-name {
  font-size: 26rpx;
  color: #333333;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  margin-bottom: 8rpx;
}

.recommend-price {
  font-size: 28rpx;
  color: #ff4400;
  font-weight: bold;
}

.product-footer {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  height: 100rpx;
  background-color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24rpx;
  box-shadow: 0 -2rpx 10rpx rgba(0, 0, 0, 0.05);
  z-index: 100;
}

.footer-left {
  display: flex;
  align-items: center;
}

.footer-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-right: 32rpx;
  position: relative;
}

.btn-icon {
  font-size: 36rpx;
  margin-bottom: 4rpx;
}

.btn-text {
  font-size: 20rpx;
  color: #666666;
}

.cart-badge {
  position: absolute;
  top: -8rpx;
  right: -16rpx;
  min-width: 28rpx;
  height: 28rpx;
  line-height: 28rpx;
  text-align: center;
  background-color: #ff4400;
  color: #ffffff;
  font-size: 20rpx;
  border-radius: 14rpx;
  padding: 0 6rpx;
}

.footer-right {
  display: flex;
  align-items: center;
}

.action-btn {
  height: 72rpx;
  padding: 0 32rpx;
  border-radius: 36rpx;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-left: 16rpx;
}

.action-text {
  font-size: 26rpx;
  color: #ffffff;
  font-weight: 500;
}

.add-cart {
  background: linear-gradient(90deg, #ffb36b, #ff9933);
}

.buy-now {
  background: linear-gradient(90deg, #ff6b6b, #e64340);
}

/* 规格弹窗样式 */
.spec-modal {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: flex-end;
  justify-content: center;
  z-index: 1000;
}

.spec-content {
  background-color: #ffffff;
  border-radius: 32rpx 32rpx 0 0;
  width: 100%;
  animation: slideUp 0.3s ease;
}

@keyframes slideUp {
  from {
    transform: translateY(100%);
  }
  to {
    transform: translateY(0);
  }
}

.spec-header {
  display: flex;
  padding: 32rpx 24rpx;
  border-bottom: 1rpx solid #f5f5f5;
  position: relative;
}

.spec-product-image {
  width: 160rpx;
  height: 160rpx;
  border-radius: 12rpx;
  margin-right: 20rpx;
}

.spec-price-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.spec-price {
  font-size: 40rpx;
  color: #ff4400;
  font-weight: bold;
}

.spec-stock {
  font-size: 24rpx;
  color: #999999;
  margin-top: 8rpx;
}

.spec-close {
  position: absolute;
  top: 24rpx;
  right: 24rpx;
  width: 48rpx;
  height: 48rpx;
  display: flex;
  align-items: center;
  justify-content: center;
}

.close-icon {
  font-size: 32rpx;
  color: #999999;
}

.spec-body {
  padding: 24rpx;
}

.spec-title {
  font-size: 28rpx;
  color: #333333;
  margin-bottom: 16rpx;
}

.spec-options {
  display: flex;
  flex-wrap: wrap;
  margin-bottom: 32rpx;
}

.spec-option {
  padding: 16rpx 32rpx;
  border: 1rpx solid #e0e0e0;
  border-radius: 8rpx;
  margin-right: 16rpx;
  margin-bottom: 16rpx;
  font-size: 26rpx;
  color: #666666;
}

.spec-option.active {
  border-color: #e64340;
  background-color: #fff5f5;
  color: #e64340;
}

.spec-quantity {
  margin-bottom: 32rpx;
}

.quantity-control {
  display: flex;
  align-items: center;
}

.control-btn {
  width: 56rpx;
  height: 56rpx;
  border: 1rpx solid #e0e0e0;
  border-radius: 8rpx;
  display: flex;
  align-items: center;
  justify-content: center;
}

.control-btn.disabled {
  border-color: #f0f0f0;
  background-color: #f9f9f9;
}

.control-btn.disabled .btn-icon {
  color: #cccccc;
}

.btn-icon {
  font-size: 32rpx;
  color: #666666;
}

.control-value {
  min-width: 80rpx;
  text-align: center;
  font-size: 28rpx;
  color: #333333;
}

.spec-actions {
  display: flex;
  padding: 24rpx;
  gap: 20rpx;
}

.spec-action-btn {
  flex: 1;
  height: 80rpx;
  border-radius: 40rpx;
  display: flex;
  align-items: center;
  justify-content: center;
}

.spec-action-btn.add-cart {
  background: linear-gradient(90deg, #ffb36b, #ff9933);
}

.spec-action-btn.buy-now {
  background: linear-gradient(90deg, #ff6b6b, #e64340);
}

.action-btn-text {
  font-size: 28rpx;
  color: #ffffff;
  font-weight: 500;
}
</style>