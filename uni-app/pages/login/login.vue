<template>
  <view class="auth-page">
    <view class="card">
      <view class="title">账号登录</view>
      <input class="input" v-model="form.username" placeholder="请输入用户名" />
      <input class="input" v-model="form.password" placeholder="请输入密码" password />
      <button class="btn" :disabled="loading" @tap="submit">{{ loading ? '登录中...' : '登录' }}</button>
      <view class="tip-row">
        <text>还没有账号？</text>
        <text class="link" @tap="toRegister">去注册</text>
      </view>
    </view>
  </view>
</template>

<script>
import request from '@/utils/http'
import { setToken, setUserInfo } from '@/utils/auth'

export default {
  data() {
    return {
      loading: false,
      form: {
        username: '',
        password: ''
      }
    }
  },
  methods: {
    async submit() {
      if (!this.form.username || !this.form.password) {
        uni.showToast({ title: '请完整填写账号和密码', icon: 'none' })
        return
      }

      this.loading = true
      try {
        const res = await request({
          url: '/api/auth/login',
          method: 'POST',
          data: this.form
        })

        const token = res?.data?.token || ''
        const user = res?.data?.user || null
        if (!token || !user) {
          throw new Error('登录响应缺少必要字段')
        }

        setToken(token)
        setUserInfo(user)
        uni.showToast({ title: '登录成功', icon: 'success' })
        setTimeout(() => {
          uni.switchTab({ url: '/pages/user/user' })
        }, 300)
      } catch (error) {
        uni.showToast({ title: error.message || '登录失败', icon: 'none' })
      } finally {
        this.loading = false
      }
    },
    toRegister() {
      uni.navigateTo({ url: '/pages/register/register' })
    }
  }
}
</script>

<style lang="scss" scoped>
.auth-page {
  min-height: 100vh;
  background: #f5f5f5;
  padding: 80rpx 30rpx;
}

.card {
  background: #fff;
  border-radius: 16rpx;
  padding: 40rpx 30rpx;
}

.title {
  font-size: 36rpx;
  font-weight: 700;
  color: #222;
  margin-bottom: 28rpx;
}

.input {
  height: 88rpx;
  border: 1rpx solid #e5e5e5;
  border-radius: 12rpx;
  padding: 0 24rpx;
  margin-bottom: 18rpx;
  background: #fff;
}

.btn {
  margin-top: 10rpx;
  background: #e64340;
  color: #fff;
  border-radius: 12rpx;
  font-size: 30rpx;
}

.tip-row {
  margin-top: 26rpx;
  font-size: 26rpx;
  color: #666;
}

.link {
  color: #e64340;
  margin-left: 8rpx;
}
</style>
