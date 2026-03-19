<template>
  <view class="page">
    <view class="page-bar">
      <button size="mini" @tap="goAdminHome">返回看板</button>
      <button size="mini" @tap="goUserCenter">个人中心</button>
    </view>
    <crud-table
      :data-list="list"
      :columns="columns"
      :mobile-columns="mobileColumns"
      :search-fields="[]"
      :total="total"
      :page="page"
      :page-size="pageSize"
      :selected-ids="[]"
      :show-checkbox="false"
      :show-add="false"
      :show-delete="false"
      :show-batch-action="false"
      :show-batch-status="false"
      edit-text="设置"
      @edit="handleEdit"
      @page-change="handlePageChange"
      @page-size-change="handlePageSizeChange"
    />

    <crud-modal
      :show="modalShow"
      :title="modalTitle"
      :fields="modalFields"
      :data="modalData"
      @close="modalShow = false"
      @submit="handleSubmit"
    />
  </view>
</template>

<script>
import CrudTable from '@/components/common/CrudTable.vue'
import CrudModal from '@/components/common/CrudModal.vue'
import request from '@/utils/http'
import { getUserInfo } from '@/utils/auth'

export default {
  components: {
    CrudTable,
    CrudModal
  },
  data() {
    return {
      list: [],
      total: 0,
      page: 1,
      pageSize: 10,
      modalShow: false,
      modalTitle: '设置用户',
      modalData: {},
      columns: [
        { key: 'username', title: '用户名', width: '150rpx' },
        { key: 'nickname', title: '昵称', width: '150rpx' },
        { key: 'phone', title: '手机号', width: '130rpx' },
        { key: 'role', title: '角色', width: '100rpx', type: 'tag', tagMap: { admin: 'tag-warning', merchant: 'tag-success', user: 'tag-info' }, formatter: (v) => ({ admin: '管理员', merchant: '商家', user: '普通用户' }[v] || v || '-') },
        { key: 'status', title: '状态', width: '100rpx', type: 'tag', tagMap: { 1: 'tag-success', 0: 'tag-danger' }, formatter: (v) => v === 1 ? '启用' : '禁用' },
        { key: 'createTime', title: '创建时间', width: '160rpx', formatter: (v) => v ? String(v).substring(0, 19).replace('T', ' ') : '-' }
      ],
      mobileColumns: [
        { key: 'username', title: '用户名' },
        { key: 'nickname', title: '昵称' },
        { key: 'role', title: '角色', type: 'tag', tagMap: { admin: 'tag-warning', merchant: 'tag-success', user: 'tag-info' }, formatter: (v) => ({ admin: '管理员', merchant: '商家', user: '普通用户' }[v] || v || '-') },
        { key: 'status', title: '状态', type: 'tag', tagMap: { 1: 'tag-success', 0: 'tag-danger' }, formatter: (v) => v === 1 ? '启用' : '禁用' }
      ],
      modalFields: [
        { key: 'role', label: '角色', type: 'select', options: [
          { label: '普通用户', value: 'user' },
          { label: '商家', value: 'merchant' },
          { label: '管理员', value: 'admin' }
        ], defaultValue: 'user' },
        { key: 'status', label: '状态', type: 'select', options: [
          { label: '启用', value: 1 },
          { label: '禁用', value: 0 }
        ], defaultValue: 1 }
      ]
    }
  },
  onShow() {
    if (!this.ensureAdminAccess()) return
    this.loadList()
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
    async loadList() {
      try {
        const res = await request({ url: '/api/admin/users', data: { page: this.page, pageSize: this.pageSize } })
        this.list = res?.data?.list || []
        this.total = res?.data?.total || 0
      } catch (error) {
        uni.showToast({ title: error.message || '加载失败', icon: 'none' })
      }
    },
    handleEdit(row) {
      this.modalTitle = `设置用户：${row.nickname || row.username}`
      this.modalData = { id: row.id, role: row.role, status: row.status }
      this.modalShow = true
    },
    async handleSubmit(data) {
      try {
        await Promise.all([
          request({ url: `/api/admin/users/${data.id}/role`, method: 'PUT', data: { role: data.role } }),
          request({ url: `/api/admin/users/${data.id}/status`, method: 'PUT', data: { status: data.status } })
        ])
        uni.showToast({ title: '用户设置已更新', icon: 'success' })
        this.modalShow = false
        this.loadList()
      } catch (error) {
        uni.showToast({ title: error.message || '保存失败', icon: 'none' })
      }
    },
    handlePageChange(page) {
      this.page = page
      this.loadList()
    },
    handlePageSizeChange(size) {
      this.pageSize = size
      this.page = 1
      this.loadList()
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
.page-bar { display: flex; justify-content: flex-end; gap: 12rpx; margin-bottom: 16rpx; }
</style>