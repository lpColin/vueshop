<template>
  <view v-if="show" class="crud-modal">
    <view class="modal-mask" @tap="handleClose"></view>
    <view class="modal-wrapper">
      <view class="modal-header">
        <text class="modal-title">{{ title }}</text>
        <text class="modal-close" @tap="handleClose">×</text>
      </view>
      <scroll-view class="modal-content" scroll-y>
        <view 
          v-for="(field, index) in fields" 
          :key="index" 
          class="form-item"
        >
          <text class="form-label">
            {{ field.label }}
            <text v-if="field.required" class="required">*</text>
          </text>
          <view class="form-control">
            <input 
              v-if="field.type === 'input' || field.type === 'text'"
              class="form-input"
              v-model="formData[field.key]"
              :placeholder="field.placeholder || `请输入${field.label}`"
              :maxlength="field.maxlength || 100"
            />
            <textarea 
              v-if="field.type === 'textarea'"
              class="form-textarea"
              v-model="formData[field.key]"
              :placeholder="field.placeholder || `请输入${field.label}`"
              :maxlength="field.maxlength || 500"
            />
            <picker 
              v-if="field.type === 'select'"
              :range="field.options"
              range-key="label"
              @change="onSelectChange($event, field.key)"
            >
              <view class="form-picker">
                {{ getPickerLabel(field, formData[field.key]) }}
              </view>
            </picker>
            <view v-if="field.type === 'image'" class="image-uploader" @tap="onChooseImage(field.key)">
              <image v-if="formData[field.key]" :src="formData[field.key]" mode="aspectFill" class="image-preview" />
              <view v-else class="image-placeholder">
                <text class="icon">+</text>
              </view>
            </view>
          </view>
        </view>
      </scroll-view>
      <view class="modal-footer">
        <button size="default" @tap="handleClose">取消</button>
        <button size="default" type="primary" @tap="handleSubmit">确定</button>
      </view>
    </view>
  </view>
</template>

<script>
export default {
  name: 'CrudModal',
  props: {
    show: {
      type: Boolean,
      default: false
    },
    title: {
      type: String,
      default: '编辑'
    },
    fields: {
      type: Array,
      default: () => []
    },
    data: {
      type: Object,
      default: () => ({})
    }
  },
  data() {
    return {
      formData: {}
    }
  },
  watch: {
    show(val) {
      if (val) {
        this.initFormData()
      }
    },
    data: {
      handler() {
        this.initFormData()
      },
      deep: true
    }
  },
  created() {
    this.initFormData()
  },
  methods: {
    initFormData() {
      this.formData = {}
      this.fields.forEach(field => {
        if (this.data[field.key] !== undefined) {
          this.formData[field.key] = this.data[field.key]
        } else if (field.defaultValue !== undefined) {
          this.formData[field.key] = field.defaultValue
        } else {
          this.formData[field.key] = ''
        }
      })
    },
    getPickerLabel(field, value) {
      if (!field.options) return value || '请选择'
      const option = field.options.find(opt => opt.value === value)
      return option ? option.label : (value || '请选择')
    },
    onSelectChange(e, key) {
      const field = this.fields.find(f => f.key === key)
      this.formData[key] = field.options[e.detail.value].value
    },
    onChooseImage(key) {
      uni.chooseImage({
        count: 1,
        sizeType: ['compressed'],
        sourceType: ['album', 'camera'],
        success: (res) => {
          this.formData[key] = res.tempFilePaths[0]
        }
      })
    },
    handleClose() {
      this.$emit('close')
    },
    handleSubmit() {
      // 表单验证
      for (const field of this.fields) {
        if (field.required && !this.formData[field.key]) {
          uni.showToast({
            title: `请输入${field.label}`,
            icon: 'none'
          })
          return
        }
      }
      this.$emit('submit', { ...this.formData })
    }
  }
}
</script>

<style lang="scss" scoped>
.crud-modal {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 1000;
}

.modal-mask {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
}

.modal-wrapper {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 90%;
  max-width: 600rpx;
  max-height: 80vh;
  background: #fff;
  border-radius: 16rpx;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24rpx;
  border-bottom: 1rpx solid #ebeef5;
}

.modal-title {
  font-size: 30rpx;
  font-weight: 600;
  color: #333;
}

.modal-close {
  font-size: 40rpx;
  color: #999;
  line-height: 1;
}

.modal-content {
  flex: 1;
  padding: 24rpx;
  overflow-y: auto;
  max-height: 60vh;
}

.form-item {
  display: flex;
  align-items: center;
  margin-bottom: 24rpx;
}

.form-item:last-child {
  margin-bottom: 0;
}

.form-label {
  width: 160rpx;
  font-size: 26rpx;
  color: #333;
  text-align: right;
  padding-right: 16rpx;
}

.required {
  color: #f56c6c;
  margin-left: 4rpx;
}

.form-control {
  flex: 1;
}

.form-input {
  width: 100%;
  height: 72rpx;
  background: #f5f7fa;
  border-radius: 8rpx;
  padding: 0 16rpx;
  font-size: 26rpx;
}

.form-textarea {
  width: 100%;
  min-height: 150rpx;
  background: #f5f7fa;
  border-radius: 8rpx;
  padding: 16rpx;
  font-size: 26rpx;
}

.form-picker {
  width: 100%;
  height: 72rpx;
  background: #f5f7fa;
  border-radius: 8rpx;
  padding: 0 16rpx;
  display: flex;
  align-items: center;
  font-size: 26rpx;
  color: #333;
}

.image-uploader {
  width: 150rpx;
  height: 150rpx;
  background: #f5f7fa;
  border-radius: 8rpx;
  display: flex;
  align-items: center;
  justify-content: center;
}

.image-preview {
  width: 100%;
  height: 100%;
  border-radius: 8rpx;
}

.image-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.image-placeholder .icon {
  font-size: 48rpx;
  color: #8c939d;
}

.modal-footer {
  display: flex;
  gap: 20rpx;
  padding: 24rpx;
  border-top: 1rpx solid #ebeef5;
}

.modal-footer button {
  flex: 1;
}
</style>