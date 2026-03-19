<template>
  <view class="auth-page">
    <view class="card">
      <view class="title">账号注册</view>
      <input class="input" v-model="form.username" placeholder="请输入用户名" />
      <input class="input" v-model="form.nickname" placeholder="请输入昵称（选填）" />
      <input class="input" v-model="form.password" placeholder="请输入密码" password />
      <input class="input" v-model="form.confirmPassword" placeholder="请确认密码" password />
      <button class="btn" :disabled="loading" @tap="submit">{{ loading ? '注册中...' : '注册' }}</button>
      <view class="tip-row">
        <text>已有账号？</text>
        <text class="link" @tap="toLogin">去登录</text>
      </view>
    </view>
  </view>
</template>

<script>
import request from '@/utils/http'

export default {
  data() {
    return {
      loading: false,
      form: {
        username: '',
        nickname: '',
        password: '',
        confirmPassword: ''
      }
    }
  },
  methods: {
    async submit() {
      if (!this.form.username || !this.form.password) {
        uni.showToast({ title: '用户名和密码必填', icon: 'none' })
        return
      }
      if (this.form.password !== this.form.confirmPassword) {
        uni.showToast({ title: '两次密码不一致', icon: 'none' })
        return
      }

      this.loading = true
      try {
        await request({
          url: '/api/auth/register',
          method: 'POST',
          data: {
            username: this.form.username,
            password: this.form.password,
            nickname: this.form.nickname || undefined
          }
        })
        uni.showToast({ title: '注册成功，请登录', icon: 'success' })
        setTimeout(() => {
          uni.redirectTo({ url: '/pages/login/login' })
        }, 500)
      } catch (error) {
        uni.showToast({ title: error.message || '注册失败', icon: 'none' })
      } finally {
        this.loading = false
      }
    },
    toLogin() {
      uni.redirectTo({ url: '/pages/login/login' })
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
