<template>
  <view class="shop-info-page">
    <view class="card">
      <view class="top">
        <image class="avatar" :src="getFullImageUrl(shopData.avatar)" mode="aspectFill" />
        <view class="title">{{ shopData.name }}</view>
        <view class="sub">{{ shopData.description }}</view>
      </view>

      <view class="row">
        <text class="label">店铺评分</text>
        <view class="value score-wrap">
          <text class="stars">{{ getStars(shopData.rating) }}</text>
          <text class="score">{{ shopData.rating }}分</text>
        </view>
      </view>

      <view class="row">
        <text class="label">关注人数</text>
        <text class="value">{{ shopData.reviewCount }}人</text>
      </view>

      <view class="row">
        <text class="label">营业状态</text>
        <text :class="['value', shopData.status === 1 ? 'open' : 'closed']">
          {{ shopData.status === 1 ? '营业中' : '已打烊' }}
        </text>
      </view>

      <view class="row" @tap="callShop">
        <text class="label">联系电话</text>
        <text class="value">{{ shopData.phone || '暂无' }}</text>
      </view>

      <view class="row">
        <text class="label">营业时间</text>
        <text class="value">{{ shopData.businessHours || '暂无' }}</text>
      </view>

      <view class="row">
        <text class="label">店铺地址</text>
        <text class="value">{{ shopData.address || '暂无' }}</text>
      </view>

      <view class="btns">
        <button class="btn gray" @tap="callShop">联系商家</button>
        <button class="btn red" @tap="enterShop">进入店铺</button>
      </view>
    </view>
  </view>
</template>

<script>
import request from '@/utils/http'

export default {
  data() {
    return {
      shopId: 1,
      shopData: {
        id: 1,
        name: '',
        description: '',
        avatar: '',
        logo: '',
        rating: 5.0,
        reviewCount: 0,
        phone: '',
        businessHours: '',
        address: '',
        status: 1
      }
    }
  },
  onLoad() {
    this.loadShopInfo()
  },
  methods: {
    getFullImageUrl(path) {
      if (!path) return ''
      if (path.startsWith('http://') || path.startsWith('https://')) return path
      if (path.startsWith('/')) {
        return 'http://localhost:5162' + path
      }
      return 'http://localhost:5162/' + path
    },
    getStars(rating) {
      const fullStars = Math.floor(rating)
      const halfStar = rating % 1 >= 0.5 ? '★' : ''
      const emptyStars = 5 - fullStars - (halfStar ? 1 : 0)
      return '★'.repeat(fullStars) + halfStar + '☆'.repeat(emptyStars)
    },
    async loadShopInfo() {
      try {
        const res = await request({ url: `/api/admin/shops?page=1&pageSize=10` })
        const shops = res?.data?.list || []
        if (shops.length > 0) {
          this.shopData = {
            ...this.shopData,
            ...shops[0]
          }
        }
      } catch (error) {
        console.error('[商城信息] 获取店铺信息失败', error)
        uni.showToast({ title: '加载失败', icon: 'none' })
      }
    },
    callShop() {
      if (this.shopData.phone) {
        uni.makePhoneCall({
          phoneNumber: this.shopData.phone
        })
      } else {
        uni.showToast({ title: '暂无联系电话', icon: 'none' })
      }
    },
    enterShop() {
      uni.switchTab({
        url: '/pages/index/index'
      })
    }
  }
}
</script>

<style lang="scss" scoped>
.shop-info-page {
  min-height: 100vh;
  background: #efefef;
  padding: 26rpx;
}

.card {
  background: #ffffff;
  border-radius: 24rpx;
  padding: 28rpx 24rpx 32rpx;
  box-shadow: 0 4rpx 20rpx rgba(0, 0, 0, 0.05);
}

.top {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 10rpx 0 24rpx;
}

.avatar {
  width: 108rpx;
  height: 108rpx;
  border-radius: 54rpx;
}

.title {
  margin-top: 14rpx;
  font-size: 38rpx;
  color: #333;
  font-weight: 700;
}

.sub {
  margin-top: 10rpx;
  color: #999;
  font-size: 24rpx;
}

.row {
  height: 88rpx;
  border-top: 1rpx solid #f0f0f0;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.label {
  color: #6e6e6e;
  font-size: 28rpx;
}

.value {
  color: #666;
  font-size: 28rpx;
}

.score-wrap {
  display: flex;
  align-items: center;
}

.stars {
  color: #f6bd16;
  font-size: 24rpx;
}

.score {
  margin-left: 10rpx;
  color: #f5a623;
  font-size: 30rpx;
  font-weight: 700;
}

.open {
  color: #35b24a;
  font-weight: 600;
}

.closed {
  color: #999;
  font-weight: 600;
}

.btns {
  margin-top: 26rpx;
  display: flex;
  gap: 18rpx;
}

.btn {
  flex: 1;
  height: 76rpx;
  border-radius: 38rpx;
  font-size: 28rpx;
  line-height: 76rpx;
}

.gray {
  background: #f0f0f0;
  color: #666;
}

.red {
  background: #e64340;
  color: #fff;
}
</style>
