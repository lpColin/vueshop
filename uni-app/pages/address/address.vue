<template>
  <view class="address-page">
    <view class="list" v-if="list.length">
      <view class="card" v-for="item in list" :key="item.id" @tap="handleChoose(item)">
        <view class="row1">
          <view>
            <text class="name">{{ item.name }}</text>
            <text class="phone">{{ item.phone }}</text>
          </view>
          <text v-if="item.isDefault" class="tag">默认</text>
        </view>
        <view class="detail">{{ fullAddress(item) }}</view>
        <view class="ops">
          <view class="op" @tap.stop="setDefault(item)">设为默认</view>
          <view class="op" @tap.stop="editItem(item)">编辑</view>
          <view class="op del" @tap.stop="removeItem(item)">删除</view>
        </view>
      </view>
    </view>
    <view v-else class="empty">暂无收货地址</view>

    <view class="footer">
      <button class="add-btn" @tap="openAdd">新增地址</button>
    </view>

    <view v-if="showForm" class="mask" @tap="closeForm">
      <view class="form" @tap.stop>
        <view class="title">{{ form.id ? '编辑地址' : '新增地址' }}</view>
        <input class="input" v-model="form.name" placeholder="收货人" />
        <input class="input" v-model="form.phone" placeholder="手机号" />
        <input class="input" v-model="form.province" placeholder="省" />
        <input class="input" v-model="form.city" placeholder="市" />
        <input class="input" v-model="form.district" placeholder="区" />
        <input class="input" v-model="form.detail" placeholder="详细地址" />
        <view class="switch-row" @tap="form.isDefault = !form.isDefault">
          <text>设为默认地址</text>
          <text>{{ form.isDefault ? '已开启' : '未开启' }}</text>
        </view>
        <view class="btns">
          <button class="btn ghost" @tap="closeForm">取消</button>
          <button class="btn" @tap="submitForm">保存</button>
        </view>
      </view>
    </view>
  </view>
</template>

<script>
import request from '@/utils/http'
import { getToken } from '@/utils/auth'

const emptyForm = () => ({
  id: null,
  name: '',
  phone: '',
  province: '',
  city: '',
  district: '',
  detail: '',
  isDefault: false
})

export default {
  data() {
    return {
      selectMode: false,
      list: [],
      showForm: false,
      form: emptyForm()
    }
  },
  onLoad(options) {
    this.selectMode = options?.select === '1'
  },
  onShow() {
    this.loadList()
  },
  methods: {
    async loadList() {
      if (!getToken()) {
        uni.showToast({ title: '请先登录', icon: 'none' })
        setTimeout(() => uni.navigateTo({ url: '/pages/login/login' }), 250)
        return
      }
      try {
        const res = await request({ url: '/api/address' })
        this.list = Array.isArray(res?.data) ? res.data : []
      } catch (error) {
        uni.showToast({ title: error.message || '加载失败', icon: 'none' })
      }
    },
    fullAddress(item) {
      return `${item.province || ''}${item.city || ''}${item.district || ''}${item.detail || ''}`
    },
    handleChoose(item) {
      if (!this.selectMode) return
      const selected = {
        id: item.id,
        name: item.name,
        phone: item.phone,
        detail: this.fullAddress(item),
        isDefault: item.isDefault
      }
      uni.setStorageSync('selectedAddressForOrder', selected)
      uni.navigateBack()
    },
    openAdd() {
      this.form = emptyForm()
      this.showForm = true
    },
    editItem(item) {
      this.form = {
        id: item.id,
        name: item.name,
        phone: item.phone,
        province: item.province,
        city: item.city,
        district: item.district,
        detail: item.detail,
        isDefault: !!item.isDefault
      }
      this.showForm = true
    },
    closeForm() {
      this.showForm = false
    },
    async submitForm() {
      if (!this.form.name || !this.form.phone || !this.form.detail) {
        uni.showToast({ title: '请填写完整信息', icon: 'none' })
        return
      }
      try {
        if (this.form.id) {
          await request({
            url: `/api/address/${this.form.id}`,
            method: 'PUT',
            data: this.form
          })
        } else {
          await request({
            url: '/api/address',
            method: 'POST',
            data: this.form
          })
        }
        this.showForm = false
        await this.loadList()
        uni.showToast({ title: '保存成功', icon: 'success' })
      } catch (error) {
        uni.showToast({ title: error.message || '保存失败', icon: 'none' })
      }
    },
    async setDefault(item) {
      try {
        await request({ url: `/api/address/${item.id}/default`, method: 'POST' })
        await this.loadList()
      } catch (error) {
        uni.showToast({ title: error.message || '操作失败', icon: 'none' })
      }
    },
    async removeItem(item) {
      try {
        await request({ url: `/api/address/${item.id}`, method: 'DELETE' })
        await this.loadList()
      } catch (error) {
        uni.showToast({ title: error.message || '删除失败', icon: 'none' })
      }
    }
  }
}
</script>

<style lang="scss" scoped>
.address-page { min-height: 100vh; background: #f5f5f5; padding: 16rpx; padding-bottom: 120rpx; }
.card { background: #fff; border-radius: 12rpx; padding: 20rpx; margin-bottom: 14rpx; }
.row1 { display: flex; justify-content: space-between; align-items: center; }
.name { color: #333; font-size: 30rpx; font-weight: 600; }
.phone { margin-left: 14rpx; color: #666; font-size: 26rpx; }
.tag { color: #e64340; border: 1rpx solid #e64340; border-radius: 8rpx; padding: 2rpx 10rpx; font-size: 20rpx; }
.detail { margin-top: 10rpx; color: #666; font-size: 24rpx; }
.ops { margin-top: 14rpx; display: flex; gap: 12rpx; }
.op { font-size: 24rpx; color: #666; padding: 6rpx 16rpx; border: 1rpx solid #ddd; border-radius: 20rpx; }
.op.del { color: #ff4d4f; border-color: #ffb9b9; }
.empty { text-align: center; color: #999; margin-top: 140rpx; }
.footer { position: fixed; left: 0; right: 0; bottom: 0; background: #fff; padding: 14rpx 20rpx; border-top: 1rpx solid #eee; }
.add-btn { background: #e64340; color: #fff; border-radius: 10rpx; }
.mask { position: fixed; inset: 0; background: rgba(0,0,0,.45); display: flex; align-items: flex-end; }
.form { width: 100%; background: #fff; border-radius: 24rpx 24rpx 0 0; padding: 24rpx; }
.title { font-size: 32rpx; font-weight: 700; margin-bottom: 14rpx; }
.input { height: 78rpx; border: 1rpx solid #ececec; border-radius: 10rpx; padding: 0 16rpx; margin-bottom: 12rpx; }
.switch-row { display: flex; justify-content: space-between; color: #666; font-size: 26rpx; margin: 10rpx 0 20rpx; }
.btns { display: flex; gap: 12rpx; }
.btn { flex: 1; background: #e64340; color: #fff; }
.btn.ghost { background: #f1f1f1; color: #666; }
</style>
