<template>
  <view class="user-management">
    <view class="page-bar">
      <view class="search-bar">
        <view class="search-input">
          <input class="input" v-model="searchKeyword" placeholder="搜索用户名/昵称/手机号" confirm-type="search" @confirm="handleSearch" />
          <text class="search-icon" @tap="handleSearch">🔍</text>
        </view>
        <button size="mini" class="btn-reset" @tap="resetSearch">重置</button>
      </view>
    </view>

    <view class="table-container">
      <view class="table-header">
        <view class="table-row header-row">
          <view class="table-cell">ID</view>
          <view class="table-cell">用户名</view>
          <view class="table-cell">昵称</view>
          <view class="table-cell">手机号</view>
          <view class="table-cell">角色</view>
          <view class="table-cell">状态</view>
          <view class="table-cell">操作</view>
        </view>
      </view>
      <view class="table-body">
        <view v-for="item in list" :key="item.id" class="table-row">
          <view class="table-cell">#{{ item.id }}</view>
          <view class="table-cell">{{ item.username || '-' }}</view>
          <view class="table-cell">{{ item.nickname || '-' }}</view>
          <view class="table-cell">{{ item.phone || '-' }}</view>
          <view class="table-cell">
            <text :class="['role-tag', getRoleClass(item.role)]">{{ getRoleText(item.role) }}</text>
          </view>
          <view class="table-cell">
            <text :class="['status-tag', item.status === 1 ? 'status-enabled' : 'status-disabled']">
              {{ item.status === 1 ? '启用' : '禁用' }}
            </text>
          </view>
          <view class="table-cell">
            <button size="mini" class="btn-edit" @tap="handleEdit(item)">设置</button>
          </view>
        </view>
      </view>
    </view>

    <view class="pagination">
      <button size="mini" @tap="prevPage" :disabled="page <= 1">上一页</button>
      <text class="page-info">第 {{ page }} 页 / 共 {{ Math.ceil(total / pageSize) }} 页</text>
      <button size="mini" @tap="nextPage" :disabled="page >= Math.ceil(total / pageSize)">下一页</button>
    </view>

    <view class="modal-mask" v-if="modalShow" @tap="closeModal">
      <view class="modal-content" @tap.stop>
        <view class="modal-header">
          <text class="modal-title">{{ modalTitle }}</text>
          <text class="modal-close" @tap="closeModal">×</text>
        </view>
        <view class="modal-body">
          <view class="form-item">
            <text class="form-label">角色</text>
            <picker :range="roleOptions" :value="roleIndex" @change="onRoleChange">
              <view class="form-picker"><text>{{ roleOptions[roleIndex] }}</text></view>
            </picker>
          </view>
          <view class="form-item">
            <text class="form-label">状态</text>
            <view class="radio-group">
              <label class="radio-item"><radio :checked="formData.status === 1" color="#007aff" @tap="formData.status = 1" /><text>启用</text></label>
              <label class="radio-item"><radio :checked="formData.status === 0" color="#007aff" @tap="formData.status = 0" /><text>禁用</text></label>
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

export default {
  name: 'UserManagement',
  data() {
    return {
      list: [], total: 0, page: 1, pageSize: 10, modalShow: false, modalTitle: '设置用户',
      formData: { id: null, role: 'user', status: 1 },
      roleOptions: ['普通用户', '商家', '管理员'], roleIndex: 0, searchKeyword: ''
    }
  },
  mounted() { this.loadList() },
  methods: {
    getRoleClass(role) { return { admin: 'role-admin', merchant: 'role-merchant', user: 'role-user' }[role] || 'role-user' },
    getRoleText(role) { return { admin: '管理员', merchant: '商家', user: '普通用户' }[role] || '普通用户' },
    async loadList() {
      try {
        const params = { page: this.page, pageSize: this.pageSize }
        if (this.searchKeyword) params.keyword = this.searchKeyword
        const res = await request({ url: '/api/admin/users', data: params })
        this.list = res?.data?.list || []
        this.total = res?.data?.total || 0
      } catch (error) { uni.showToast({ title: error.message || '加载失败', icon: 'none' }) }
    },
    handleSearch() { this.page = 1; this.loadList() },
    resetSearch() { this.searchKeyword = ''; this.page = 1; this.loadList() },
    onRoleChange(e) { this.roleIndex = e.detail.value },
    handleEdit(row) {
      this.modalTitle = '设置用户'
      this.formData = { id: row.id, role: row.role || 'user', status: row.status ?? 1 }
      this.roleIndex = { admin: 2, merchant: 1, user: 0 }[row.role] || 0
      this.modalShow = true
    },
    closeModal() { this.modalShow = false },
    async handleSubmit() {
      try {
        const roleValue = ['user', 'merchant', 'admin'][this.roleIndex]
        await request({
          url: `/api/admin/users/${this.formData.id}`,
          method: 'PUT',
          data: { role: roleValue, status: this.formData.status }
        })
        uni.showToast({ title: '设置成功', icon: 'success' })
        this.closeModal(); this.loadList()
      } catch (error) { uni.showToast({ title: error.message || '设置失败', icon: 'none' }) }
    },
    prevPage() { if (this.page > 1) { this.page--; this.loadList() } },
    nextPage() { if (this.page < Math.ceil(this.total / this.pageSize)) { this.page++; this.loadList() } }
  }
}
</script>

<style lang="scss" scoped>
.user-management { padding: 20px; }
.page-bar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.search-bar { display: flex; gap: 16px; padding: 16px; background: #fff; border-radius: 8px; align-items: center; flex: 1; }
.search-input { display: flex; align-items: center; border: 1px solid #ddd; border-radius: 8px; padding: 0 16px; background: #f5f5f5; flex: 1; .input { height: 40px; font-size: 14px; flex: 1; } .search-icon { font-size: 18px; padding-left: 10px; cursor: pointer; } }
.btn-reset { font-size: 14px; }
.table-container { background: #fff; border-radius: 8px; overflow: hidden; margin-bottom: 20px; }
.table-header { background: #f5f7fa; }
.table-row { display: flex; align-items: center; border-bottom: 1px solid #eee; &.header-row { font-weight: bold; color: #666; } }
.table-cell { padding: 16px; text-align: center; font-size: 14px; &:nth-child(1) { width: 80px; } &:nth-child(2), &:nth-child(3) { flex: 1; } &:nth-child(4) { width: 150px; } &:nth-child(5) { width: 120px; } &:nth-child(6) { width: 100px; } &:nth-child(7) { width: 100px; } }
.role-tag { display: inline-block; padding: 4px 12px; border-radius: 4px; font-size: 12px; &.role-admin { background: #fff3e0; color: #e65100; } &.role-merchant { background: #e8f5e9; color: #2e7d32; } &.role-user { background: #e3f2fd; color: #1976d2; } }
.status-tag { display: inline-block; padding: 4px 12px; border-radius: 4px; font-size: 12px; &.status-enabled { background: #e8f5e9; color: #2e7d32; } &.status-disabled { background: #fff3e0; color: #e65100; } }
.btn-edit { background: #2196f3; color: #fff; font-size: 12px; padding: 0 16px; height: 32px; line-height: 32px; }
.pagination { display: flex; align-items: center; justify-content: center; gap: 20px; padding: 20px; background: #fff; border-radius: 8px; .page-info { font-size: 14px; color: #666; } }
.modal-mask { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; }
.modal-content { background: #fff; border-radius: 12px; width: 90%; max-width: 400px; }
.modal-header { display: flex; align-items: center; justify-content: space-between; padding: 20px; border-bottom: 1px solid #eee; .modal-title { font-size: 18px; font-weight: bold; } .modal-close { font-size: 24px; color: #999; cursor: pointer; } }
.modal-body { padding: 20px; }
.form-item { margin-bottom: 20px; .form-label { display: block; font-size: 14px; color: #333; margin-bottom: 8px; } .form-picker { height: 40px; line-height: 40px; background: #f5f5f5; border-radius: 6px; padding: 0 12px; font-size: 14px; border: 1px solid #ddd; } .radio-group { display: flex; gap: 30px; .radio-item { display: flex; align-items: center; gap: 8px; font-size: 14px; } } }
.modal-footer { display: flex; gap: 12px; padding: 20px; border-top: 1px solid #eee; button { flex: 1; font-size: 16px; } .btn-cancel { background: #f5f5f5; color: #666; } .btn-confirm { background: #007aff; color: #fff; } }
</style>
