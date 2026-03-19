<template>
  <view class="crud-table">
    <!-- 查询条件区域 -->
    <view class="search-area" v-if="searchFields.length > 0">
      <view class="search-row">
        <view 
          v-for="(field, index) in searchFields" 
          :key="index" 
          class="search-item"
        >
          <text class="search-label">{{ field.label }}：</text>
          <input 
            v-if="field.type === 'input'"
            class="search-input"
            v-model="searchData[field.key]"
            :placeholder="field.placeholder || `请输入${field.label}`"
          />
          <picker 
            v-if="field.type === 'select'"
            :range="field.options"
            range-key="label"
            @change="onSearchSelectChange($event, field.key)"
          >
            <view class="search-picker">
              {{ searchData[field.key] !== '' ? searchData[field.key] : field.placeholder || '请选择' }}
            </view>
          </picker>
        </view>
        <view class="search-btns">
          <button size="mini" type="primary" @tap="handleSearch">搜索</button>
          <button size="mini" @tap="handleReset">重置</button>
        </view>
      </view>
    </view>

    <!-- 操作按钮区域 -->
    <view class="action-area">
      <view class="action-left">
        <button v-if="showAdd" size="mini" type="primary" @tap="handleAdd">
          <text class="btn-icon">+</text> 新增
        </button>
        <button 
          v-if="showBatchAction && selectedIds.length > 0" 
          size="mini" 
          type="warn"
          @tap="handleBatchDelete"
        >
          批量删除
        </button>
        <button 
          v-if="showBatchStatus && selectedIds.length > 0" 
          size="mini" 
          @tap="handleBatchEnable"
        >
          批量启用
        </button>
        <button 
          v-if="showBatchStatus && selectedIds.length > 0" 
          size="mini" 
          @tap="handleBatchDisable"
        >
          批量禁用
        </button>
      </view>
    </view>

    <!-- PC 端表格 -->
    <view class="table-container" v-if="!isMobile">
      <view class="table">
        <view class="table-header">
          <view v-if="showCheckbox" class="table-col checkbox-col">
            <checkbox-group @change="onSelectAllChange">
              <checkbox :checked="isAllSelected" />
            </checkbox-group>
          </view>
          <view v-if="showIndex" class="table-col index-col">序号</view>
          <view 
            v-for="(col, index) in columns" 
            :key="index" 
            class="table-col"
            :style="{ width: col.width || 'auto' }"
          >
            {{ col.title }}
            <text v-if="col.sortable" class="sort-icon" @tap="handleSort(col.key)">
              {{ getSortIcon(col.key) }}
            </text>
          </view>
          <view v-if="showAction" class="table-col action-col">操作</view>
        </view>
        <view class="table-body">
          <view 
            v-for="(row, rowIndex) in dataList" 
            :key="rowIndex" 
            class="table-row"
          >
            <view v-if="showCheckbox" class="table-col checkbox-col">
              <checkbox-group @change="onSelectChange(row)">
                <checkbox :checked="selectedIds.includes(row.id)" />
              </checkbox-group>
            </view>
            <view v-if="showIndex" class="table-col index-col">{{ (page - 1) * pageSize + rowIndex + 1 }}</view>
            <view 
              v-for="(col, colIndex) in columns" 
              :key="colIndex" 
              class="table-col"
              :style="{ width: col.width || 'auto' }"
            >
              <text v-if="col.type === 'tag'" :class="['tag', getTagClass(col, row)]">
                {{ getCellValue(col, row) }}
              </text>
              <text v-else>{{ getCellValue(col, row) }}</text>
            </view>
            <view v-if="showAction" class="table-col action-col">
              <button v-if="showEdit" size="mini" class="action-btn edit" @tap="handleEdit(row)">{{ editText }}</button>
              <button v-if="showDelete" size="mini" type="warn" class="action-btn delete" @tap="handleDelete(row)">{{ deleteText }}</button>
            </view>
          </view>
          <view v-if="dataList.length === 0" class="empty-data">
            <text>暂无数据</text>
          </view>
        </view>
      </view>
    </view>

    <!-- 移动端卡片列表 -->
    <view class="card-list" v-else>
      <view 
        v-for="(row, rowIndex) in dataList" 
        :key="rowIndex" 
        class="card-item"
      >
        <view v-if="showCheckbox" class="card-checkbox">
          <checkbox-group @change="onSelectChange(row)">
            <checkbox :checked="selectedIds.includes(row.id)" />
          </checkbox-group>
        </view>
        <view class="card-content">
          <view v-for="(col, colIndex) in mobileColumns" :key="colIndex" class="card-row">
            <text class="card-label">{{ col.title }}：</text>
            <text v-if="col.type === 'tag'" :class="['tag', getTagClass(col, row)]">
              {{ getCellValue(col, row) }}
            </text>
            <text v-else>{{ getCellValue(col, row) }}</text>
          </view>
        </view>
        <view class="card-actions">
          <button v-if="showEdit" size="mini" class="action-btn edit" @tap="handleEdit(row)">{{ editText }}</button>
          <button v-if="showDelete" size="mini" type="warn" class="action-btn delete" @tap="handleDelete(row)">{{ deleteText }}</button>
        </view>
      </view>
      <view v-if="dataList.length === 0" class="empty-data">
        <text>暂无数据</text>
      </view>
    </view>

    <!-- 分页组件 -->
    <view class="pagination" v-if="showPagination && total > 0">
      <view class="pagination-info">共 {{ total }} 条</view>
      <view v-if="!isMobile" class="pagination-pc">
        <picker :range="pageSizeOptions" @change="onPageSizeChange">
          <view class="page-size-picker">{{ pageSize }}条/页</view>
        </picker>
        <button size="mini" :disabled="page === 1" @tap="onPageChange(page - 1)">上一页</button>
        <view class="page-numbers">
          <view 
            v-for="p in pageNumbers" 
            :key="p" 
            :class="['page-number', { active: p === page }]"
            @tap="onPageChange(p)"
          >
            {{ p }}
          </view>
        </view>
        <button size="mini" :disabled="page >= totalPages" @tap="onPageChange(page + 1)">下一页</button>
        <view class="page-jump">
          <text>跳至</text>
          <input type="number" v-model="jumpPage" class="jump-input" />
          <text>页</text>
          <button size="mini" @tap="handleJump">GO</button>
        </view>
      </view>
      <view v-else class="pagination-mobile">
        <button size="mini" :disabled="page === 1" @tap="onPageChange(page - 1)">上一页</button>
        <text class="page-info">{{ page }} / {{ totalPages }}</text>
        <button size="mini" :disabled="page >= totalPages" @tap="onPageChange(page + 1)">下一页</button>
      </view>
    </view>
  </view>
</template>

<script>
export default {
  name: 'CrudTable',
  props: {
    // 数据列表
    dataList: {
      type: Array,
      default: () => []
    },
    // 列配置
    columns: {
      type: Array,
      default: () => []
    },
    // 移动端显示的列（不指定则默认显示前 3 列）
    mobileColumns: {
      type: Array,
      default: () => []
    },
    // 查询字段配置
    searchFields: {
      type: Array,
      default: () => []
    },
    // 总条数
    total: {
      type: Number,
      default: 0
    },
    // 当前页码
    page: {
      type: Number,
      default: 1
    },
    // 每页条数
    pageSize: {
      type: Number,
      default: 10
    },
    // 是否显示复选框
    showCheckbox: {
      type: Boolean,
      default: true
    },
    // 是否显示序号
    showIndex: {
      type: Boolean,
      default: true
    },
    // 是否显示操作列
    showAction: {
      type: Boolean,
      default: true
    },
    // 是否显示新增按钮
    showAdd: {
      type: Boolean,
      default: true
    },
    // 是否显示编辑按钮
    showEdit: {
      type: Boolean,
      default: true
    },
    // 是否显示删除按钮
    showDelete: {
      type: Boolean,
      default: true
    },
    editText: {
      type: String,
      default: '编辑'
    },
    deleteText: {
      type: String,
      default: '删除'
    },
    // 是否显示批量操作
    showBatchAction: {
      type: Boolean,
      default: true
    },
    // 是否显示批量状态操作
    showBatchStatus: {
      type: Boolean,
      default: true
    },
    // 是否显示分页
    showPagination: {
      type: Boolean,
      default: true
    },
    // 已选中的 ID 列表
    selectedIds: {
      type: Array,
      default: () => []
    },
    // 排序字段
    sortField: {
      type: String,
      default: ''
    },
    // 排序方向
    sortOrder: {
      type: String,
      default: ''
    }
  },
  data() {
    return {
      isMobile: false,
      searchData: {},
      jumpPage: '',
      pageSizeOptions: ['10', '20', '50', '100']
    }
  },
  computed: {
    totalPages() {
      return Math.ceil(this.total / this.pageSize) || 1
    },
    pageNumbers() {
      const pages = []
      const total = this.totalPages
      let start = Math.max(1, this.page - 2)
      let end = Math.min(total, this.page + 2)
      for (let i = start; i <= end; i++) {
        pages.push(i)
      }
      return pages
    },
    isAllSelected() {
      return this.dataList.length > 0 && 
             this.dataList.every(item => this.selectedIds.includes(item.id))
    }
  },
  created() {
    this.checkDevice()
    // 初始化搜索数据
    this.searchFields.forEach(field => {
      this.searchData[field.key] = field.defaultValue !== undefined ? field.defaultValue : ''
    })
  },
  methods: {
    checkDevice() {
      const info = uni.getSystemInfoSync()
      this.isMobile = (info.windowWidth || 0) < 768
    },
    getCellValue(col, row) {
      if (col.formatter) {
        return col.formatter(row[col.key], row)
      }
      return row[col.key] !== undefined ? row[col.key] : '-'
    },
    getTagClass(col, row) {
      const value = row[col.key]
      if (col.tagMap) {
        return col.tagMap[value] || ''
      }
      if (col.key === 'status') {
        return value === 1 ? 'tag-success' : 'tag-danger'
      }
      return ''
    },
    getSortIcon(key) {
      if (this.sortField !== key) return '⇅'
      return this.sortOrder === 'asc' ? '↑' : '↓'
    },
    onSearchSelectChange(e, key) {
      const field = this.searchFields.find(f => f.key === key)
      const value = field.options[e.detail.value].value
      this.searchData[key] = value
    },
    handleSearch() {
      this.$emit('search', { ...this.searchData })
    },
    handleReset() {
      this.searchFields.forEach(field => {
        this.searchData[field.key] = field.defaultValue !== undefined ? field.defaultValue : ''
      })
      this.$emit('search', { ...this.searchData })
    },
    handleAdd() {
      this.$emit('add')
    },
    handleEdit(row) {
      this.$emit('edit', row)
    },
    handleDelete(row) {
      this.$emit('delete', row)
    },
    handleBatchDelete() {
      this.$emit('batch-delete', [...this.selectedIds])
    },
    handleBatchEnable() {
      this.$emit('batch-status', { ids: [...this.selectedIds], status: 1 })
    },
    handleBatchDisable() {
      this.$emit('batch-status', { ids: [...this.selectedIds], status: 0 })
    },
    onSelectChange(row) {
      this.$emit('selection-change', row)
    },
    onSelectAllChange(e) {
      this.$emit('selection-all-change', e.detail.value.length > 0)
    },
    handleSort(key) {
      const order = this.sortField === key && this.sortOrder === 'asc' ? 'desc' : 'asc'
      this.$emit('sort', { field: key, order })
    },
    onPageSizeChange(e) {
      const size = parseInt(this.pageSizeOptions[e.detail.value])
      this.$emit('page-size-change', size)
    },
    onPageChange(p) {
      if (p >= 1 && p <= this.totalPages) {
        this.$emit('page-change', p)
      }
    },
    handleJump() {
      const p = parseInt(this.jumpPage)
      if (p >= 1 && p <= this.totalPages) {
        this.$emit('page-change', p)
        this.jumpPage = ''
      } else {
        uni.showToast({ title: '请输入有效页码', icon: 'none' })
      }
    }
  }
}
</script>

<style lang="scss" scoped>
.crud-table {
  background: #fff;
  border-radius: 12rpx;
  padding: 20rpx;
}

/* 查询区域 */
.search-area {
  margin-bottom: 20rpx;
  padding: 20rpx;
  background: #f5f7fa;
  border-radius: 8rpx;
}

.search-row {
  display: flex;
  flex-wrap: wrap;
  gap: 20rpx;
  align-items: center;
}

.search-item {
  display: flex;
  align-items: center;
  gap: 10rpx;
}

.search-label {
  font-size: 24rpx;
  color: #606266;
  white-space: nowrap;
}

.search-input {
  width: 300rpx;
  height: 64rpx;
  background: #fff;
  border: 1rpx solid #dcdfe6;
  border-radius: 6rpx;
  padding: 0 16rpx;
  font-size: 24rpx;
}

.search-picker {
  width: 300rpx;
  height: 64rpx;
  background: #fff;
  border: 1rpx solid #dcdfe6;
  border-radius: 6rpx;
  padding: 0 16rpx;
  display: flex;
  align-items: center;
  font-size: 24rpx;
  color: #606266;
}

.search-btns {
  display: flex;
  gap: 10rpx;
  margin-left: auto;
}

/* 操作区域 */
.action-area {
  margin-bottom: 20rpx;
}

.action-left {
  display: flex;
  gap: 10rpx;
}

.btn-icon {
  font-size: 28rpx;
  font-weight: bold;
}

/* PC 端表格 */
.table-container {
  overflow-x: auto;
}

.table {
  min-width: 100%;
}

.table-header {
  display: flex;
  background: #f5f7fa;
  border-radius: 6rpx;
  font-size: 24rpx;
  color: #606266;
  font-weight: 600;
}

.table-body {
  margin-top: 10rpx;
}

.table-row {
  display: flex;
  padding: 20rpx 0;
  border-bottom: 1rpx solid #ebeef5;
}

.table-row:last-child {
  border-bottom: none;
}

.table-col {
  padding: 0 16rpx;
  display: flex;
  align-items: center;
  font-size: 24rpx;
  color: #333;
}

.checkbox-col, .index-col {
  justify-content: center;
  min-width: 80rpx;
}

.action-col {
  justify-content: center;
  gap: 10rpx;
}

.action-btn {
  padding: 0 16rpx;
  font-size: 22rpx;
}

.action-btn.edit {
  color: #409eff;
  background: #ecf5ff;
  border-color: #d9ecff;
}

.action-btn.delete {
  color: #f56c6c;
  background: #fef0f0;
  border-color: #fde2e2;
}

.sort-icon {
  margin-left: 8rpx;
  color: #c0c4cc;
  font-size: 20rpx;
}

/* 移动端卡片 */
.card-list {
  display: flex;
  flex-direction: column;
  gap: 16rpx;
}

.card-item {
  display: flex;
  flex-direction: column;
  padding: 20rpx;
  background: #f9fafc;
  border-radius: 8rpx;
  border: 1rpx solid #ebeef5;
}

.card-checkbox {
  margin-bottom: 10rpx;
}

.card-content {
  display: flex;
  flex-direction: column;
  gap: 10rpx;
}

.card-row {
  display: flex;
  align-items: center;
  font-size: 24rpx;
  color: #333;
}

.card-label {
  color: #909399;
  min-width: 120rpx;
}

.card-actions {
  display: flex;
  gap: 10rpx;
  margin-top: 16rpx;
  justify-content: flex-end;
}

/* 标签样式 */
.tag {
  padding: 4rpx 12rpx;
  border-radius: 4rpx;
  font-size: 22rpx;
}

.tag-success {
  background: #f0f9eb;
  color: #67c23a;
}

.tag-danger {
  background: #fef0f0;
  color: #f56c6c;
}

.tag-warning {
  background: #fdf6ec;
  color: #e6a23c;
}

.tag-info {
  background: #f4f4f5;
  color: #909399;
}

/* 空数据 */
.empty-data {
  text-align: center;
  padding: 60rpx 0;
  color: #909399;
  font-size: 26rpx;
}

/* 分页 */
.pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 20rpx;
  padding-top: 20rpx;
  border-top: 1rpx solid #ebeef5;
}

.pagination-info {
  font-size: 24rpx;
  color: #909399;
}

.pagination-pc {
  display: flex;
  align-items: center;
  gap: 10rpx;
}

.page-size-picker {
  height: 56rpx;
  background: #fff;
  border: 1rpx solid #dcdfe6;
  border-radius: 6rpx;
  padding: 0 16rpx;
  display: flex;
  align-items: center;
  font-size: 22rpx;
  color: #606266;
}

.page-numbers {
  display: flex;
  gap: 8rpx;
}

.page-number {
  width: 56rpx;
  height: 56rpx;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #fff;
  border: 1rpx solid #dcdfe6;
  border-radius: 6rpx;
  font-size: 22rpx;
  color: #606266;
}

.page-number.active {
  background: #409eff;
  color: #fff;
  border-color: #409eff;
}

.page-jump {
  display: flex;
  align-items: center;
  gap: 8rpx;
}

.jump-input {
  width: 80rpx;
  height: 56rpx;
  background: #fff;
  border: 1rpx solid #dcdfe6;
  border-radius: 6rpx;
  padding: 0 8rpx;
  font-size: 22rpx;
  text-align: center;
}

.pagination-mobile {
  display: flex;
  align-items: center;
  gap: 20rpx;
}

.page-info {
  font-size: 24rpx;
  color: #909399;
}

/* 移动端适配 */
@media (max-width: 768px) {
  .search-input,
  .search-picker {
    width: 240rpx;
  }
  
  .search-btns {
    width: 100%;
    justify-content: flex-start;
    margin-left: 0;
    margin-top: 10rpx;
  }
}
</style>