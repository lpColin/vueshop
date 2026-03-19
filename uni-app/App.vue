<script>
export default {
  // #ifdef H5
  errorCaptured(err) {
    const msg = String(err && err.message ? err.message : err)
    if (msg.includes("read only property '_'") || msg.includes('updateSlots')) {
      console.warn('[兼容处理] 已拦截 slots 只读异常:', msg)
      return false
    }
    return true
  },
  // #endif
  onLaunch() {
    console.log('App Launch')
  }
}
</script>

<style lang="scss">
page {
  background-color: #f5f5f5;
  font-size: 28rpx;
  line-height: 1.5;
}

view, text, image, scroll-view {
  box-sizing: border-box;
}

button {
  border: none;
  outline: none;
}

button::after {
  border: none;
}
</style>